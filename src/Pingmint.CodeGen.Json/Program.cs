using Pingmint.CodeGen.CSharp;

using static System.Console;

namespace Pingmint.CodeGen.Json;

internal static partial class Program
{
    internal static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            WriteLine("""

            jsoncodegen arguments:
              -a or --access: access modifier for generated types
              -c or --class:  class name with namespace for the serializer
              -i or --input:  path to the specification
              -m or --make:   flag that specifies whether to generate model classes as well
              -o or --output: path for the generated C# file

            """);
            return 1;
        }

        String? inputFileArg = null;
        String? outputFileArg = null;
        String? classPath = null;
        String? accessModifier = null;
        Boolean makeClasses = false;
        using (var argy = args.AsEnumerable().GetEnumerator())
        {
            String? NextArg()
            {
                if (!argy.MoveNext()) return null;
                return argy.Current;
            }
            while (NextArg() is { } arg)
            {
                switch (arg)
                {
                    case "-i":
                    case "--input":
                    {
                        inputFileArg = NextArg();
                        break;
                    }
                    case "-o":
                    case "--output":
                    {
                        outputFileArg = NextArg();
                        break;
                    }
                    case "-c":
                    case "--class":
                    {
                        classPath = NextArg();
                        break;
                    }
                    case "-m":
                    case "--make":
                    {
                        makeClasses = true;
                        break;
                    }
                    case "-a":
                    case "--access":
                    {
                        accessModifier = NextArg();
                        break;
                    }

                    default:
                    {
                        WriteLine("Unexpected argument: " + arg);
                        return 1;
                    }
                }
            }
        }

        if (classPath is null)
        {
            WriteLine("Missing argument: --class");
            return 1;
        }

        var i = classPath.LastIndexOf('.');
        var classNamespace = classPath.Substring(0, i);
        var className = classPath.Substring(i + 1);
        var inputLines = File.ReadAllLines(inputFileArg);

        var syntaxRoot = new Model.Syntax.RootNode
        {
            ClassNamespace = classNamespace,
            ClassName = className,
            Objects = ParseSyntax(inputLines),
        };

        var codeRoot = GetCodeRoot(syntaxRoot, makeClasses, accessModifier);
        var code = GenerateCode(codeRoot);

        using TextWriter textWriter = outputFileArg switch
        {
            { } => new StreamWriter(outputFileArg),
            _ => Console.Out
        };
        textWriter.Write(code.ToString());
        textWriter.Close();
        return 0;
    }

    private static List<Model.Syntax.ObjectNode> ParseSyntax(String[] inputLines)
    {
        var nodes = new List<Model.Syntax.ObjectNode>();
        var props = new List<Model.Syntax.PropertyNode>();

        var current = new Model.Syntax.ObjectNode() { Properties = props };
        void Post()
        {
            if (current.Name == null) return;
            nodes.Add(current);
            props = new();
            current = new() { Properties = props };
        }
        foreach (var line in inputLines)
        {
            if (line.Split("//", 2, StringSplitOptions.TrimEntries) is not [String trim, ..]) continue;
            if (String.IsNullOrEmpty(trim)) { continue; }
            if (trim.StartsWith("//")) { continue; }

            if (trim.StartsWith(":"))
            {
                Post();
                current.Name = trim.Substring(1).Trim();
                current.IsInterface = true;
            }
            else if (trim.StartsWith("-"))
            {
                var trim2 = trim[1..].Trim();
                if (trim2.Split(':', 2, StringSplitOptions.TrimEntries) is not [String leftSide, String rightSide]) throw new InvalidOperationException("unable to parse property type");
                bool isArray = false;
                bool isDict = false;
                if (rightSide.StartsWith('[') && rightSide.EndsWith(']'))
                {
                    isArray = true;
                    rightSide = rightSide[1..^1];
                }
                if (rightSide.StartsWith('{') && rightSide.EndsWith('}'))
                {
                    isDict = true;
                    rightSide = rightSide[1..^1];
                }
                if (leftSide.Split("=>", 2, StringSplitOptions.TrimEntries) is not [String key, String name])
                {
                    key = leftSide;
                    name = leftSide;
                }
                key = TrimEnclosure(key, '"', '"');
                Model.Syntax.PropertyNode item = new()
                {
                    Key = key,
                    Name = name,
                    Type = rightSide,
                    IsArray = isArray,
                    IsDictionary = isDict,
                };
                props.Add(item);
            }
            else
            {
                Post();
                if (trim.Split(":", 2, StringSplitOptions.TrimEntries) is [String lhs, String rhs])
                {
                    current.Name = lhs;
                    var inherit = rhs.Split(",", StringSplitOptions.TrimEntries);
                    current.Inherit = inherit.ToList();
                }
                else
                {
                    current.Name = trim;
                }
            }
        }
        Post();
        return nodes;
    }

    private static String TrimEnclosure(String value, Char left, Char right) => (value.StartsWith(left) && value.EndsWith(right)) ? value[1..^1] : value;

    private static Model.Code.Root GetCodeRoot(Model.Syntax.RootNode syntax, Boolean makeClasses, String accessModifier)
    {
        var codeObjects = new List<Model.Code.ObjectNode>();
        var codeArrays = new List<Model.Code.ArrayNode>();
        var codeClasses = new List<Model.Code.ClassNode>();
        var code = new Model.Code.Root()
        {
            ClassNamespace = syntax.ClassNamespace,
            ClassName = syntax.ClassName,
            Objects = codeObjects,
            Arrays = codeArrays,
            Classes = codeClasses,
            AccessModifier = accessModifier,
        };

        var internalCount = 0;

        foreach (var node in syntax.Objects)
        {
            var props = new List<Model.Code.ObjectNodeProperty>();

            String? classNamespace;
            String className;
            var index = node.Name.LastIndexOf('.');
            if (index == -1)
            {
                classNamespace = null;
                className = node.Name;
            }
            else
            {
                classNamespace = node.Name[..index];
                className = node.Name[(index + 1)..];
            }

            // Replace namespace path with valid C# property name
            var sharedInstanceName = PropertyNameRegex().Replace(node.Name, "_");

            codeObjects.Add(new()
            {
                ClassNamespace = classNamespace,
                ClassName = className,
                ClassAccessModifier = accessModifier,
                SharedInstanceName = sharedInstanceName,
                Properties = props,
                IsInterface = node.IsInterface,
            });
        }

        foreach (var (syntaxNode, codeNode) in syntax.Objects.Zip(codeObjects))
        {
            var props = codeNode.Properties;

            if (!syntaxNode.Name.EndsWith(codeNode.ClassName)) throw new InvalidOperationException("assertion failed: mismatched syntax node and class node");

            var classProps = new List<Model.Code.ClassPropertyNode>();
            var classNode = new Model.Code.ClassNode
            {
                ClassAccessModifier = codeNode.ClassAccessModifier,
                ClassName = codeNode.ClassName,
                Properties = classProps,
                IsInterface = syntaxNode.IsInterface,
            };
            codeClasses.Add(classNode);

            if (syntaxNode.Inherit is { } inherit)
            {
                var list = classNode.Inherit = new();
                foreach (var type in inherit)
                {
                    var foundNode = syntax.Objects.FirstOrDefault(i => i.Name == type);
                    if (foundNode == null)
                    {
                        throw new InvalidOperationException($"Unable to find requested type: {type}");
                    }
                    list.Add(foundNode.Name);
                }
            }

            static (Model.Code.ISetter, Model.Code.NodeType) GetTypeInfo(String type, List<Model.Code.ObjectNode> codeObjects)
            {
                Model.Code.ISetter itemSetter;
                Model.Code.NodeType itemType;
                switch (type)
                {
                    case "bool":
                    case "Boolean":
                    {
                        itemSetter = Model.Code.BoolSetter.Instance;
                        itemType = Model.Code.NodeType.Boolean;
                        break;
                    }
                    case "int":
                    case "Int32":
                    {
                        itemSetter = Model.Code.IntSetter.Instance;
                        itemType = Model.Code.NodeType.Number;
                        break;
                    }
                    case "long":
                    case "Int64":
                    {
                        itemSetter = Model.Code.Int64Setter.Instance;
                        itemType = Model.Code.NodeType.Number;
                        break;
                    }
                    case "string":
                    case "String":
                    {
                        itemSetter = Model.Code.StringSetter.Instance;
                        itemType = Model.Code.NodeType.String;
                        break;
                    }
                    // TODO: array of arrays
                    default:
                    {
                        var foundNode = codeObjects.FirstOrDefault(i => i.ClassFullName == type);
                        if (foundNode == null)
                        {
                            throw new InvalidOperationException($"Unable to find requested type: {type}");
                        }
                        itemSetter = new Model.Code.InternalSetter(foundNode.SharedInstanceName);
                        itemType = Model.Code.NodeType.Object;
                        break;
                    }
                }
                return (itemSetter, itemType);
            }

            void AddProp(Model.Syntax.PropertyNode prop, Boolean skipSerializer = false)
            {
                classProps.Add(new()
                {
                    Name = prop.Name,
                    Type = prop.IsArray ? $"List<{prop.Type}>" : prop.IsDictionary ? $"Dictionary<String, {prop.Type}>" : prop.Type,
                });

                if (prop.IsArray)
                {
                    var className = $"InternalSerializer{internalCount++}";
                    props.Add(new()
                    {
                        Key = prop.Key,
                        PropertyName = prop.Name,
                        Type = Model.Code.NodeType.Array,
                        ItemSetter = new Model.Code.InternalArraySetter(className),
                    });
                    if (!skipSerializer)
                    {
                        var (itemSetter, itemType) = GetTypeInfo(prop.Type, codeObjects);
                        var array = new Model.Code.ArrayNode()
                        {
                            ClassName = className,
                            ItemTypeName = prop.Type,
                            ItemSetter = itemSetter,
                            Type = itemType,
                        };
                        codeArrays.Add(array);
                    }
                }
                else if (prop.IsDictionary)
                {
                    if (prop.Key != "*") { throw new NotSupportedException("Custom dictionaries are not yet supported"); }
                    var add = new Model.Code.ObjectNodeProperty
                    {
                        Key = prop.Key,
                        PropertyName = prop.Name,
                    };
                    (add.ItemSetter, add.Type) = GetTypeInfo(prop.Type, codeObjects);
                    codeNode.WildcardProperty = add;
                }
                else
                {
                    var add = new Model.Code.ObjectNodeProperty
                    {
                        Key = prop.Key,
                        PropertyName = prop.Name,
                    };
                    (add.ItemSetter, add.Type) = GetTypeInfo(prop.Type, codeObjects);
                    props.Add(add);
                }
            }
            if (syntaxNode.Inherit is { } inherit2)
            {
                foreach (var type in inherit2)
                {
                    var foundNode = syntax.Objects.FirstOrDefault(i => i.Name == type);
                    if (foundNode == null)
                    {
                        throw new InvalidOperationException($"Unable to find requested type: {type}");
                    }
                    foreach (var prop in foundNode.Properties)
                    {
                        AddProp(prop);
                    }
                }
            }

            foreach (var prop in syntaxNode.Properties)
            {
                AddProp(prop, skipSerializer: syntaxNode.IsInterface);
            }
        }

        if (!makeClasses) { code.Classes.Clear(); }

        return code;
    }

    private static Model.Syntax.ObjectNode SyntaxObject(String name, List<Model.Syntax.PropertyNode> props) => new()
    {
        Name = name,
        Properties = props
    };

    private static Model.Syntax.PropertyNode SyntaxProperty(String key, String name, String type) => new()
    {
        Key = key,
        Name = name,
        Type = type,
        IsArray = false,
    };

    private static Model.Syntax.PropertyNode SyntaxArray(String key, String name, String type) => new()
    {
        Key = key,
        Name = name,
        Type = type,
        IsArray = true,
    };

    private static String GenerateCode(Model.Code.Root root)
    {
        var code = new CodeWriter();

        code.Line("#nullable enable");
        code.UsingNamespace("System");
        code.UsingNamespace("System.Collections.Generic");
        code.UsingNamespace("System.Text.Json");
        code.Line();
        var fileNamespace = root.ClassNamespace;
        code.FileNamespace(fileNamespace);
        code.Line();
        var interfaceModifiers = root.AccessModifier is { } interfaceAccess ? $"{interfaceAccess} " : "";
        code.Line("{0}partial interface IJsonSerializer<T>", interfaceModifiers);
        using (code.CreateBraceScope())
        {
            code.Line("T Deserialize(ref Utf8JsonReader reader);");
            code.Line("void Serialize(ref Utf8JsonWriter writer, T value);");
        }

        WriteRoot(code, root);

        foreach (var type in root.Classes)
        {
            if (type.IsInterface)
            {
                var modifiers = type.ClassAccessModifier is { } access ? $"{access} partial" : "partial";
                code.Line($"{modifiers} interface {type.ClassName}");
                using (code.CreateBraceScope())
                {
                    foreach (var prop in type.Properties)
                    {
                        code.Line("{0}? {1} {{ get; set; }}", GetShortTypeName(fileNamespace, prop.Type), prop.Name);
                    }
                }
            }
            else
            {
                var modifiers = type.ClassAccessModifier is { } access ? $"{access} sealed" : "sealed";
                var which = (type.Inherit is { } inherit) ?
                    code.PartialClass(modifiers, type.ClassName, String.Join(", ", inherit)) :
                    code.PartialClass(modifiers, type.ClassName);
                using (which)
                {
                    foreach (var prop in type.Properties)
                    {
                        code.Line("public {0}? {1} {{ get; set; }}", GetShortTypeName(fileNamespace, prop.Type), prop.Name);
                    }
                }
            }
        }

        return code.ToString();
    }

    private static void WriteRoot(CodeWriter code, Model.Code.Root root)
    {
        var implements = GetImplements(root);

        var modifiers = root.AccessModifier is { } access ? $"{access} sealed" : "sealed";
        using (code.PartialClass(modifiers, root.ClassName, implements))
        {
            WriteSharedInstances(code, root);
            code.Line();

            WriteHelpers(code);
            code.Line();

            foreach (var node in root.Objects)
            {
                if (node.IsInterface) { continue; }
                WriteObjectNode(code, node);
            }

            // Array
            foreach (var arrayNode in root.Arrays)
            {
                WriteArrayNode(code, arrayNode);
            }
        }
    }

    private static String GetImplements(Model.Code.Root root)
    {
        string? imp = null;
        foreach (var node in root.Objects)
        {
            if (node.IsInterface) { continue; }
            var next = $"IJsonSerializer<{node.ClassFullName}>";
            imp += imp == null ? next : ", " + next;
        }
        return imp ?? throw new NullReferenceException("failed to build interface implementer list");
    }

    private static void WriteSharedInstances(CodeWriter code, Model.Code.Root root)
    {
        foreach (var node in root.Objects)
        {
            if (node.IsInterface) { continue; }
            code.Line("public static readonly IJsonSerializer<{0}> {2} = new {1}();", GetShortTypeName(root.FileNamespace, node.ClassFullName), GetShortTypeName(root.FileNamespace, root.ClassFullName), node.SharedInstanceName);
        }
    }

    private static void WriteObjectNode(CodeWriter code, Model.Code.ObjectNode node)
    {
        using (code.ExplicitInterfaceMethod("void", $"IJsonSerializer<{node.ClassFullName}>", "Serialize", $"ref Utf8JsonWriter writer, {node.ClassFullName} value"))
        {
            code.Line("if (value is null) { writer.WriteNullValue(); return; }");
            code.Line("writer.WriteStartObject();");
            if (node.Properties.Count > 0 || node.WildcardProperty is not null)
            {
                foreach (var prop in node.Properties)
                {
                    var localName = $"local{prop.PropertyName}";
                    using (code.If("value.{0} is {{ }} {1}", prop.PropertyName, localName))
                    {
                        code.Line("writer.WritePropertyName(\"{0}\");", prop.Key);
                        prop.ItemSetter.WriteSerializeStatement(code, "writer", localName);
                    }
                }
                if (node.WildcardProperty is { } prop2)
                {
                    var localName = $"local{prop2.PropertyName}";
                    using (code.If("value.{0} is {{ }} {1}", prop2.PropertyName, localName))
                    using (code.ForEach($"var ({localName}Key, {localName}Value) in {localName}"))
                    {
                        code.Line("writer.WritePropertyName({0});", $"{localName}Key");
                        prop2.ItemSetter.WriteSerializeStatement(code, "writer", $"{localName}Value");
                    }
                }
            }
            code.Line("writer.WriteEndObject();");
        }
        code.Line();
        using (code.ExplicitInterfaceMethod(node.ClassFullName, $"IJsonSerializer<{node.ClassFullName}>", "Deserialize", "ref Utf8JsonReader reader"))
        {
            code.Line("var obj = new {0}();", node.ClassFullName);
            using (code.While("true"))
            {
                using (code.Switch("Next(ref reader)"))
                {
                    if (node.Properties.Count > 0 || node.WildcardProperty is not null)
                    {
                        using (code.SwitchCase("JsonTokenType.PropertyName"))
                        {
                            bool isFirst = true;
                            bool didHandleWildcard = false;
                            foreach (var prop in node.Properties)
                            {
                                WriteObjectNodeProperty(code, prop, isFirst);
                                isFirst = false;
                            }
                            if (node.WildcardProperty is { } wildcard)
                            {
                                WriteWildcardProperty(code, wildcard, isFirst);
                            }
                            else
                            {
                                code.Line();
                                code.Line("reader.Skip();");
                                code.Line("break;");
                            }
                        }
                    }
                    using (code.SwitchCase("JsonTokenType.EndObject"))
                    {
                        code.Return("obj");
                    }
                    using (code.SwitchDefault())
                    {
                        code.Line("reader.Skip();");
                        code.Line("break;");
                    }
                }
            }
        }
    }

    private static void WriteWildcardProperty(CodeWriter code, Model.Code.ObjectNodeProperty prop, Boolean isFirst)
    {
        var reader = "reader";
        var jsonTokenType = GetTokenTypeFromPropertyType(prop.Type);

        code.Line("obj.{0} ??= new();", prop.PropertyName);
        code.Line("var lhs = reader.GetString() ?? throw new NullReferenceException();");
        code.Line("var rhs = Next(ref reader) switch", prop.PropertyName);
        using (code.CreateBraceScope(preamble: null, withClosingBrace: ";"))
        {
            code.Line("JsonTokenType.Null => null,");
            switch (prop.Type)
            {
                case Model.Code.NodeType.Object: code.Line("JsonTokenType.StartObject => {1},", jsonTokenType, prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName}")); break;
                case Model.Code.NodeType.Array: code.Line("JsonTokenType.StartArray => {1},", jsonTokenType, prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName} ?? new()")); break;
                case Model.Code.NodeType.Boolean:
                {
                    code.Line("JsonTokenType.True => {0},", prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName} ?? new()"));
                    code.Line("JsonTokenType.False => {0},", prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName} ?? new()"));
                    break;
                }
                default: code.Line("JsonTokenType.{0} => {1},", jsonTokenType, prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName}")); break;
            }
            code.Line("var unexpected => throw new InvalidOperationException($\"unexpected token type for {0}: {{unexpected}} \")", prop.PropertyName);
        }
        code.Line("obj.{0}.Add(lhs, rhs);", prop.PropertyName);
        code.Line("break;");
    }

    private static void WriteObjectNodeProperty(CodeWriter code, Model.Code.ObjectNodeProperty prop, Boolean isFirst)
    {
        var which = isFirst ?
            code.If("reader.ValueTextEquals(\"{0}\")", prop.Key) :
            code.ElseIf("reader.ValueTextEquals(\"{0}\")", prop.Key);
        using (which)
        {
            var reader = "reader";
            var jsonTokenType = GetTokenTypeFromPropertyType(prop.Type);

            code.Line("obj.{0} = Next(ref reader) switch", prop.PropertyName);
            using (code.CreateBraceScope(preamble: null, withClosingBrace: ";"))
            {
                code.Line("JsonTokenType.Null => null,");
                switch (prop.Type)
                {
                    case Model.Code.NodeType.Object: code.Line("JsonTokenType.StartObject => {1},", jsonTokenType, prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName}")); break;
                    case Model.Code.NodeType.Array: code.Line("JsonTokenType.StartArray => {1},", jsonTokenType, prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName} ?? new()")); break;
                    case Model.Code.NodeType.Boolean:
                    {
                        code.Line("JsonTokenType.True => {0},", prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName} ?? new()"));
                        code.Line("JsonTokenType.False => {0},", prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName} ?? new()"));
                        break;
                    }
                    default: code.Line("JsonTokenType.{0} => {1},", jsonTokenType, prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName}")); break;
                }
                code.Line("var unexpected => throw new InvalidOperationException($\"unexpected token type for {0}: {{unexpected}} \")", prop.PropertyName);
            }
            code.Line("break;");
        }
    }

    private static String GetTokenTypeFromPropertyType(Model.Code.NodeType type) => type switch
    {
        Model.Code.NodeType.Array => "StartArray",
        Model.Code.NodeType.Object => "StartObject",
        Model.Code.NodeType.String => "String",
        Model.Code.NodeType.Number => "Number",
        Model.Code.NodeType.Boolean => "Boolean", // not real, actually "True" and "False"
        _ => throw new InvalidOperationException($"unexpected token type in {nameof(WriteObjectNodeProperty)}: {type}")
    };

    private static void WriteArrayNode(CodeWriter code, Model.Code.ArrayNode node)
    {
        var internalSerializerName = node.ClassName;
        var internalSerializerItemType = node.ItemTypeName;
        var reader = "reader";
        using (code.Class("private static", internalSerializerName))
        {
            code.Line($"public static void Serialize<TArray>(ref Utf8JsonWriter writer, TArray array) where TArray : ICollection<{internalSerializerItemType}>");
            using (code.CreateBraceScope())
            {
                code.Line("if (array is null) { writer.WriteNullValue(); return; }");
                code.Line("writer.WriteStartArray();");
                using (code.ForEach("var item in array"))
                {
                    node.ItemSetter.WriteSerializeStatement(code, "writer", "item");
                }
                code.Line("writer.WriteEndArray();");
            }
            code.Line();
            code.Line($"public static TArray Deserialize<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<{internalSerializerItemType}>");
            using (code.CreateBraceScope())
            {
                using (code.While("true"))
                {
                    using (code.Switch("Next(ref reader)"))
                    {

                        using (code.SwitchCase("JsonTokenType.Null"))
                        {
                            // TODO: handle case where JSON token is null, but C# item is not nullable
                            code.Line("reader.Skip();");
                            code.Line("break;");
                        }

                        switch (node.Type)
                        {
                            case Model.Code.NodeType.Boolean:
                            {
                                code.Line($"case JsonTokenType.True: array.Add(true); break;");
                                code.Line($"case JsonTokenType.False: array.Add(false); break;");
                                break;
                            }
                            case Model.Code.NodeType.String:
                            case Model.Code.NodeType.Number:
                            case Model.Code.NodeType.Object:
                            case var other when node.ItemSetter is not null:
                            {
                                var jsonTokenType = GetTokenTypeFromPropertyType(node.Type);
                                using (code.SwitchCase("JsonTokenType.{0}", jsonTokenType))
                                {
                                    node.ItemSetter.WriteDeserializeStatement(code, reader, "var item");
                                    code.Line("array.Add({0});", "item");
                                    code.Line("break;");
                                }
                                break;
                            }
                            default:
                            {
                                throw new InvalidOperationException($"Unexpected node type in {nameof(WriteArrayNode)}: {node.Type}");
                            }
                        }

                        using (code.SwitchCase("JsonTokenType.EndArray"))
                        {
                            code.Line("return array;");
                        }
                        using (code.SwitchDefault())
                        {
                            code.Line("reader.Skip();");
                            code.Line("break;");
                        }
                    }
                }
            }
        }
    }

    private static void WriteHelpers(CodeWriter code)
    {
        code.Line("private static JsonTokenType Next(ref Utf8JsonReader reader) => reader.Read() ? reader.TokenType : throw new InvalidOperationException(\"Unable to read next token from Utf8JsonReader\");");
    }

    private static String GetShortTypeName(String fileNamespace, String typeName) => typeName.StartsWith(fileNamespace + ".") ? typeName.Substring(fileNamespace.Length + 1) : typeName;

    [System.Text.RegularExpressions.GeneratedRegex("[^a-zA-Z0-9_]")]
    private static partial System.Text.RegularExpressions.Regex PropertyNameRegex();
}

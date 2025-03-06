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
                    case "ulong":
                    case "UInt64":
                    {
                        itemSetter = Model.Code.UInt64Setter.Instance;
                        itemType = Model.Code.NodeType.Number;
                        break;
                    }
                    case "decimal":
                    case "Decimal":
                    {
                        itemSetter = Model.Code.DecimalSetter.Instance;
                        itemType = Model.Code.NodeType.Number;
                        break;
                    }
                    case "float":
                    case "Single":
                    {
                        itemSetter = Model.Code.FloatSetter.Instance;
                        itemType = Model.Code.NodeType.Number;
                        break;
                    }
                    case "double":
                    case "Double":
                    {
                        itemSetter = Model.Code.DoubleSetter.Instance;
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
                        itemSetter = new Model.Code.InternalSetter(foundNode.ClassFullName);
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
                    var uniqueSuffix = $"{internalCount++}";
                    props.Add(new()
                    {
                        Key = prop.Key,
                        PropertyName = prop.Name,
                        PropertyType = prop.Type,
                        Type = Model.Code.NodeType.Array,
                        ItemSetter = new Model.Code.InternalArraySetter(uniqueSuffix),
                    });
                    if (!skipSerializer)
                    {
                        var (itemSetter, itemType) = GetTypeInfo(prop.Type, codeObjects);
                        var array = new Model.Code.ArrayNode()
                        {
                            UniqueSuffix = uniqueSuffix,
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
                        PropertyType = prop.Type,
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
                        PropertyType = prop.Type,
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
                    code.PartialRecordClass(modifiers, type.ClassName, String.Join(", ", inherit)) :
                    code.PartialRecordClass(modifiers, type.ClassName);
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
        var modifiers = root.AccessModifier is { } access ? $"{access} partial" : "partial";
        code.StartLine();
        code.Line("{0} class {1}", modifiers, root.ClassName);
        using (code.CreateBraceScope())
        {
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

    private static void WriteObjectNode(CodeWriter code, Model.Code.ObjectNode node)
    {
        using (code.Method("public static", "void", "Serialize", $"Utf8JsonWriter writer, {node.ClassFullName}? value"))
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
        using (code.Method("public static", "void", "Deserialize", $"ref Utf8JsonReader reader, {node.ClassFullName} obj"))
        {
            using (code.While("true"))
            {
                code.Line("if (!reader.Read()) throw new InvalidOperationException(\"Unable to read next token from Utf8JsonReader\");");
                using (code.Switch("reader.TokenType"))
                {
                    if (node.Properties.Count > 0 || node.WildcardProperty is not null)
                    {
                        using (code.SwitchCase("JsonTokenType.PropertyName"))
                        {
                            bool isFirst = true;
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
                                code.Line("reader.Skip();"); // move to property value
                                code.Line("reader.Skip();"); // skip property value
                                code.Line("break;");
                            }
                        }
                    }
                    code.Line("case JsonTokenType.EndObject: { return; }");
                    code.Line("default: { reader.Skip(); break; }");
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
        code.Line("if (!reader.Read()) throw new InvalidOperationException(\"Unable to read next token from Utf8JsonReader\");");
        code.Line("{0} rhs;", prop.PropertyType);
        code.Line("if (reader.TokenType == JsonTokenType.Null) {{ break; }}", prop.PropertyName);
        switch (prop.Type)
        {
            case Model.Code.NodeType.Object:
            {
                code.Line("else if (reader.TokenType == JsonTokenType.StartObject) {{ rhs = new(); {1}; }}",
                prop.PropertyName,
                prop.ItemSetter.GetDeserializeExpression(reader, "rhs"));
                break;
            }
            case Model.Code.NodeType.Array:
            {
                code.Line("else if (reader.TokenType == JsonTokenType.StartArray) {{ rhs = {1}; }}",
                prop.PropertyName,
                prop.ItemSetter.GetDeserializeExpression(reader, "rhs"));
                break;
            }
            case Model.Code.NodeType.Boolean:
            {
                code.Line("else if (reader.TokenType == JsonTokenType.True) {{ rhs = true; }}", prop.PropertyName);
                code.Line("else if (reader.TokenType == JsonTokenType.False) {{ rhs = false; }}", prop.PropertyName);
                break;
            }
            default:
            {
                code.Line("else if (reader.TokenType == JsonTokenType.{2}) {{ rhs = {1}; }}",
                prop.PropertyName,
                prop.ItemSetter.GetDeserializeExpression(reader, "rhs"),
                jsonTokenType);
                break;
            }
        }
        code.Line("else throw new InvalidOperationException($\"unexpected token type for {0}: {{reader.TokenType}} \");", prop.PropertyName);
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

            code.Line("if (!reader.Read()) throw new InvalidOperationException(\"Unable to read next token from Utf8JsonReader\");");
            code.Line("if (reader.TokenType == JsonTokenType.Null) {{ obj.{0} = null; break; }}", prop.PropertyName);
            {
                switch (prop.Type)
                {
                    case Model.Code.NodeType.Object:
                    {
                        code.Line("if (reader.TokenType == JsonTokenType.StartObject) {{ obj.{0} = new(); {1}; break; }}",
                        prop.PropertyName,
                        prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName}"));
                        break;
                    }
                    case Model.Code.NodeType.Array:
                    {
                        code.Line("if (reader.TokenType == JsonTokenType.StartArray) {{ obj.{0} = {1}; break; }}",
                        prop.PropertyName,
                        prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName} ?? new()"));
                        break;
                    }
                    case Model.Code.NodeType.Boolean:
                    {
                        code.Line("if (reader.TokenType == JsonTokenType.True) {{ obj.{0} = true; break; }}", prop.PropertyName);
                        code.Line("if (reader.TokenType == JsonTokenType.False) {{ obj.{0} = false; break; }}", prop.PropertyName);
                        break;
                    }
                    case Model.Code.NodeType.String:
                    {
                        code.Line("if (reader.TokenType == JsonTokenType.{2}) {{ obj.{0} = {1}!; break; }}", // null-forgiving is ok because we know it's a string
                        prop.PropertyName,
                        prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName}"),
                        jsonTokenType);
                        break;
                    }
                    default:
                    {
                        code.Line("if (reader.TokenType == JsonTokenType.{2}) {{ obj.{0} = {1}; break; }}",
                        prop.PropertyName,
                        prop.ItemSetter.GetDeserializeExpression(reader, $"obj.{prop.PropertyName}"),
                        jsonTokenType);
                        break;
                    }
                }
                code.Line("throw new InvalidOperationException($\"unexpected token type for {0}: {{reader.TokenType}} \");", prop.PropertyName);
            }
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
        var uniqueSuffix = node.UniqueSuffix;
        var internalSerializerItemType = node.ItemTypeName;
        var reader = "reader";
        code.Line($"private static void Serialize{uniqueSuffix}<TArray>(Utf8JsonWriter writer, TArray array) where TArray : ICollection<{internalSerializerItemType}>");
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
        code.Line($"private static TArray Deserialize{uniqueSuffix}<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<{internalSerializerItemType}>");
        using (code.CreateBraceScope())
        {
            using (code.While("true"))
            {
                code.Line("if (!reader.Read()) throw new InvalidOperationException(\"Unable to read next token from Utf8JsonReader\");");
                using (code.Switch("reader.TokenType"))
                {
                    code.Line("case JsonTokenType.Null: { reader.Skip(); break; }");

                    switch (node.Type)
                    {
                        case Model.Code.NodeType.Boolean:
                        {
                            code.Line("case JsonTokenType.True: { array.Add(true); break; }");
                            code.Line("case JsonTokenType.False: { array.Add(false); break; }");
                            break;
                        }
                        case Model.Code.NodeType.String:
                        {
                            using (code.SwitchCase("JsonTokenType.String"))
                            {
                                node.ItemSetter.WriteDeserializeStatement(code, reader, internalSerializerItemType, "item");
                                code.Line("array.Add({0}!);", "item"); // null-forgiving is ok because we know it's a string
                                code.Line("break;");
                            }
                            break;
                        }
                        case Model.Code.NodeType.Number:
                        case Model.Code.NodeType.Object:
                        case var other when node.ItemSetter is not null:
                        {
                            var jsonTokenType = GetTokenTypeFromPropertyType(node.Type);
                            using (code.SwitchCase("JsonTokenType.{0}", jsonTokenType))
                            {
                                node.ItemSetter.WriteDeserializeStatement(code, reader, internalSerializerItemType, "item");
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

                    code.Line("case JsonTokenType.EndArray: { return array; }");
                    code.Line("default: { reader.Skip(); break; }");
                }
            }
        }
    }

    private static String GetShortTypeName(String fileNamespace, String typeName) => typeName.StartsWith(fileNamespace + ".") ? typeName.Substring(fileNamespace.Length + 1) : typeName;

    [System.Text.RegularExpressions.GeneratedRegex("[^a-zA-Z0-9_]")]
    private static partial System.Text.RegularExpressions.Regex PropertyNameRegex();
}

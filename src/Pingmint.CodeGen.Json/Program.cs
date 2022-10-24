using Pingmint.CodeGen.CSharp;
using Pingmint.CodeGen.Json.Test;

using static System.Console;

namespace Pingmint.CodeGen.Json;

internal static class Program
{
    internal static int Main(string[] args)
    {
        if (args.Length < 1)
        {
            WriteLine("""

            jsoncodegen arguments:
              -i or --input:  path to the specification
              -o or --output: path for the generated C# file
              -c or --class:  class name with namespace for the serializer
              -m or --make:   flag that specifies whether to generate model classes as well

            """);
            return 1;
        }

        String? inputFileArg = null;
        String? outputFileArg = null;
        String? classPath = null;
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

        var codeRoot = GetCodeRoot(syntaxRoot, makeClasses);
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

        String? parent = null;
        void Post()
        {
            if (parent == null) return;
            nodes.Add(new()
            {
                Name = parent,
                Properties = props,
            });
        }
        foreach (var line in inputLines)
        {
            var trim = line.Trim();
            if (String.IsNullOrEmpty(trim)) { continue; }
            if (trim.StartsWith("-"))
            {
                var trim2 = trim[1..].Trim();
                if (trim2.Split(':', 2, StringSplitOptions.TrimEntries) is not [String leftSide, String rightSide]) throw new InvalidOperationException();
                bool isArray = false;
                if (rightSide.StartsWith('[') && rightSide.EndsWith(']'))
                {
                    isArray = true;
                    rightSide = rightSide[1..^1];
                }
                if (leftSide.Split("=>", 2, StringSplitOptions.TrimEntries) is not [String key, String name])
                {
                    key = leftSide;
                    name = leftSide;
                }
                props.Add(new()
                {
                    Key = key,
                    Name = name,
                    Type = rightSide,
                    IsArray = isArray,
                });
            }
            else
            {
                Post();
                parent = trim;
                props = new();
            }
        }
        Post();
        return nodes;
    }

    private static Model.Code.Root GetCodeRoot(Model.Syntax.RootNode syntax, Boolean makeClasses)
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
        };

        var instanceCount = 0;
        var internalCount = 0;

        foreach (var node in syntax.Objects)
        {
            var props = new List<Model.Code.ObjectNodeProperty>();
            codeObjects.Add(new()
            {
                ClassNamespace = syntax.ClassNamespace,
                ClassName = node.Name,
                SharedInstanceName = $"Instance{instanceCount++}",
                Properties = props,
            });
        }

        foreach (var pair in syntax.Objects.Zip(codeObjects))
        {
            var syntaxNode = pair.First;
            var codeNode = pair.Second;
            var props = codeNode.Properties;

            if (syntaxNode.Name != codeNode.ClassName) throw new InvalidOperationException();

            var classProps = new List<Model.Code.ClassPropertyNode>();
            var classNode = new Model.Code.ClassNode
            {
                ClassName = codeNode.ClassName,
                Properties = classProps,
            };
            codeClasses.Add(classNode); // TODO: only if requested

            foreach (var prop in syntaxNode.Properties)
            {
                classProps.Add(new()
                {
                    Name = prop.Name,
                    Type = prop.IsArray ? $"List<{prop.Type}>" : prop.Type,
                });

                if (prop.IsArray)
                {
                    Model.Code.ISetter itemSetter;
                    switch (prop.Type)
                    {
                        case "int":
                        case "Int32":
                        {
                            itemSetter = Model.Code.IntSetter.Instance;
                            break;
                        }
                        case "string":
                        case "String":
                        {
                            itemSetter = Model.Code.StringSetter.Instance;
                            break;
                        }
                        default:
                        {
                            var foundNode = codeObjects.First(i => i.ClassName == prop.Type);
                            itemSetter = new Model.Code.InternalSetter(foundNode.SharedInstanceName);
                            break;
                        }
                    }
                    var className = $"InternalSerializer{internalCount++}";
                    var array = new Model.Code.ArrayNode()
                    {
                        ClassName = className,
                        ItemTypeName = prop.Type,
                        ItemSetter = itemSetter,
                    };
                    codeArrays.Add(array);
                    props.Add(new()
                    {
                        Key = prop.Key,
                        PropertyName = prop.Name,
                        Type = Model.Code.ObjectNodePropertyType.Array,
                        ItemSetter = new Model.Code.InternalSetter(className + ".Instance"),
                    });
                }
                else
                {
                    var add = new Model.Code.ObjectNodeProperty
                    {
                        Key = prop.Key,
                        PropertyName = prop.Name,
                    };
                    switch (prop.Type)
                    {
                        case "int":
                        case "Int32":
                        {
                            add.Type = Model.Code.ObjectNodePropertyType.Number;
                            add.ItemSetter = Model.Code.IntSetter.Instance;
                            break;
                        }
                        case "string":
                        case "String":
                        {
                            add.Type = Model.Code.ObjectNodePropertyType.String;
                            add.ItemSetter = Model.Code.StringSetter.Instance;
                            break;
                        }
                        default:
                        {
                            var foundNode = codeObjects.First(i => i.ClassName == prop.Type);
                            add.Type = Model.Code.ObjectNodePropertyType.Object;
                            add.ItemSetter = new Model.Code.InternalSetter(foundNode.SharedInstanceName);
                            break;
                        }
                    }
                    props.Add(add);
                }
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
        code.UsingNamespace("System.Text.Json");
        code.Line();
        code.FileNamespace(root.ClassNamespace);
        code.Line();
        code.Line("public interface IJsonSerializer<T>");
        using (code.CreateBraceScope())
        {
            code.Line("T Deserialize(ref Utf8JsonReader reader);");
        }

        WriteRoot(code, root);

        foreach (var type in root.Classes)
        {
            using (code.PartialClass("sealed", type.ClassName))
            {
                foreach (var prop in type.Properties)
                {
                    code.Line("public {0}? {1} {{ get; set; }}", prop.Type, prop.Name);
                }
            }
        }

        return code.ToString();
    }

    private static void WriteRoot(CodeWriter code, Model.Code.Root root)
    {
        var implements = GetImplements(root);

        using (code.Class("internal sealed", root.ClassName, implements))
        {
            WriteSharedInstances(code, root);
            code.Line();

            WriteHelpers(code);
            code.Line();

            foreach (var sampleNode in root.Objects)
            {
                WriteObjectNode(code, sampleNode);
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
            var next = $"IJsonSerializer<{node.ClassFullName}>";
            imp += imp == null ? next : ", " + next;
        }
        return imp ?? throw new NullReferenceException();
    }

    private static void WriteSharedInstances(CodeWriter code, Model.Code.Root root)
    {
        foreach (var node in root.Objects)
        {
            code.Line("private static readonly IJsonSerializer<{0}> {2} = new {1}();", node.ClassFullName, root.ClassFullName, node.SharedInstanceName);
        }
    }

    private static void WriteObjectNode(CodeWriter code, Model.Code.ObjectNode node)
    {
        using (code.ExplicitInterfaceMethod(node.ClassFullName, $"IJsonSerializer<{node.ClassFullName}>", "Deserialize", "ref Utf8JsonReader reader"))
        {
            code.Line("var obj = new {0}();", node.ClassFullName);
            using (code.While("true"))
            {
                using (code.Switch("Next(ref reader)"))
                {
                    if (node.Properties.Count > 0)
                    {
                        using (code.SwitchCase("JsonTokenType.PropertyName"))
                        {
                            foreach (var prop in node.Properties)
                            {
                                WriteObjectNodeProperty(code, prop);
                            }
                            code.Line();
                            code.Line("Skip(ref reader);");
                            code.Line("break;");
                        }
                    }
                    using (code.SwitchCase("JsonTokenType.EndObject"))
                    {
                        code.Return("obj");
                    }
                    using (code.SwitchDefault())
                    {
                        code.Line("Skip(ref reader);");
                        code.Line("break;");
                    }
                }
            }
        }
    }

    private static void WriteObjectNodeProperty(CodeWriter code, Model.Code.ObjectNodeProperty prop)
    {
        using (code.If("reader.ValueTextEquals(\"{0}\")", prop.Key))
        {
            var next = "next";
            code.Line("var {0} = Next(ref reader);", next);
            switch (prop.Type)
            {
                case Model.Code.ObjectNodePropertyType.Array:
                {
                    using (code.If("next == JsonTokenType.StartArray"))
                    {
                        var reader = "reader";
                        prop.ItemSetter.WriteAbove(code, reader);
                        code.Line("obj.{1} = {0};", prop.ItemSetter.GetExpression(reader), prop.PropertyName);
                        prop.ItemSetter.WriteBelow(code, reader);
                        code.Line("break;");
                    }
                    using (code.Else())
                    {
                        code.Line("Skip(ref reader);");
                        code.Line("break;");
                    }
                    break;
                }
                case Model.Code.ObjectNodePropertyType.Object:
                {
                    using (code.If("next == JsonTokenType.StartObject"))
                    {
                        var reader = "reader";
                        prop.ItemSetter.WriteAbove(code, reader);
                        code.Line("obj.{1} = {0};", prop.ItemSetter.GetExpression(reader), prop.PropertyName);
                        prop.ItemSetter.WriteBelow(code, reader);
                        code.Line("break;");
                    }
                    using (code.Else())
                    {
                        code.Line("Skip(ref reader);");
                        code.Line("break;");
                    }
                    break;
                }
                case Model.Code.ObjectNodePropertyType.String:
                {
                    var reader = "reader";
                    WriteObjectNodePropertySetter(code, reader, next, prop);
                    code.Line("break;");
                    break;
                }
                case Model.Code.ObjectNodePropertyType.Number:
                {
                    var reader = "reader";
                    WriteObjectNodePropertySetter(code, reader, next, prop);
                    code.Line("break;");
                    break;
                }
                default: throw new InvalidOperationException();
            }
        }
    }

    private static void WriteObjectNodePropertySetter(CodeWriter code, String reader, String nextToken, Model.Code.ObjectNodeProperty prop)
    {
        var tokenType = prop.Type switch
        {
            Model.Code.ObjectNodePropertyType.String => "JsonTokenType.String",
            Model.Code.ObjectNodePropertyType.Number => "JsonTokenType.Number",
            _ => throw new InvalidOperationException(),
        };
        using (code.If("{0} == JsonTokenType.Null", nextToken, tokenType))
        {
            WriteModelSetter(code, reader, prop.PropertyName, Model.Code.NullSetter.Instance);
        }
        using (code.ElseIf("{0} == {1}", nextToken, tokenType))
        {
            WriteModelSetter(code, reader, prop.PropertyName, prop.ItemSetter);
        }
        using (code.Else())
        {
            code.Line("throw new InvalidOperationException();");
        }
    }

    private static void WriteModelSetter(CodeWriter code, String reader, String PropertyName, Model.Code.ISetter ItemSetter)
    {
        ItemSetter.WriteAbove(code, reader);
        code.Line("obj.{1} = {0};", ItemSetter.GetExpression(reader), PropertyName);
        ItemSetter.WriteBelow(code, reader);
    }

    private static void WriteModelAdder(CodeWriter code, String reader, Model.Code.ArrayNode prop)
    {
        prop.ItemSetter.WriteAbove(code, reader);
        code.Line("obj.Add({0});", prop.ItemSetter.GetExpression(reader));
        prop.ItemSetter.WriteBelow(code, reader);
    }

    private static void WriteArrayNode(CodeWriter code, Model.Code.ArrayNode node)
    {
        var internalSerializerName = node.ClassName;
        var internalSerializerType = $"List<{node.ItemTypeName}>";
        var reader = "reader";
        using (code.Class("private", internalSerializerName, $"IJsonSerializer<{internalSerializerType}>"))
        {
            code.Line("public static readonly IJsonSerializer<{0}> Instance = new {1}();", internalSerializerType, internalSerializerName);
            code.Line();
            using (code.Method("public", internalSerializerType, "Deserialize", "ref Utf8JsonReader reader"))
            {
                code.Line("var obj = new {0}();", internalSerializerType);
                using (code.While("true"))
                {
                    using (code.Switch("Next(ref reader)"))
                    {
                        using (code.SwitchCase("JsonTokenType.Number"))
                        {
                            WriteModelAdder(code, reader, node);
                            code.Line("break;");
                        }
                        using (code.SwitchCase("JsonTokenType.EndArray"))
                        {
                            code.Line("return obj;");
                        }
                        using (code.SwitchDefault())
                        {
                            code.Line("Skip(ref reader);");
                            code.Line("break;");
                        }
                    }
                }
            }
        }
    }

    private static void WriteHelpers(CodeWriter code)
    {
        code.Line("""
            private static JsonTokenType Next(ref Utf8JsonReader reader) => reader.Read() ? reader.TokenType : throw new InvalidOperationException();

                private static void Skip(ref Utf8JsonReader reader)
                {
                    var depth = 0;
                    do
                    {
                        switch (Next(ref reader))
                        {
                            case JsonTokenType.Null:
                            case JsonTokenType.String:
                            case JsonTokenType.True:
                            case JsonTokenType.False:
                            case JsonTokenType.Number:
                            case JsonTokenType.PropertyName:
                            {
                                break;
                            }
                            case JsonTokenType.StartArray:
                            case JsonTokenType.StartObject:
                            {
                                depth++;
                                break;
                            }
                            case JsonTokenType.EndArray:
                            case JsonTokenType.EndObject:
                            {
                                depth--;
                                break;
                            }
                            default:
                            {
                                throw new InvalidOperationException();
                            }
                        }
                    } while (depth > 0);
                }
            """);
    }
}

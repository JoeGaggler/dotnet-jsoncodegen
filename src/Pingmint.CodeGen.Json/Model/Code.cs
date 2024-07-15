namespace Pingmint.CodeGen.Json.Model.Code;

public class Root
{
    public String FileNamespace => ClassNamespace; // just in case these would vary in the future
    public String ClassNamespace { get; set; }
    public String ClassName { get; set; }
    public String ClassFullName => $"{ClassNamespace}.{ClassName}";
    public String? AccessModifier { get; set; }
    public List<ObjectNode> Objects { get; set; }
    public List<ArrayNode> Arrays { get; set; }
    public List<ClassNode> Classes { get; set; }
}

public enum NodeType { Number, String, Boolean, Object, Array }

public class ObjectNode
{
    public String? ClassNamespace { get; set; }
    public String ClassName { get; set; }
    public String ClassFullName => (ClassNamespace is null) ? ClassName : $"{ClassNamespace}.{ClassName}";
    public String? ClassAccessModifier { get; set; }
    public List<ObjectNodeProperty> Properties { get; set; }
    public ObjectNodeProperty WildcardProperty { get; set; }
    public Boolean IsInterface { get; set; }
}

public class ObjectNodeProperty
{
    public String Key { get; set; }
    public String PropertyName { get; set; }
    public NodeType Type { get; set; }
    public String PropertyType { get; set; }
    public ISetter ItemSetter { get; set; } //
}

public class ArrayNode
{
    public String UniqueSuffix { get; set; }
    public String ItemTypeName { get; set; }
    public ISetter ItemSetter { get; set; }
    public NodeType Type { get; set; }
}

public class ClassNode
{
    public String ClassName { get; set; }
    public String? ClassAccessModifier { get; set; }
    public List<ClassPropertyNode> Properties { get; set; }
    public Boolean IsInterface { get; set; }
    public List<String> Inherit { get; set; }
}

public class ClassPropertyNode
{
    public String Name { get; set; }
    public String Type { get; set; }
}

public interface ISetter
{
    String GetDeserializeExpression(String reader, String target);

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target);

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value);
}

public class IntSetter : ISetter
{
    public static readonly IntSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetInt32()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target) =>
        code.Line($"var {target} = {reader}.GetInt32();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteNumberValue({1});", writer, value);
}

public class Int64Setter : ISetter
{
    public static readonly Int64Setter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetInt64()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target) =>
        code.Line($"var {target} = {reader}.GetInt64();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteNumberValue({1});", writer, value);
}

public class DecimalSetter : ISetter
{
    public static readonly DecimalSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetDecimal()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target) =>
        code.Line($"var {target} = {reader}.GetDecimal();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteNumberValue({1});", writer, value);
}

public class FloatSetter : ISetter
{
    public static readonly FloatSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetSingle()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target) =>
        code.Line($"var {target} = {reader}.GetSingle();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteNumberValue({1});", writer, value);
}

public class DoubleSetter : ISetter
{
    public static readonly DoubleSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetDouble()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target) =>
        code.Line($"var {target} = {reader}.GetDouble();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteNumberValue({1});", writer, value);
}

public class BoolSetter : ISetter
{
    public static readonly BoolSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetBoolean()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target) =>
        code.Line($"var {target} = {reader}.GetBoolean();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteBooleanValue({1});", writer, value);
}

public class StringSetter : ISetter
{
    public static readonly StringSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetString()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target) =>
        code.Line($"var {target} = {reader}.GetString();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteStringValue({1});", writer, value);
}

public class InternalSetter : ISetter
{
    private readonly string type;

    public InternalSetter(String type)
    {
        this.type = type;
    }

    public String GetDeserializeExpression(String reader, String? target) =>
        String.Format("Deserialize(ref {0}, {1})", reader, target);

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target)
    {
        code.Line("{0} {1} = new();", type, target);
        code.Line("Deserialize(ref {0}, {1});", reader, target);
    }

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("Serialize({0}, {1});", writer, value);
}

public class InternalArraySetter : ISetter
{
    private readonly String uniqueSuffix;

    public InternalArraySetter(String uniqueSuffix)
    {
        this.uniqueSuffix = uniqueSuffix;
    }

    public String GetDeserializeExpression(String reader, String? target) =>
        String.Format("Deserialize{0}(ref {1}, {2})", this.uniqueSuffix, reader, target);

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String type, String target)
    {
        code.Line("{0} {1} = new();", type, target);
        code.Line("Deserialize{0}(ref {1}, {2});", this.uniqueSuffix, reader, target);
    }

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("Serialize{0}({1}, {2});", this.uniqueSuffix, writer, value);
}

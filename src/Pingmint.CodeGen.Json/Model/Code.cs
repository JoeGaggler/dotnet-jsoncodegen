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
    public String ClassNamespace { get; set; }
    public String ClassName { get; set; }
    public String ClassFullName => $"{ClassNamespace}.{ClassName}";
    public String? ClassAccessModifier { get; set; }
    public String SharedInstanceName { get; set; }
    public List<ObjectNodeProperty> Properties { get; set; }
    public ObjectNodeProperty WildcardProperty { get; set; }
    public Boolean IsInterface { get; set; }
}

public class ObjectNodeProperty
{
    public String Key { get; set; }
    public String PropertyName { get; set; }
    public NodeType Type { get; set; }
    public ISetter ItemSetter { get; set; }
}

public class ArrayNode
{
    public String ClassName { get; set; }
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
}

public class ClassPropertyNode
{
    public String Name { get; set; }
    public String Type { get; set; }
}

public interface ISetter
{
    String GetDeserializeExpression(String reader, String target) =>
        String.Format("null /* TODO: implement WriteDeserializeStatement in {0} */", GetType().FullName);

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String? target = null) =>
        code.Line("{0} = null; // TODO: implement WriteDeserializeStatement in {1}", reader, GetType().FullName);

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteNullValue(); // TODO: implement WriteSerializeStatement in {1}", writer, GetType().FullName);
}

public class NullSetter : ISetter
{
    public static readonly NullSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) => "null";
    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String? target) => code.Line("{0} = null;");
    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) => code.Line("{0}.WriteNullValue();", writer);
}

public class IntSetter : ISetter
{
    public static readonly IntSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetInt32()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String? target) =>
        code.Line($"{target} = {reader}.GetInt32();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteNumberValue({1});", writer, value);
}

public class BoolSetter : ISetter
{
    public static readonly BoolSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetBoolean()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String? target) =>
        code.Line($"{target} = {reader}.GetBoolean();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteBooleanValue({1});", writer, value);
}

public class StringSetter : ISetter
{
    public static readonly StringSetter Instance = new();
    public String GetDeserializeExpression(String reader, String? target) =>
        $"{reader}.GetString()";

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String? target) =>
        code.Line($"{target} = {reader}.GetString();");

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.WriteStringValue({1});", writer, value);
}

public class InternalSetter : ISetter
{
    private readonly String Serializer;

    public InternalSetter(String serializer)
    {
        this.Serializer = serializer;
    }

    public String GetDeserializeExpression(String reader, String? target) =>
        String.Format("{0}.Deserialize(ref {1})", this.Serializer, reader);

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String? target) =>
        code.Line("{2} = {0}.Deserialize(ref {1});", this.Serializer, reader, target);

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.Serialize(ref {1}, {2});", this.Serializer, writer, value);
}

public class InternalArraySetter : ISetter
{
    private readonly String Serializer;

    public InternalArraySetter(String serializer)
    {
        this.Serializer = serializer;
    }

    public String GetDeserializeExpression(String reader, String? target) =>
        String.Format("{0}.Deserialize(ref {1}, {2})", this.Serializer, reader, target);

    public void WriteDeserializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String reader, String? target) =>
        code.Line("{2} = {0}.Deserialize(ref {1}, {2});", this.Serializer, reader, target);

    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) =>
        code.Line("{0}.Serialize(ref {1}, {2});", this.Serializer, writer, value);
}

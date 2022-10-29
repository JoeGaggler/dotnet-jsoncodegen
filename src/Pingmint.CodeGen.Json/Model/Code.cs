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

public class ObjectNode
{
    public String ClassNamespace { get; set; }
    public String ClassName { get; set; }
    public String ClassFullName => $"{ClassNamespace}.{ClassName}";
    public String? ClassAccessModifier { get; set; }
    public String SharedInstanceName { get; set; }
    public List<ObjectNodeProperty> Properties { get; set; }
}

public enum ObjectNodePropertyType { Number, String, Boolean, Object, Array }

public class ObjectNodeProperty
{
    public String Key { get; set; }
    public String PropertyName { get; set; }
    public ObjectNodePropertyType Type { get; set; }
    public ISetter ItemSetter { get; set; }
}

public class ArrayNode
{
    public String ClassName { get; set; }
    public String ItemTypeName { get; set; }
    public ISetter ItemSetter { get; set; }
    public ObjectNodePropertyType Type { get; set; }
}

public class ClassNode
{
    public String ClassName { get; set; }
    public String? ClassAccessModifier { get; set; }
    public List<ClassPropertyNode> Properties { get; set; }
}

public class ClassPropertyNode
{
    public String Name { get; set; }
    public String Type { get; set; }
}

public interface ISetter
{
    String GetDeserializeExpression(String reader);
    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) => code.Line("{0}.WriteNullValue(); // TODO: implement WriteSerializeStatement in {1}", writer, GetType().FullName);
}

public class NullSetter : ISetter
{
    public static readonly NullSetter Instance = new();
    public String GetDeserializeExpression(String reader) => "null";
    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) => code.Line("{0}.WriteNullValue();", writer);
}

public class IntSetter : ISetter
{
    public static readonly IntSetter Instance = new();
    public String GetDeserializeExpression(String reader) => $"{reader}.GetInt32()";
    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) => code.Line("{0}.WriteNumberValue({1});", writer, value);
}

public class StringSetter : ISetter
{
    public static readonly StringSetter Instance = new();
    public String GetDeserializeExpression(String reader) => $"{reader}.GetString()";
    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) => code.Line("{0}.WriteStringValue({1});", writer, value);
}

public class InternalSetter : ISetter
{
    private readonly String Serializer;

    public InternalSetter(String serializer)
    {
        this.Serializer = serializer;
    }

    public String GetDeserializeExpression(String reader) => $"{this.Serializer}.Deserialize(ref reader)";
    public void WriteSerializeStatement(Pingmint.CodeGen.CSharp.CodeWriter code, String writer, String value) => code.Line("{0}.Serialize(ref {1}, {2});", this.Serializer, writer, value);
}

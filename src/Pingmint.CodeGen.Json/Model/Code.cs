namespace Pingmint.CodeGen.Json.Model.Code;

public class Root
{
    public String ClassNamespace { get; set; }
    public String ClassName { get; set; }
    public String ClassFullName => $"{ClassNamespace}.{ClassName}";
    public List<ObjectNode> Objects { get; set; }
    public List<ArrayNode> Arrays { get; set; }
    public List<ClassNode> Classes { get; set; }
}

public class ObjectNode
{
    public String ClassNamespace { get; set; }
    public String ClassName { get; set; }
    public String ClassFullName => $"{ClassNamespace}.{ClassName}";
    public String SharedInstanceName { get; set; }
    public List<ObjectNodeProperty> Properties { get; set; }
}

public enum ObjectNodePropertyType { Number, String, Boolean, Object, Array }

public class ObjectNodeProperty
{
    public String Key { get; set; }
    public String PropertyName { get; set; }
    //public Type PropertyType { get; set; } // TODO
    public ObjectNodePropertyType Type { get; set; }
    public ISetter ItemSetter { get; set; }
}

public class ArrayNode
{
    public String ClassName { get; set; }
    public String ItemTypeName { get; set; }
    public ISetter ItemSetter { get; set; }
}

public class ClassNode
{
    public String ClassName { get; set; }
    public List<ClassPropertyNode> Properties { get; set; }
}

public class ClassPropertyNode
{
    public String Name { get; set; }
    public String Type { get; set; }
}

public interface ISetter
{
    void WriteAbove(Pingmint.CodeGen.CSharp.CodeWriter code, String reader);
    void WriteBelow(Pingmint.CodeGen.CSharp.CodeWriter code, String reader);
    String GetExpression(String reader);
}

public class NullSetter : ISetter
{
    public static readonly NullSetter Instance = new();
    public void WriteAbove(Pingmint.CodeGen.CSharp.CodeWriter code, String reader) { }
    public void WriteBelow(Pingmint.CodeGen.CSharp.CodeWriter code, String reader) { }
    public String GetExpression(String reader) => "null";
}

public class IntSetter : ISetter
{
    public static readonly IntSetter Instance = new();
    public void WriteAbove(Pingmint.CodeGen.CSharp.CodeWriter code, String reader) { }
    public void WriteBelow(Pingmint.CodeGen.CSharp.CodeWriter code, String reader) { }
    public String GetExpression(String reader) => $"{reader}.GetInt32()";
}

public class StringSetter : ISetter
{
    public static readonly StringSetter Instance = new();
    public void WriteAbove(Pingmint.CodeGen.CSharp.CodeWriter code, String reader) { }
    public void WriteBelow(Pingmint.CodeGen.CSharp.CodeWriter code, String reader) { }
    public String GetExpression(String reader) => $"{reader}.GetString()";
}

public class InternalSetter : ISetter
{
    private readonly String Serializer;

    public InternalSetter(String serializer)
    {
        this.Serializer = serializer;
    }

    public void WriteAbove(Pingmint.CodeGen.CSharp.CodeWriter code, String reader) { }
    public void WriteBelow(Pingmint.CodeGen.CSharp.CodeWriter code, String reader) { }
    public String GetExpression(String reader) => $"{this.Serializer}.Deserialize(ref reader)";
}

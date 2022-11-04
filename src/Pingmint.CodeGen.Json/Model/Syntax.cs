namespace Pingmint.CodeGen.Json.Model.Syntax;

sealed class RootNode
{
    public String ClassNamespace { get; set; }
    public String ClassName { get; set; }
    public List<ObjectNode> Objects { get; set; }
}

sealed class ObjectNode
{
    public String Name { get; set; }
    public Boolean IsInterface { get; set; }
    public List<PropertyNode> Properties { get; set; }
    public List<String> Inherit { get; set; }
}

sealed class PropertyNode
{
    public String Key { get; set; }
    public String Name { get; set; }
    public String Type { get; set; }
    public Boolean IsArray { get; set; }
    public Boolean IsDictionary { get; set; }
}

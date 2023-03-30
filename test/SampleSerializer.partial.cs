namespace Pingmint.CodeGen.Json.Test
{

    public partial class Sample { }
    public partial class SampleSerializer { }
    public partial interface IJsonSerializer<T> { }

    namespace Subspace
    {
        public sealed partial class Sample : ICount, IName
        {
            public int? Count { get; set; }
            public bool? IsTrue { get; set; }
            public List<bool>? Bools { get; set; }
            public String? Name { get; set; }
            public List<Int32>? Items { get; set; }
            public Int32? Id { get; set; }
            public Sample? Recursion { get; set; }
            public List<Sample>? Items2 { get; set; }
            public Dictionary<String, String>? Extensions { get; set; }
        }
    }
    public partial interface IName
    {
        String? Name { get; set; }
    }
    public partial interface ICount
    {
        int? Count { get; set; }
        bool? IsTrue { get; set; }
        List<bool>? Bools { get; set; }
    }
}

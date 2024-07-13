namespace Pingmint.CodeGen.Json.Test
{

    public partial class Sample { }
    public partial class SampleSerializer { }

    namespace Subspace
    {
        public sealed partial class Sample : ICount, IName
        {
            public Int64? Count { get; set; }
            public bool? IsTrue { get; set; }
            public List<bool?>? Bools { get; set; }
            public String? Name { get; set; }
            public List<Int64?>? Items { get; set; }
            public Int64? Id { get; set; }
            public Decimal? Percent { get; set; }
            public Sample? Recursion { get; set; }
            public List<Sample>? Items2 { get; set; }
            public Dictionary<String, Sample?>? Mapping { get; set; }
        }
    }
    public partial interface IName
    {
        String? Name { get; set; }
    }
    public partial interface ICount
    {
        Int64? Count { get; set; }
        bool? IsTrue { get; set; }
        List<bool?>? Bools { get; set; }
    }
}

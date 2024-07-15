using System.Text;
using System.Text.Json;

namespace Pingmint.CodeGen.Json.Test;


public class SampleTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var json = File.ReadAllText("Sample.json");

        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        Assert.IsTrue(reader.Read());
        Subspace.Sample model = new();
        SampleSerializer.Deserialize(ref reader, model);

        Assert.That(model.Name, Is.EqualTo("hi"));
        Assert.That(model.Meta.Status, Is.EqualTo("active"));
        Assert.That(model.MetaList.Count, Is.EqualTo(2));

        String outJson;
        using (var mem = new MemoryStream())
        {
            var writer = new Utf8JsonWriter(mem, new JsonWriterOptions() { Indented = true });
            try
            {
                SampleSerializer.Serialize(writer, model);
            }
            finally
            {
                writer.Dispose();
            }
            outJson = Encoding.UTF8.GetString(mem.ToArray());
        }
        Console.WriteLine(outJson);

        Assert.Pass();
    }
}

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

        Pingmint.CodeGen.Json.Test.IJsonSerializer<Pingmint.CodeGen.Json.Test.Subspace.Sample> ser = new SampleSerializer();
        var reader = new Utf8JsonReader(Encoding.UTF8.GetBytes(json));
        reader.Read();
        var model = ser.Deserialize(ref reader);
        Assert.That(model.Name, Is.EqualTo("hi"));

        String outJson;
        using (var mem = new MemoryStream())
        {
            var writer = new Utf8JsonWriter(mem, new JsonWriterOptions() { Indented = true });
            try
            {
                ser.Serialize(ref writer, model);
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

#nullable enable
using System.Text.Json;

namespace Pingmint.CodeGen.Json.Test;

public interface IJsonSerializer<T>
{
	T Deserialize(ref Utf8JsonReader reader);
}
internal sealed class SampleSerializer : IJsonSerializer<Pingmint.CodeGen.Json.Test.Sample>
{
	private static readonly IJsonSerializer<Pingmint.CodeGen.Json.Test.Sample> Instance0 = new Pingmint.CodeGen.Json.Test.SampleSerializer();

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

	Pingmint.CodeGen.Json.Test.Sample IJsonSerializer<Pingmint.CodeGen.Json.Test.Sample>.Deserialize(ref Utf8JsonReader reader)
	{
		var obj = new Pingmint.CodeGen.Json.Test.Sample();
		while (true)
		{
			switch (Next(ref reader))
			{
				case JsonTokenType.PropertyName:
				{
					if (reader.ValueTextEquals("items"))
					{
						var next = Next(ref reader);
						if (next == JsonTokenType.StartArray)
						{
							obj.Items = InternalSerializer0.Instance.Deserialize(ref reader);
							break;
						}
						else
						{
							Skip(ref reader);
							break;
						}
					}
					if (reader.ValueTextEquals("name"))
					{
						var next = Next(ref reader);
						if (next == JsonTokenType.Null)
						{
							obj.Name = null;
						}
						else if (next == JsonTokenType.String)
						{
							obj.Name = reader.GetString();
						}
						else
						{
							throw new InvalidOperationException();
						}
						break;
					}
					if (reader.ValueTextEquals("id"))
					{
						var next = Next(ref reader);
						if (next == JsonTokenType.Null)
						{
							obj.Id = null;
						}
						else if (next == JsonTokenType.Number)
						{
							obj.Id = reader.GetInt32();
						}
						else
						{
							throw new InvalidOperationException();
						}
						break;
					}
					if (reader.ValueTextEquals("recursion"))
					{
						var next = Next(ref reader);
						if (next == JsonTokenType.StartObject)
						{
							obj.Recursion = Instance0.Deserialize(ref reader);
							break;
						}
						else
						{
							Skip(ref reader);
							break;
						}
					}

					Skip(ref reader);
					break;
				}
				case JsonTokenType.EndObject:
				{
					return obj;
				}
				default:
				{
					Skip(ref reader);
					break;
				}
			}
		}
	}
	private class InternalSerializer0 : IJsonSerializer<List<Int32>>
	{
		public static readonly IJsonSerializer<List<Int32>> Instance = new InternalSerializer0();

		public List<Int32> Deserialize(ref Utf8JsonReader reader)
		{
			var obj = new List<Int32>();
			while (true)
			{
				switch (Next(ref reader))
				{
					case JsonTokenType.Number:
					{
						obj.Add(reader.GetInt32());
						break;
					}
					case JsonTokenType.EndArray:
					{
						return obj;
					}
					default:
					{
						Skip(ref reader);
						break;
					}
				}
			}
		}
	}
}
sealed partial class Sample
{
	public List<Int32>? Items { get; set; }
	public String? Name { get; set; }
	public Int32? Id { get; set; }
	public Sample? Recursion { get; set; }
}

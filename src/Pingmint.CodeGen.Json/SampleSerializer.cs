#nullable enable
using System.Text.Json;

namespace Pingmint.CodeGen.Json.Test;

partial interface IJsonSerializer<T>
{
	T Deserialize(ref Utf8JsonReader reader);
	void Serialize(ref Utf8JsonWriter writer, T value);
}
sealed partial class SampleSerializer : IJsonSerializer<Pingmint.CodeGen.Json.Test.Sample>
{
	private static readonly IJsonSerializer<Pingmint.CodeGen.Json.Test.Sample> Instance0 = new Pingmint.CodeGen.Json.Test.SampleSerializer();

	private static JsonTokenType Next(ref Utf8JsonReader reader) => reader.Read() ? reader.TokenType : throw new InvalidOperationException();

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
							reader.Skip();
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
							reader.Skip();
							break;
						}
					}

					reader.Skip();
					break;
				}
				case JsonTokenType.EndObject:
				{
					return obj;
				}
				default:
				{
					reader.Skip();
					break;
				}
			}
		}
	}

	void IJsonSerializer<Pingmint.CodeGen.Json.Test.Sample>.Serialize(ref Utf8JsonWriter writer, Pingmint.CodeGen.Json.Test.Sample value)
	{
		if (value is null) { writer.WriteNullValue(); return; }
		writer.WriteStartObject();
		if (value.Items is { } Items)
		{
			writer.WritePropertyName("items");
			InternalSerializer0.Instance.Serialize(ref writer, Items);
		}
		if (value.Name is { } Name)
		{
			writer.WritePropertyName("name");
			writer.WriteStringValue(Name);
		}
		if (value.Id is { } Id)
		{
			writer.WritePropertyName("id");
			writer.WriteNumberValue(Id);
		}
		if (value.Recursion is { } Recursion)
		{
			writer.WritePropertyName("recursion");
			Instance0.Serialize(ref writer, Recursion);
		}
		writer.WriteEndObject();
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
						reader.Skip();
						break;
					}
				}
			}
		}

		public void Serialize(ref Utf8JsonWriter writer, List<Int32> value)
		{
			if (value is null) { writer.WriteNullValue(); return; }
			writer.WriteStartArray();
			foreach (var item in value)
			{
				writer.WriteNumberValue(item);
			}
			writer.WriteEndArray();
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

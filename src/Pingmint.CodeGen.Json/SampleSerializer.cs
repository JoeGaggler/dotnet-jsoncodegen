#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Pingmint.CodeGen.Json.Test;

public partial interface IJsonSerializer<T>
{
	T Deserialize(ref Utf8JsonReader reader);
	void Serialize(ref Utf8JsonWriter writer, T value);
}
public sealed partial class SampleSerializer : IJsonSerializer<Pingmint.CodeGen.Json.Test.Sample>
{
	public static readonly IJsonSerializer<Sample> Sample = new SampleSerializer();

	private static JsonTokenType Next(ref Utf8JsonReader reader) => reader.Read() ? reader.TokenType : throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");

	void IJsonSerializer<Pingmint.CodeGen.Json.Test.Sample>.Serialize(ref Utf8JsonWriter writer, Pingmint.CodeGen.Json.Test.Sample value)
	{
		if (value is null) { writer.WriteNullValue(); return; }
		writer.WriteStartObject();
		if (value.Items is { } localItems)
		{
			writer.WritePropertyName("items");
			InternalSerializer0.Instance.Serialize(ref writer, localItems);
		}
		if (value.Name is { } localName)
		{
			writer.WritePropertyName("name");
			writer.WriteStringValue(localName);
		}
		if (value.Id is { } localId)
		{
			writer.WritePropertyName("id");
			writer.WriteNumberValue(localId);
		}
		if (value.Recursion is { } localRecursion)
		{
			writer.WritePropertyName("recursion");
			Sample.Serialize(ref writer, localRecursion);
		}
		if (value.Items2 is { } localItems2)
		{
			writer.WritePropertyName("items2");
			InternalSerializer1.Instance.Serialize(ref writer, localItems2);
		}
		writer.WriteEndObject();
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
						obj.Items = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartArray => InternalSerializer0.Instance.Deserialize(ref reader),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Items: {unexpected} ")
						};
						break;
					}
					if (reader.ValueTextEquals("name"))
					{
						obj.Name = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.String => reader.GetString(),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Name: {unexpected} ")
						};
						break;
					}
					if (reader.ValueTextEquals("id"))
					{
						obj.Id = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.Number => reader.GetInt32(),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Id: {unexpected} ")
						};
						break;
					}
					if (reader.ValueTextEquals("recursion"))
					{
						obj.Recursion = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartObject => Sample.Deserialize(ref reader),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Recursion: {unexpected} ")
						};
						break;
					}
					if (reader.ValueTextEquals("items2"))
					{
						obj.Items2 = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartArray => InternalSerializer1.Instance.Deserialize(ref reader),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Items2: {unexpected} ")
						};
						break;
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
	private class InternalSerializer0 : IJsonSerializer<List<Int32>>
	{
		public static readonly IJsonSerializer<List<Int32>> Instance = new InternalSerializer0();

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

		public List<Int32> Deserialize(ref Utf8JsonReader reader)
		{
			var obj = new List<Int32>();
			while (true)
			{
				switch (Next(ref reader))
				{
					case JsonTokenType.Null:
					{
						reader.Skip();
						break;
					}
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
	}
	private class InternalSerializer1 : IJsonSerializer<List<Sample>>
	{
		public static readonly IJsonSerializer<List<Sample>> Instance = new InternalSerializer1();

		public void Serialize(ref Utf8JsonWriter writer, List<Sample> value)
		{
			if (value is null) { writer.WriteNullValue(); return; }
			writer.WriteStartArray();
			foreach (var item in value)
			{
				Sample.Serialize(ref writer, item);
			}
			writer.WriteEndArray();
		}

		public List<Sample> Deserialize(ref Utf8JsonReader reader)
		{
			var obj = new List<Sample>();
			while (true)
			{
				switch (Next(ref reader))
				{
					case JsonTokenType.Null:
					{
						reader.Skip();
						break;
					}
					case JsonTokenType.StartObject:
					{
						obj.Add(Sample.Deserialize(ref reader));
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
	}
}
public sealed partial class Sample
{
	public List<Int32>? Items { get; set; }
	public String? Name { get; set; }
	public Int32? Id { get; set; }
	public Sample? Recursion { get; set; }
	public List<Sample>? Items2 { get; set; }
}

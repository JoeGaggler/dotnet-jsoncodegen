#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Pingmint.CodeGen.Json.Test;

public static partial class SampleSerializer
{
	private static JsonTokenType Next(ref Utf8JsonReader reader) => reader.Read() ? reader.TokenType : throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");

	private delegate void DeserializerDelegate<T>(ref Utf8JsonReader r, out T value);
	private static T GetOutParam<T>(ref Utf8JsonReader reader, DeserializerDelegate<T> func) { func(ref reader, out T value); return value; }

	public static void Serialize(Utf8JsonWriter writer, Subspace.Sample value)
	{
		if (value is null) { writer.WriteNullValue(); return; }
		writer.WriteStartObject();
		if (value.Count is { } localCount)
		{
			writer.WritePropertyName("count");
			writer.WriteNumberValue(localCount);
		}
		if (value.IsTrue is { } localIsTrue)
		{
			writer.WritePropertyName("isTrue");
			writer.WriteBooleanValue(localIsTrue);
		}
		if (value.Bools is { } localBools)
		{
			writer.WritePropertyName("bools");
			Serialize0(writer, localBools);
		}
		if (value.Name is { } localName)
		{
			writer.WritePropertyName("name");
			writer.WriteStringValue(localName);
		}
		if (value.Items is { } localItems)
		{
			writer.WritePropertyName("items");
			Serialize1(writer, localItems);
		}
		if (value.Id is { } localId)
		{
			writer.WritePropertyName("id");
			writer.WriteNumberValue(localId);
		}
		if (value.Recursion is { } localRecursion)
		{
			writer.WritePropertyName("recursion");
			Serialize(writer, localRecursion);
		}
		if (value.Items2 is { } localItems2)
		{
			writer.WritePropertyName("items2");
			Serialize2(writer, localItems2);
		}
		if (value.Percent is { } localPercent)
		{
			writer.WritePropertyName("percent");
			writer.WriteNumberValue(localPercent);
		}
		if (value.Extensions is { } localExtensions)
		{
			foreach (var (localExtensionsKey, localExtensionsValue) in localExtensions)
			{
				writer.WritePropertyName(localExtensionsKey);
				writer.WriteStringValue(localExtensionsValue);
			}
		}
		writer.WriteEndObject();
	}

	public static void Deserialize(ref Utf8JsonReader reader, out Subspace.Sample obj)
	{
		obj = new Subspace.Sample();
		while (true)
		{
			switch (Next(ref reader))
			{
				case JsonTokenType.PropertyName:
				{
					if (reader.ValueTextEquals("count"))
					{
						obj.Count = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.Number => reader.GetInt64(),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Count: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("isTrue"))
					{
						obj.IsTrue = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.True => reader.GetBoolean(),
							JsonTokenType.False => reader.GetBoolean(),
							var unexpected => throw new InvalidOperationException($"unexpected token type for IsTrue: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("bools"))
					{
						obj.Bools = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartArray => Deserialize0(ref reader, obj.Bools ?? new()),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Bools: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("name"))
					{
						obj.Name = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.String => reader.GetString(),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Name: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("items"))
					{
						obj.Items = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartArray => Deserialize1(ref reader, obj.Items ?? new()),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Items: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("id"))
					{
						obj.Id = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.Number => reader.GetInt64(),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Id: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("recursion"))
					{
						obj.Recursion = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartObject => GetOutParam<Subspace.Sample>(ref reader, Deserialize),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Recursion: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("items2"))
					{
						obj.Items2 = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartArray => Deserialize2(ref reader, obj.Items2 ?? new()),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Items2: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("percent"))
					{
						obj.Percent = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.Number => reader.GetDecimal(),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Percent: {unexpected} ")
						};
						break;
					}
					obj.Extensions ??= new();
					var lhs = reader.GetString() ?? throw new NullReferenceException();
					var rhs = Next(ref reader) switch
					{
						JsonTokenType.Null => null,
						JsonTokenType.String => reader.GetString(),
						var unexpected => throw new InvalidOperationException($"unexpected token type for Extensions: {unexpected} ")
					};
					obj.Extensions.Add(lhs, rhs);
					break;
				}
				case JsonTokenType.EndObject:
				{
					return;
				}
				default:
				{
					reader.Skip();
					break;
				}
			}
		}
	}
	private static void Serialize0<TArray>(Utf8JsonWriter writer, TArray array) where TArray : ICollection<bool>
	{
		if (array is null) { writer.WriteNullValue(); return; }
		writer.WriteStartArray();
		foreach (var item in array)
		{
			writer.WriteBooleanValue(item);
		}
		writer.WriteEndArray();
	}

	private static TArray Deserialize0<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<bool>
	{
		while (true)
		{
			switch (Next(ref reader))
			{
				case JsonTokenType.Null:
				{
					reader.Skip();
					break;
				}
				case JsonTokenType.True: array.Add(true); break;
				case JsonTokenType.False: array.Add(false); break;
				case JsonTokenType.EndArray:
				{
					return array;
				}
				default:
				{
					reader.Skip();
					break;
				}
			}
		}
	}
	private static void Serialize1<TArray>(Utf8JsonWriter writer, TArray array) where TArray : ICollection<Int64>
	{
		if (array is null) { writer.WriteNullValue(); return; }
		writer.WriteStartArray();
		foreach (var item in array)
		{
			writer.WriteNumberValue(item);
		}
		writer.WriteEndArray();
	}

	private static TArray Deserialize1<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<Int64>
	{
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
					var item = reader.GetInt64();
					array.Add(item);
					break;
				}
				case JsonTokenType.EndArray:
				{
					return array;
				}
				default:
				{
					reader.Skip();
					break;
				}
			}
		}
	}
	private static void Serialize2<TArray>(Utf8JsonWriter writer, TArray array) where TArray : ICollection<Subspace.Sample>
	{
		if (array is null) { writer.WriteNullValue(); return; }
		writer.WriteStartArray();
		foreach (var item in array)
		{
			Serialize(writer, item);
		}
		writer.WriteEndArray();
	}

	private static TArray Deserialize2<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<Subspace.Sample>
	{
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
					Deserialize(ref reader, out Subspace.Sample value);
					var item = value;
					array.Add(item);
					break;
				}
				case JsonTokenType.EndArray:
				{
					return array;
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

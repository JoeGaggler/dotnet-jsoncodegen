#nullable enable
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Pingmint.CodeGen.Json.Test;

public static partial class SampleSerializer
{
	public static void Serialize(Utf8JsonWriter writer, Subspace.Sample? value)
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
		if (value.Meta is { } localMeta)
		{
			writer.WritePropertyName("meta");
			Serialize(writer, localMeta);
		}
		if (value.MetaList is { } localMetaList)
		{
			writer.WritePropertyName("metaList");
			Serialize3(writer, localMetaList);
		}
		if (value.Mapping is { } localMapping)
		{
			foreach (var (localMappingKey, localMappingValue) in localMapping)
			{
				writer.WritePropertyName(localMappingKey);
				Serialize(writer, localMappingValue);
			}
		}
		writer.WriteEndObject();
	}

	public static void Deserialize(ref Utf8JsonReader reader, Subspace.Sample obj)
	{
		while (true)
		{
			if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
			switch (reader.TokenType)
			{
				case JsonTokenType.PropertyName:
				{
					if (reader.ValueTextEquals("count"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Count = null; break; }
						if (reader.TokenType == JsonTokenType.Number) { obj.Count = reader.GetInt64(); break; }
						throw new InvalidOperationException($"unexpected token type for Count: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("isTrue"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.IsTrue = null; break; }
						if (reader.TokenType == JsonTokenType.True) { obj.IsTrue = true; break; }
						if (reader.TokenType == JsonTokenType.False) { obj.IsTrue = false; break; }
						throw new InvalidOperationException($"unexpected token type for IsTrue: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("bools"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Bools = null; break; }
						if (reader.TokenType == JsonTokenType.StartArray) { obj.Bools = new(); Deserialize0(ref reader, obj.Bools); break; }
						throw new InvalidOperationException($"unexpected token type for Bools: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("name"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Name = null; break; }
						if (reader.TokenType == JsonTokenType.String) { obj.Name = reader.GetString()!; break; }
						throw new InvalidOperationException($"unexpected token type for Name: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("items"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Items = null; break; }
						if (reader.TokenType == JsonTokenType.StartArray) { obj.Items = new(); Deserialize1(ref reader, obj.Items); break; }
						throw new InvalidOperationException($"unexpected token type for Items: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("id"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Id = null; break; }
						if (reader.TokenType == JsonTokenType.Number) { obj.Id = reader.GetInt64(); break; }
						throw new InvalidOperationException($"unexpected token type for Id: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("recursion"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Recursion = null; break; }
						if (reader.TokenType == JsonTokenType.StartObject) { obj.Recursion = new(); Deserialize(ref reader, obj.Recursion); break; }
						throw new InvalidOperationException($"unexpected token type for Recursion: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("items2"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Items2 = null; break; }
						if (reader.TokenType == JsonTokenType.StartArray) { obj.Items2 = new(); Deserialize2(ref reader, obj.Items2); break; }
						throw new InvalidOperationException($"unexpected token type for Items2: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("percent"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Percent = null; break; }
						if (reader.TokenType == JsonTokenType.Number) { obj.Percent = reader.GetDecimal(); break; }
						throw new InvalidOperationException($"unexpected token type for Percent: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("meta"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Meta = null; break; }
						if (reader.TokenType == JsonTokenType.StartObject) { obj.Meta = new(); Deserialize(ref reader, obj.Meta); break; }
						throw new InvalidOperationException($"unexpected token type for Meta: {reader.TokenType} ");
					}
					else if (reader.ValueTextEquals("metaList"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.MetaList = null; break; }
						if (reader.TokenType == JsonTokenType.StartArray) { obj.MetaList = new(); Deserialize3(ref reader, obj.MetaList); break; }
						throw new InvalidOperationException($"unexpected token type for MetaList: {reader.TokenType} ");
					}
					obj.Mapping ??= new();
					var lhs = reader.GetString() ?? throw new NullReferenceException();
					if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
					Subspace.Sample rhs;
					if (reader.TokenType == JsonTokenType.Null) { break; }
					else if (reader.TokenType == JsonTokenType.StartObject) { rhs = new(); Deserialize(ref reader, rhs); }
					else throw new InvalidOperationException($"unexpected token type for Mapping: {reader.TokenType} ");
					obj.Mapping.Add(lhs, rhs);
					break;
				}
				case JsonTokenType.EndObject: { return; }
				default: { reader.Skip(); break; }
			}
		}
	}
	public static void Serialize(Utf8JsonWriter writer, Subspace.Meta? value)
	{
		if (value is null) { writer.WriteNullValue(); return; }
		writer.WriteStartObject();
		if (value.Status is { } localStatus)
		{
			writer.WritePropertyName("status");
			writer.WriteStringValue(localStatus);
		}
		writer.WriteEndObject();
	}

	public static void Deserialize(ref Utf8JsonReader reader, Subspace.Meta obj)
	{
		while (true)
		{
			if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
			switch (reader.TokenType)
			{
				case JsonTokenType.PropertyName:
				{
					if (reader.ValueTextEquals("status"))
					{
						if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
						if (reader.TokenType == JsonTokenType.Null) { obj.Status = null; break; }
						if (reader.TokenType == JsonTokenType.String) { obj.Status = reader.GetString()!; break; }
						throw new InvalidOperationException($"unexpected token type for Status: {reader.TokenType} ");
					}

					reader.Skip();
					reader.Skip();
					break;
				}
				case JsonTokenType.EndObject: { return; }
				default: { reader.Skip(); break; }
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

	private static void Deserialize0<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<bool>
	{
		while (true)
		{
			if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: { reader.Skip(); break; }
				case JsonTokenType.True: { array.Add(true); break; }
				case JsonTokenType.False: { array.Add(false); break; }
				case JsonTokenType.EndArray: { return; }
				default: { reader.Skip(); break; }
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

	private static void Deserialize1<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<Int64>
	{
		while (true)
		{
			if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: { reader.Skip(); break; }
				case JsonTokenType.Number:
				{
					var item = reader.GetInt64();
					array.Add(item);
					break;
				}
				case JsonTokenType.EndArray: { return; }
				default: { reader.Skip(); break; }
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

	private static void Deserialize2<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<Subspace.Sample>
	{
		while (true)
		{
			if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: { reader.Skip(); break; }
				case JsonTokenType.StartObject:
				{
					Subspace.Sample item = new();
					Deserialize(ref reader, item);
					array.Add(item);
					break;
				}
				case JsonTokenType.EndArray: { return; }
				default: { reader.Skip(); break; }
			}
		}
	}
	private static void Serialize3<TArray>(Utf8JsonWriter writer, TArray array) where TArray : ICollection<Subspace.Meta>
	{
		if (array is null) { writer.WriteNullValue(); return; }
		writer.WriteStartArray();
		foreach (var item in array)
		{
			Serialize(writer, item);
		}
		writer.WriteEndArray();
	}

	private static void Deserialize3<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<Subspace.Meta>
	{
		while (true)
		{
			if (!reader.Read()) throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");
			switch (reader.TokenType)
			{
				case JsonTokenType.Null: { reader.Skip(); break; }
				case JsonTokenType.StartObject:
				{
					Subspace.Meta item = new();
					Deserialize(ref reader, item);
					array.Add(item);
					break;
				}
				case JsonTokenType.EndArray: { return; }
				default: { reader.Skip(); break; }
			}
		}
	}
}

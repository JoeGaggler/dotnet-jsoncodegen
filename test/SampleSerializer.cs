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
public sealed partial class SampleSerializer : IJsonSerializer<Subspace.Sample>
{
	public static readonly IJsonSerializer<Subspace.Sample> Subspace_Sample = new SampleSerializer();

	private static JsonTokenType Next(ref Utf8JsonReader reader) => reader.Read() ? reader.TokenType : throw new InvalidOperationException("Unable to read next token from Utf8JsonReader");

	void IJsonSerializer<Subspace.Sample>.Serialize(ref Utf8JsonWriter writer, Subspace.Sample value)
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
			InternalSerializer0.Serialize(ref writer, localBools);
		}
		if (value.Name is { } localName)
		{
			writer.WritePropertyName("name");
			writer.WriteStringValue(localName);
		}
		if (value.Items is { } localItems)
		{
			writer.WritePropertyName("items");
			InternalSerializer1.Serialize(ref writer, localItems);
		}
		if (value.Id is { } localId)
		{
			writer.WritePropertyName("id");
			writer.WriteNumberValue(localId);
		}
		if (value.Recursion is { } localRecursion)
		{
			writer.WritePropertyName("recursion");
			Subspace_Sample.Serialize(ref writer, localRecursion);
		}
		if (value.Items2 is { } localItems2)
		{
			writer.WritePropertyName("items2");
			InternalSerializer2.Serialize(ref writer, localItems2);
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

	Subspace.Sample IJsonSerializer<Subspace.Sample>.Deserialize(ref Utf8JsonReader reader)
	{
		var obj = new Subspace.Sample();
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
							JsonTokenType.Number => reader.GetInt32(),
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
							JsonTokenType.StartArray => InternalSerializer0.Deserialize(ref reader, obj.Bools ?? new()),
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
							JsonTokenType.StartArray => InternalSerializer1.Deserialize(ref reader, obj.Items ?? new()),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Items: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("id"))
					{
						obj.Id = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.Number => reader.GetInt32(),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Id: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("recursion"))
					{
						obj.Recursion = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartObject => Subspace_Sample.Deserialize(ref reader),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Recursion: {unexpected} ")
						};
						break;
					}
					else if (reader.ValueTextEquals("items2"))
					{
						obj.Items2 = Next(ref reader) switch
						{
							JsonTokenType.Null => null,
							JsonTokenType.StartArray => InternalSerializer2.Deserialize(ref reader, obj.Items2 ?? new()),
							var unexpected => throw new InvalidOperationException($"unexpected token type for Items2: {unexpected} ")
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
	private static class InternalSerializer0
	{
		public static void Serialize<TArray>(ref Utf8JsonWriter writer, TArray array) where TArray : ICollection<bool>
		{
			if (array is null) { writer.WriteNullValue(); return; }
			writer.WriteStartArray();
			foreach (var item in array)
			{
				writer.WriteBooleanValue(item);
			}
			writer.WriteEndArray();
		}

		public static TArray Deserialize<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<bool>
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
	}
	private static class InternalSerializer1
	{
		public static void Serialize<TArray>(ref Utf8JsonWriter writer, TArray array) where TArray : ICollection<Int32>
		{
			if (array is null) { writer.WriteNullValue(); return; }
			writer.WriteStartArray();
			foreach (var item in array)
			{
				writer.WriteNumberValue(item);
			}
			writer.WriteEndArray();
		}

		public static TArray Deserialize<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<Int32>
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
						var item = reader.GetInt32();
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
	private static class InternalSerializer2
	{
		public static void Serialize<TArray>(ref Utf8JsonWriter writer, TArray array) where TArray : ICollection<Subspace.Sample>
		{
			if (array is null) { writer.WriteNullValue(); return; }
			writer.WriteStartArray();
			foreach (var item in array)
			{
				Subspace_Sample.Serialize(ref writer, item);
			}
			writer.WriteEndArray();
		}

		public static TArray Deserialize<TArray>(ref Utf8JsonReader reader, TArray array) where TArray : ICollection<Subspace.Sample>
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
						var item = Subspace_Sample.Deserialize(ref reader);
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
}

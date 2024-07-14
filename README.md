# dotnet-jsoncodegen

A dotnet tool that generates code that can serialize JSON as described by a simple schema.

## Overview

There are [several](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/overview) ways to read and write JSON in dotnet. This tool leverages the benefits of `Utf8JsonReader` and `Utf8JsonWriter`, producing a C# file that you can use as-is, edit manually, or extend via a `partial` implementation. The generated code does not perform any reflection, and essentially uses the "[fast path](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/reflection-vs-source-generation?pivots=dotnet-8-0#source-generation)" for both serialization [and deserialization](https://github.com/dotnet/runtime/issues/55043).

## Getting Started

Here is an example JSON document that we want to work with:

```json
{
    "id": 1,
    "name": "Joe",
    "children": [
        { "id": 2, "name": "Morgan" },
        { "id": 2, "name": "Casey" },
    ]
}
```

This document represents a person with `id` and `name` properties, and also a `children` property that is an array of other person objects.

We can represent a person in dotnet by defining a `Person` class like this:

```csharp
class Person
{
    public int Id;
    public string Name;
    public List<Person> Children;
}
```

We then write a simple text description of the JSON document and how it maps to the C# type:

```
Person
- id => Id : int
- name => Name : string
- children => Children : [Person]
```

The first line indicates the C# type name that represents the JSON object.
The subsequent lines describes the mapping of the JSON object's properties to the C# type's properties.

The properties mappings consist of three basic parts:
* JSON property key
* C# property name
* C# property type

The `children` property has a type of `[Person]`, which means an array of `Person` objects. The C# property can be any collection type that has an `Add(Person p)` method and likewise implements an enumerable with `Person` elements.

## Schema

In addition to the objects and arrays demonstrated in the previous section, the schema also supports the following:

### Wildcard property

In some models, an object member's name is unspecified, but is still useful. For this reason the schema respresent a "wildcard" property key, as shown here:

```
Person
- name => Name : string
- * => Data : {string}
```

When a `Person` is deserialized, the `name` property will populate into the `Name` field, however any unspecified properties will instead populate `Data`, which is represented as a C# `Dictionary<String, T>` where `T` is the type indicated inside the braces.

## Notes

### Null values

This tools considers `null` values as equivalent to absent values. This is technically incorrect JSON behavior, but I currently have no practical use case for an explicit `null` value. I may encounter an API that considers an absent value as an intent to leave the existing value as-is vs nulling it out -- in that case I would update this tool for my needs.

The benefit of this decision is that the consuming code much simpler.

One less-obvious side-effect of this decision is that deserialization will omit `null` values from collections, misrepresenting the collection size:

```json
{
    "items": [
        null,
        { "name": "alpha" },
        null,
        { "name": "beta" },
        null
    ]
}
```

The resulting C# object will have an `items` collection property with only two elements instead of five.

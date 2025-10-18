# GuidSupplement

Starting with .NET 9, Guid has added functionality compliant with UUID Version 7.
However, these features are not available in older .NET environments (e.g., .NET Standard 2.0), so GuidSupplement provides nearly equivalent functionality for these platforms.

## Table of Contents

- [Target Runtime](#target-runtime)
- [GuidVersion7](#guidversion7)
  - [Create](#create)
  - [IsVersion7](#isversion7)
  - [GetTimestamp](#gettimestamp)
  - [GetUnixTimeSeconds](#getunixtimeseconds)
  - [TimestampComparer](#timestampcomparer)
- [GuidExtensions](#guidextensions)
  - [Guid.TryWriteBytes](#guidtrywritebytes)
  - [Guid.ToByteArray](#guidtobytearray)
  - [Guid.GetVersion](#guidgetversion)
  - [Guid.GetVariant](#guidgetvariant)
- [License](#license)

## Target Runtime

.NET Standard 2.0, .NET Standard 2.1, .NET 8, .NET 9

## `GuidVersion7` class

### `Create` method

Creates a Guid compliant with UUID v7.
For target runtimes below .NET 9, it executes processing equivalent to the `Guid.CreateVersion7` method.
For .NET 9 and above, it executes the `Guid.CreateVersion7` method directly.

```csharp
var guid = GuidVersion7.Create();
var guid = GuidVersion7.Create(DateTimeOffset.UtcNow);
```

### `IsVersion7` method

Returns true if the Guid is compliant with UUID v7.
Returns false otherwise.

```csharp
var guid = GuidVersion7.Create();

if (GuidVersion7.IsVersion7(id))
{
    Console.WriteLine("Id is UUID version 7");
}
```

### `GetTimestamp` method

Retrieves the timestamp.
Throws an exception if the Guid is not compliant with UUID v7.

```csharp
var id = GuidVersion7.Create();
DateTimeOffset timestamp = GuidVersion7.GetTimestamp(id);
```

### `GetUnixTimeSeconds` method

Retrieves the timestamp as Unix epoch milliseconds.
Throws an exception if the Guid is not compliant with UUID v7.

```csharp
var id = GuidVersion7.Create();
long timestamp = GuidVersion7.GetUnixTimeSeconds(id);
```

### `TimestampComparer` property

```csharp
var ids = new List<Guid>();
ids.Add(GuidVersion7.Create(new DateTimeOffset(2025, 10, 17, 23, 49, 0, TimeSpan.Zero)));
ids.Add(GuidVersion7.Create(new DateTimeOffset(2025, 10, 17, 23, 49, 1, TimeSpan.Zero)));
ids.Add(GuidVersion7.Create(new DateTimeOffset(2025, 10, 17, 23, 49, 2, TimeSpan.Zero)));
ids.Add(GuidVersion7.Create(new DateTimeOffset(2025, 10, 17, 23, 49, 3, TimeSpan.Zero)));

var shuffled = ids.OrderBy(_ => Guid.NewGuid()).ToList();
shuffled.Sort(GuidVersion7.TimestampComparer);
```

## GuidExtensions

Some features added in .NET 8 and .NET 9 can be used in older .NET environments.

### `Guid.TryWriteBytes` method

```csharp
var id = new Guid("00112233-4455-6677-8899-aabbccddeeff");

Span<byte> value = stackalloc byte[16];
id.TryWriteBytes(value);
// [51, 34, 17, 0, 85, 68, 119, 102, 136, 153, 170, 187, 204, 221, 238, 255]
```

### `Guid.ToByteArray(bool bigEndian)` method

```csharp
var id = new Guid("00112233-4455-6677-8899-aabbccddeeff");

var bytes = id.ToByteArray(false);
// [51, 34, 17, 0, 85, 68, 119, 102, 136, 153, 170, 187, 204, 221, 238, 255]
```

### `Guid.GetVersion` method

Performs the same operation as the `Guid.Version` property.

```csharp
var id = Guid.NewGuid();
var version = id.GetVersion(); // 4

var id = GuidVersion7.Create();
var version = id.GetVersion(); // 7
```

### `Guid.GetVariant` method

Performs the same operation as the `Guid.Variant` property.

```csharp
var id = Guid.NewGuid();
var variant = id.GetVariant();

var id = GuidVersion7.Create();
var variant = id.GetVariant();
```

## License

MIT License. Some code is implemented based on [dotnet/runtime](https://github.com/dotnet/runtime), Please check the original license.
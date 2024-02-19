# PalWorld.Sav.Serializer

This project is a .NET library for serializing and deserializing Unreal Engine save files. It uses a Rust library, `ue_gvas_handler`, which is a wrapper around the `uesave` Rust crate.

## Structure

The project is structured as follows:

- `src/PalWorld.Sav.Serializer`: This is the .NET wrapper around the Rust library. It provides a `UeSave` class that you can use to serialize and deserialize data in the Unreal Engine save format.

- `src/UnrealSave-Serializer`: This is the Rust library that does the actual serialization and deserialization. It uses the `uesave` Rust crate to deserialize GVAS data.

- `examples/PalWorld.Sav.Examples.Deserializer`: This is an example project that shows how to use the `PalWorld.Sav.Serializer` library.

## Getting Started

### Prerequisites

- .NET 8.0
- Rust 1.57.0 or later

### Building

1. Build the `ue_gvas_handler` Rust library:

   Navigate to the `src/ue_gvas_handler` directory and run `cargo build --release`. This will create a DLL in the `target/release` directory.

2. Build the .NET project:

   Navigate to the `src/PalWorld.Sav.Serializer` directory and run `dotnet build`. This will automatically copy the `ue_gvas_handler.dll` to the output directory of the .NET project.

## Usage

In your C# code, you can use the `UeSave` class like this:

```csharp
string? result = await ueSave.DeserializeAsync(data, map);

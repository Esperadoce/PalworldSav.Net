# PalWorld.Sav.Serializer

This project is a .NET library for serializing and deserializing Unreal Engine save files. It uses a Rust library, `uesave`, which is a wrapper around the `uesave` Rust crate.

## Getting Started

### Prerequisites

- .NET 8.0
- Rust 1.57.0 or later

### Building

1. Build the `ue_gvas_handler` Rust library:

   Navigate to the `ue_gvas_handler` directory and run `cargo build --release`. This will create a DLL in the `target/release` directory.

2. Build the .NET project:

   Navigate to the `PalWorld.Sav.Serializer` directory and run `dotnet build`. This will automatically copy the `ue_gvas_handler.dll` to the output directory of the .NET project.

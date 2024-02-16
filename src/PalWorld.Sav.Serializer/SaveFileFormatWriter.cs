using System;

namespace PalWorld.Sav.Serializer;

// The SaveFileFormatWriter class represents a writer for saving data to a file in a specific format.
// It takes a file path and decompressed data as input parameters.
public class SaveFileFormatWriter(string filePath, Memory<byte> decompressedData)
{
    // The file path of the save file.
    public string FilePath { get; } = filePath;

    // The decompressed data to be written to the save file.
    public Memory<byte> DecompressedData { get; } = decompressedData;

}

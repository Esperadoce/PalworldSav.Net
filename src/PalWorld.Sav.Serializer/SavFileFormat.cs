using System;

namespace PalWorld.Sav.Serializer;

/// <summary>
/// Represents a .sav binary file format.
/// </summary>
public class SavFileFormat(
    string filePath,
    int lenDecompressed,
    int lenCompressed,
    int magic,
    Memory<byte> decompressedData)
{
    /// <summary>
    /// Gets the file path of the .sav file.
    /// </summary>
    public string FilePath { get; } = filePath;

    /// <summary>
    /// Gets the length of the decompressed data.
    /// </summary>
    public int LenDecompressed { get; } = lenDecompressed;

    /// <summary>
    /// Gets the length of the compressed data.
    /// </summary>
    public int LenCompressed { get; } = lenCompressed;

    /// <summary>
    /// Gets the magic number.
    /// </summary>
    public int Magic { get; } = magic;

    /// <summary>
    /// Gets the decompressed data.
    /// </summary>
    public Memory<byte> DecompressedData { get; } = decompressedData;
    
    
    public override string ToString()
    {
        return $"FilePath : {FilePath}" + Environment.NewLine +
               $"LenDecompressed : {LenDecompressed}" + Environment.NewLine +
               $"LenCompressed : {LenCompressed}," + Environment.NewLine +
               $"Magic : {Magic}," + Environment.NewLine +
               $"DecompressedData : {DecompressedData}," + Environment.NewLine;
    }
}
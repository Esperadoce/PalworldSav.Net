using System;

namespace PalWorld.Sav.Serializer;

/// <summary>
/// Represents a .sav binary file format.
/// </summary>
public class SavFileFormatReader
{
    public SavFileFormatReader(string filePath, int lenDecompressed, int lenCompressed, int magic,
        Memory<byte> decompressedData)
    {
        FilePath = filePath;
        LenDecompressed = lenDecompressed;
        LenCompressed = lenCompressed;
        Magic = magic;
        DecompressedData = decompressedData;
    }

    /// <summary>
    /// Gets the file path of the .sav file.
    /// </summary>
    public string FilePath { get; }

    /// <summary>
    /// Gets the length of the decompressed data.
    /// </summary>
    public int LenDecompressed { get; }

    /// <summary>
    /// Gets the length of the compressed data.
    /// </summary>
    public int LenCompressed { get; }

    /// <summary>
    /// Gets the magic number.
    /// </summary>
    public int Magic { get; }

    /// <summary>
    /// Gets the decompressed data.
    /// </summary>
    public Memory<byte> DecompressedData { get; }
    
    
    public override string ToString()
    {
        return $"FilePath : {FilePath}" + Environment.NewLine +
               $"LenDecompressed : {LenDecompressed}" + Environment.NewLine +
               $"LenCompressed : {LenCompressed}," + Environment.NewLine +
               $"Magic : {Magic}," + Environment.NewLine +
               $"DecompressedData : {DecompressedData}," + Environment.NewLine;
    }
}
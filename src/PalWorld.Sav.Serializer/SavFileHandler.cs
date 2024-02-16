using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace PalWorld.Sav.Serializer;

/// <summary>
/// Handles .sav files.
/// </summary>
public static class SavFileHandler
{
    /// <summary>
    /// Analyses the .sav file.
    /// </summary>
    /// <param name="filepath">The path to the .sav file.</param>
    /// <returns>A SavFileFormat object representing the .sav file.</returns>
    public static async Task<SavFileFormat> AnalyseSavAsync(string filepath)
    {
        if (string.IsNullOrEmpty(filepath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filepath));
        }

        await using var stream = File.OpenRead(filepath);

        stream.Seek(0, SeekOrigin.Begin);

        var lenDecompressed = await ReadIntAsync(stream);
        var lenCompressed = await ReadIntAsync(stream);
        var magic = await ReadIntAsync(stream);

        byte[] decompressedData;

        if (stream.ReadByte() != 0x78 || stream.ReadByte() != 0x9C) //zlib header
            throw new Exception("Incorrect zlib header");

        switch (magic >> 24)
        {
            case 0x32:
            case 0x31:
                await using (var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
                {
                    using var result = new MemoryStream();
                    await deflateStream.CopyToAsync(result);
                    decompressedData = result.ToArray();
                }

                break;
            default:
                throw new Exception("Unsupported magic number.");
        }

        return new SavFileFormat(filepath, lenDecompressed, lenCompressed, magic, decompressedData);
    }

    private static async Task<int> ReadIntAsync(Stream stream)
    {
        var buffer = new byte[sizeof(int)];
        await stream.ReadExactlyAsync(buffer, 0, sizeof(int));
        return BitConverter.ToInt32(buffer, 0);
    }
}
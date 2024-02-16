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
    public static async Task<SavFileFormatReader> AnalyseSavAsync(string filepath)
    {
        if (string.IsNullOrEmpty(filepath))
        {
            throw new ArgumentException("File path cannot be null or empty.", nameof(filepath));
        }

        await using var stream = File.OpenRead(filepath);
        var length = new FileInfo(filepath).Length;

        stream.Seek(0, SeekOrigin.Begin);

        var lenDecompressed = await ReadIntAsync(stream);
        var lenCompressed = await ReadIntAsync(stream);
        var magic = await ReadIntAsync(stream);

        byte[] decompressedData;
        byte zlibHeader = (byte)stream.ReadByte();
        if (zlibHeader != 0x78 && zlibHeader != 0x9C) //zlib header
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

        return new SavFileFormatReader(filepath, lenDecompressed, lenCompressed, magic, decompressedData);
    }

    /// <summary>
    /// Writes the provided SaveFileFormatWriter data to a file asynchronously.
    /// </summary>
    /// <param name="savFileFormat">The SaveFileFormatWriter object containing the data to write.</param>
    /// <param name="overwrite">Whether to overwrite the file if it already exists. Defaults to false.</param>
    public static async Task WriteSavAsync(SaveFileFormatWriter savFileFormat, bool overwrite = false)
    {
        // Check if the file already exists and if we should not overwrite it
        if (File.Exists(savFileFormat.FilePath) && !overwrite)
        {
            throw new Exception("File already exists.");
        }

        // Create a file stream with a buffer size of 4MB
        const int bufferSize = 4194304;
        await using var fileStream = new FileStream(savFileFormat.FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, bufferSize, FileOptions.Asynchronous);

        // Write the decompressed data to a temporary memory stream
        await using var tmpMemoryStream = new MemoryStream(savFileFormat.DecompressedData.ToArray());
        // Compress the data using GZip
        await using var gzipStream = new GZipStream(tmpMemoryStream, CompressionMode.Compress, true);

        // Write the compressed data to a new memory stream
        await using var compressedMemoryStream = new MemoryStream();
        await gzipStream.CopyToAsync(compressedMemoryStream);

        // Get the lengths of the decompressed and compressed data
        var lenDecompressed = savFileFormat.DecompressedData.Length;
        var lenCompressed = (int)compressedMemoryStream.Length;

        // Define some constants
        int magic = 0x31323334;
        byte zlibHeader = 0x78;

        // Write the lengths and constants to the file stream
        await WriteIntAsync(fileStream, lenDecompressed);
        await WriteIntAsync(fileStream, lenCompressed);
        await WriteIntAsync(fileStream, magic);
        await fileStream.WriteAsync(new byte[] { zlibHeader }, 0, 1);

        // Write the compressed data to the file stream
        await compressedMemoryStream.CopyToAsync(fileStream);
    }

    private static async Task<int> ReadIntAsync(Stream stream)
    {
        var buffer = new byte[sizeof(int)];
        await stream.ReadExactlyAsync(buffer, 0, sizeof(int));
        return BitConverter.ToInt32(buffer, 0);
    }

    private static async Task WriteIntAsync(Stream stream, int value)
    {

        var buffer = BitConverter.GetBytes(value);
        await stream.WriteAsync(buffer, 0, sizeof(int));
    }
}
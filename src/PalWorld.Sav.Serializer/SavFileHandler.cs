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
    private const int BufferSize = 4194304;
    private const int ZlibHeader1 = 0x78;
    private const int ZlibHeader2 = 0x9C;
    private const int MagicNumber1 = 0x32;
    private const int MagicNumber2 = 0x31;
    private const int MagicConstant = 0x31323334;

    /// <summary>
    /// Analyses the .sav file.
    /// </summary>
    /// <param name="filepath">The path to the .sav file.</param>
    /// <returns>A SavFileFormat object representing the .sav file.</returns>
    /// <exception cref="ArgumentException">Thrown when the provided filepath is null or empty.</exception>
    /// <exception cref="InvalidDataException">Thrown when the zlib header in the file is incorrect.</exception>
    /// <exception cref="NotSupportedException">Thrown when the magic number in the file is unsupported.</exception>
    public static async Task<SavFileFormatReader> AnalyseSavAsync(string filepath)
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
        if ((byte)stream.ReadByte() != ZlibHeader1 || (byte)stream.ReadByte() != ZlibHeader2) //zlib header
            throw new InvalidDataException("Incorrect zlib header");

        switch (magic >> 24)
        {
            case MagicNumber1:
            case MagicNumber2:
                await using (var deflateStream = new DeflateStream(stream, CompressionMode.Decompress, true))
                {
                    using var result = new MemoryStream();
                    await deflateStream.CopyToAsync(result);
                    decompressedData = result.ToArray();
                }

                break;
            default:
                throw new NotSupportedException("Unsupported magic number.");
        }

        return new SavFileFormatReader(filepath, lenDecompressed, lenCompressed, magic, decompressedData);
    }

    /// <summary>
    /// Writes the provided SaveFileFormatWriter data to a file asynchronously.
    /// </summary>
    /// <param name="savFileFormat">The SaveFileFormatWriter object containing the data to write.</param>
    /// <param name="overwrite">Whether to overwrite the file if it already exists. Defaults to false.</param>
    /// <exception cref="IOException">Thrown when the file already exists and overwrite is set to false.</exception>
    public static async Task WriteSavAsync(SaveFileFormatWriter savFileFormat, bool overwrite = false)
    {
        // Check if the file already exists and if we should not overwrite it
        if (File.Exists(savFileFormat.FilePath) && !overwrite)
        {
            throw new IOException($"The file at path {savFileFormat.FilePath} already exists and overwrite is set to false.");
        }

        // Create a file stream with a buffer size of 4MB
        await using var fileStream = new FileStream(savFileFormat.FilePath, FileMode.Create, FileAccess.ReadWrite, FileShare.None, BufferSize, FileOptions.Asynchronous);

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

        // Write the lengths and constants to the file stream
        await WriteIntAsync(fileStream, lenDecompressed);
        await WriteIntAsync(fileStream, lenCompressed);
        await WriteIntAsync(fileStream, MagicConstant);
        await fileStream.WriteAsync([ZlibHeader1], 0, 1);

        // Write the compressed data to the file stream
        await compressedMemoryStream.CopyToAsync(fileStream);
    }

    /// <summary>
    /// Reads an integer from the provided stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <returns>The integer read from the stream.</returns>
    private static async Task<int> ReadIntAsync(Stream stream)
    {
        var buffer = new byte[sizeof(int)];
        await stream.ReadExactlyAsync(buffer, 0, sizeof(int));
        return BitConverter.ToInt32(buffer, 0);
    }

    /// <summary>
    /// Writes an integer to the provided stream asynchronously.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="value">The integer value to write.</param>
    private static async Task WriteIntAsync(Stream stream, int value)
    {

        var buffer = BitConverter.GetBytes(value);
        await stream.WriteAsync(buffer, 0, sizeof(int));
    }
}
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

public static class JsonCompressor
{
    public static string Compress(string json)
    {
        var buffer = Encoding.UTF8.GetBytes(json);
        using var memoryStream = new MemoryStream();
        using (var brotliStream = new BrotliStream(memoryStream, CompressionLevel.Fastest)) brotliStream.Write(buffer, 0, buffer.Length);
        return Convert.ToBase64String(memoryStream.ToArray());
    }

    public static string Decompress(string compressedBase64)
    {
        var buffer = Convert.FromBase64String(compressedBase64);
        using var memoryStream = new MemoryStream(buffer);
        using var brotliStream = new BrotliStream(memoryStream, CompressionMode.Decompress);
        using var streamReader = new StreamReader(brotliStream, Encoding.UTF8);
        return streamReader.ReadToEnd();
    }
}
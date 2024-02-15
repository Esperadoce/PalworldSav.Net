using System.Runtime.InteropServices;

namespace PalWorldSavSerializer;

/// <summary>
/// Provides methods for serializing and deserializing data in the Unreal Engine save format.
/// </summary>
public class UeSave
{
    
    /// <summary>
    /// Deserializes the provided data using the provided map.
    /// </summary>
    /// <param name="data">The data to deserialize.</param>
    /// <param name="map">The map to use for deserialization.</param>
    /// <returns>The deserialized data as a string.</returns>
    public async Task<string?> DeserializeAsync(byte[] data, Dictionary<string, string> map)
    {
        if (data == null || data.Length == 0)
        {
            throw new ArgumentException("Data cannot be null or empty.", nameof(data));
        }
        if (map == null || map.Count == 0)
        {
            throw new ArgumentException("Map cannot be null or empty.", nameof(map));
        }

        GCHandle handle = GCHandle.Alloc(data, GCHandleType.Pinned);
        IntPtr pointer = handle.AddrOfPinnedObject();

        // Call the deserialize function on a separate thread
        IntPtr result = await Task.Run(() => 
            InternalBridge.deserialize(pointer, (UIntPtr)data.Length, map.Select(kv => new KeyValuePair { Key = kv.Key, Value = kv.Value }).ToArray(), map.Count));

        string? resultString = Marshal.PtrToStringAnsi(result);

        InternalBridge.free_rust_string(result);
        handle.Free();

        return resultString;
    }

    /// <summary>
    /// Serializes the provided JSON string to a Sav data.
    /// </summary>
    /// <param name="json">The JSON string to serialize.</param>
    /// <returns>The serialized data as a Sav data byte array.</returns>
    public async Task<Memory<byte>> SerializeAsync(string json)
    {
        if (string.IsNullOrEmpty(json))
        {
            throw new ArgumentException("JSON string cannot be null or empty.", nameof(json));
        }

        // Call the serialize function on a separate thread
        var result = await Task.Run(() =>
        {
            UIntPtr size;
            IntPtr ptr = InternalBridge.serialize(json, out size);
            return (ptr, size);
        });

        // Convert the result to a byte array
        byte[] serializedData = new byte[result.size.ToUInt32()];
        Marshal.Copy(result.ptr, serializedData, 0, serializedData.Length);
        InternalBridge.free_rust_vec(result.ptr);

        return serializedData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyValuePair
    {
        [MarshalAs(UnmanagedType.LPStr)]
        public string Key;

        [MarshalAs(UnmanagedType.LPStr)]
        public string Value;
    }

    private static class InternalBridge
    {
        [DllImport("bridge_ffi_ue")]
        public static extern IntPtr deserialize(IntPtr buffer, UIntPtr size, KeyValuePair[] map, int mapLength);

        [DllImport("bridge_ffi_ue", CallingConvention = CallingConvention.Cdecl)]
        public static extern IntPtr serialize(string data, out UIntPtr size);

        [DllImport("bridge_ffi_ue")]
        public static extern void free_rust_string(IntPtr s);

        [DllImport("bridge_ffi_ue")]
        public static extern void free_rust_vec(IntPtr p);
    }
}
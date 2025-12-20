using System;
using System.IO;
using UnityEngine;

public static class ProtobufHelper
{
    public static byte[] Serialize<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
            ProtoBuf.Serializer.Serialize(stream, obj);
            return stream.ToArray();
        }
    }

    public static T Deserialize<T>(byte[] data)
    {
        using (var stream = new MemoryStream(data))
        {
            return ProtoBuf.Serializer.Deserialize<T>(stream);
        }
    }

    public static byte[] Encode<T>(UInt16 cmd, UInt16 param, T data) where T : global::ProtoBuf.IExtensible
    {
        byte[] dataByte = Serialize(data);

        int bodyLength = 2 + 2 + dataByte.Length;
        int totalLength = 4 + bodyLength;
        byte[] buffer = new byte[totalLength];
        int offset = 0;

        WriteInt32(buffer, ref offset, bodyLength);
        WriteUInt16(buffer, ref offset, cmd);
        WriteUInt16(buffer, ref offset, param);

        if (dataByte.Length > 0)
            Buffer.BlockCopy(dataByte, 0, buffer, offset, dataByte.Length);

        return buffer;
    }

    public static (UInt16 cmd, UInt16 param) DecodeHeader(byte[] data)
    {
        if (data.Length < 8)
            Debug.LogError("消息缺失长度/cmd/param信息");

        int offset = 0;

        int totalLength = ReadInt32(data, ref offset);

        if (totalLength + 4 != data.Length)
            Debug.LogError($"长度不匹配: 头部声明{totalLength}, 实际{data.Length - 4}");

        ushort cmd = ReadUInt16(data, ref offset);
        ushort param = ReadUInt16(data, ref offset);

        return (cmd, param);
    }

    public static T DecodeData<T>(byte[] data) where T : global::ProtoBuf.IExtensible
    {
        int offset = 0;
        int totalLength = ReadInt32(data, ref offset);
        ushort cmd = ReadUInt16(data, ref offset);
        ushort param = ReadUInt16(data, ref offset);

        int dataByteLength = data.Length - offset;
        byte[] dataBuffer = new byte[dataByteLength];
        Buffer.BlockCopy(data, offset, dataBuffer, 0, dataByteLength);

        T result = Deserialize<T>(dataBuffer);
        return result;
    }

    private static void WriteInt32(byte[] buffer, ref int offset, int value)
    {
        buffer[offset++] = (byte)(value >> 24);
        buffer[offset++] = (byte)(value >> 16);
        buffer[offset++] = (byte)(value >> 8);
        buffer[offset++] = (byte)value;
    }

    private static void WriteUInt16(byte[] buffer, ref int offset, ushort value)
    {
        buffer[offset++] = (byte)(value >> 8);
        buffer[offset++] = (byte)value;
    }

    private static int ReadInt32(byte[] data, ref int offset)
    {
        int value = (data[offset] << 24) | (data[offset + 1] << 16) | (data[offset + 2] << 8) | data[offset + 3];
        offset += 4;
        return value;
    }

    private static ushort ReadUInt16(byte[] data, ref int offset)
    {
        ushort value = (ushort)((data[offset] << 8) | data[offset + 1]);
        offset += 2;
        return value;
    }
}

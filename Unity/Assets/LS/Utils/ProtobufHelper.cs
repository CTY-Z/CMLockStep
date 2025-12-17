using System.IO;

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

    public static T DeserializeUnity<T>(byte[] data)
    {
        using (var stream = new MemoryStream(data))
        {
            return ProtoBuf.Serializer.Deserialize<T>(stream);
        }
    }
}

using System.Buffers;
using MessagePack;
using MessagePack.Formatters;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    /// <summary>
    /// For Rust Vec&lt;u8&gt; fields without serde_bytes (e.g. AppAuthenticationToken): rmp-serde
    /// encodes them as an array of integers, not msgpack bin. Writes an int array; reads either form.
    /// </summary>
    public sealed class ByteArrayAsIntArrayFormatter : IMessagePackFormatter<byte[]>
    {
        public void Serialize(ref MessagePackWriter writer, byte[] value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteArrayHeader(value.Length);
            foreach (byte b in value)
                writer.Write(b);
        }

        public byte[] Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
                return null;

            if (reader.NextMessagePackType == MessagePackType.Binary)
                return reader.ReadBytes()?.ToArray();

            int count = reader.ReadArrayHeader();
            var result = new byte[count];
            for (int i = 0; i < count; i++)
                result[i] = reader.ReadByte();

            return result;
        }
    }
}

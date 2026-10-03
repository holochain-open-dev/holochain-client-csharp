using System;
using System.Buffers;
using MessagePack;
using MessagePack.Formatters;
using NextGenSoftware.Holochain.HoloNET.Client.Data.App.Responses.Objects;

namespace NextGenSoftware.Holochain.HoloNET.Client.Data.Admin.Requests.Objects
{
    /// <summary>
    /// Decodes holochain_conductor_api::CellInfo, which is adjacently tagged:
    /// #[serde(tag = "type", content = "value", rename_all = "snake_case")]
    /// Wire: {"type": "provisioned" | "cloned" | "stem", "value": {...}}
    /// A formatter is needed because the type of "value" depends on "type".
    /// </summary>
    public sealed class CellInfoFormatter : IMessagePackFormatter<CellInfo>
    {
        public void Serialize(ref MessagePackWriter writer, CellInfo value, MessagePackSerializerOptions options)
        {
            if (value == null)
            {
                writer.WriteNil();
                return;
            }

            writer.WriteMapHeader(2);
            writer.Write("type");

            switch (value.CellInfoType)
            {
                case CellInfoType.Provisioned:
                    writer.Write("provisioned");
                    writer.Write("value");
                    MessagePackSerializer.Serialize(ref writer, value.Provisioned, options);
                    break;
                case CellInfoType.Cloned:
                    writer.Write("cloned");
                    writer.Write("value");
                    MessagePackSerializer.Serialize(ref writer, value.Cloned, options);
                    break;
                case CellInfoType.Stem:
                    writer.Write("stem");
                    writer.Write("value");
                    MessagePackSerializer.Serialize(ref writer, value.Stem, options);
                    break;
                default:
                    throw new InvalidOperationException("CellInfo has no variant set.");
            }
        }

        public CellInfo Deserialize(ref MessagePackReader reader, MessagePackSerializerOptions options)
        {
            if (reader.TryReadNil())
                return null;

            string type = null;
            ReadOnlySequence<byte>? rawValue = null;
            int count = reader.ReadMapHeader();

            for (int i = 0; i < count; i++)
            {
                switch (reader.ReadString())
                {
                    case "type": type = reader.ReadString(); break;
                    // Captured raw because serde does not guarantee "type" precedes "value".
                    case "value": rawValue = reader.ReadRaw(); break;
                    default: reader.Skip(); break;
                }
            }

            var result = new CellInfo();
            if (rawValue == null)
                return result;

            switch (type)
            {
                case "provisioned": result.Provisioned = MessagePackSerializer.Deserialize<ProvisionedCell>(rawValue.Value, options); break;
                case "cloned": result.Cloned = MessagePackSerializer.Deserialize<ClonedCell>(rawValue.Value, options); break;
                case "stem": result.Stem = MessagePackSerializer.Deserialize<StemCell>(rawValue.Value, options); break;
            }

            return result;
        }
    }
}

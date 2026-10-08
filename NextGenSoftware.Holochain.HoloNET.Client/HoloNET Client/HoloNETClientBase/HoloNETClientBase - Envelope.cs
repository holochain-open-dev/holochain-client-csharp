using MessagePack;

namespace NextGenSoftware.Holochain.HoloNET.Client
{
    public abstract partial class HoloNETClientBase
    {
        /// <summary>
        /// AdminResponse/AppResponse are {"type": ..., "value": payload}. Returns the payload as T,
        /// or default(T) when there is no "value" (unit variants such as app_disabled).
        /// Decoders must use this rather than deserialising the whole envelope as T, which
        /// silently yields an empty object.
        /// </summary>
        protected T DeserializeResponseValue<T>(byte[] envelope)
        {
            return DeserializeResponseValue<T>(envelope, messagePackSerializerOptions);
        }

        public static T DeserializeResponseValue<T>(byte[] envelope, MessagePackSerializerOptions options)
        {
            var reader = new MessagePackReader(envelope);
            int count = reader.ReadMapHeader();

            for (int i = 0; i < count; i++)
            {
                if (reader.ReadString() == "value")
                    return MessagePackSerializer.Deserialize<T>(reader.ReadRaw(), options);

                reader.Skip();
            }

            return default;
        }

        /// <summary>
        /// Reads only the envelope's "type". Dispatch must not deserialise the whole payload as
        /// dynamic: payloads keyed by hashes (e.g. network_metrics_dumped's HashMap&lt;DnaHash, _&gt;)
        /// throw under MessagePackSecurity.UntrustedData, and the response was then never routed.
        /// </summary>
        public static string ReadResponseType(byte[] envelope)
        {
            var reader = new MessagePackReader(envelope);
            int count = reader.ReadMapHeader();

            for (int i = 0; i < count; i++)
            {
                if (reader.ReadString() == "type")
                    return reader.ReadString();

                reader.Skip();
            }

            return null;
        }

        /// <summary>Returns the envelope's "value" as JSON (null when absent), for payloads exposed as JSON strings.</summary>
        protected string DeserializeResponseValueAsJson(byte[] envelope)
        {
            var reader = new MessagePackReader(envelope);
            int count = reader.ReadMapHeader();

            for (int i = 0; i < count; i++)
            {
                if (reader.ReadString() == "value")
                    return MessagePackSerializer.ConvertToJson(reader.ReadRaw());

                reader.Skip();
            }

            return null;
        }
    }
}

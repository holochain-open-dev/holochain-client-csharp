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

        /// <summary>
        /// peer_meta_info's value is BTreeMap&lt;DnaHash, BTreeMap&lt;String, PeerMetaInfo { meta_value, expires_at }&gt;&gt;: a map keyed
        /// by the DNA hash bytes. It used to be read as an object with a "dna_hash_peer_meta" field, which never exists, so the
        /// result was always empty. Keys become HoloHash strings.
        /// </summary>
        protected PeerMetaInfoResponse DecodePeerMetaInfo(byte[] envelope)
        {
            byte[] raw = ReadRawValue(envelope);
            if (raw == null)
                return null;

            var result = new PeerMetaInfoResponse { dna_hash_peer_meta = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, dynamic>>() };
            var reader = new MessagePackReader(raw);
            int dnas = reader.ReadMapHeader();

            for (int i = 0; i < dnas; i++)
            {
                byte[] dnaHash = System.Buffers.BuffersExtensions.ToArray(reader.ReadBytes().Value);
                var metas = new System.Collections.Generic.Dictionary<string, dynamic>();
                int count = reader.ReadMapHeader();

                for (int j = 0; j < count; j++)
                {
                    string key = reader.ReadString();
                    metas[key] = MessagePackSerializer.Deserialize<System.Collections.Generic.Dictionary<string, object>>(reader.ReadRaw(), messagePackSerializerOptions);
                }

                result.dna_hash_peer_meta[ConvertHoloHashToString(dnaHash)] = metas;
            }

            return result;
        }

        /// <summary>Returns the raw msgpack of the envelope's "value" (null when absent).</summary>
        public static byte[] ReadRawValue(byte[] envelope)
        {
            var reader = new MessagePackReader(envelope);
            int count = reader.ReadMapHeader();

            for (int i = 0; i < count; i++)
            {
                if (reader.ReadString() == "value")
                    return System.Buffers.BuffersExtensions.ToArray(reader.ReadRaw());

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

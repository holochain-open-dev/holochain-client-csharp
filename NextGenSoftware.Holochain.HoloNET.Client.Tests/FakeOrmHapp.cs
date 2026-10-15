using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MessagePack;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// An in-memory stand-in for the OASIS hApp's avatar and avatar-collection zome functions, served through
    /// <see cref="FakeConductor"/>, so ORM paths (errors, sync wrappers, observable collections) run without a conductor.
    /// It answers with real Holochain 0.7 Record shapes and the same argument shapes the real zome functions take.
    /// </summary>
    internal sealed class FakeOrmHapp
    {
        private sealed class Stored
        {
            public byte Id;
            public Dictionary<string, object> Fields;
            public byte? Original;
            public bool Deleted;
        }

        private readonly Dictionary<byte, Stored> _records = new Dictionary<byte, Stored>();
        private readonly Dictionary<string, List<byte>> _collections = new Dictionary<string, List<byte>>();
        private byte _next = 10;

        /// <summary>Zome functions that should answer with a conductor error (as a failing zome would).</summary>
        public HashSet<string> Failing { get; } = new HashSet<string>();

        /// <summary>Every (function, decoded argument) the client sent, in order.</summary>
        public ConcurrentQueue<(string Fn, object Args)> Calls { get; } = new ConcurrentQueue<(string, object)>();

        public void Attach(FakeConductor fake) => fake.Respond = Respond;

        private object Respond(string type, byte[] value)
        {
            if (type != "call_zome")
                return FakeConductor.Ok("ok", null);

            var signed = MessagePackSerializer.Deserialize<Dictionary<string, object>>(value, FakeConductor.Options);
            var call = MessagePackSerializer.Deserialize<Dictionary<string, object>>((byte[])signed["bytes"], FakeConductor.Options);
            string fn = (string)call["fn_name"];
            object args = MessagePackSerializer.Deserialize<object>((byte[])call["payload"], FakeConductor.Options);
            Calls.Enqueue((fn, args));

            if (Failing.Contains(fn))
                return FakeConductor.Error("ribosome_error", $"fake {fn} failed");

            try
            {
                lock (_records)
                    return FakeConductor.Ok("zome_called", MessagePackSerializer.Serialize(Handle(fn, args)));
            }
            catch (Exception ex)
            {
                return FakeConductor.Error("ribosome_error", ex.Message);
            }
        }

        public static byte Id(object hash) => ((byte[])hash)[3];

        private static Dictionary<string, object> Fields(object entry) =>
            ((Dictionary<object, object>)entry).ToDictionary(kv => (string)kv.Key, kv => kv.Value);

        private static Dictionary<object, object> Map(object args) => (Dictionary<object, object>)args;

        private Stored Create(Dictionary<string, object> fields, byte? original = null)
        {
            var stored = new Stored { Id = _next++, Fields = fields, Original = original };
            _records[stored.Id] = stored;
            return stored;
        }

        private Stored Get(object hash) => _records.TryGetValue(Id(hash), out var s) && !s.Deleted ? s : null;

        private Stored Latest(Stored s)
        {
            var newer = _records.Values.Where(r => r.Original == s.Id || (r.Original.HasValue && _records[r.Original.Value].Original == s.Id)).OrderBy(r => r.Id).LastOrDefault();
            return newer ?? s;
        }

        private object Handle(string fn, object args)
        {
            switch (fn)
            {
                case "create_entry_avatar":
                    return Record(Create(Fields(args)));

                case "get_entry_avatar":
                    {
                        var s = Get(args);
                        return s == null ? null : Record(s);
                    }

                case "get_avatar_by_username":
                    {
                        var s = _records.Values.Where(r => !r.Deleted && (string)r.Fields["username"] == (string)args).OrderBy(r => r.Id).LastOrDefault();
                        return s == null ? null : Record(s);
                    }

                case "update_entry_avatar":
                    {
                        var input = Map(args);
                        var previous = Get(input["original_action_hash"]) ?? throw new InvalidOperationException("record not found");
                        return Record(Create(Fields(input["updated_entry"]), previous.Id));
                    }

                case "delete_entry_avatar":
                    {
                        var s = Get(args) ?? throw new InvalidOperationException("record not found");
                        s.Deleted = true;
                        return Record(s);
                    }

                case "delete_avatar_by_username":
                    {
                        var s = _records.Values.Where(r => !r.Deleted && (string)r.Fields["username"] == (string)args).OrderBy(r => r.Id).LastOrDefault() ?? throw new InvalidOperationException("record not found");
                        s.Deleted = true;
                        return Record(s);
                    }

                case "get_avatar_collection":
                    return Collection((string)args).Select(id => Record(Latest(_records[id]))).ToList();

                case "add_avatar_to_collection":
                    {
                        var input = Map(args);
                        Collection((string)input["collection_anchor"]).Add(Id(input["entry_action_hash"]));
                        return null;
                    }

                case "remove_avatar_from_collection":
                    {
                        var input = Map(args);
                        Collection((string)input["collection_anchor"]).Remove(Id(input["entry_action_hash"]));
                        return null;
                    }

                case "batch_update_avatar_collection":
                    {
                        var input = Map(args);
                        var members = Collection((string)input["collection_anchor"]);
                        var saved = new List<object>();
                        foreach (var added in (object[])input["added"])
                        {
                            var s = Create(Fields(added));
                            members.Add(s.Id);
                            saved.Add(Record(s));
                        }
                        foreach (var removed in (object[])input["removed"])
                            members.Remove(Id(removed));
                        foreach (var updated in (object[])input["updated"])
                        {
                            var u = Map(updated);
                            var previous = Get(u["original_action_hash"]) ?? throw new InvalidOperationException("record not found");
                            saved.Add(Record(Create(Fields(u["updated_entry"]), previous.Id)));
                        }
                        return saved;
                    }

                default:
                    throw new InvalidOperationException($"no zome function {fn}");
            }
        }

        private List<byte> Collection(string anchor)
        {
            anchor = string.IsNullOrWhiteSpace(anchor) ? "default" : anchor;
            if (!_collections.TryGetValue(anchor, out var members))
                _collections[anchor] = members = new List<byte>();
            return members;
        }

        /// <summary>A Holochain 0.7 Record (Create, or Update when it replaces an earlier version) for a stored entry.</summary>
        private static Dictionary<string, object> Record(Stored s)
        {
            var content = new Dictionary<string, object>
            {
                ["type"] = s.Original.HasValue ? "Update" : "Create",
                ["author"] = Agent(),
                ["timestamp"] = 1792029232652253L + s.Id,
                ["action_seq"] = (int)s.Id,
                ["prev_action"] = Action(9),
                ["entry_type"] = new Dictionary<string, object> { ["App"] = new Dictionary<string, object> { ["entry_index"] = 0, ["zome_index"] = 0, ["visibility"] = "Public" } },
                ["entry_hash"] = Hash(0x21, s.Id),
                ["weight"] = new Dictionary<string, object> { ["bucket_id"] = 0, ["units"] = 0, ["rate_bytes"] = 0 }
            };

            if (s.Original.HasValue)
            {
                content["original_action_address"] = Action(s.Original.Value);
                content["original_entry_address"] = Hash(0x21, s.Original.Value);
            }

            return new Dictionary<string, object>
            {
                ["signed_action"] = new Dictionary<string, object>
                {
                    ["hashed"] = new Dictionary<string, object> { ["content"] = content, ["hash"] = Action(s.Id) },
                    ["signature"] = new byte[64]
                },
                ["entry"] = new Dictionary<string, object> { ["Present"] = new Dictionary<string, object> { ["entry_type"] = "App", ["entry"] = MessagePackSerializer.Serialize(s.Fields) } }
            };
        }
    }
}

using System;
using System.IO;
using System.Linq;
using NextGenSoftware.Holochain.HoloNET.ORM.Generation;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>
    /// HolochainHappGenerator (what WEB5 STAR generates hApps with). These check the generated files; the generated hApp is
    /// also compiled with cargo, packed with hc and driven by HoloNET ORM on real 0.7.0 / 0.7.1 conductors
    /// (scripts/build-generated-happ.sh + LiveGeneratedHappTests).
    /// </summary>
    public class HappGeneratorTests
    {
        /// <summary>The sample game hApp the live tests build and run against.</summary>
        public static HappSpec SampleSpec(HappHolochainVersion version = HappHolochainVersion.V0_7_1) => new HappSpec
        {
            Name = "holonet_game",
            CSharpNamespace = "HoloNETGame",
            HolochainVersion = version,
            Zomes =
            {
                new HappZomeSpec("Game",
                    new HappEntrySpec("Player",
                        new HappFieldSpec("Name", "string"), new HappFieldSpec("Level", "int"), new HappFieldSpec("Score", "long"),
                        new HappFieldSpec("IsOnline", "bool"), new HappFieldSpec("Rating", "double"), new HappFieldSpec("JoinedOn", "DateTime"),
                        new HappFieldSpec("Id", "Guid")),
                    new HappEntrySpec("GameItem", new HappFieldSpec("Title", "string"), new HappFieldSpec("Type", "string")))
            }
        };

        private static string TempDir() => Path.Combine(Path.GetTempPath(), "happgen-" + Guid.NewGuid().ToString("N").Substring(0, 8));

        [Fact]
        public void Generate_WritesACompleteWorkspace()
        {
            string dir = TempDir();
            try
            {
                var files = HolochainHappGenerator.Generate(SampleSpec(), dir).Select(f => Path.GetRelativePath(dir, f).Replace('\\', '/')).ToList();

                Assert.Contains("Cargo.toml", files);
                Assert.Contains("dnas/holonet_game/zomes/integrity/game_integrity/src/lib.rs", files);
                Assert.Contains("dnas/holonet_game/zomes/coordinator/game/src/lib.rs", files);
                Assert.Contains("dnas/holonet_game/workdir/dna.yaml", files);
                Assert.Contains("workdir/happ.yaml", files);
                Assert.Contains("CSharp/Player.cs", files);
                Assert.Contains("CSharp/GameItem.cs", files);

                string cargo = File.ReadAllText(Path.Combine(dir, "Cargo.toml"));
                Assert.Contains("hdk = \"=0.7.1\"", cargo);
                Assert.Contains("hdi = \"=0.8.1\"", cargo);

                string integrity = File.ReadAllText(Path.Combine(dir, "dnas/holonet_game/zomes/integrity/game_integrity/src/lib.rs"));
                Assert.Contains("pub struct Player {", integrity);
                Assert.Contains("pub level: i32,", integrity);
                Assert.Contains("pub score: i64,", integrity);
                Assert.Contains("pub is_online: bool,", integrity);
                Assert.Contains("pub rating: f64,", integrity);
                Assert.Contains("pub joined_on: String,", integrity);
                Assert.Contains("pub r#type: String,", integrity); // a Rust keyword, escaped
                Assert.Contains("Player(Player),", integrity);
                Assert.Contains("AllPlayers,", integrity);

                string coordinator = File.ReadAllText(Path.Combine(dir, "dnas/holonet_game/zomes/coordinator/game/src/lib.rs"));
                foreach (string fn in new[] { "create_player", "get_player", "update_player", "delete_player", "get_all_players", "get_player_collection", "add_player_to_collection", "remove_player_from_collection", "batch_update_player_collection", "create_game_item" })
                    Assert.Contains($"pub fn {fn}(", coordinator);
                Assert.Contains("pub struct UpdatePlayerInput { pub original_action_hash: ActionHash, pub updated_entry: Player }", coordinator);

                string model = File.ReadAllText(Path.Combine(dir, "CSharp/Player.cs"));
                Assert.Contains("namespace HoloNETGame", model);
                Assert.Contains("public class Player : HoloNETEntryBase", model);
                Assert.Contains("[HolochainRustFieldName(\"is_online\")]", model);
                Assert.Contains("public bool IsOnline { get; set; }", model);
                Assert.Contains("public string JoinedOn { get; set; }", model); // DateTime kept as a string
                Assert.Contains("\"get_player_collection\"", model);

                Assert.Contains("name: game_integrity", File.ReadAllText(Path.Combine(dir, "dnas/holonet_game/workdir/dna.yaml")));
                Assert.Contains("path: \"../dnas/holonet_game/workdir/holonet_game.dna\"", File.ReadAllText(Path.Combine(dir, "workdir/happ.yaml")));
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        [Fact]
        public void Generate_070_UsesThe070Crates()
        {
            string dir = TempDir();
            try
            {
                HolochainHappGenerator.Generate(SampleSpec(HappHolochainVersion.V0_7_0), dir);
                string cargo = File.ReadAllText(Path.Combine(dir, "Cargo.toml"));
                Assert.Contains("hdk = \"=0.7.0\"", cargo);
                Assert.Contains("hdi = \"=0.8.0\"", cargo);
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        [Fact]
        public void Generate_RejectsBadSpecs()
        {
            Assert.Throws<ArgumentNullException>(() => HolochainHappGenerator.Generate(null, TempDir()));
            Assert.Throws<ArgumentException>(() => HolochainHappGenerator.Generate(new HappSpec { Name = "" }, TempDir()));
            Assert.Throws<ArgumentException>(() => HolochainHappGenerator.Generate(new HappSpec { Name = "x" }, TempDir()));
            Assert.Throws<ArgumentException>(() => HolochainHappGenerator.Generate(new HappSpec { Name = "x", Zomes = { new HappZomeSpec("", new HappEntrySpec("A")) } }, TempDir()));
            Assert.Throws<ArgumentException>(() => HolochainHappGenerator.Generate(new HappSpec { Name = "x", Zomes = { new HappZomeSpec("z", new HappEntrySpec("")) } }, TempDir()));
            Assert.Throws<ArgumentException>(() => HolochainHappGenerator.Generate(new HappSpec { Name = "x", Zomes = { new HappZomeSpec("z", new HappEntrySpec("A", new HappFieldSpec("F", "decimal"))) } }, TempDir()));
            Assert.Throws<ArgumentException>(() => HolochainHappGenerator.Generate(new HappSpec { Name = "x", Zomes = { new HappZomeSpec("z", new HappEntrySpec("A", new HappFieldSpec("Name", "string"), new HappFieldSpec("name", "string"))) } }, TempDir()));
        }

        [Fact]
        public void FromStarDnaFolder_ReadsZomesHolonsAndFields()
        {
            string dir = TempDir();
            Directory.CreateDirectory(dir);
            try
            {
                File.WriteAllText(Path.Combine(dir, "SuperZome.cs"), @"
using NextGenSoftware.OASIS.STAR.DNA;
namespace Test
{
    public class SuperZome : ZomeDNA
    {
        public class SuperTest : HolonDNA
        {
            public string TestString { get; set; }
            public int TestInt { get; set; }
            public bool TestBool { get; set; }
        }

        public class SuperHolon : HolonDNA
        {
            public long? Big { get; set; }
        }
    }

    public class OtherZome : ZomeDNA
    {
        public class Thing : HolonDNA { public double Weight { get; set; } }
    }
}");
                HappSpec spec = HolochainHappGenerator.FromStarDnaFolder(dir, "My Planet");
                Assert.Equal("MyPlanet", spec.CSharpNamespace);
                Assert.Equal(new[] { "SuperZome", "OtherZome" }, spec.Zomes.Select(z => z.Name).ToArray());
                Assert.Equal(new[] { "SuperTest", "SuperHolon" }, spec.Zomes[0].Entries.Select(e => e.Name).ToArray());
                Assert.Equal(new[] { "TestString:string", "TestInt:int", "TestBool:bool" }, spec.Zomes[0].Entries[0].Fields.Select(f => f.Name + ":" + f.CSharpType).ToArray());
                Assert.Equal("long", spec.Zomes[0].Entries[1].Fields[0].CSharpType);
                Assert.Equal("Thing", Assert.Single(spec.Zomes[1].Entries).Name);

                // ...and it generates.
                Assert.NotEmpty(HolochainHappGenerator.Generate(spec, Path.Combine(dir, "out")));
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }

        [Theory]
        [InlineData("PlayerName", "player_name", "PlayerName")]
        [InlineData("player_name", "player_name", "PlayerName")]
        [InlineData("HTTPServer", "http_server", "HttpServer")]
        [InlineData("My Planet", "my_planet", "MyPlanet")]
        [InlineData("3d", "_3d", "3d")]
        public void Names(string input, string snake, string pascal)
        {
            Assert.Equal(snake, HolochainHappGenerator.Snake(input));
            Assert.Equal(pascal, HolochainHappGenerator.Pascal(input));
        }

        /// <summary>Writes the sample hApp where scripts/build-generated-happ.sh asks (HOLONET_GENERATE_HAPP_TO); skipped otherwise.</summary>
        [SkippableFact]
        public void WriteSampleHappForTheBuildScript()
        {
            string to = Environment.GetEnvironmentVariable("HOLONET_GENERATE_HAPP_TO");
            Skip.If(string.IsNullOrEmpty(to), "Only run by scripts/build-generated-happ.sh.");
            string version = Environment.GetEnvironmentVariable("HOLONET_GENERATE_HAPP_VERSION");
            HolochainHappGenerator.Generate(SampleSpec(version == "0.7.0" ? HappHolochainVersion.V0_7_0 : HappHolochainVersion.V0_7_1), to);
        }
    }
}

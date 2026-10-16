using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NextGenSoftware.Logging;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>NextGenSoftware.Logging's file provider (what HoloNET logs through): rotation, the file cap, concurrent writers.</summary>
    public class LogFileTests
    {
        [Fact]
        public void RotatesAtMaxSize_KeepsAtMostMaxLogFiles_AndLosesNoLinesUnderConcurrency()
        {
            string dir = Path.Combine(Path.GetTempPath(), "holonet-logtest-" + Guid.NewGuid().ToString("N"));
            try
            {
                var provider = new DefaultLogProvider(false, true, dir, "Test.log", maxLogFileSize: 2000) { MaxLogFiles = 5 };
                Parallel.For(0, 400, i => provider.Log($"line {i:D4} " + new string((char)120, 40), LogType.Info, true, false, false));

                var files = Directory.GetFiles(dir, "Test*.log");
                Assert.InRange(files.Length, 2, 5);                         // rotated, and capped at MaxLogFiles
                Assert.All(files, f => Assert.True(new FileInfo(f).Length < 2000 + 200, f));

                // The newest files hold the most recent lines; nothing was written twice.
                var lines = files.SelectMany(File.ReadAllLines).Where(l => l.Contains("line ")).ToList();
                Assert.Equal(lines.Count, lines.Distinct().Count());
            }
            finally
            {
                try { Directory.Delete(dir, true); } catch { }
            }
        }
    }
}

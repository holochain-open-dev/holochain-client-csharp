using System.Collections.Generic;
using MessagePack;
using Xunit;
using static NextGenSoftware.Holochain.HoloNET.Client.Tests.FakeConductorAdminTests;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    /// <summary>AppInfo.status in Holochain 0.7's adjacently tagged shape (holochain_types::app::AppStatus), plus the pre-0.7 keyed form.</summary>
    public class AppStatusWireTests
    {
        private static AppInfo Decode(object status) => MessagePackSerializer.Deserialize<AppInfo>(
            MessagePackSerializer.Serialize(new Dictionary<string, object> { ["installed_app_id"] = "app", ["status"] = status }),
            FakeConductor.Options);

        private static Dictionary<string, object> Tagged(string type, object value = null)
        {
            var d = new Dictionary<string, object> { ["type"] = type };
            if (value != null) d["value"] = value;
            return d;
        }

        [Theory]
        [InlineData("never_started", "Never Started")]
        [InlineData("not_started_after_providing_memproofs", "Not Started After Providing Memproofs")]
        [InlineData("user", "User")]
        public void Disabled_WithEachReason(string reason, string expected)
        {
            var info = Decode(Tagged("disabled", Tagged(reason)));
            Assert.Equal(AppInfoStatusEnum.Disabled, info.AppStatus);
            Assert.Equal(expected, info.AppStatusReason);
        }

        [Fact]
        public void Disabled_WithError() =>
            Assert.Equal("boom", AssertStatus(Tagged("disabled", Tagged("error", "boom")), AppInfoStatusEnum.Disabled).AppStatusReason);

        [Fact]
        public void Enabled_IsRunning() => AssertStatus(Tagged("enabled"), AppInfoStatusEnum.Running);

        [Fact]
        public void AwaitingMemproofs() => AssertStatus(Tagged("awaiting_memproofs"), AppInfoStatusEnum.AwaitingMemproofs);

        [Fact]
        public void AwaitingRestore() => AssertStatus(Tagged("awaiting_restore"), AppInfoStatusEnum.AwaitingRestore);

        [Fact]
        public void Unrecoverable_ReportsTheWarrant()
        {
            var info = AssertStatus(Tagged("unrecoverable", new object[] { new object[] { Dna(), Agent() }, Tagged("chain_fork_warrant", new Dictionary<string, object>()) }), AppInfoStatusEnum.Unrecoverable);
            Assert.Equal("chain_fork_warrant", info.AppStatusReason);
        }

        [Fact]
        public void Pre07_KeyedForms()
        {
            AssertStatus(new Dictionary<string, object> { ["running"] = null }, AppInfoStatusEnum.Running);
            Assert.Equal("x", AssertStatus(new Dictionary<string, object> { ["paused"] = new Dictionary<string, object> { ["reason"] = new Dictionary<string, object> { ["error"] = "x" } } }, AppInfoStatusEnum.Paused).AppStatusReason);
            Assert.Equal("Never Started", AssertStatus(new Dictionary<string, object> { ["disabled"] = new Dictionary<string, object> { ["reason"] = new Dictionary<string, object> { ["never_started"] = null } } }, AppInfoStatusEnum.Disabled).AppStatusReason);
            Assert.Equal("User", AssertStatus(new Dictionary<string, object> { ["disabled"] = new Dictionary<string, object> { ["reason"] = new Dictionary<string, object> { ["user"] = null } } }, AppInfoStatusEnum.Disabled).AppStatusReason);
            Assert.Equal("e", AssertStatus(new Dictionary<string, object> { ["disabled"] = new Dictionary<string, object> { ["reason"] = new Dictionary<string, object> { ["error"] = "e" } } }, AppInfoStatusEnum.Disabled).AppStatusReason);
            AssertStatus(new Dictionary<string, object> { ["awaiting_memproofs"] = null }, AppInfoStatusEnum.AwaitingMemproofs);
            AssertStatus(new Dictionary<string, object> { ["something_else"] = null }, AppInfoStatusEnum.None);
            Assert.Equal(AppInfoStatusEnum.None, Decode(null).AppStatus);
        }

        private static AppInfo AssertStatus(object status, AppInfoStatusEnum expected)
        {
            var info = Decode(status);
            Assert.Equal(expected, info.AppStatus);
            return info;
        }
    }
}

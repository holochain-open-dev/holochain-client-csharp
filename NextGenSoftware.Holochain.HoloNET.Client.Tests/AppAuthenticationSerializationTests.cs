using System;
using MessagePack;
using NextGenSoftware.Holochain.HoloNET.Client.Data.App.Requests;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests
{
    public sealed class AppAuthenticationSerializationTests
    {
        private static readonly MessagePackSerializerOptions Options =
            MessagePackSerializerOptions.Standard.WithSecurity(MessagePackSecurity.UntrustedData);

        [Fact]
        public void AuthenticationEnvelopeMatchesHolochain07WireContract()
        {
            byte[] token = { 0x01, 0x7f, 0x80, 0xff };

            byte[] wire = HoloNETClientAppBase.CreateAuthenticationEnvelope(token);
            byte[] expected =
            {
                0x82,
                0xa4, 0x74, 0x79, 0x70, 0x65,
                0xac, 0x61, 0x75, 0x74, 0x68, 0x65, 0x6e, 0x74, 0x69, 0x63, 0x61, 0x74, 0x65,
                0xa4, 0x64, 0x61, 0x74, 0x61,
                0xc4, 0x0d,
                0x81, 0xa5, 0x74, 0x6f, 0x6b, 0x65, 0x6e,
                0xc4, 0x04, 0x01, 0x7f, 0x80, 0xff
            };
            AppAuthenticationEnvelope envelope =
                MessagePackSerializer.Deserialize<AppAuthenticationEnvelope>(wire, Options);
            AppAuthenticationRequest request =
                MessagePackSerializer.Deserialize<AppAuthenticationRequest>(envelope.Data, Options);

            Assert.Equal("authenticate", envelope.Type);
            Assert.Equal(token, request.Token);
            Assert.Equal(expected, wire);
        }

        [Fact]
        public void AuthenticationEnvelopeRejectsMissingToken()
        {
            Assert.Throws<ArgumentException>(() => HoloNETClientAppBase.CreateAuthenticationEnvelope(null));
            Assert.Throws<ArgumentException>(() => HoloNETClientAppBase.CreateAuthenticationEnvelope(Array.Empty<byte>()));
        }
    }
}

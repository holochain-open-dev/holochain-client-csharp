using Chaos.NaCl;
using Xunit;

namespace NextGenSoftware.Holochain.HoloNET.Client.Tests;

public sealed class Ed25519ManagedTests
{
    [Fact]
    public void ManagedSigner_MatchesRfc8032EmptyMessageVector()
    {
        var seed = Convert.FromHexString(
            "9D61B19DEFFD5A60BA844AF492EC2CC44449C5697B326919703BAC031CAE7F60");
        var expectedPublicKey = Convert.FromHexString(
            "D75A980182B10AB7D54BFED3C964073A0EE172F3DAA62325AF021A68F707511A");
        var expectedSignature = Convert.FromHexString(
            "E5564300C360AC729086E2CC806E828A84877F1EB8E5D974D873E06522490155" +
            "5FB8821590A33BACC61E39701CF9B46BD25BF5F0595BBE24655141438E7A100B");

        Ed25519.KeyPairFromSeed(out var publicKey, out var privateKey, seed);
        var signature = Ed25519.Sign(Array.Empty<byte>(), privateKey);

        Assert.Equal(expectedPublicKey, publicKey);
        Assert.Equal(expectedSignature, signature);
        Assert.True(Ed25519.Verify(signature, Array.Empty<byte>(), publicKey));
    }
}

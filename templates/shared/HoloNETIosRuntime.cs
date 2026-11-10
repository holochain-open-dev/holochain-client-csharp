// iOS only. Starts the Holochain conductor inside the app and hands HoloNET an app connection to it.
//
// An iPhone cannot run a separate conductor process, so the conductor is linked into the app (Holochain 0.7, wasmi interpreter: iOS forbids
// JIT). This file calls the C ABI of HoloNETIosBridge.xcframework (holo_ios_*, from the android-service-runtime repo), which you put in
// runtime/ios (see README.md). Without it IsBridgeAvailable is false, and the app connects to a conductor you run instead.
//
// STATUS: the conductor runs on the iOS simulator in the OASIS team's CI. This wrapper has not been compiled or run by us on a Mac, and it is not
// proven on a physical iPhone. Treat it as ready to test.
#if IOS
using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NextGenSoftware.Holochain.HoloNET.Client;

namespace HoloNETDemos
{
    /// <summary>An app connection to the conductor inside this app.</summary>
    public sealed class IosRuntimeSession
    {
        /// <summary>The conductor's app interface port (connect to ws://127.0.0.1:Port).</summary>
        public int Port { get; set; }
        /// <summary>The app authentication token for that interface.</summary>
        public byte[] AuthenticationToken { get; set; }
    }

    public static unsafe class HoloNETIosRuntime
    {
        [DllImport("__Internal")]
        private static extern void holo_ios_start_and_setup(int requestId, byte[] appBundle, int appBundleLength, string installedAppId,
            string networkSeed, string bootstrapUrl, string relayUrl, delegate* unmanaged<int, int, IntPtr, IntPtr, IntPtr, void> callback);
        [DllImport("__Internal")]
        private static extern void holo_ios_sign_zome_call(int requestId, string provenance, string dnaHash, string agentPubKey, string zomeName, string fnName,
            string capSecret, string payload, string nonce, long expiresAt, delegate* unmanaged<int, IntPtr, IntPtr, IntPtr, IntPtr, void> callback);
        [DllImport("__Internal")]
        private static extern void holo_ios_stop(int requestId, delegate* unmanaged<int, byte, IntPtr, IntPtr, void> callback);

        private static readonly ConcurrentDictionary<int, TaskCompletionSource<IosRuntimeSession>> Sessions = new();
        private static readonly ConcurrentDictionary<int, TaskCompletionSource<(byte[], byte[])>> Signs = new();
        private static readonly ConcurrentDictionary<int, TaskCompletionSource<bool>> Operations = new();
        private static int _nextRequestId;

        /// <summary>True when HoloNETIosBridge.xcframework is linked into this app.</summary>
        public static bool IsBridgeAvailable =>
            NativeLibrary.TryGetExport(NativeLibrary.GetMainProgramHandle(), "holo_ios_start_and_setup", out _);

        /// <summary>The hApp bytes: the file at <paramref name="path"/> if it exists, else oasis.happ from the app bundle.</summary>
        public static byte[] LoadHapp(string path)
        {
            if (!string.IsNullOrEmpty(path) && System.IO.File.Exists(path)) return System.IO.File.ReadAllBytes(path);
            return System.IO.File.ReadAllBytes(System.IO.Path.Combine(Foundation.NSBundle.MainBundle.BundlePath, "oasis.happ"));
        }

        /// <summary>Starts the conductor inside the app and installs the hApp.</summary>
        public static Task<IosRuntimeSession> StartAsync(byte[] happBundle, string installedAppId, string networkSeed = null,
            string bootstrapUrl = "https://dev-test-bootstrap2.holochain.org", string relayUrl = "https://use1-1.relay.n0.iroh-canary.iroh.link./")
        {
            if (!IsBridgeAvailable) throw new InvalidOperationException("HoloNETIosBridge.xcframework is not linked into this app (see runtime/ios).");
            int id = Interlocked.Increment(ref _nextRequestId);
            var done = new TaskCompletionSource<IosRuntimeSession>(TaskCreationOptions.RunContinuationsAsynchronously);
            Sessions[id] = done;
            holo_ios_start_and_setup(id, happBundle, happBundle.Length, installedAppId, networkSeed, bootstrapUrl, relayUrl, &OnSession);
            return done.Task;
        }

        /// <summary>Asks the conductor to sign a zome call with the agent key it holds.</summary>
        public static Task<(byte[] Bytes, byte[] Signature)> SignZomeCallAsync(ZomeCallToSign call)
        {
            int id = Interlocked.Increment(ref _nextRequestId);
            var done = new TaskCompletionSource<(byte[], byte[])>(TaskCreationOptions.RunContinuationsAsynchronously);
            Signs[id] = done;
            holo_ios_sign_zome_call(id, B64(call.CellIdAgentPubKey), B64(call.CellIdDnaHash), B64(call.CellIdAgentPubKey), call.ZomeName, call.FnName,
                "", B64(call.Payload), B64(call.Nonce), call.ExpiresAt, &OnSign);
            return done.Task;
        }

        /// <summary>Stops the conductor.</summary>
        public static Task StopAsync()
        {
            if (!IsBridgeAvailable) return Task.CompletedTask;
            int id = Interlocked.Increment(ref _nextRequestId);
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            Operations[id] = done;
            holo_ios_stop(id, &OnOperation);
            return done.Task;
        }

        [UnmanagedCallersOnly]
        private static void OnSession(int requestId, int port, IntPtr tokenBase64, IntPtr errorCode, IntPtr message)
        {
            if (!Sessions.TryRemove(requestId, out var done)) return;
            if (errorCode != IntPtr.Zero) { done.TrySetException(new InvalidOperationException(Marshal.PtrToStringAnsi(errorCode) + ": " + Marshal.PtrToStringAnsi(message))); return; }
            done.TrySetResult(new IosRuntimeSession { Port = port, AuthenticationToken = Convert.FromBase64String(Marshal.PtrToStringAnsi(tokenBase64) ?? "") });
        }

        [UnmanagedCallersOnly]
        private static void OnSign(int requestId, IntPtr bytesBase64, IntPtr signatureBase64, IntPtr errorCode, IntPtr message)
        {
            if (!Signs.TryRemove(requestId, out var done)) return;
            if (errorCode != IntPtr.Zero) { done.TrySetException(new InvalidOperationException(Marshal.PtrToStringAnsi(errorCode) + ": " + Marshal.PtrToStringAnsi(message))); return; }
            done.TrySetResult((Convert.FromBase64String(Marshal.PtrToStringAnsi(bytesBase64) ?? ""), Convert.FromBase64String(Marshal.PtrToStringAnsi(signatureBase64) ?? "")));
        }

        [UnmanagedCallersOnly]
        private static void OnOperation(int requestId, byte ok, IntPtr errorCode, IntPtr message)
        {
            if (!Operations.TryRemove(requestId, out var done)) return;
            if (ok != 0) done.TrySetResult(true);
            else done.TrySetException(new InvalidOperationException(Marshal.PtrToStringAnsi(errorCode) + ": " + Marshal.PtrToStringAnsi(message)));
        }

        private static string B64(byte[] v) => Convert.ToBase64String(v ?? Array.Empty<byte>());
    }

    /// <summary>Signs HoloNET's zome calls through the conductor in this app. Set it as HoloNETClientAppAgent.ZomeCallSigner.</summary>
    public sealed class IosRuntimeZomeCallSigner : IZomeCallSigner
    {
        public async Task<ZomeCallParamsSigned> SignZomeCallAsync(ZomeCallToSign call)
        {
            var (bytes, signature) = await HoloNETIosRuntime.SignZomeCallAsync(call);
            return new ZomeCallParamsSigned { bytes = bytes, signature = signature };
        }
    }
}
#endif

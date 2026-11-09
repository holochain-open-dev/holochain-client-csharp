// Android only. Starts a Holochain conductor on the phone and hands HoloNET an app connection to it.
//
// Two modes, the same behaviour as Our World (OASIS Edge):
//   1. shared   - the phone has the Holochain Service Runtime app installed. This app asks it for access (the user approves a
//                 notification once) and uses its one conductor, which every Holochain app on the phone shares.
//   2. embedded - the runtime app is not installed, not approved in time, or not compatible: the conductor built into this app is used.
//
// It talks to the Kotlin bridge (one.oasisomniverse.holooasis.unity.HolochainUnityBridge, source in android-runtime/ of the HoloNET repo)
// over JNI. The bridge and the Holochain runtime libraries ship as Android libraries (.aar): put them in this project's
// runtime/android folder (see README.md). Without them IsBridgeAvailable is false and StartAsync reports that, so the app can
// connect to a conductor you run instead. No Unity is involved.
#if ANDROID
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Android.Runtime;
using NextGenSoftware.Holochain.HoloNET.Client;

namespace HoloNETDemos
{
    /// <summary>An app connection to the conductor on the phone.</summary>
    public sealed class AndroidRuntimeSession
    {
        /// <summary>The conductor's app interface port on this phone (connect to ws://127.0.0.1:Port).</summary>
        public int Port { get; set; }
        /// <summary>The app authentication token for that interface.</summary>
        public byte[] AuthenticationToken { get; set; }
        /// <summary>"shared" (the phone's Holochain Service Runtime) or "embedded" (the conductor built into this app).</summary>
        public string Mode { get; set; }
    }

    public static class HoloNETAndroidRuntime
    {
        private const string Bridge = "one/oasisomniverse/holooasis/unity/HolochainUnityBridge";
        private const string SharedRuntimePackage = "org.holochain.androidserviceruntime.app";
        private static readonly object Gate = new object();

        /// <summary>Progress: "awaiting_approval" (ask the user to approve Holochain access in the notification), "shared", "shared_unavailable", "fallback".</summary>
        public static event Action<string, string> StatusChanged;

        /// <summary>True when the bridge library (the .aar files) is part of this app.</summary>
        public static bool IsBridgeAvailable
        {
            get
            {
                try { var c = JNIEnv.FindClass(Bridge); JNIEnv.DeleteLocalRef(c); return true; }
                catch (Exception) { return false; }
            }
        }

        /// <summary>True when the phone has the shared Holochain Service Runtime app installed.</summary>
        public static bool IsSharedRuntimeInstalled
        {
            get
            {
                try { Android.App.Application.Context.PackageManager.GetPackageInfo(SharedRuntimePackage, 0); return true; }
                catch (Exception) { return false; }
            }
        }

        /// <summary>The hApp bytes: the file at <paramref name="path"/> if it exists, else the "oasis.happ" Android asset the template packs into the app.</summary>
        public static byte[] LoadHapp(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
                return File.ReadAllBytes(path);
            using var asset = Android.App.Application.Context.Assets.Open("oasis.happ");
            using var ms = new MemoryStream();
            asset.CopyTo(ms);
            return ms.ToArray();
        }

        /// <summary>
        /// Starts the conductor (shared runtime first, then the built-in one) and installs the hApp.
        /// Waits up to <paramref name="approvalTimeoutMs"/> for the user to approve access in the shared runtime's notification.
        /// </summary>
        public static Task<AndroidRuntimeSession> StartAsync(byte[] happBundle, string installedAppId, string networkSeed = null,
            bool preferShared = true, long approvalTimeoutMs = 60000,
            string bootstrapUrl = "https://dev-test-bootstrap2.holochain.org", string relayUrl = "https://use1-1.relay.n0.iroh-canary.iroh.link./")
        {
            if (!IsBridgeAvailable)
                throw new InvalidOperationException("The Holochain Android bridge (the .aar files in runtime/android) is not part of this app.");

            var done = new TaskCompletionSource<AndroidRuntimeSession>(TaskCreationOptions.RunContinuationsAsynchronously);
            var session = Callback("SessionCallback", (name, a) =>
            {
                if (name == "onSuccess")
                    done.TrySetResult(new AndroidRuntimeSession
                    {
                        Port = ((Java.Lang.Integer)a[0]).IntValue(),
                        AuthenticationToken = Convert.FromBase64String(a[1].ToString()),
                        Mode = GetMode()
                    });
                else if (name == "onError")
                    done.TrySetException(new InvalidOperationException(a[0] + ": " + a[1]));
            });
            var status = Callback("StatusCallback", (name, a) => StatusChanged?.Invoke(a[0].ToString(), a[1].ToString()));

            IntPtr cls = JNIEnv.FindClass(Bridge);
            IntPtr bundle = JNIEnv.NewArray(happBundle);
            IntPtr id = JNIEnv.NewString(installedAppId), seed = JNIEnv.NewString(networkSeed), boot = JNIEnv.NewString(bootstrapUrl), relay = JNIEnv.NewString(relayUrl);
            try
            {
                IntPtr m = JNIEnv.GetStaticMethodID(cls, "startAndSetup",
                    "(Landroid/content/Context;[BLjava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;ZJ" +
                    "L" + Bridge + "$StatusCallback;L" + Bridge + "$SessionCallback;)V");
                JNIEnv.CallStaticVoidMethod(cls, m, new JValue(Android.App.Application.Context.Handle), new JValue(bundle), new JValue(id), new JValue(seed),
                    new JValue(boot), new JValue(relay), new JValue(preferShared), new JValue(approvalTimeoutMs), new JValue(status.Handle), new JValue(session.Handle));
            }
            finally
            {
                foreach (var p in new[] { cls, bundle, id, seed, boot, relay }) if (p != IntPtr.Zero) JNIEnv.DeleteLocalRef(p);
            }
            return done.Task;
        }

        /// <summary>Asks the conductor to sign a zome call with the agent key it holds (the app has no admin access to do it itself).</summary>
        public static Task<(byte[] Bytes, byte[] Signature)> SignZomeCallAsync(ZomeCallToSign call)
        {
            var done = new TaskCompletionSource<(byte[], byte[])>(TaskCreationOptions.RunContinuationsAsynchronously);
            var cb = Callback("SignCallback", (name, a) =>
            {
                if (name == "onSuccess") done.TrySetResult((Convert.FromBase64String(a[0].ToString()), Convert.FromBase64String(a[1].ToString())));
                else if (name == "onError") done.TrySetException(new InvalidOperationException(a[0] + ": " + a[1]));
            });
            var s = new[]
            {
                B64(call.CellIdAgentPubKey), B64(call.CellIdDnaHash), B64(call.CellIdAgentPubKey), call.ZomeName, call.FnName, "", B64(call.Payload), B64(call.Nonce)
            };
            IntPtr cls = JNIEnv.FindClass(Bridge);
            var refs = new List<IntPtr> { cls };
            try
            {
                var args = new List<JValue>();
                foreach (var v in s) { var r = JNIEnv.NewString(v); refs.Add(r); args.Add(new JValue(r)); }
                args.Add(new JValue(call.ExpiresAt));
                args.Add(new JValue(cb.Handle));
                IntPtr m = JNIEnv.GetStaticMethodID(cls, "signZomeCall",
                    "(Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;Ljava/lang/String;J" +
                    "L" + Bridge + "$SignCallback;)V");
                JNIEnv.CallStaticVoidMethod(cls, m, args.ToArray());
            }
            finally { foreach (var p in refs) JNIEnv.DeleteLocalRef(p); }
            return done.Task;
        }

        /// <summary>Stops the built-in conductor (the shared runtime belongs to the phone and keeps running).</summary>
        public static Task StopAsync()
        {
            if (!IsBridgeAvailable) return Task.CompletedTask;
            var done = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var cb = Callback("OperationCallback", (name, a) =>
            {
                if (name == "onSuccess") done.TrySetResult(true);
                else if (name == "onError") done.TrySetException(new InvalidOperationException(a[0] + ": " + a[1]));
            });
            IntPtr cls = JNIEnv.FindClass(Bridge);
            try
            {
                IntPtr m = JNIEnv.GetStaticMethodID(cls, "stop", "(L" + Bridge + "$OperationCallback;)V");
                JNIEnv.CallStaticVoidMethod(cls, m, new JValue(cb.Handle));
            }
            finally { JNIEnv.DeleteLocalRef(cls); }
            return done.Task;
        }

        private static string GetMode()
        {
            IntPtr cls = JNIEnv.FindClass(Bridge);
            try
            {
                IntPtr m = JNIEnv.GetStaticMethodID(cls, "getMode", "()Ljava/lang/String;");
                return JNIEnv.GetString(JNIEnv.CallStaticObjectMethod(cls, m), JniHandleOwnership.TransferLocalRef);
            }
            finally { JNIEnv.DeleteLocalRef(cls); }
        }

        private static string B64(byte[] v) => Convert.ToBase64String(v ?? Array.Empty<byte>());

        // A java.lang.reflect.Proxy implementing one of the bridge's callback interfaces: no Java glue code is needed.
        private static Java.Lang.Object Callback(string iface, Action<string, Java.Lang.Object[]> handler)
        {
            using var type = Java.Lang.Class.ForName(Bridge.Replace('/', '.') + "$" + iface);
            return Java.Lang.Reflect.Proxy.NewProxyInstance(type.ClassLoader, new[] { type }, new Handler(handler));
        }

        private sealed class Handler : Java.Lang.Object, Java.Lang.Reflect.IInvocationHandler
        {
            private readonly Action<string, Java.Lang.Object[]> _handler;
            public Handler(Action<string, Java.Lang.Object[]> handler) => _handler = handler;

            public Java.Lang.Object Invoke(Java.Lang.Object proxy, Java.Lang.Reflect.Method method, Java.Lang.Object[] args)
            {
                switch (method.Name)
                {
                    case "hashCode": return Java.Lang.Integer.ValueOf(GetHashCode());
                    case "equals": return Java.Lang.Boolean.ValueOf(ReferenceEquals(proxy, args?[0]));
                    case "toString": return new Java.Lang.String("HoloNET bridge callback");
                    default: _handler(method.Name, args ?? Array.Empty<Java.Lang.Object>()); return null;
                }
            }
        }
    }

    /// <summary>Signs HoloNET's zome calls through the conductor on the phone. Set it as HoloNETClientAppAgent.ZomeCallSigner.</summary>
    public sealed class AndroidRuntimeZomeCallSigner : IZomeCallSigner
    {
        public async Task<ZomeCallParamsSigned> SignZomeCallAsync(ZomeCallToSign call)
        {
            var (bytes, signature) = await HoloNETAndroidRuntime.SignZomeCallAsync(call);
            return new ZomeCallParamsSigned { bytes = bytes, signature = signature };
        }
    }
}
#endif

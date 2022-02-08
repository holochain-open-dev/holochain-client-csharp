# holochain-client-csharp

using the following references, we can implement the beginnings of a conductor client library for C#. 
- https://hackmd.io/wRoLSZRHRniiq_FlMcmHrA
- https://github.com/holochain/holochain-client-rust
- https://github.com/holochain/holochain-client-js 
- https://github.com/Sprillow/electron-holochain-template/blob/main/web/src/simpleZomeClient.ts
- Basically a full implementation (but not working for dellams?) https://gist.github.com/Jakintosh/8578f39a66e50c4beb24bc34275e6ce5

Connor proposes that we run tests against https://github.com/holochain/happ-build-tutorial

There are a whole bunch of forum posts, Dev Camp discord chatter, and github issues that all precede and provide background on why to start this:
- https://github.com/holochain/holochain/issues/905
- (C++ but related) https://forum.holochain.org/t/wanted-a-c-client/6669


## Proposed API

One class which hosts the open websocket connection
  - reason, being able to hold open a persistent connection, and not needing to have many open connections with the conductor

One class which needs an instance of that websocket class passed to it, which is an abstraction over a Zome. So it is a Zome client. That way, one only needs to pass in an argument to make a zome call telling it which 'function' you wish to call in holochain. 

## HoloNET Code Has Migrated

### Background

Original HoloNET code (world's first .NET & Unity client for Holochain written back in 2019 and was fully operational with the previous version of Holochain (Redux), but unfortunately had issues getting it working with RSM. The previous version also came bundled with the holochain conductor so it could auto-start/shutdown the conductor and be fully integrated with any .NET or Unity application. This code/functionality is still in there and will now work again that we have a Windows binary again (NixOS broke this feature previously).

### Current Issues

The current issue is to do with that the Holochain conductor not recognising the data format that is sent from the .NET MessagePack Serialiser. In the code you will find two different types, one is the fastest and most popular .NET MessagePack Serialiser (https://github.com/neuecc/MessagePack-CSharp). This is the one currently active. The other one (currently commented out) is the Unity one that was used by Jakintosh Unity implementation above but unfortunately even though I did my best to port this to .NET it appears not to be compatible because the data packets it sends are too small as spotted by @connor.

The WebSocket code is in it's own library/project and currently uses the one ported from the Unity project Jakintosh implemented. This one does appear to be compatible with .NET. The original HoloNET WebSocket code is still there and will likely be the one to be used again once we have tracked down what the issues are. I attempted to port both the WebSocket and MessagePack libraries uses in the Unity implementation to attempt to track down what the issue was.

As far as I can see the rest of the code is equivalent and is sending the same data formats etc across to the conductor. Currently the error message I get back from the Conductor is below:

```
←[1;34mhc-sandbox:←[0m Created ["C:\\Users\\david\\AppData\\Local\\Temp\\IQUXzSuv8uMAoWMUaGz0A"]

Conductor ready.
←[1;34mhc-sandbox:←[0m Running conductor on admin port 52212
←[1;34mhc-sandbox:←[0m Attaching app port 8888
←[1;34mhc-sandbox:←[0m App port attached at 8888
←[1;34mhc-sandbox:←[0m Connected successfully to a running holochain
←[2mFeb 02 20:36:43.785←[0m ←[31mERROR←[0m holochain_websocket::websocket: Websocket failed to deserialize SerializedBytesError(Deserialize("invalid type: string \"Request\", expected u64"))
←[2mFeb 02 20:36:43.788←[0m ←[31mERROR←[0m holochain_websocket::websocket: Websocket: Bad message type Pong([])
←[2mFeb 02 20:36:43.788←[0m ←[31mERROR←[0m holochain_websocket::websocket: Websocket: Bad message type Pong([])
←[2mFeb 02 20:37:13.728←[0m ←[31mERROR←[0m holochain_websocket::websocket: Websocket: Bad message type Pong([])
```

This is very odd because it appears it is expecting a U64 for the RequestType rather than a string?

Looking at Jakintosh code you can see that the request is surrounded by additional quotes:

```
[Serializable]
	public class WireMessage : RustEnum<byte[]> {

		public long id;

		public WireMessage (
			string type,
			byte[] data,
			long id
		) : base( type, data ) {
			this.id = id;
		}

		public override string ToString () => $"{{\n  type: \"{type}\",\n  id: {id},\n  data: {System.Text.Encoding.UTF8.GetString( data )}\n}}";
	}
```

If I attempt to do the same with the HoloNET code then I get back this error from the conductor:

```
Conductor ready.
←[1;34mhc-sandbox:←[0m Running conductor on admin port 54443
←[1;34mhc-sandbox:←[0m Attaching app port 8888
←[1;34mhc-sandbox:←[0m App port attached at 8888
←[1;34mhc-sandbox:←[0m Connected successfully to a running holochain
←[2mFeb 03 13:59:17.168←[0m ←[31mERROR←[0m holochain_websocket::websocket: Websocket failed to deserialize SerializedBytesError(Deserialize("invalid type: string \"\\\"Request\\\"\", expected u64"))
←[2mFeb 03 13:59:34.580←[0m ←[31mERROR←[0m holochain_websocket::websocket: Websocket: Bad message type Pong([])
←[2mFeb 03 14:00:04.538←[0m ←[31mERROR←[0m holochain_websocket::websocket: Websocket: Bad message type Pong([])
←[2mFeb 03 14:00:34.552←[0m ←[31mERROR←[0m holochain_websocket::websocket: Websocket: Bad message type Pong([])
←[2mFeb 03 14:00:42.297←[0m ←[31mERROR←[0m holochain_websocket::websocket: websocket_error_from_network=Io(Os { code: 10054, kind: ConnectionReset, message: "An existing connection was forcibly closed by the remote host." })
```

This is as far as I have got to date so would appreciate any help I can get on this please, thank you. :)

You can view the full discussion going on since Aug to get this to work here:
https://github.com/holochain/holochain/issues/905

It actually goes back further than this but this is the latest thread regarding this... :)

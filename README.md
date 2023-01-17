# HoloNET Holochain .NET/Unity Client

  # Table of contents

  - [Overview](#overview)
  - [HoloNET Code Has Migrated](#holonet-code-has-migrated)
  - [Background](#background)
  - [Initial RSM Version](#initial-rsm-version)
  - [How To Use HoloNET](#how-to-use-holonet)
    - [Quick Start](#quick-start)
    - [The Power of .NET Async Methods](#the-power-of-net-async-methods)
      - [New Hybrid Async/Event Model](#new-hybrid-async/event-model)
    - [Events](#events)
      - [OnConnected](#onconnected)
      - [OnAppInfoCallBack](#onappinfocallback)
      - [OnReadyForZomeCalls](#onreadyforzomecalls)
      - [OnDataReceived](#ondatareceived)
      - [OnZomeFunctionCallBack](#onzomefunctioncallback)
      - [OnSignalsCallBack](#onsignalscallback)
      - [OnConductorDebugCallBack](#onconductordebugcallback)
      - [OnDisconnected](#ondisconnected)
      - [OnError](#onerror)
    - [Methods](#methods)
      - [Connect](#connect)
      - [StartConductor](#startconductor)
      - [GetAgentPubKeyAndDnaHashFromSandbox](#getagentpubkeyanddnahashfromsandbox)
      - [GetAgentPubKeyAndDnaHashFromConductor](#getagentpubkeyanddnahashfromconductor)
      - [SendHoloNETRequest](#sendholonetrequest)
      - [CallZomeFunctionAsync](#callzomefunctionasync)
        - [Overload 1](#overload-1)
        - [Overload 2](#overload-2)
        - [Overload 3](#overload-3)
        - [Overload 4](#overload-4)
        - [Overload 5](#overload-5)
        - [Overload 6](#overload-6)
        - [Overload 7](#overload-7)
        - [Overload 8](#overload-8)
        - [Overload 9](#overload-9)
        - [Overload 10](#overload-10)
      - [Disconnect](#disconnect)
      - [ShutDownAllConductors](#shutdownallconductors)
      - [ClearCache](#clearcache)
      - [ConvertHoloHashToBytes](#convertholohashtobytes)
      - [ConvertHoloHashToString](#convertholohashtostring)
    - [Properties](#properties)
      - [Config](#config)
      - [WebSocket](#websocket)
        - [Config](#config)
        - [State](#state)
      - [State](#state)
      - [EndPoint](#endpoint)
    - [Logging](#logging)
    - [Test Harness](#test-harness)
      - [NetworkServiceProvider](#networkserviceprovider)
      - [NetworkServiceProviderMode](#networkserviceprovidermode)
  - [HoloOASIS](#holooasis)
  - [HoloUnity](#holounity)
    - [Using HoloUnity](#using-holounity)
    - [Events](#events)
    - [Methods](#methods)
    - [Properties](#properties)
  - [Why this is important & vital to the holochain community](#why-this-is-important--vital-to-the-holochain-community)
  - [What's Next?](#whats-next)
    - [Unity Asset](#unity-asset)
    - [.NET HDK Low Code Generator](#net-hdk-low-code-generator)
    - [Restore Holochain Support For The OASIS API](#restore-holochain-support-for-the-oasis-api)
    - [WEB5 STAR Omniverse Interoperable Metaverse Low Code Generator](#web5-star-omniverse-interoperable-metaverse-low-code-generator)
  - [Donations Welcome! Thank you!](#donations-welcome-thank-you)
  - [Do You Want To Get Involved?](#get-involved)

## Overview

The world's first .NET & Unity client for [Holochain](http://holochain.org).

This library will allow you to connect any .NET or Unity client to Holochain and enjoy the power of a fully de-centralised distributed P2P multi-network agent-centric architecture.

This will help massively turbo charge the holochain ecosystem by opening it up to the massive .NET and Unity communities and open up many more possibilities of the things that can be built on top of Holochain. You can build almost anything you can imagine with .NET and/or Unity from websites, desktop apps, smartphone apps, services, AAA Games and lots more! They can target every device and platform out there from XBox, PS4, Wii, PC, Linux, Mac, iOS, Android, Windows Phone, iPad, Tablets, SmartTV, VR/AR/XR, MagicLeap, etc

**We are a BIG fan of Holochain and are very passionate about it and see a BIG future for it! We feel this is the gateway to taking Holochain mainstream! ;-)**

There are two versions of HoloNET:

[NextGenSoftware.Holochain.HoloNET.Client](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client) - Lightweight version that does not come with the holochain binaries (hc.exe and holochain.exe).

[NextGenSoftware.Holochain.HoloNET.Client.Embedded](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.Embedded) - This version comes with the holochain binaries (hc.exe and holochain.exe) integrated.

You can also find the Test Harness here:

[NextGenSoftware.Holochain.HoloNET.Client.TestHarness](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.TestHarness)

Read more on how to use the [Test Harness here](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/NextGenSoftware.Holochain.HoloNET.Client.TestHarness).

## HoloNET Code Has Migrated

This code was migrated from the main OASIS API/STAR Metaverse/HoloNET/.NET HDK code found here:
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

## Background

Original HoloNET code was written back in 2019 and was fully operational with the previous version of Holochain (Redux), but unfortuntley had issues getting it working with RSM (Refactored State Model/latest version)

https://www.ourworldthegame.com/single-post/2019/08/14/world-exclusive-holochain-talking-to-unity

The previous version also came bundled with the holochain conductor so it could auto-start/shutdown the conductor and be fully integrated with any .NET or Unity application. This code/fuctionaility is still in there and will now work again that we have a Windows binary again (NixOS broke this feature previously).

It was featured on Dev Pulse 44 here: \
https://medium.com/holochain/updated-quick-start-guide-the-gift-of-holonet-and-conversations-that-matter-on-the-holochain-8e08efde1f58 \
https://www.ourworldthegame.com/single-post/2019/09/10/holonet-was-featured-in-the-latest-holochain-dev-pulse

## Initial RSM Version

We are pleased that after nearly 2 years we have now finally got this upgraded to work with RSM thanks to Connors help, who we are eternally grateful to! :)

https://www.ourworldthegame.com/single-post/holonet-rsm-breakthrough-at-long-last

Please check out the above link, there you will find more details on what has changed from the previous Redux HoloNET version as well as some documentation on how to use it... :)

We will also add it here soon...


## How To Use HoloNET 

**NOTE: This documentation is a WIP, it will be completed soon, please bare with us, thank you! :)**

### Quick Start

You start by instantiating a new HoloNETClient class passing in the holochain websocket URI to the constructor as seen below:

````c#
HoloNETClient holoNETClient = new HoloNETClient("ws://localhost:8888");
````

Next, you can subscribe to a number of different events:

````c#
holoNETClient.OnConnected += HoloNETClient_OnConnected;
holoNETClient.OnAppInfoCallBack += HoloNETClient_OnAppInfoCallBack;
holoNETClient.OnReadyForZomeCalls += HoloNETClient_OnReadyForZomeCalls;
holoNETClient.OnDataReceived += HoloNETClient_OnDataReceived;
holoNETClient.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;
holoNETClient.OnSignalsCallBack += HoloNETClient_OnSignalsCallBack;
holoNETClient.OnDisconnected += HoloNETClient_OnDisconnected;
holoNETClient.OnError += HoloNETClient_OnError;
holoNETClient.OnConductorDebugCallBack += HoloNETClient_OnConductorDebugCallBack;
holoNETClient.OnHolochainConductorsShutdownComplete += _holoNETClient_OnHolochainConductorsShutdownComplete;
holoNETClient.OnHoloNETShutdownComplete += _holoNETClient_OnHoloNETShutdownComplete;
````

Now you can call the [Connect](#connect) method to connect to Holochain.

````c#
await holoNETClient.Connect();
````

The Connection method has two optional parameters, the first is getAgentPubKeyAndDnaHashFromConductor and the second is getAgentPubKeyAndDnaHashFromSandbox, both default to true.

By default HoloNET will automatically query the AgentPubKey & DnaHash from the Conductor, if that fails, it will try from the hc dev sandbox command. If that fails you will need to manually set them.

To manually set the AgentPubKey & DnaHash use the following:

````c#
//Use this if you to manually pass in the AgentPubKey &DnaHash(otherwise it will be automatically queried from the conductor or sandbox).
_holoNETClient.Config.AgentPubKey = "YOUR KEY";
_holoNETClient.Config.DnaHash = "YOUR HASH";

await _holoNETClient.Connect(false, false);
````

Once it connects successfully it will raise the [OnConnected](#onconnected) event and then start automatically querying the conductor for the cell id's containing the AgentPubKey & DnaHash (unless you changed the default params above to false).

It will then raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event signalling you can now make zome calls to the conductor.

Now you can call one of the [CallZomeFunctionAsync()](#callzomefunctionasync) overloads:

````c#
await _holoNETClient.CallZomeFunctionAsync("1", "numbers", "add_ten", ZomeCallback, new { number = 10 });
````

Please see below for more details on the various overloads available for this call as well as the data you get back from this call and the other methods and events you can use...


### The Power of .NET Async Methods

You will notice that the above calls have the `await` keyword prefixing them. This is how you call an `async` method in C#. All of HoloNET, HoloOASIS & OASIS API methods are async methods. This simply means that they do not block the calling thread so if this is running on a UI thread it will not freeze the UI. Using the `await` keyword allows you to call an `async` method as if it was a synchronous one. This means it will not call the next line until the async method has returned. The power of this is that you no longer need to use lots of messy callback functions cluttering up your code as has been the pass with non-async programming. The code path is also a lot easier to follow and maintain.

Read more here:
https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/async/

#### New Hybrid Async/Event Model

Saying this, there may be scenarious where you need to use the older non async style methods and use callbacks instead so starting from HoloNET 2.0.0, it also provides non async versions of each method. On top of this each method now has optional params where you can choose to ask HoloNET to wait for the Holochain Conductor to return a response or to return to the calling method immediately and use a callback event instead.

### Events
<a name="events"></a>

You can subscribe to a number of different events:

| Event                                                                           | Description                                                                                                                                                                                                                                 |
| --------------------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [OnConnected](#onconnected)                                                     | Fired when the client has successfully connected to the Holochain conductor.                                                                                                                                                                |
| [OnAppInfoCallBack](#onappinfocallback)                                         | Fired when the client receives AppInfo from the conductor containing the cell id for the running hApp (which in itself contains the AgentPubKey & DnaHash). It also contains the AppId and other info.                                      |
| [OnReadyForZomeCalls](#onreadyforzomecalls)                                     | Fired when the client has successfully connected and reteived the AgentPubKey & DnaHash, meaning it is ready to make zome calls to the Holochain conductor.                                                                                 |
| [OnDataReceived](#ondatareceived)                                               | Fired when any data is received from the Holochain conductor. This returns the raw data.                                                                                                                                                    |
| [OnZomeFunctionCallBack](#onzomefunctioncallback)                               | Fired when the Holochain conductor returns the response from a zome function call. This returns the raw data as well as the parsed data returned from the zome function. It also returns the id, zome and zome function that made the call. |
| [OnSignalsCallBack](#onsignalscallback)                                         | Fired when the Holochain conductor sends signals data. NOTE: This is still waiting for hc to flresh out the details for how this will work. Currently this returns the raw signals data.                                                    | 
| [OnConductorDebugCallBack](#onconductordebugcallback)                           | Fired when the Holochain conductor sends debug info.                                                                                                                                                                                        |
| [OnDisconnected](#ondisconnected)                                               | Fired when the client disconnected from the Holochain conductor.                                                                                                                                                                            |
| [OnError](#onerror)                                                             | Fired when an error occurs, check the params for the cause of the error.                                                                                                                                                                    |
| [OnHolochainConductorsShutdownComplete](#OnHolochainConductorsShutdownComplete) | Fired when all Holochain Conductors have been shutdown.                                                                                                                                                                                     |
| [OnHoloNETShutdownComplete](#OnHoloNETShutdownComplete)                         | Fired when HoloNET has completed shutting down (this includes closing all connections and shutting down all Holochain Conductor).                                                                                                           |

#### OnConnected
Fired when the client has successfully connected to the Holochain conductor. 

````c#
holoNETClient.OnConnected += HoloNETClient_OnConnected;

private static void HoloNETClient_OnConnected(object sender, ConnectedEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: CONNECTED CALLBACK: Connected to ", e.EndPoint));
            Console.WriteLine("");
        }
````

| Parameter          | Description                                         |
|--------------------|-----------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.        |


#### OnAppInfoCallBack
Fired when the client receives AppInfo from the Holochain conductor containing the cell id for the running hApp (which in itself contains the AgentPubKey & DnaHash). It also contains the AppId and other info.

````c#
holoNETClient.OnAppInfoCallBack += HoloNETClient_OnAppInfoCallBack;

private static void HoloNETClient_OnAppInfoCallBack(object sender, AppInfoCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: APPINFO CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id, ", AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash, ", Installed App Id: ", e.InstalledAppId, ", Raw Binary Data: ",  e.RawBinaryData, ", Raw JSON Data: ", e.RawJSONData), LogType.Info);
            Console.WriteLine("");
        }
````

| Parameter                                     | Description                                                                                                                                                                                                                                                                                                                                             |
|-----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint                                      | The URI EndPoint of the Holochain conductor.                                                                                                                                                                                                                                                                                                            |
| Id                                            | The id that made the request.                                                                                                                                                                                                                                                                                                                           |
| AgentPubKey                                   | The AgentPubKey for the hApp.                                                                                                                                                                                                                                                                                                                           |
| DnaHash                                       | The DnaHash for the hApp.                                                                                                                                                                                                                                                                                                                               |
| InstalledAppId                                | The InstalledAppId for the hApp.                                                                                                                                                                                                                                                                                                                        |
| RawBinaryData                                 | The raw binary data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                              |
| RawBinaryDataAsString                         | The raw binary data returned from the Holochain conductor formatted as a string (useful for debugging/logging etc).                                                                                                                                                                                                                                     |
| RawBinaryDataDecoded                          | The raw binary data returned from the Holochain conductor decoded into a string using UTF8 encoding.                                                                                                                                                                                                                                                    |
| RawBinaryDataAfterMessagePackDecode           | The raw binary data after it has been decoded by MessagePack.                                                                                                                                                                                                                                                                                           |
| RawBinaryDataAfterMessagePackDecodeAsString   | The raw binary data after it has been decoded by MessagePack formatted as a string (useful for debugging/logging etc).                                                                                                                                                                                                                                  |
| RawBinaryDataAfterMessagePackDecodeDecoded    | The raw binary data after it has been decoded by MessagePack decoded into a string using UTF8 encoding.                                                                                                                                                                                                                                                 |
| RawJSONData                                   | The raw JSON data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                                |
| WebSocketResult                               | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info. |


#### OnReadyForZomeCalls

Fired when the client has successfully connected and reteived the AgentPubKey & DnaHash, meaning it is ready to make zome calls to the holochain conductor.  

````c#
private async static void _holoNETClient_OnReadyForZomeCalls(object sender, ReadyForZomeCallsEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: READY FOR ZOME CALLS EVENT HANDLER: EndPoint: ", e.EndPoint, ", AgentPubKey: ", e.AgentPubKey, ", DnaHash: ", e.DnaHash));
            Console.WriteLine("");
            Console.WriteLine("Calling Test Zome...\n");

            //await _holoNETClient.CallZomeFunctionAsync("1", "our_world_core", "test", ZomeCallback, null);
            //await _holoNETClient.CallZomeFunctionAsync("1", "whoami", "whoami", ZomeCallback, null);
            //await _holoNETClient.CallZomeFunctionAsync("1", "whoami", "whoami", ZomeCallback, null);
            await _holoNETClient.CallZomeFunctionAsync("1", "numbers", "add_ten", ZomeCallback, new { number = 10 });

            // Load testing
            //   for (int i = 0; i < 100; i++)
            //     await _holoNETClient.CallZomeFunctionAsync("1", "numbers", "add_ten", ZomeCallback, new { number = 10 });

            //  await _holoNETClient.Disconnect();
        }
````

| Parameter          | Description                                        |
|--------------------|----------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.       |
| AgentPubKey        | The AgentPubKey for the hApp.                      |
| DnaHash            | The DnaHash for the hApp.                          |


#### OnDataReceived
Fired when any data is received from the Holochain conductor. This returns the raw data.  

````c#
holoNETClient.OnDataReceived += HoloNETClient_OnDataReceived;

private static void HoloNETClient_OnDataReceived(object sender, HoloNETDataReceivedEventArgs e)
        {
            if (!e.IsConductorDebugInfo)
            {
                Console.WriteLine(string.Concat("\nTEST HARNESS: DATA RECEIVED EVENT HANDLER: EndPoint: ", e.EndPoint, ", Raw JSON Data: ", e.RawJSONData, ", Raw Binary Data: ", e.RawBinaryData));
                Console.WriteLine("");
            }
        }
````

| Parameter                                     | Description                                                                                                                                                                                                                                                                                                                                             |
|-----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint                                      | The URI EndPoint of the Holochain conductor.                                                                                                                                                                                                                                                                                                            |
| Id                                            | The id that made the request.                                                                                                                                                                                                                                                                                                                           |                                                                                                                                                                                                                                                                                                                              |
| InstalledAppId                                | The InstalledAppId for the hApp.                                                                                                                                                                                                                                                                                                                        |
| RawBinaryData                                 | The raw binary data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                              |
| RawBinaryDataAsString                         | The raw binary data returned from the Holochain conductor formatted as a string (useful for debugging/logging etc).                                                                                                                                                                                                                                     |
| RawBinaryDataDecoded                          | The raw binary data returned from the Holochain conductor decoded into a string using UTF8 encoding.                                                                                                                                                                                                                                                    |
| RawBinaryDataAfterMessagePackDecode           | The raw binary data after it has been decoded by MessagePack.                                                                                                                                                                                                                                                                                           |
| RawBinaryDataAfterMessagePackDecodeAsString   | The raw binary data after it has been decoded by MessagePack formatted as a string (useful for debugging/logging etc).                                                                                                                                                                                                                                  |
| RawBinaryDataAfterMessagePackDecodeDecoded    | The raw binary data after it has been decoded by MessagePack decoded into a string using UTF8 encoding.                                                                                                                                                                                                                                                 |
| RawJSONData                                   | The raw JSON data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                                |
| WebSocketResult                               | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info. |

#### OnZomeFunctionCallBack

Fired when the Holochain conductor returns the response from a zome function call. This returns the raw binary & raw JSON data as well as the actual parsed data returned from the zome function. It also returns the id, zome and zome function that made the call.                      

````c#
holoNETClient.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;

 private static void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: ZOME FUNCTION CALLBACK EVENT HANDLER: ", ProcessZomeFunctionCallBackEventArgs(e)));
            Console.WriteLine("");
        }

private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";
            
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            if (args.Entry != null && args.Entry.EntryDataObject != null)
            {
                AvatarEntryDataObject avatar = args.Entry.EntryDataObject as AvatarEntryDataObject;

                if (avatar != null)
                    result = BuildEntryDataObjectMessage(avatar, "Entry.EntryDataObject", result);
            }
            
            if (_avatarEntryDataObject != null)
                result = BuildEntryDataObjectMessage(_avatarEntryDataObject, "Global.EntryDataObject", result);

            return result;
        }
````             

| Parameter                                     | Description                                                                                                                                                                                                                                                                                                                                             |
|-----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint                                      | The URI EndPoint of the Holochain conductor.                                                                                                                                                                                                                                                                                                            |
| Id                                            | The id that made the request.                                                                                                                                                                                                                                                                                                                           |                                                                                                                                                                                                                                                                                                                              |
| Zome                                          | The zome that made the request.                                                                                                                                                                                                                                                                                                                         |
| ZomeFunction                                  | The zome function that made the request.                                                                                                                                                                                                                                                                                                                |
| ZomeReturnData                                | The parsed data that the zome function returned. HoloNET will parse & convert the best it can from the Rust Holochain Conductor format to a C# friendly one such as converting from base64 encoding, etc.                                                                                                                                               |
| RawZomeReturnData                             | The raw binary data that the zome function returned.                                                                                                                                                                                                                                                                                                    |
| ZomeReturnHash                                | The ActionHash returned from a zome call (if it returned one, is null if not).                                                                                                                                                                                                                                                                          |
| KeyValuePair                                  | Contains all of the data returned from the zome call in a simple Dictionary keyvalue pair. The conductor for some reason returns a complex nested structure so is difficult and tedious to get to all the data needed or to quickly view all of it at once.                                                                                             |
| KeyValuePairAsString                          | Contains the same data from KeyValuePair but formatted as a simple string, which can be used for logging, displaying, etc.                                                                                                                                                                                                                              |
| Entry                                         | The entry dictionary containing the actual user data (after it has been processed/decoded) retrived from the zome call. This is the property that will be most valuable to the caller. This also now includes a EntryDataObject property containing a dynamic data object that is mapped to the dictionary using the optional type                      |
|                                               | (or actual data object) passed into the [CallZomeFunctionAsync](#callzomefunctionasync) method. This effectively maps the rust data struct properties contained in the hApp to your C# data object/class.                                                                                                                                               |
| RawBinaryData                                 | The raw binary data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                              |
| RawBinaryDataAsString                         | The raw binary data returned from the Holochain conductor formatted as a string (useful for debugging/logging etc).                                                                                                                                                                                                                                     |
| RawBinaryDataDecoded                          | The raw binary data returned from the Holochain conductor decoded into a string using UTF8 encoding.                                                                                                                                                                                                                                                    |
| RawBinaryDataAfterMessagePackDecode           | The raw binary data after it has been decoded by MessagePack.                                                                                                                                                                                                                                                                                           |
| RawBinaryDataAfterMessagePackDecodeAsString   | The raw binary data after it has been decoded by MessagePack formatted as a string (useful for debugging/logging etc).                                                                                                                                                                                                                                  |
| RawBinaryDataAfterMessagePackDecodeDecoded    | The raw binary data after it has been decoded by MessagePack decoded into a string using UTF8 encoding.                                                                                                                                                                                                                                                 |
| RawJSONData                                   | The raw JSON data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                                |
| WebSocketResult                               | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info. |

 The Entry property contains the following sub-properties:

 | Parameter            | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
 |----------------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
 | Bytes                | Contains the raw bytes returned from the Holochain Conductor for the data entry.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                    |
 | BytesString          | Contains the raw bytes returned from the Holochain Conductor for the data entry as a comma delimited string, which can be used for logging/debugging etc.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
 | Entry                | Contains the keyvalue pair dictionary for the entry data returned from the conductor.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
 | EntryDataObject      | If the type of the entry data object (entryDataObjectTypeReturnedFromZome param) or the actual data object (entryDataObjectReturnedFromZome param) is passed into one of the [CallZomeFunctionAsync](#callzomefunctionasync) overloads then HoloNET will attempt to map the hApp rust properties (contained in the struct) to the C# type/class/object passed in and then pass the newly created dynamic object (will update the existing data object if entryDataObjectReturnedFromZome param is used) back to the caller via this property. It will also update the actual data object (by ref) if entryDataObjectReturnedFromZome param is used. |


 As an example of how to access the entry data and see it mapped to your custom type/object check out the [HoloNET Test Harness](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/NextGenSoftware.Holochain.HoloNET.Client.TestHarness), especially this function below:

 ````c#

private static void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
        {
            bool disconect = false;
            Console.WriteLine(string.Concat("TEST HARNESS: ZOME FUNCTION CALLBACK EVENT HANDLER: ", ProcessZomeFunctionCallBackEventArgs(e)));
            Console.WriteLine("");
        }


private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";
            
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            if (args.Entry != null && args.Entry.EntryDataObject != null)
            {
                AvatarEntryDataObject avatar = args.Entry.EntryDataObject as AvatarEntryDataObject;

                if (avatar != null)
                    result = BuildEntryDataObjectMessage(avatar, "Entry.EntryDataObject", result);
            }
            
            if (_avatarEntryDataObject != null)
                result = BuildEntryDataObjectMessage(_avatarEntryDataObject, "Global.EntryDataObject", result);

            return result;
        }

        private static string BuildEntryDataObjectMessage(AvatarEntryDataObject avatar, string header, string message)
        {
            message = string.Concat(message, "\n\n", header, ".FirstName: ", avatar.FirstName);
            message = string.Concat(message, "\n", header, ".LastName: ", avatar.LastName);
            message = string.Concat(message, "\n", header, ".Email: ", avatar.Email);
            message = string.Concat(message, "\n", header, ".DOB: ", avatar.DOB);

            return message;
        }
 ````
 
 This is how you can map your rust data to a C# object, the call to the Holochain Conductor is in another earlier place in the Test Harness:

  ````c#
 _holoNETClient.CallZomeFunctionAsync("oasis", "get_entry_avatar", ZomeCallback, e.ZomeReturnHash, typeof(AvatarEntryDataObject));
  ````

  And the AvatarEntryDataObject class/type looks like this:

   ````c#
/// <summary>
    /// This example is to be used with the CallZomeFunction overloads on HoloNETClient that take either a type of the EntryDataObject (Type entryDataObjectTypeReturnedFromZome) to map the zome function returned data onto or the actual instance of a dynamic object (dynamic entryDataObjectReturnedFromZome) to map onto.
    /// </summary>
    public class AvatarEntryDataObject
    {
        /// <summary>
        /// GUID Id that is consistent across multiple versions of the entry (each version has a different hash).
        /// </summary>
        [HolochainPropertyName("id")]
        public Guid Id { get; set; }

        [HolochainPropertyName("first_name")]
        public string FirstName { get; set; }

        [HolochainPropertyName("last_name")]
        public string LastName { get; set; }

        [HolochainPropertyName("email")]
        public string Email { get; set; }

        [HolochainPropertyName("dob")]
        public DateTime DOB { get; set; }

        /// <summary>
        /// The date the entry was created.
        /// </summary>
        [HolochainPropertyName("created_date")]
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// The AgentId who created the entry.
        /// </summary>
        [HolochainPropertyName("created_by")]
        public string CreatedBy { get; set; }

        /// <summary>
        /// The date the entry was last modified.
        /// </summary>
        [HolochainPropertyName("modified_date")]
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// The AgentId who modifed the entry.
        /// </summary>
        [HolochainPropertyName("modified_by")]
        public string ModifiedBy { get; set; }

        /// <summary>
        /// The date the entry was soft deleted.
        /// </summary>
        [HolochainPropertyName("deleted_date")]
        public DateTime DeletedDate { get; set; }

        /// <summary>
        /// The AgentId who deleted the entry.
        /// </summary>
        [HolochainPropertyName("deleted_by")]
        public string DeletedBy { get; set; }

        /// <summary>
        /// Flag showing the whether this entry is active or not.
        /// </summary>
        [HolochainPropertyName("is_active")]
        public bool IsActive { get; set; }

        /// <summary>
        /// The current version of the entry.
        /// </summary>
        [HolochainPropertyName("version")]
        public int Version { get; set; } = 1;
    }
  ````

  The e.ZomeReturnHash is the hash returned from a previous call to the conductor:

````c#
  await _holoNETClient.CallZomeFunctionAsync("oasis", "create_entry_avatar", ZomeCallback, new { id = 1, first_name = "David", last_name = "Ellams", email = "davidellams@hotmail.com", dob = "11/07/1980" });
````

#### OnSignalsCallBack

Fired when the Holochain conductor sends signals data.

````c#
holoNETClient.OnSignalsCallBack += HoloNETClient_OnSignalsCallBack;

private static void HoloNETClient_OnSignalsCallBack(object sender, SignalsCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: SIGINALS CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id, ", Data: ", e.RawJSONData, ", AgentPubKey =  ", e.AgentPubKey, ", DnaHash = ", e.DnaHash, ", Signal Type: ", Enum.GetName(typeof(SignalType), e.SignalType), ", Signal Data: ", e.SignalDataAsString));
            Console.WriteLine("");
        }
````   

 | Parameter                                     | Description                                                                                                                                                                                                                                                                                                                                             |
 |-----------------------------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
 | EndPoint                                      | The URI EndPoint of the Holochain conductor.                                                                                                                                                                                                                                                                                                            |
 | Id                                            | The id that made the request.                                                                                                                                                                                                                                                                                                                           |                                                                                                                                                                                                                                                                                                                              |
 | AgentPubKey                                   | The Agent Public Key of the hApp that is running in the Holochain Conductor.                                                                                                                                                                                                                                                                            |
 | DnaHash                                       | The DNA Hash of the hApp that is running in the Holochain Conductor.                                                                                                                                                                                                                                                                                    |
 | SignalType                                    | An enum containing the SignalType, can be either App or System.                                                                                                                                                                                                                                                                                         |
 | SignalData                                    | The Signal Data decoded into a dictionary with keyvalue pairs.                                                                                                                                                                                                                                                                                          |
 | SignalDataAsString                            | The Signal Data decoded into a string with keyvalue pairs.                                                                                                                                                                                                                                                                                              |
 | RawSignalData                                 | The Raw Signal Data returned from the conductor decoded into a HoloNETSignalData object containing a CellData (contains a 2 dimensonal array containing the AgentPubKey & DnaHash) & SignalData binary array.                                                                                                                                           |
 | RawBinaryData                                 | The raw binary data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                              |
 | RawBinaryDataAsString                         | The raw binary data returned from the Holochain conductor formatted as a string (useful for debugging/logging etc).                                                                                                                                                                                                                                     |
 | RawBinaryDataDecoded                          | The raw binary data returned from the Holochain conductor decoded into a string using UTF8 encoding.                                                                                                                                                                                                                                                    |
 | RawBinaryDataAfterMessagePackDecode           | The raw binary data after it has been decoded by MessagePack.                                                                                                                                                                                                                                                                                           |
 | RawBinaryDataAfterMessagePackDecodeAsString   | The raw binary data after it has been decoded by MessagePack formatted as a string (useful for debugging/logging etc).                                                                                                                                                                                                                                  |
 | RawBinaryDataAfterMessagePackDecodeDecoded    | The raw binary data after it has been decoded by MessagePack decoded into a string using UTF8 encoding.                                                                                                                                                                                                                                                 |
 | RawJSONData                                   | The raw JSON data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                                |
 | WebSocketResult                               | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info. |


 #### OnConductorDebugCallBack

Fired when the Holochain conductor sends debug info.

````c#
private static void HoloNETClient_OnConductorDebugCallBack(object sender, ConductorDebugCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("OnConductorDebugCallBack: EndPoint: ", e.EndPoint, ", Data: ", e.RawJSONData, ", NumberDelayedValidations: ", e.NumberDelayedValidations, ", NumberHeldAspects: ", e.NumberHeldAspects, ", NumberHeldEntries: ", e.NumberHeldEntries, ", NumberPendingValidations: ", e.NumberPendingValidations, ", NumberRunningZomeCalls: ", e.NumberRunningZomeCalls, ", Offline: ", e.Offline, ", Type: ", e.Type));
            Console.WriteLine("");
        }
````   

 | Parameter                | Description                                                                                                                                                                                                 |
 |--------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
 | EndPoint                 | The URI EndPoint of the Holochain conductor.                                                                                                                                                                | 
 | NumberDelayedValidations | The number of delayed validations.                                                                                                                                                                          |
 | NumberHeldAspects        | The number of held aspects.                                                                                                                                                                                 |
 | NumberHeldEntries        | The number of held entries.                                                                                                                                                                                 |
 | NumberPendingValidations | The number of pending validations.                                                                                                                                                                          |
 | NumberRunningZomeCalls   | The number of running zome calls.                                                                                                                                                                                                                                                                                                                     |
 | Offline                  | Whether offline or not.                                                                                                                                                                                                                                                                                                                                 |
 | Type                     | Type of conductor running.                                                                                                                                                                                                                                                                                                                              |
 | RawBinaryData            | The raw binary data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                              |
 | RawJSONData              | The raw JSON data returned from the Holochain conductor.                                                                                                                                                                                                                                                                                                |
 | WebSocketResult          | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info. |
 | IsError                  | True if there was an error during the initialization, false if not.                                                                                                                                                                                                                                                                                     |
 | Message                  | If there was an error this will contain the error message, this normally includes a stacktrace to help you track down the cause. If there was no error it can contain any other message such as status etc or will be blank.                                                                                                                            |


 **NOTE: This is from the previous version of HoloNET running against the previous version of Holochain (Redux) & needs to be updated for the new RSM version, coming soon...**


#### OnDisconnected
Fired when the client has successfully disconnected from the Holochain conductor. 

````c#
holoNETClient.OnDisconnected += HoloNETClient_OnDisconnected;

private static void HoloNETClient_OnDisconnected(object sender, DisconnectedEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: DISCONNECTED CALL BACK: Disconnected from ", e.EndPoint, ". Resason: ", e.Reason));
            Console.WriteLine("");
        }
````

| Parameter          | Description                                        |
|--------------------|----------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.       |
| Reason             | The reason for the disconnection.                  |


#### OnError

Fired when an error occurs, check the params for the cause of the error.       

````c#
holoNETClient.OnError += HoloNETClient_OnError;

private static void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: ERROR EVENT HANDLER: Error Occured. Resason: ", e.Reason, ", EndPoint: ", e.EndPoint, ",Error Details: ", e.ErrorDetails));
            Console.WriteLine("");
        }
````

| Parameter          | Description                                                                                                     |
|--------------------|-----------------------------------------------------------------------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.                                                                    |
| Reason             | The reason for the error.                                                                                       |
| ErrorDetails       | A more detailed description of the error, this normally includes a stacktrace to help you track down the cause. |

#### OnHolochainConductorsShutdownComplete

Fired when all of the Holochain Conductors have shutdown.

````c#
_holoNETClient.OnHolochainConductorsShutdownComplete += _holoNETClient_OnHolochainConductorsShutdownComplete;

private static void _holoNETClient_OnHolochainConductorsShutdownComplete(object sender, HolochainConductorsShutdownEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: OnHolochainConductorsShutdownComplete, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}, NumberOfHcExeInstancesShutdown: { e.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: {e.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: {e.NumberOfRustcExeInstancesShutdown}.");
        }
````

| Parameter                             | Description                                                                                                                                                                                                                  |
|---------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint                              | The URI EndPoint of the Holochain conductor.                                                                                                                                                                                 |
| AgentPubKey                           | The Agent Public Key of the hApp that was running in the Holochain Conductor.                                                                                                                                                |
| DnaHash                               | The DNA Hash of the hApp that was running in the Holochain Conductor.                                                                                                                                                        |
| NumberOfHcExeInstancesShutdown        | The number of hc.exe instances that were shutdown.                                                                                                                                                                           |
| NumberOfHolochainExeInstancesShutdown | The number of holochain.exe instances that were shutdown.                                                                                                                                                                    |
| NumberOfRustcExeInstancesShutdown     | The number of rustc.exe instances that were shutdown.                                                                                                                                                                        |
| IsError                               | True if there was an error during the initialization, false if not.                                                                                                                                                          |
| Message                               | If there was an error this will contain the error message, this normally includes a stacktrace to help you track down the cause. If there was no error it can contain any other message such as status etc or will be blank. |


#### OnHoloNETShutdownComplete

Fired when all of the Holochain Conductors have shutdown.

````c#
_holoNETClient.OnHoloNETShutdownComplete += _holoNETClient_OnHoloNETShutdownComplete;

private static void _holoNETClient_OnHoloNETShutdownComplete(object sender, HoloNETShutdownEventArgs e)
        {
            string msg = $"TEST HARNESS: OnHoloNETShutdownComplete, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}";
            
            if (e.HolochainConductorsShutdownEventArgs != null)
                msg = string.Concat(msg, $", NumberOfHcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: { e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: { e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}");

            CLIEngine.ShowMessage(msg);
        }
````

| Parameter                                                                  | Description                                                                                                                                                                                                                  |
|----------------------------------------------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint                                                                   | The URI EndPoint of the Holochain conductor.                                                                                                                                                                                 |
| AgentPubKey                                                                | The Agent Public Key of the hApp that was running in the Holochain Conductor.                                                                                                                                                |
| DnaHash                                                                    | The DNA Hash of the hApp that was running in the Holochain Conductor.                                                                                                                                                        |
| HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown        | The number of hc.exe instances that were shutdown.                                                                                                                                                                           |
| HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown | The number of holochain.exe instances that were shutdown.                                                                                                                                                                    |
| HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown     | The number of rustc.exe instances that were shutdown.                                                                                                                                                                        |
| IsError                                                                    | True if there was an error during the initialization, false if not.                                                                                                                                                          |
| Message                                                                    | If there was an error this will contain the error message, this normally includes a stacktrace to help you track down the cause. If there was no error it can contain any other message such as status etc or will be blank. |


### Methods

HoloNETClient contains the following methods:

|Method                                                                                    |Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                     |
|------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
|[Connect](#connect)                                                                       | This method simply connects to the Holochain conductor. It raises the [OnConnected](#onconnected) event once it is has successfully established a connection. It then calls the [RetreiveAgentPubKeyAndDnaHash](#RetreiveAgentPubKeyAndDnaHash) method to retrive the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediatley and then call the [OnConnected](#onconnected) event once it has finished connecting.                                                                                                                                      |
|[StartConductor](#startconductor)                                                         | This method will start the Holochain Conducutor using the approprtiate settings defined in the [HoloNETConfig](#holonetconfig).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
|[RetreiveAgentPubKeyAndDnaHash](#RetreiveAgentPubKeyAndDnaHash)                           | This method will retrive the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retreiving from the Conductor first. It will call [RetreiveAgentPubKeyAndDnaHashFromConductor](#retreiveagentpubkeyanddnahashfromconductor) and [RetreiveAgentPubKeyAndDnaHashFromConductor](#retreiveagentpubkeyanddnahashfromconductor) internally.                                                                                                                                                                                                                                                                                          |
|[RetreiveAgentPubKeyAndDnaHashFromSandbox](#retreiveagentpubkeyanddnahashfromsandbox)     | This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retreives them and the WebSocket has connected to the Holochain Conductor. If it fails to retreive the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToGetFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the |[GetAgentPubKeyAndDnaHashFromConductor](#getagentpubkeyanddnahashfromconductor) method to attempt to retreive them directly from the conductor (default).                                                                                                       |
|[RetreiveAgentPubKeyAndDnaHashFromConductor](#retreiveagentpubkeyanddnahashfromconductor) | This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the [Connect](#connect) method will automatically call this by default). Once it has retreived them and the WebSocket has connceted to the Holochain Conductor it will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event. If it fails to retreive the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToGetFromSandboxIfConductorFails` flag is true (defaults to true), it will call the |[GetAgentPubKeyAndDnaHashFromSandbox](#getagentpubkeyanddnahashfromsandbox) method.                                                                                                             |
|[SendHoloNETRequest](#sendholonetrequest)                                                 | This method allows you to send your own raw request to holochain. This method raises the [OnDataReceived](#ondatareceived) event once it has received a response from the Holochain conductor. Please see the [Events](#events) section above for more info on how to use this event. You would rarely need to use this and we highly recommend you use the [CallZomeFunctionAsync](#callzomefunctionasync) method instead.                                                                                                                                                                                                                                                                                    |
|[CallZomeFunction](#callzomefunction)                                                     | This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the [OnZomeFunctionCallBack](#onzomefunctioncallback) event once it has received a response from the Holochain conductor.                                                                                                                                                                                                                                                                                                                                            |
|[Disconnect](#disconnect)                                                                 | This method disconnects the client from the Holochain conductor. It raises the [OnDisconnected](#ondisconnected) event once it is has successfully disconnected. It will then automatically call the [ShutDownHolochainConductors](#ShutDownHolochainConductors) method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.       |
|[ShutDownHolochainConductors](#ShutDownHolochainConductors)                               | Will automatically shutdown the current Holochain Conductor (if the `shutdownHolochainConductorsMode` param is set to `ShutdownCurrentConductorOnly`) or active Holochain Conductors (if the `shutdownHolochainConductorsMode` param is set to `ShutdownAllConductors`). If the `shutdownHolochainConductorsMode` param is set to `UseConfigSettings` then it will use the `HoloNETClient.Config.AutoShutdownHolochainConductor` and `HoloNETClient.Config.ShutDownALLHolochainConductors` flags to determine which mode to use. The [Disconnect](#disconnect) method will automatically call this once it has finished disconnecting from the Holochain Conductor.                                            |     
|[ShutdownHoloNET](#ShutdownHoloNET)                                                       | This method will shutdown HoloNET by first calling the [Disconnect](#disconnect) method to disconnect from the Holochain Conductor and then calling the [ShutDownHolochainConductors](#ShutDownHolochainConductors) method to shutdown any running Holochain Conductors. This method will then raise the [OnHoloNETShutdown](OnHoloNETShutdownComplete) event.                                                                                                                                                                                                                                                                                                                                                 |
|[ClearCache](#clearcache)                                                                 | Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the [CallZomeFunction](#callzomefunction) methods if the `cacheData` parm was set to true for any of the calls.                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
|[ConvertHoloHashToBytes](#ConvertHoloHashToBytes)                                         | Utiltity method to convert a string to base64 encoded bytes (Holochain Conductor format). This is used to convert the AgentPubKey & DnaHash when making a zome call.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
|[ConvertHoloHashToString](#ConvertHoloHashToString)                                       | Utiltity method to convert from base64 bytes (Holochain Conductor format) to a friendly C# format. This is used to convert the AgentPubKey & DnaHash retreived from the Conductor.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
|[WaitTillReadyForZomeCallsAsync](WaitTillReadyForZomeCallsAsync)                          | This method will wait (non blocking) until HoloNET is ready to make zome calls after it has connected to the Holochain Conductor and retrived the AgentPubKey & DnaHash. It will then return to the caller with the AgentPubKey & DnaHash. This method will return the same time the [OnReadyForZomeCalls](#OnReadyForZomeCalls) event is raised. Unlike all the other methods, this one only contains an async version because the non async version would block all other threads including any UI ones etc.                                                                                                                                                                                                 |
|[MapEntryDataObject](#MapEntryDataObject)                                                 | This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the [CallZomeFunction](#callzomefunction) method. Alternatively the type of the data object can be passed in. Either way the now mapped and populated data object is then returned in the EntryDataObject property during the [OnZomeFunctionCallBack](OnZomeFunctionCallBack) event. Please see [OnZomeFunctionCallBack](OnZomeFunctionCallBack) for more info. This method is called internally but can also be called manually and is used by the [HoloNETEntryBaseClass](#HoloNETEntryBaseClass).                                                                                                   |

**NOTE:** All methods contain both a non async version and async version to cater for all use case scenarios.

#### Connect

This method simply connects to the Holochain conductor. It raises the [OnConnected](#onconnected) event once it is has successfully established a connection. It then calls the [RetreiveAgentPubKeyAndDnaHash](#RetreiveAgentPubKeyAndDnaHash) method to retrive the AgentPubKey & DnaHash. If the `connectedCallBackMode` flag is set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediatley and then call the [OnConnected](#onconnected) event once it has finished connecting. Please see the [Events](#events) section above for more info on how to use this event.

```c#
public async Task Connect(ConnectedCallBackMode connectedCallBackMode = ConnectedCallBackMode.WaitForHolochainConductorToConnect, RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = true)
```
| Parameter                                            | Description                                                                                           
| ---------------------------------------------------- | -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| connectedCallBackMode                                | If set to `WaitForHolochainConductorToConnect` (default) it will await until it is connected before returning, otherwise it will return immediatley and then call the [OnConnected](#onconnected) event once it has finished connecting.                                                 |
| retreiveAgentPubKeyAndDnaHashMode                    | If set to `Wait` (default) it will await until it has finished retreiving the AgentPubKey & DnaHash before returning, otherwise it will return immediatley and then call the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it has finished retreiving the DnaHash & AgentPubKey.|
| getAgentPubKeyAndDnaHashFromConductor                | Set this to true for HoloNET to automatically retreive the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.                                                                                                                             |
| getAgentPubKeyAndDnaHashFromSandbox                  | Set this to true if you wish HoloNET to automatically retreive the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.                                                                                                                              |
| automaticallyAttemptToGetFromConductorIfSandBoxFails | If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.                                                                                           |   
| automaticallyAttemptToGetFromSandBoxIfConductorFails | If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.                                                                                           |
| updateConfig                                         | Set this to true (default) to automatically update the [HoloNETConfig](#holonetconfig) once it has retreived the DnaHash & AgentPubKey.                                                                                                                                                  |

**NOTE: If both params are set to true it will first attempt to retreive the AgentPubKey & DnaHash from the Conductor, if that fails it will then attempt to retreive them from the hc sandbox command (it will still do this even if getAgentPubKeyAndDnaHashFromSandbox is set to false).**


#### StartConductor

This method will start the Holochain Conducutor using the approprtiate settings defined in the [HoloNETConfig](#holonetconfig).

```c#
public async Task StartConductor()
```

#### RetreiveAgentPubKeyAndDnaHash

This method will retrive the AgentPubKey & DnaHash from either the Holochain Conductor or HC Sandbox depending on what params are passed in. It will default to retreiving from the Conductor first. It will call [RetreiveAgentPubKeyAndDnaHashFromConductor](#retreiveagentpubkeyanddnahashfromconductor) and [RetreiveAgentPubKeyAndDnaHashFromConductor](#retreiveagentpubkeyanddnahashfromconductor) internally.                                                                                                                                                                                                                                                            

```c#
public async Task<AgentPubKeyDnaHash> RetreiveAgentPubKeyAndDnaHashAsync(RetreiveAgentPubKeyAndDnaHashMode retreiveAgentPubKeyAndDnaHashMode = RetreiveAgentPubKeyAndDnaHashMode.Wait, bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = true, bool automaticallyAttemptToGetFromConductorIfSandBoxFails = true, bool automaticallyAttemptToGetFromSandBoxIfConductorFails = true, bool updateConfig = true)
```
| Parameter                                            | Description                                                                                           
| ---------------------------------------------------- | -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| retreiveAgentPubKeyAndDnaHashMode                    | If set to `Wait` (default) it will await until it has finished retreiving the AgentPubKey & DnaHash before returning, otherwise it will return immediatley and then call the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it has finished retreiving the DnaHash & AgentPubKey.|
| getAgentPubKeyAndDnaHashFromConductor                | Set this to true for HoloNET to automatically retreive the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true.                                                                                                                             |
| getAgentPubKeyAndDnaHashFromSandbox                  | Set this to true if you wish HoloNET to automatically retreive the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.                                                                                                                              |
| automaticallyAttemptToGetFromConductorIfSandBoxFails | If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.                                                                                           |   
| automaticallyAttemptToGetFromSandBoxIfConductorFails | If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.                                                                                           |
| updateConfig                                         | Set this to true (default) to automatically update the [HoloNETConfig](#holonetconfig) once it has retreived the DnaHash & AgentPubKey.                                                                                                                                                  |

**NOTE: If both params are set to true it will first attempt to retreive the AgentPubKey & DnaHash from the Conductor, if that fails it will then attempt to retreive them from the hc sandbox command (it will still do this even if getAgentPubKeyAndDnaHashFromSandbox is set to false).**



#### RetreiveAgentPubKeyAndDnaHashFromSandbox

This method gets the AgentPubKey & DnaHash from the HC Sandbox command. It will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event once it successfully retreives them and the WebSocket has connected to the Holochain Conductor. If it fails to retreive the AgentPubKey and DnaHash from the HC Sandbox and the optional `automaticallyAttemptToGetFromConductorIfSandBoxFails` flag is true (defaults to true), it will call the |[GetAgentPubKeyAndDnaHashFromConductor](#getagentpubkeyanddnahashfromconductor) method to attempt to retreive them directly from the conductor (default).

```c#
public async Task<AgentPubKeyDnaHash> RetreiveAgentPubKeyAndDnaHashFromSandboxAsync(bool updateConfig = true, bool automaticallyAttemptToGetFromConductorIfSandBoxFails = true)
```

| Parameter                                            | Description                                                                                           
| ---------------------------------------------------- | -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| automaticallyAttemptToGetFromConductorIfSandBoxFails | If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the Holochain Conductor if it fails to get them from the HC Sandbox command. This defaults to true.                                                                                           |   
| updateConfig                                         | Set this to true (default) to automatically update the [HoloNETConfig](#holonetconfig) once it has retreived the DnaHash & AgentPubKey.                                                                                                                                                  |


#### RetreiveAgentPubKeyAndDnaHashFromConductor

This method gets the AgentPubKey & DnaHash from the Holochain Conductor (the [Connect](#connect) method will automatically call this by default). Once it has retreived them and the WebSocket has connceted to the Holochain Conductor it will raise the [OnReadyForZomeCalls](#onreadyforzomecalls) event. If it fails to retreive the AgentPubKey and DnaHash from the Conductor and the optional `automaticallyAttemptToGetFromSandboxIfConductorFails` flag is true (defaults to true), it will call the |[GetAgentPubKeyAndDnaHashFromSandbox](#getagentpubkeyanddnahashfromsandbox) method. 

```c#
 public async Task GetAgentPubKeyAndDnaHashFromConductor(bool updateConfig = true)
```

| Parameter                                            | Description                                                                                           
| ---------------------------------------------------- | -----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| automaticallyAttemptToGetFromSandBoxIfConductorFails | If this is set to true it will automatically attempt to get the AgentPubKey & DnaHash from the HC Sandbox command if it fails to get them from the Holochain Conductor. This defaults to true.                                                                                           |
| updateConfig                                         | Set this to true (default) to automatically update the [HoloNETConfig](#holonetconfig) once it has retreived the DnaHash & AgentPubKey.                                                                                                                                                  |


#### SendHoloNETRequest

This method allows you to send your own raw request to holochain. This method raises the [OnDataRecived](#ondatareceived) event once it has received a response from the Holochain conductor. Please see the [Events](#events) section above for more info on how to use this event.

You would rarely need to use this and we highly recommend you use the [CallZomeFunctionAsync](#callzomefunctionasync) method instead.

````c#
public async Task SendHoloNETRequest(string id, HoloNETData holoNETData)
 ````

| Paramameter | Description                                                       |
|-------------|-------------------------------------------------------------------|
| holoNETData | The raw data packet you wish to send to the Holochain conductor.  |



#### CallZomeFunctionAsync

This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data.

This method raises the [OnZomeFunctionCallBack](#onzomefunctioncallback) event once it has received a response from the Holochain conductor. Please see the [Events](#events) section above for more info on how to use this event.

##### Overload 1

````c#
public async Task CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
````

| Parameter                           | Description                                                                                    
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| id                                  | The unique id you wish to assign for this call (NOTE: There is an overload that omits this param, use this overload if you wish HoloNET to auto-generate and manage the id's for you).                                                                                                                                                                                                                                                                                                                                                                                                |
| zome                                | The name of the zome you wish to target.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| function                            | The name of the zome function you wish to call.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| delegate                            | A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event.                                                                                                                                                                                                                                                                                                                                                                                                                                    |
| paramsObject                        | A basic CLR object containing the params the zome function is expecting.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| matchIdToInstanceZomeFuncInCallback | This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the instance, zome  zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself.  |
| cachReturnData                      | This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.                                                                                                                                                                                                                                                                 |                                                     
| entryDataObjectTypeReturnedFromZome | This is an optional param, where the caller can pass in the type of the dynamic data object they wish the entry data returned to be mapped to.                                                                                                                                                                                                                                                                                                                                                                                                                                        |

**NOTE** - If you pass in the entryDataObjectTypeReturnedFromZome type then you need to make sure on that type defintion (class) you specify the Holochain Property Names defined in the hApp rust code as shown in the follow example:

**NOTE** - You can alternatively use other overloads below that take a optional param of entryDataObjectReturnedFromZome instead of entryDataObjectTypeReturnedFromZome. You then pass in an intance of the object you wish to have the properties mapped to the data returned from the zome call.

````c#
public class Avatar
    {
        [HolochainPropertyName("first_name")]
        public string FirstName { get; set; }

        [HolochainPropertyName("last_name")]
        public string LastName { get; set; }

        [HolochainPropertyName("email")]
        public string Email { get; set; }

        [HolochainPropertyName("dob")]
        public DateTime DOB { get; set; }
    }
 ````

 This would then be used like this:

 ````c#
 _holoNETClient.CallZomeFunctionAsync("oasis", "get_entry_avatar", ZomeCallback, e.ZomeReturnHash, true, false, typeof(Avatar));
  ````

Where e.ZomeReturnHash is the hash of the entry to load. This is part of the example used in the [HoloNET Test Harness](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/NextGenSoftware.Holochain.HoloNET.Client.TestHarness), and the e.ZomeReturnHash is the hash returned from the previous call to CallZomeFunctionAsync where the zome function "create_oasis_entry" was called.

Please see the [OnZomeFunctionCallBack](#onzomefunctioncallback) event for more info on how to then map the data returned to your custom class.

#####  Overload 2

````c#
public async Task CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
````

This overload is similar to the one above except you pass in the actual dynamic data object that will be populated/updated with the results from the zome function response using the entryDataObjectReturnedFromZome paramerter.

#####  Overload 3

````c#
 public async Task CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

This overload is similar to the top one except it omits the `id` and `matchIdToInstanceZomeFuncInCallback` param's forcing HoloNET to auto-generate and manage the id's itself. 

#####  Overload 4

````c#
 public async Task CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

This overload is similar to overload 2 except it omits the `id` and `matchIdToInstanceZomeFuncInCallback` param's forcing HoloNET to auto-generate and manage the id's itself. 


##### Overload 5

````c#
public async Task CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

This overload is similar to the first one, except it is missing the `callback` param. For this overload you would subscribe to the `OnZomeFunctionCallBack` event. You can of course subscribe to this event for the other overloads too, it just means you will then get two callbacks, one for the event handler for `OnZomeFunctionalCallBack` and one for the callback delegate you pass in as a param to this method. The choice is yours on how you wish to use this method...


##### Overload 6

````c#
public async Task CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

This overload is similar to the one above, except you pass in the actual dynamic data object that will be populated/updated with the results from the zome function response using the entryDataObjectReturnedFromZome paramerter.


##### Overload 7

````c#
public async Task CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

This overload is similar to the one above except it omits the `id` and `matchIdToInstanceZomeFuncInCallback` param's forcing HoloNET to auto-generate and manage the id's itself. It is also missing the `callback` param. For this overload you would subscribe to the `OnZomeFunctionCallBack` event. You can of course subscribe to this event for the other overloads too, it just means you will then get two callbacks, one for the event handler for `OnZomeFunctionalCallBack` and one for the callback delegate you pass in as a param to this method. The choice is yours on how you wish to use this method...


##### Overload 8

````c#
public async Task CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

This overload is similar to the one above, except you pass in the actual dynamic data object that will be populated/updated with the results from the zome function response using the entryDataObjectReturnedFromZome paramerter.


##### Overload 9

````c#
public async Task CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

##### Overload 10

````c#
public async Task CallZomeFunctionAsync(string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

##### Overload 11

````c#
public async Task CallZomeFunctionAsync(string zome, string function, object paramsObject, dynamic entryDataObjectReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

 This overload is similar to the one above, except you pass in the actual dynamic data object that will be populated/updated with the results from the zome function response using the entryDataObjectReturnedFromZome paramerter.

##### Overload 12

````c#
public async Task CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, Type entryDataObjectTypeReturnedFromZome = null, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

##### Overload 13

````c#
public async Task CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

##### Overload 14

````c#
public async Task CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

##### Overload 15

````c#
public async Task CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
 ````

##### Overload 16

 ````c#
 public async Task CallZomeFunctionAsync(string zome, string function, object paramsObject, ZomeResultCallBackMode zomeResultCallBackMode = ZomeResultCallBackMode.WaitForHolochainConductorResponse)
  ````

##### Overload 17

 ````c#
 public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, object paramsObject)
  ````

##### Overload 18

````c#
public async Task<ZomeFunctionCallBackEventArgs> CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject)
````

#### Disconnect

This method disconnects the client from the Holochain conductor. It raises the [OnDisconnected](#ondisconnected) event once it is has successfully disconnected. It will then automatically call the [ShutDownAllHolochainConductors](#ShutDownAllHolochainConductors) method (if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`). If the `disconnectedCallBackMode` flag is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected. Please see the [Events](#events) section above for more info on how to use this event.

```c#
public async Task Disconnect(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
```

| Parameter                           | Description                                                                                    
| ----------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| disconnectedCallBackMode            | If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.                                                                                                                                                                                                                                                                                |
| shutdownHolochainConductorsMode     | Once it has successfully disconneced it will automatically call the [ShutDownAllHolochainConductors](#ShutDownAllHolochainConductors) method if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the [ShutDownConductors](#ShutDownConductors) method below for more detail.                                                                                                                                 |                                                                                                                                                                                                                                                                                                                                                            


#### ShutDownHolochainConductors

Will automatically shutdown the current Holochain Conductor (if the `shutdownHolochainConductorsMode` param is set to `ShutdownCurrentConductorOnly`) or all Holochain Conductors (if the `shutdownHolochainConductorsMode` param is set to `ShutdownAllConductors`). If the `shutdownHolochainConductorsMode` param is set to `UseConfigSettings` then it will use the `HoloNETClient.Config.AutoShutdownHolochainConductor` and `HoloNETClient.Config.ShutDownALLHolochainConductors` flags to determine which mode to use. The [Disconnect](#disconnect) method will automatically call this once it has finished disconnecting from the Holochain Conductor.

```c#
public async Task<HolochainConductorsShutdownEventArgs> ShutDownHolochainConductorsAsync(ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
```

| Parameter                           | Description                                                                                    
| ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| shutdownHolochainConductorsMode     | If this flag is set to `ShutdownCurrentConductorOnly` it will shutdown the currently running Holochain Conductor only. If it is set to `ShutdownAllConductors` it will shutdown all running Holochain Condcutors. If it is set to `UseConfigSettings` (default) then it will use the `HoloNETClient.Config.AutoShutdownHolochainConductor` and `HoloNETClient.Config.ShutDownALLHolochainConductors` flags to determine which mode to use. |


#### ShutdownHoloNET

This method will shutdown HoloNET by first calling the [Disconnect](#disconnect) method to disconnect from the Holochain Conductor and then calling the [ShutDownHolochainConductors](#ShutDownHolochainConductors) method to shutdown any running Holochain Conductors. This method will then raise the [OnHoloNETShutdown](OnHoloNETShutdownComplete) event. This method works very similar to the [Disconnect](Disconnect) method except it also clears the loggers, does any other shutdown tasks necessary and then returns a `HoloNETShutdownEventArgs` object.

```c#
public async Task<HoloNETShutdownEventArgs> ShutdownHoloNETAsync(DisconnectedCallBackMode disconnectedCallBackMode = DisconnectedCallBackMode.WaitForHolochainConductorToDisconnect, ShutdownHolochainConductorsMode shutdownHolochainConductorsMode = ShutdownHolochainConductorsMode.UseConfigSettings)
```

| Parameter                           | Description                                                                                    
| ----------------------------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| disconnectedCallBackMode            | If this is set to `WaitForHolochainConductorToDisconnect` (default) then it will await until it has disconnected before returning to the caller, otherwise (it is set to `UseCallBackEvents`) it will return immediately and then raise the [OnDisconnected](#ondisconnected) once it is disconnected.                                                                                                                                                                                                                                                                                |
| shutdownHolochainConductorsMode     | Once it has successfully disconneced it will automatically call the [ShutDownAllHolochainConductors](#ShutDownAllHolochainConductors) method if the `shutdownHolochainConductorsMode` flag (defaults to `UseConfigSettings`) is not set to `DoNotShutdownAnyConductors`. Other values it can be are 'ShutdownCurrentConductorOnly' or 'ShutdownAllConductors'. Please see the [ShutDownConductors](#ShutDownConductors) method below for more detail.                                                                                                                                 |                                                                                                                                                                                                                                                                                                                                                            


#### ClearCache

Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the [CallZomeFunction](#callzomefunction) methods if the `cacheData` parm was set to true for any of the calls.

````c#
public void ClearCache()
````


#### ConvertHoloHashToBytes

Utiltity method to convert a string to base64 encoded bytes (Holochain Conductor format). This is used to convert the AgentPubKey & DnaHash when making a zome call.|

````c#
public byte[] ConvertHoloHashToBytes(string hash)
````

#### ConvertHoloHashToString

Utiltity method to convert from base64 bytes (Holochain Conductor format) to a friendly C# format. This is used to convert the AgentPubKey & DnaHash retreived from the Conductor.|

````c#
public string ConvertHoloHashToString(byte[] bytes)
````

#### WaitTillReadyForZomeCallsAsync

This method will wait (non blocking) until HoloNET is ready to make zome calls after it has connected to the Holochain Conductor and retrived the AgentPubKey & DnaHash. It will then return to the caller with the AgentPubKey & DnaHash. This method will return the same time the [OnReadyForZomeCalls](#OnReadyForZomeCalls) event is raised. Unlike all the other methods, this one only contains an async version because the non async version would block all other threads including any UI ones etc.

````c#
public async Task<ReadyForZomeCallsEventArgs> WaitTillReadyForZomeCallsAsync()
````

#### MapEntryDataObject

This method maps the data returned from the Conductor zome call onto a dynamic data object passed into the [CallZomeFunction](#callzomefunction) method. Alternatively the type of the data object can be passed in. Either way the now mapped and populated data object is then returned in the `EntryDataObject` property during the [OnZomeFunctionCallBack](#OnZomeFunctionCallBack) event. Please see [OnZomeFunctionCallBack](#OnZomeFunctionCallBack) for more info. This method is called internally but can also be called manually and is used by the [HoloNETEntryBaseClass](#HoloNETEntryBaseClass).   

````c#
public dynamic MapEntryDataObject(Type entryDataObjectType, Dictionary<string, string> keyValuePairs)
public async Task<dynamic> MapEntryDataObjectAsync(Type entryDataObjectType, Dictionary<string, string> keyValuePairs)
public dynamic MapEntryDataObject(dynamic entryDataObject, Dictionary<string, string> keyValuePairs)
public async Task<dynamic> MapEntryDataObjectAsync(dynamic entryDataObject, Dictionary<string, string> keyValuePairs)
````

### Properties

HoloNETClient contains the following properties:

| Property                                                            | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
|---------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| [Config](#config)                                                   | This property contains a struct called `HoloNETConfig` containing the sub-properties: AgentPubKey, DnaHash, FullPathToRootHappFolder, FullPathToCompiledHappFolder, HolochainConductorMode, FullPathToExternalHolochainConductorBinary, FullPathToExternalHCToolBinary, SecondsToWaitForHolochainConductorToStart, AutoStartHolochainConductor, ShowHolochainConductorWindow, AutoShutdownHolochainConductor, ShutDownALLHolochainConductors, HolochainConductorToUse, OnlyAllowOneHolochainConductorToRunAtATime, LoggingMode & ErrorHandlingBehaviour. |
| [WebSocket](#websocket)                                             | This property contains the internal [NextGenSoftware WebSocket](https://www.nuget.org/packages/NextGenSoftware.WebSocket) that HoloNET uses. You can use this property to access the current state of the WebSocket as well as configure more options.                                                                                                                                                                                                                                                                                                   |
| [State](#state)                                                     | This property is a shortcut to the State property on the [WebSocket](#websocket) property above.                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| [EndPoint](#endpoint)                                               | This property is a shortcut to the EndPoint property on the [WebSocket](#websocket) property above.                                                                                                                                                                                                                                                                                                                                                                                                                                                      |
| [IsReadyForZomesCalls](#IsReadyForZomesCalls)                       | This property is a boolean and will return true when HoloNET is ready for zome calls after the [OnReadyForZomeCalls](OnReadyForZomeCalls) event is raised.                                                                                                                                                                                                                                                                                                                                                                                               |
| [RetreivingAgentPubKeyAndDnaHash](#RetreivingAgentPubKeyAndDnaHash) | This property is a boolean and will return true when HoloNET is retreiving the AgentPubKey & DnaHash using one of the [RetreiveAgentPubKeyAndDnaHash](#RetreiveAgentPubKeyAndDnaHash), [RetreiveAgentPubKeyAndDnaHashFromSandbox](#retreiveagentpubkeyanddnahashfromsandbox) or [RetreiveAgentPubKeyAndDnaHashFromConductor](#retreiveagentpubkeyanddnahashfromconductor) methods (by default this will occur automativally after it has connected to the Holochain Conductor).                                                                          |

#### Config

This property contains a struct called `HoloNETConfig` containing the following sub-properties:

|Property                                    |Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
|--------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| AgentPubKey                                | The AgentPubKey to use for Zome calls. If this is not set then HoloNET will automatically retreive this along with the DnaHash after it connects (if the [Connect](#connect) method defaults are not overriden).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| DnaHash                                    | The DnaHash to use for Zome calls. If this is not set then HoloNET will automatically retreive this along with the AgentPubKey after it connects (if the [Connect](#connect) method defaults are not overriden).                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          |
| FullPathToRootHappFolder                   | The full path to the root of the hApp that HoloNET will start the Holochain Conductor (currenty uses hc.exe) with and then make zome calls to.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                            |
| FullPathToCompiledHappFolder               | The full path to the compiled hApp that HoloNET will start the Holochain Conductor (currenty uses hc.exe) with and then make zome calls to.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| HolochainConductorMode                     | Tells HoloNET how to auto-start the Holochain Conductor. It can be one of the following values: `UseExternal` - Will use the hc.exe specififed in the `FullPathToExternalHCToolBinary` property if `HolochainConductorToUse` property is set to `Hc`. It will use the holochain.exe specefied in the `FullPathToExternalHolochainConductorBinary` property if `HolochainConductorToUse` property is set to `Holochain`. If `HolochainConductorMode` is set to `UseEmbedded` then it will use the embdedded/integrated hc.exe/holochain.exe if the app is using the [NextGenSoftware.Holochain.HoloNET.Client.Embedded](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.Embedded) package, otherwise it will throw an exception. Finally, if `HolochainConductorMode` is set to `UseSystemGlobal` (default), then it will automatically use the installed version of hc.exe & holochain.exe on the target machine. |
| HolochainConductorToUse                    | This is the Holochain Conductor to use for the auto-start Holochain Conductor feature. It can be either `Holochain` or `Hc`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                              |
| FullPathToExternalHolochainConductorBinary | The full path to the Holochain Conductor exe (holochain.exe) that HoloNET will auto-start if `HolochainConductorToUse` is set to `Holochain`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                             |
| FullPathToExternalHCToolBinary             | The full path to the Holochain Conductor exe (hc.exe) that HoloNET will auto-start if `HolochainConductorToUse` is set to `Hc`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |
| SecondsToWaitForHolochainConductorToStart  | The seconds to wait for the Holochain Conductor to start before attempting to [connect](#connect) to it.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                  |
| AutoStartHolochainConductor                | Set this to true if you with HoloNET to auto-start the Holochain Conductor defined in the `FullPathToExternalHolochainConductorBinary parameter if `HolochainConductorToUse` is `Holochain`, otherwise if it`s `Hc` then it will use `FullPathToExternalHCToolBinary`. Default is true.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                   |
| ShowHolochainConductorWindow               | Set this to true if you wish HoloNET to show the Holochain Conductor window whilst it is starting it (will be left open until the conductor is automatically shutdown again when HoloNET disconects if `AutoShutdownHolochainConductor` is true.)                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         |
| AutoShutdownHolochainConductor             | Set this to true if you wish HoloNET to auto-shutdown the Holochain Conductor after it [disconnects](#disconnect). Default is true.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| ShutDownALLHolochainConductors             | Set this to true if you wish HoloNET to auto-shutdown ALL Holochain Conductors after it [disconnects](#disconnect). Default is false. Set this to true if you wish to make sure there are none left running to prevent memory leaks. You can also of course manually call the [ShutDownAllConductors](#ShutDownAllConductors) if you wish.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                |
| OnlyAllowOneHolochainConductorToRunAtATime | Set this to true if you wish HoloNET to allow only ONE Holochain Conductor to run at a time. The default is false.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| LoggingMode                                | This passes through to the static LogConfig.LoggingMode property in [NextGenSoftware.Logging](https://www.nuget.org/packages/NextGenSoftware.Logging) package. It can be either `WarningsErrorsInfoAndDebug`, `WarningsErrorsAndInfo`, `WarningsAndErrors` or `ErrorsOnly`.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                               |
| ErrorHandlingBehaviour                     | An enum that specifies what to do when anm error occurs. The options are: `AlwaysThrowExceptionOnError`, `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` & `NeverThrowExceptions`). The default is `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` meaning it will only throw an error if the `OnError` event has not been subscribed to. This delegates error handling to the caller. If no event has been subscribed then HoloNETClient will throw an error. `AlwaysThrowExceptionOnError` will always throw an error even if the `OnError` event has been subscribed to. The `NeverThrowException` enum option will never throw an error even if the `OnError` event has not been subscribed to. Regardless of what enum is selected, the error will always be logged using whatever ILogger`s have been injected into the constructor or set on the static Logging.Loggers property.                        |

#### WebSocket

This property contains a refrence to the internal [NextGenSoftware WebSocket](https://www.nuget.org/packages/NextGenSoftware.WebSocket) that HoloNET uses. You can use this property to access the current state of the WebSocket as well as configure more options.

##### Config

It has a sub-property called Config that contains the following options:

|Property|Description  |
|--|--|
|TimeOutSeconds  | The time in seconds before the connection times out when calling either method `SendHoloNETRequest` or `CalLZomeFunction`. This defaults to 30 seconds.|
|NeverTimeOut|Set this to true if you wish the connection to never time out when making a call from methods `SendHoloNETRequest` and `CallZomeFunction`. This defaults to false.
|KeepAliveSeconds| This is the time to keep the connection alive in seconds. This defaults to 30 seconds.
|ReconnectionAttempts| The number of times HoloNETClient will attempt to re-connect if the connection is dropped. The default is 5.|
|ReconnectionIntervalSeconds|The time to wait between each re-connection attempt. The default is 5 seconds.|
|SendChunkSize| The size of the buffer to use when sending data to the Holochain Conductor. The default is 1024 bytes.
|ReceiveChunkSizeDefault| The size of the buffer to use when receiving data from the Holochain Conductor. The default is 1024 bytes. |
|ErrorHandlingBehaviour | An enum that specifies what to do when anm error occurs. The options are: `AlwaysThrowExceptionOnError`, `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` & `NeverThrowExceptions`). The default is `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` meaning it will only throw an error if the `OnError` event has not been subscribed to. This delegates error handling to the caller. If no event has been subscribed then HoloNETClient will throw an error. `AlwaysThrowExceptionOnError` will always throw an error even if the `OnError` event has been subscribed to. The `NeverThrowException` enum option will never throw an error even if the `OnError` event has not been subscribed to. Regardless of what enum is selected, the error will always be logged using whatever `ILogger`s have been injected into the constructor or set on the static Logging.Loggers property.

##### State

Contains an enumeration that can be one of the following values:

None,
Connecting,
Open,
CloseSent,
CloseReceived,
Closed,
Aborted

#### State

Is a shortcut to the WebSocket.State enumeration above.

#### EndPoint

Is the endpoint URI that HoloNET is running on.

### Logging

Both HoloNET & the [NextGenSoftware.WebSocket](https://www.nuget.org/packages/NextGenSoftware.WebSocket) package that HoloNET uses allow either a ILogger or collection of ILogger's to be injected in through Constuctor DI (Depenecy Injection). 

````c#
 public HoloNETClient(string holochainConductorURI = "ws://localhost:8888", bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "HoloNET.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
{
    Logging.Logging.Loggers.Add(new DefaultLogger(logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour));
    Init(holochainConductorURI);
}

public HoloNETClient(ILogger logger, string holochainConductorURI = "ws://localhost:8888")
{
    Logging.Logging.Loggers.Add(logger);
    Init(holochainConductorURI);
}

public HoloNETClient(IEnumerable<ILogger> loggers, string holochainConductorURI = "ws://localhost:8888")
{
    Logging.Logging.Loggers = new List<ILogger>(loggers);
    Init(holochainConductorURI);
}
````

````c#
public WebSocket(string endPointURI, bool logToConsole = true, bool logToFile = true, string releativePathToLogFolder = "Logs", string logFileName = "NextGenSoftwareWebSocket.log", bool addAdditionalSpaceAfterEachLogEntry = false, bool showColouredLogs = true, ConsoleColor debugColour = ConsoleColor.White, ConsoleColor infoColour = ConsoleColor.Green, ConsoleColor warningColour = ConsoleColor.Yellow, ConsoleColor errorColour = ConsoleColor.Red)
{
    EndPoint = endPointURI;
    Logging.Logging.Loggers.Add(new DefaultLogger(logToConsole, logToFile, releativePathToLogFolder, logFileName, addAdditionalSpaceAfterEachLogEntry, showColouredLogs, debugColour, infoColour, warningColour, errorColour));
    Init();
}

public WebSocket(string endPointURI, IEnumerable<ILogger> loggers)
{
    EndPoint = endPointURI;
    Logging.Logging.Loggers = new List<ILogger>(loggers);
    Init();
}

public WebSocket(string endPointURI, ILogger logger)
{
    Logging.Logging.Loggers.Add(logger);
    EndPoint = endPointURI;
    Init();
}
````

All NextGen Software libraries such as HoloNET, WebSocket etc use [NextGenSoftware.Logging](https://www.nuget.org/packages/NextGenSoftware.Logging). By default if no ILogger is injected in then they will automatically use the built in `DefaultLogger`, which comes with both File & Animated Coloured Console logging out of the box. Under the hood it uses the [NextGenSoftware.CLI.Engine](https://www.nuget.org/packages/NextGenSoftware.CLI.Engine) package to enable the colour and animation.

The DefaultLogger has the following options that can also be configured:

|Property|Description  |
|--|--|
|LogDirectory  | The directory where logs will be created.|
|LogFileName| The name of the log file to create. |
|LogToConsole| Set this to true to log to the console. The default is true. |
|LogToFile| Set this to true to log to the file. The default is true. |
|AddAdditionalSpaceAfterEachLogEntry| Set this to true to add additional space after each log entry. The default is false. |
|ShowColouredLogs| Set this to true to enable coloured logs in the console. This default to true. |
|DebugColour| The colour to use for `Debug` log enries to the console. |
|InfoColour| The colour to use for `Info` log enries to the console. |
|WarningColour| The colour to use for `Warning` log enries to the console. |
|ErrorColour| The colour to use for `Error` log enries to the console. |

You are welcome to create your own loggers to inject in, you simply need to implement the simple ILogger interface:

````c#
public interface ILogger
    {
        void Log(string message, LogType type, bool showWorkingAnimation = false);
        void Log(string message, LogType type, ConsoleColor consoleColour, bool showWorkingAnimation = false);
        void Shutdown();
    }
````

The Shutdown method is not used by the `DefaultLogger`, and so far is only used by the [NextGenSoftware.Logging.NLog](https://www.nuget.org/packages/NextGenSoftware.Logging.Nlog) package.


### HoloNETEntryBaseClass

This is a new abstract class introduced in HoloNET 2 that wraps around the HoloNETClient so you do not need to interact with the client directly. Instead it allows very simple CRUD operations ([Load](#loadHoloNETEntryBaseClass), [Save](#saveHoloNETEntryBaseClass) & [Delete](#deleteHoloNETEntryBaseClass)) to be performed on your custom data object that extends this class. Your custom data object represents the data (Holochain Entry) returned from a zome call and HoloNET will handle the mapping onto your data object automatically.

It has two constructors as can be seen below, one that allows you to pass in a HoloNETClient instance (which can be shared with other classes that extend the HoloNETEntryBaseClass or [HoloNETAuditEntryBaseClass](#HoloNETAuditEntryBaseClass)) or if you do not pass a HoloNETClient instance in using the other constructor it will create its own internal instance to use just for this class.

Here is a simple example of how to use it:

````c#
    /// <summary>
    /// This example creates its own internal instance of the HoloNETClient, you should only use this if you will be extending only one HoloNETAuditEntryBaseClass/HoloNETEntryBaseClass otherwise use the Multiple Class Example.
    /// </summary>
    public class Avatar : HoloNETEntryBaseClass
    {
        public Avatar() : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar") { }
        public Avatar(HoloNETConfig holoNETConfig) : base("oasis", "get_entry_avatar", "create_entry_avatar", "update_entry_avatar", "delete_entry_avatar", holoNETConfig) { }

        [HolochainPropertyName("first_name")]
        public string FirstName { get; set; }

        [HolochainPropertyName("last_name")]
        public string LastName { get; set; }

        [HolochainPropertyName("email")]
        public string Email { get; set; }

        [HolochainPropertyName("dob")]
        public DateTime DOB { get; set; }
    }
````

**NOTE:** Each property that you wish to have mapped to a property/field in your rust code needs to have the HolochainPropertyName attribute applied to it specifying the name of the field in your rust struct that is to be mapped to this c# property.

**NOTE:** This is a preview of some of the advanced functionality that will be present in the upcoming [.NET HDK Low Code Generator](#net-hdk-low-code-generator), which generates dynamic rust and c# code from your metadata freeing you to focus on your amazing business idea and creativity rather than worrying about learning Holochain, Rust and then getting it to all work in Windows and with C#.

#### Events
<a name="HoloNETEntryBaseClassEvents"></a>

The HoloNETEntryBaseClass has the following events you can subscribe to:

| Event                           | Description                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                       |
| --------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [OnError](#OnError)             | Fired when there is an error either in HoloNETEntryBaseClass or the HoloNET client itself.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        |
| [OnInitialized](#OnInitialized) | Fired after the [Initialize](#Initialize) method has finished initializing the HoloNET client. This will also call the [Connect](#Connect) and [RetreiveAgentPubKeyAndDnaHash](#RetreiveAgentPubKeyAndDnaHash) methods on the HoloNET client. This event is then fired when the HoloNET client has successfully connected to the Holochain Conductor, retreived the AgentPubKey & DnaHash & then fired the [OnReadyForZomeCalls](#OnReadyForZomeCalls) event.                                                                                                                                                                                                                                                                                                                                                                                                                                     |
| [OnLoaded](#OnLoaded)           | Fired after the [Load](#Load) method has finished loading the Holochain entry from the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                 |
| [OnSaved](#OnSaved)             | Fired after the [Save](#Save) method has finished saving the Holochain entry to the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeCreateEntryFunction` or property [ZomeCreateEntryFunction](#ZomeCreateEntryFunction) if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and [ZomeUpdateEntryFunction](#ZomeUpdateEntryFunction) property if it's an existing entry (previously saved object containing a valid value for the [EntryHash](#EntryHash) property). Once it has saved the entry it will then update the [EntryHash](#entryHash) property with the entry hash returned from the zome call/conductor. The [PreviousVersionEntryHash](#PreviousVersionEntryHash) property is also set to the previous EntryHash (if there is one). |
| [OnDeleted](#OnDeleted)         | Fired after the [Delete](#Delete) method has finished deleting the Holochain entry and has received a response from the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeDeleteEntryFunction` or property `ZomeDeleteEntryFunction`. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor.                                                                                                                                                                                                                                                                                                                                                                                                                              |
| [OnClosed](#OnClosed)           | Fired after the [Close](#Close) method has finished closing the connection to the Holochain Conductor and has shutdown all running Holochain Conductors (if configured to do so). This method calls the [ShutdownHoloNET](#ShutdownHoloNET) internally.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                           |

#### OnError

Fired when there is an error either in HoloNETEntryBaseClass or the HoloNET client itself.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         

````c#
holoNETEntryBaseClass.OnError += holoNETEntryBaseClass_OnError;

 private static void holoNETEntryBaseClass_OnError(object sender, HoloNETErrorEventArgs e)
        {
            CLIEngine.ShowErrorMessage($"TEST HARNESS: holoNETEntryBaseClass_OnError, EndPoint: {e.EndPoint}, Reason: {e.Reason}");
        }
````

| Parameter          | Description                                         |
|--------------------|-----------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.        |
| Reason             | The reason for the error.                           |


#### OnInitialized

Fired when there is an error either in HoloNETEntryBaseClass or the HoloNET client itself.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         

````c#
holoNETEntryBaseClass.OnInitialized += holoNETEntryBaseClass_OnInitialized;

 private static void holoNETEntryBaseClass_OnInitialized(object sender, ReadyForZomeCallsEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: holoNETEntryBaseClass_OnInitialized, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, IsError: {e.IsError}, Message: {e.Message}");
        }
````

| Parameter          | Description                                                                                                                                 |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.                                                                                                |
| AgentPubKey        | The Agent Public Key returned from the Holochain Conductor.                                                                                 |
| IsError            | True if there was an error during the initialization, false if not.                                                                         |
| Message            | If there was an error, this will contain the error message, otherwise it can contain any other message such as status etc or will be blank. |


#### OnLoaded

Fired after the [Load](#Load) method has finished loading the Holochain entry from the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                         

````c#
holoNETEntryBaseClass.OnLoaded += holoNETEntryBaseClass_OnLoaded;

 private static void holoNETEntryBaseClass_OnLoaded(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: holoNETEntryBaseClass_OnLoaded: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";
            
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            if (args.Entry != null && args.Entry.EntryDataObject != null)
            {
                AvatarEntryDataObject avatar = args.Entry.EntryDataObject as AvatarEntryDataObject;

                if (avatar != null)
                    result = BuildEntryDataObjectMessage(avatar, "Entry.EntryDataObject", result);
            }
            
            if (_avatarEntryDataObject != null)
                result = BuildEntryDataObjectMessage(_avatarEntryDataObject, "Global.EntryDataObject", result);

            return result;
        }

        private static string BuildEntryDataObjectMessage(AvatarEntryDataObject avatar, string header, string message)
        {
            message = string.Concat(message, "\n\n", header, ".FirstName: ", avatar.FirstName);
            message = string.Concat(message, "\n", header, ".LastName: ", avatar.LastName);
            message = string.Concat(message, "\n", header, ".Email: ", avatar.Email);
            message = string.Concat(message, "\n", header, ".DOB: ", avatar.DOB);

            return message;
        }
````

| Parameter          | Description                                                                                                                                 |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.                                                                                                |
| AgentPubKey        | The Agent Public Key returned from the Holochain Conductor.                                                                                 |
| IsError            | True if there was an error during the initialization, false if not.                                                                         |
| Message            | If there was an error, this will contain the error message, otherwise it can contain any other message such as status etc or will be blank. |


#### OnSaved

Fired after the [Save](#Save) method has finished saving the Holochain entry to the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeCreateEntryFunction` or property [ZomeCreateEntryFunction](#ZomeCreateEntryFunction) if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and [ZomeUpdateEntryFunction](#ZomeUpdateEntryFunction) property if it's an existing entry (previously saved object containing a valid value for the [EntryHash](#EntryHash) property). Once it has saved the entry it will then update the [EntryHash](#entryHash) property with the entry hash returned from the zome call/conductor. The [PreviousVersionEntryHash](#PreviousVersionEntryHash) property is also set to the previous EntryHash (if there is one).

````c#
holoNETEntryBaseClass.OnSaved += holoNETEntryBaseClass_OnSaved;

 private static void holoNETEntryBaseClass_OnSaved(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: holoNETEntryBaseClass_OnSaved: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";
            
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            if (args.Entry != null && args.Entry.EntryDataObject != null)
            {
                AvatarEntryDataObject avatar = args.Entry.EntryDataObject as AvatarEntryDataObject;

                if (avatar != null)
                    result = BuildEntryDataObjectMessage(avatar, "Entry.EntryDataObject", result);
            }
            
            if (_avatarEntryDataObject != null)
                result = BuildEntryDataObjectMessage(_avatarEntryDataObject, "Global.EntryDataObject", result);

            return result;
        }

        private static string BuildEntryDataObjectMessage(AvatarEntryDataObject avatar, string header, string message)
        {
            message = string.Concat(message, "\n\n", header, ".FirstName: ", avatar.FirstName);
            message = string.Concat(message, "\n", header, ".LastName: ", avatar.LastName);
            message = string.Concat(message, "\n", header, ".Email: ", avatar.Email);
            message = string.Concat(message, "\n", header, ".DOB: ", avatar.DOB);

            return message;
        }
````

#### OnDeleted

Fired after the [Delete](#Delete) method has finished deleting the Holochain entry and has received a response from the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeDeleteEntryFunction` or property `ZomeDeleteEntryFunction`. It then updates the HoloNET Entry Data Object with the response received from the Holochain Conductor.                                                                                                                                                                                                                                                                                                                                                                                                                            

````c#
holoNETEntryBaseClass.OnDeleted += holoNETEntryBaseClass_OnDeleted;

private static void holoNETEntryBaseClass_OnDeleted(object sender, ZomeFunctionCallBackEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: holoNETEntryBaseClass_OnDeleted: {ProcessZomeFunctionCallBackEventArgs(e)}");
        }

        private static string ProcessZomeFunctionCallBackEventArgs(ZomeFunctionCallBackEventArgs args)
        {
            string result = "";
            
            result = string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nZomeReturnHash: ", args.ZomeReturnHash, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Data: ", args.RawBinaryData, "\nRaw Binary Data As String: ", args.RawBinaryDataAsString, "\nRaw Binary Data Decoded: ", args.RawBinaryDataDecoded, "\nRaw Binary Data After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecode, "\nRaw Binary Data After MessagePack Decode As String: ", args.RawBinaryDataAfterMessagePackDecodeAsString, "\nRaw Binary Data Decoded After MessagePack Decode: ", args.RawBinaryDataAfterMessagePackDecodeDecoded, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\nIsError: ", args.IsError ? "true" : "false", "\nMessage: ", args.Message);

            if (!string.IsNullOrEmpty(args.KeyValuePairAsString))
                result = string.Concat(result, "\n\nProcessed Zome Return Data:\n", args.KeyValuePairAsString);

            if (args.Entry != null && args.Entry.EntryDataObject != null)
            {
                AvatarEntryDataObject avatar = args.Entry.EntryDataObject as AvatarEntryDataObject;

                if (avatar != null)
                    result = BuildEntryDataObjectMessage(avatar, "Entry.EntryDataObject", result);
            }
            
            if (_avatarEntryDataObject != null)
                result = BuildEntryDataObjectMessage(_avatarEntryDataObject, "Global.EntryDataObject", result);

            return result;
        }

        private static string BuildEntryDataObjectMessage(AvatarEntryDataObject avatar, string header, string message)
        {
            message = string.Concat(message, "\n\n", header, ".FirstName: ", avatar.FirstName);
            message = string.Concat(message, "\n", header, ".LastName: ", avatar.LastName);
            message = string.Concat(message, "\n", header, ".Email: ", avatar.Email);
            message = string.Concat(message, "\n", header, ".DOB: ", avatar.DOB);

            return message;
        }
````

| Parameter          | Description                                                                                                                                 |
|--------------------|---------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.                                                                                                |
| AgentPubKey        | The Agent Public Key returned from the Holochain Conductor.                                                                                 |
| IsError            | True if there was an error during the initialization, false if not.                                                                         |
| Message            | If there was an error, this will contain the error message, otherwise it can contain any other message such as status etc or will be blank. |


#### OnClosed

Fired after the [Close](#Close) method has finished closing the connection to the Holochain Conductor and has shutdown all running Holochain Conductors (if configured to do so). This method calls the [ShutdownHoloNET](#ShutdownHoloNET) internally.                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                          

````c#
holoNETEntryBaseClass.OnClosed += holoNETEntryBaseClass_OnClosed;

private static void holoNETEntryBaseClass_OnClosed(object sender, HoloNETShutdownEventArgs e)
        {
            CLIEngine.ShowMessage($"TEST HARNESS: holoNETEntryBaseClass_OnClosed, EndPoint: {e.EndPoint}, AgentPubKey: {e.AgentPubKey}, DnaHash: {e.DnaHash}, NumberOfHcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHcExeInstancesShutdown}, NumberOfHolochainExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfHolochainExeInstancesShutdown}, NumberOfRustcExeInstancesShutdown: {e.HolochainConductorsShutdownEventArgs.NumberOfRustcExeInstancesShutdown}, IsError: {e.IsError}, Message: {e.Message}");
        }
````

| Parameter                             | Description                                                                                                                                                                                                                  |
|---------------------------------------|------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| EndPoint                              | The URI EndPoint of the Holochain conductor.                                                                                                                                                                                 |
| AgentPubKey                           | The Agent Public Key of the hApp that was running in the Holochain Conductor.                                                                                                                                                |
| DnaHash                               | The DNA Hash of the hApp that was running in the Holochain Conductor.                                                                                                                                                        |
| NumberOfHcExeInstancesShutdown        | The number of hc.exe instances that were shutdown.                                                                                                                                                                           |
| NumberOfHolochainExeInstancesShutdown | The number of holochain.exe instances that were shutdown.                                                                                                                                                                    |
| NumberOfRustcExeInstancesShutdown     | The number of rustc.exe instances that were shutdown.                                                                                                                                                                        |
| IsError                               | True if there was an error during the initialization, false if not.                                                                                                                                                          |
| Message                               | If there was an error this will contain the error message, this normally includes a stacktrace to help you track down the cause. If there was no error it can contain any other message such as status etc or will be blank. |


#### Methods

The HoloNETEntryBaseClass has the following methods:

| Method                                                              | Description                                                                                                                                                                                                                                 |
| --------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Initialize](#Initialize)                                           | Fired when the HoloNET client has successfully connected to the Holochain Conductor, retreived the AgentPubKey & DnaHash & then fired the [OnReadyForZomeCalls](#OnReadyForZomeCalls) event.                                                                                                                                                               |
| [Load](#Load)                                                       | Fired when the the HoloNET client has finished loading the Holochain entry from the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object.
| [Save](#Save)                                                       | Fired when the the HoloNET client has finished saving the Holochain entry to the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeCreateEntryFunction` or property [ZomeCreateEntryFunction](#ZomeCreateEntryFunction) if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and [ZomeUpdateEntryFunction](#ZomeUpdateEntryFunction) property if it's an existing entry (previously saved object containing a valid value for the [EntryHash](#EntryHash) property). Once it has saved the entry it will then update the [EntryHash](#entryHash) property with the entry hash returned from the zome call/conductor. The [PreviousVersionEntryHash](#PreviousVersionEntryHash) property is also set to the previous EntryHash (if there is one).
| [Delete](#Delete)                                                   | Fired when any data is received from the Holochain conductor. This returns the raw data.                                                                                                                                                    |
| [Close](#Close)                                                     | Fired when the Holochain conductor returns the response from a zome function call. This returns the raw data as well as the parsed data returned from the zome function. It also returns the id, zome and zome function that made the call. |
| [WaitTillHoloNETInitializedAsync](#WaitTillHoloNETInitializedAsync) |

#### Properties

The HoloNETEntryBaseClass has the following properties:

| Property                                                            | Description                                                                                                                                                                                                                                 |
| --------------------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [Initialize](#Initialize)                                           | Fired when the HoloNET client has successfully connected to the Holochain Conductor, retreived the AgentPubKey & DnaHash & then fired the [OnReadyForZomeCalls](#OnReadyForZomeCalls) event.                                                                                                                                                               |
| [Load](#Load)                                                       | Fired when the the HoloNET client has finished loading the Holochain entry from the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeLoadEntryFunction` or property `ZomeLoadEntryFunction` and then maps the data returned from the zome call onto your data object.
| [Save](#Save)                                                       | Fired when the the HoloNET client has finished saving the Holochain entry to the Holochain Conductor. This calls the [CallZomeFunction](#CallZomeFunction) on the HoloNET client passing in the zome function name specefied in the constructor param `zomeCreateEntryFunction` or property [ZomeCreateEntryFunction](#ZomeCreateEntryFunction) if it is a new entry (empty object) or the `zomeUpdateEntryFunction` param and [ZomeUpdateEntryFunction](#ZomeUpdateEntryFunction) property if it's an existing entry (previously saved object containing a valid value for the [EntryHash](#EntryHash) property). Once it has saved the entry it will then update the [EntryHash](#entryHash) property with the entry hash returned from the zome call/conductor. The [PreviousVersionEntryHash](#PreviousVersionEntryHash) property is also set to the previous EntryHash (if there is one).
| [Delete](#Delete)                                                   | Fired when any data is received from the Holochain conductor. This returns the raw data.                                                                                                                                                    |
| [Close](#Close)                                                     | Fired when the Holochain conductor returns the response from a zome function call. This returns the raw data as well as the parsed data returned from the zome function. It also returns the id, zome and zome function that made the call. |
| [WaitTillHoloNETInitializedAsync](#WaitTillHoloNETInitializedAsync) |



### HoloNETAuditEntryBaseClass

#### Events

#### Methods

#### Properties


### Test Harness

You can find the Test Harness here:

[NextGenSoftware.Holochain.HoloNET.Client.TestHarness](https://www.nuget.org/packages/NextGenSoftware.Holochain.HoloNET.Client.TestHarness)

Read more on how to use the [Test Harness here](https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/NextGenSoftware.Holochain.HoloNET.Client.TestHarness).



<!--
#### NetworkServiceProvider

This is a property where the network service provider can be injected. The provider needs to implement the `IHoloNETClientNET` interface. 

The interface currently looks like this:

````c#
	public interface IHoloNETClientNET
    {
        //async Task<bool> Connect(Uri EndPoint);
        bool Connect(Uri EndPoint);
        bool Disconnect();
        bool SendData(string Data);
        string ReceiveData();

        NetSocketState NetSocketState { get; set; }
    }
````

**NOTE: This is currently not used and is future work to be done...**

The two currently planned providers will be WebSockets & HTTP but if for whatever reason Holochain decide they need to use another protocol then a new one can easily be implemented without having to refactor any existing code.

Currently the WebSocket JSON RPC implementation is deeply integrated into the HoloNETClient so this needs splitting out into its own project. We hope to get this done soon... We can then also at the same time implement the HTTP implementation. 


#### NetworkServiceProviderMode

This is a simple enum, which currently has these values:

````c#
public enum NetworkServiceProviderMode
    {
        WebSockets,
        HTTP,
        External
    }
````

The plan was to have WebSockets and HTTP built into the current implementation (but will still be injected in from a separate project). If there is a need a cut-down lite version of HoloNETClient can easily be implemented with just one of them injected in.

The External enum was to be used by any other external implementation that implements the `IHoloNETClientNET` and would be for future use if Holochain decide they wish to use another protocol.
-->

**More to come soon...**


## HoloOASIS

`HoloOASIS` uses the [HoloNETClient](#how-to-use-holonet) to implement a Storage Provider ([IOASISStorage](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK#ioasisstorage)) for the OASIS System. It will soon also implement a Network Provider ([IOASISNET](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK#ioasisnet))
 for the OASIS System that will leverage Holochain to create it's own private de-centralised distributed network called `ONET` (as seen on the [OASIS Architecture Diagram](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK#the-oasis-architecture)).

This is a good example to see how to use [HoloNETClient](#how-to-use-holonet) in a real world game/platform (OASIS/Our World).

Check out the [full documentation here](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/#holooasis).

## HoloUnity

We will soon be creating a Asset for the Unity Asset Store that will include [HoloNET](#how-to-use-holonet) along with Unity wrappers and examples of how to use [HoloNET](#how-to-use-holonet) inside Unity.

In the codebase you will find a project called [NextGenSoftware.OASIS.API.FrontEnd.Unity](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/tree/master/NextGenSoftware.OASIS.API.FrontEnd.Unity), which shows how the `AvatarManager` found inside the `OASIS API Core` ([NextGenSoftware.OASIS.API.Core](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK#project-structure)) is used. When you instantiate the `AvatarManager` you inject into a Storage Provider that implements the [IOASISStorage](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK#ioasisstorage) interface. Currently the only provider implemented is the [HoloOASIS](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK#holooasis) Provider.

The actual Our World Unity code is not currently stored in this repo due to size restrictions but we may consider using GitHub LFS (Large File Storage) later on. We are also looking at GitLab and other alternatives to see if they allow greater storage capabilities free out of the box (since we are currently working on a very tight budget but you could change that by donating below! ;-) ).

![alt text](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/blob/master/Images/HolochainTalkingToUnity.jpg "Holochain talking to Unity")

Here is a preview of the OASIS API/Avatar/Karma System... more to come soon... ;-)

**As with the rest of the project, if you have any suggestions we would love to hear from you! :)**

### Using HoloUnity

You start by instantiating the `AvatarManager` class found within the [NextGenSoftware.OASIS.API.Core](#https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/tree/master/NextGenSoftware.OASIS.API.Core) project.

````c#
// Inject in the HoloOASIS Storage Provider (this could be moved to a config file later so the 
// providers can be sweapped without having to re-compile.
ProfileManager = new ProfileManager(new HoloOASIS("ws://localhost:8888"));
````

Now, load the users Profile:

````c#
IProfile profile = await ProfileManager.LoadProfileAsync(username, password);

if (profile != null)
{
	//TODO: Bind profile info to Unity Avatar UI here.
}
````

The full code for the screenshot above that loads the users profile/avatar data from holochain and displays it in Unity is below:

````c#
using NextGenSoftware.OASIS.API.Core;
using NextGenSoftware.OASIS.API.Providers.HoloOASIS.Unity;
using UnityEngine;
using TMPro;
using System.Threading.Tasks;

public class OASISAvatarManager : MonoBehaviour
{
    ProfileManager ProfileManager { get; set; }  //If the ProfileManager is going to contain additional business logic not contained in the providers then use this.
    public GameObject ProfileUsername;
    public GameObject ProfileFullName;
    public GameObject ProfileDOB;
    public GameObject ProfileEmail;
    public GameObject ProfileAddress;
    public GameObject ProfileKarma;
    public GameObject ProfileLevel;

    async Task Start()
    {
    	// Inject in the HoloOASIS Storage Provider (this could be moved to a config file later so the 
        // providers can be sweapped without having to re-compile.
        ProfileManager = new ProfileManager(new HoloOASIS("ws://localhost:8888"));
        ProfileManager.OnProfileManagerError += ProfileManager_OnProfileManagerError;
        ProfileManager.OASISStorageProvider.OnStorageProviderError += OASISStorageProvider_OnStorageProviderError;

        //StorageProvider = new HoloOASIS("ws://localhost:8888");
	
        await LoadProfile();    
    }

    private async Task LoadProfile()
    {
        //IProfile profile = await ProfileManager.LoadProfileAsync("dellams", "1234");
        IProfile profile = await ProfileManager.LoadProfileAsync("QmR6A1gkSmCsxnbDF7V9Eswnd4Kw9SWhuf8r4R643eDshg");

        if (profile != null)
        {
            (ProfileFullName.GetComponent<TextMeshProUGUI>()).text = string.Concat(profile.Title, " ", profile.FirstName, " ", profile.LastName);
            (ProfileUsername.GetComponent<TextMeshProUGUI>()).text = profile.Username;
            (ProfileDOB.GetComponent<TextMeshProUGUI>()).text = profile.DOB;
            (ProfileEmail.GetComponent<TextMeshProUGUI>()).text = profile.Email;
            //(ProfileAddress.GetComponent<TextMeshProUGUI>()).text = profile.PlayerAddress;
            (ProfileKarma.GetComponent<TextMeshProUGUI>()).text = profile.Karma.ToString();
            (ProfileLevel.GetComponent<TextMeshProUGUI>()).text = profile.Level.ToString();
        }
    }

    private void OASISStorageProvider_OnStorageProviderError(object sender, ProfileManagerErrorEventArgs e)
    {
        Debug.Log("Error occured in the OASIS Storage Provider: " + e.Reason + ", Error Details: " + e.ErrorDetails);
    }

    private void ProfileManager_OnProfileManagerError(object sender, ProfileManagerErrorEventArgs e)
    {
        Debug.Log("Error occured in the OASIS Profile Manager: " + e.Reason + ", Error Details: " + e.ErrorDetails);
    }

    // Update is called once per frame
    void Update ()
    {
		
    }
}

````

Instead of using the OASIS `ProfileManager` to load the data, we could use [HoloNETClient](#holonet) directly, the code would then look like this:

````c#
using NextGenSoftware.OASIS.API.Core;
//using NextGenSoftware.OASIS.API.Providers.HoloOASIS.Unity;
using NextGenSoftware.Holochain.HoloNET.Client.Unity;
using NextGenSoftware.Holochain.HoloNET.Client.Core;
using UnityEngine;
using TMPro;
using Newtonsoft.Json;
using System.Threading.Tasks;

public class OASISAvatarManager : MonoBehaviour
{
    ProfileManager ProfileManager { get; set; }  //If the ProfileManager is going to contain additional
 business logic not contained in the providers then use this.
    public GameObject ProfileUsername;
    public GameObject ProfileFullName;
    public GameObject ProfileDOB;
    public GameObject ProfileEmail;
    public GameObject ProfileAddress;
    public GameObject ProfileKarma;
    public GameObject ProfileLevel;

    async Task Start()
    {
    	/*
    	// Inject in the HoloOASIS Storage Provider (this could be moved to a config file later so the 
        // providers can be sweapped without having to re-compile.
        ProfileManager = new ProfileManager(new HoloOASIS("ws://localhost:8888"));
        ProfileManager.OnProfileManagerError += ProfileManager_OnProfileManagerError;
        ProfileManager.OASISStorageProvider.OnStorageProviderError += OASISStorageProvider_OnStorageProviderError;
	
        await LoadProfile();
	*/
	
        HoloNETClient holoNETClient = new HoloNETClient("ws://localhost:8888");
        holoNETClient.OnZomeFunctionCallBack += HoloNETClient_OnZomeFunctionCallBack;
        holoNETClient.OnConnected += HoloNETClient_OnConnected;
        holoNETClient.OnError += HoloNETClient_OnError;
        holoNETClient.OnDataReceived += HoloNETClient_OnDataReceived;

        await holoNETClient.Connect();
        await holoNETClient.CallZomeFunctionAsync("test-instance", "our_world_core", "load_profile", new { address = "QmVtt5dEZEyTUioyh59XfFc3KWuaifK92Mc2KTXGauSbS9" });
    }

    private void HoloNETClient_OnDataReceived(object sender, DataReceivedEventArgs e)
    {
        Debug.Log(string.Concat("Data Received: EndPoint: ", e.EndPoint, "RawJSONData: ", e.RawJSONData));
    }

    private void HoloNETClient_OnError(object sender, HoloNETErrorEventArgs e)
    {
        Debug.Log(string.Concat("Error Occured. Resason: ", e.Reason, ", EndPoint: ", e.EndPoint, ", Details: ", e.ErrorDetails.ToString()));
    }

    private void HoloNETClient_OnConnected(object sender, ConnectedEventArgs e)
    {
        Debug.Log("Connected to Holochain Conductor: " + e.EndPoint);
    }

    private void HoloNETClient_OnZomeFunctionCallBack(object sender, ZomeFunctionCallBackEventArgs e)
    {
        Debug.Log(string.Concat("ZomeFunction CallBack: EndPoint: ", e.EndPoint, ", Id: ", e.Id, ", Instance: ", e.Instance, ", Zome: ", e.Zome, ", ZomeFunction: ", e.ZomeFunction, ", Data: ", e.ZomeReturnData, ", Raw Zome Return Data: ", e.RawZomeReturnData, ", Raw JSON Data: ", e.RawJSONData, ", IsCallSuccessful: ", e.IsCallSuccessful ? "true" : "false"));

        Profile profile = JsonConvert.DeserializeObject<Profile>(string.Concat("{", e.ZomeReturnData, "}"));

        if (profile != null)
        {
            (ProfileFullName.GetComponent<TextMeshProUGUI>()).text = string.Concat(profile.Title, " ", profile.FirstName, " ", profile.LastName);
            (ProfileUsername.GetComponent<TextMeshProUGUI>()).text = profile.Username;
            (ProfileDOB.GetComponent<TextMeshProUGUI>()).text = profile.DOB;
            (ProfileEmail.GetComponent<TextMeshProUGUI>()).text = profile.Email;
            //(ProfileAddress.GetComponent<TextMeshProUGUI>()).text = profile.PlayerAddress;
            (ProfileKarma.GetComponent<TextMeshProUGUI>()).text = profile.Karma.ToString();
            (ProfileLevel.GetComponent<TextMeshProUGUI>()).text = profile.Level.ToString();
        }
    }

    /*
    private async Task LoadProfile()
    {
        //StorageProvider = new HoloOASIS("ws://localhost:8888");

        //IProfile profile = await ProfileManager.LoadProfileAsync("dellams", "1234");
        IProfile profile = await ProfileManager.LoadProfileAsync("QmR6A1gkSmCsxnbDF7V9Eswnd4Kw9SWhuf8r4R643eDshg");

        if (profile != null)
        {
            (ProfileFullName.GetComponent<TextMeshProUGUI>()).text = string.Concat(profile.Title, " ", profile.FirstName, " ", profile.LastName);
            (ProfileUsername.GetComponent<TextMeshProUGUI>()).text = profile.Username;
            (ProfileDOB.GetComponent<TextMeshProUGUI>()).text = profile.DOB;
            (ProfileEmail.GetComponent<TextMeshProUGUI>()).text = profile.Email;
            //(ProfileAddress.GetComponent<TextMeshProUGUI>()).text = profile.PlayerAddress;
            (ProfileKarma.GetComponent<TextMeshProUGUI>()).text = profile.Karma.ToString();
            (ProfileLevel.GetComponent<TextMeshProUGUI>()).text = profile.Level.ToString();
        }
    }

    private void OASISStorageProvider_OnStorageProviderError(object sender, ProfileManagerErrorEventArgs e)
    {
        Debug.Log("Error occured in the OASIS Storage Provider: " + e.Reason + ", Error Details: " + e.ErrorDetails);
    }

    private void ProfileManager_OnProfileManagerError(object sender, ProfileManagerErrorEventArgs e)
    {
        Debug.Log("Error occured in the OASIS Profile Manager: " + e.Reason + ", Error Details: " + e.ErrorDetails);
    }
    */

    // Update is called once per frame
    void Update ()
    {
		
    }
}
````

This is how other Unity developers would connect to Holochain using HoloNETClient, because they may not be using the OASIS API. 

Of course if they wanted use the OASIS API then the first code listing is how it would be done.


### Events

### Methods

### Properties

**More to come soon...**




## Why this is important & vital to the holochain community

This is really vital and important to the wonderful holochain commnity because it will open the doors to the massive .NET, Unity and Enterprise sectors bringing the flood of devs, resources and exposure we all really want holochain to see... :)

So I hope now this has been split out into it's own indepenent repo and been officially handed over to the holochain open source community others can now jump in and get involved...


## What's Next? 

### Unity Asset

We plan to create a Unity Asset making it easier for other .NET & Unity devs to get involved with Holochain and get building hApps... 

We will release this soon...

In the meantime people can make use of this NuGet package.

### .NET HDK Low Code Generator

We can then get back to the .NET HDK Low Code Generator (will migrate this [repo](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/tree/master/NextGenSoftware.Holochain.HoloNET.HDK.Core) over to here soon...), this uses HoloNET to call into the dynamically generated rust and c# code. The rust code acts as a DAL (Data Access Layer) and the C# code acts as the BLL (Business Logic Layer). The generated C# code wraps around calls to HoloNET. This code has also already been written and is working with the previous version of Holochain (Redux).

This also allows devs to make use of all the libraries and resources available to them in Rust, .NET & Unity! 😊

So it will not take long to get this working with RSM (we just need to create the RSM CRUD templates for it to use).

Then, we can add the WASM compilation option to allow it to directly generate the .NET code to WASM so it can be used directly with the conductor so it will not even need the rust code at all then! :)

All of this helps bring yet more C# devs to Holochain who do not have time to learn Rust... :)

### Restore Holochain Support For The OASIS API

Now HoloNET is working again, we can add support for the OASIS API, which will then support Holochain as well as allow bridging to other Blockchains, DB's etc such as Ethereum, EOS, Solana, MongoDB, SQLLite, Neo4j, ThreeFold, ActivityPub, SOLID, IPFS, etc...

We can now complete HOLOOASIS provider meaning we can bridge holochain to all WEB2 (dbs, clouds, etc) and WEB3 blockchains etc giving a easy migration path to holochain. 

Read more here: \
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

### WEB5 STAR Omniverse Interoperable Metaverse Low Code Generator

It also allows the STAR Omniverse Interoperable Metaverse Low Code Generator to dynamically generate rust and c# code allowing people to focus on their idea rather than the lower level implementations and allow them to build metaverse experiences on top of holochain. 

The back-end is very close to completion and the front-end is in progress... :)

This is an evolution of the .NET HDK Low Code Generator so works the same way in that is generates dynamic rust and c# code, except it now also generates moons, planets, stars, galaxies, universes, etc and allows them to run across any blockchain, web2 cloud/db, IPFS, Holochain, etc. Everything that the OASIS API supports. STAR integrates both the .NET HDK (containing HoloNET) as well as the OASIS API and then expresses it in a cyberspace ontolgy and soooooo much more! ;-) It also allows other metaverses to be integrated in the Open Omniverse, which is the game of and simulation of life...

Read more here: \
https://www.ourworldthegame.com/single-post/announcing-star-odk-hdk-cosmic \
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK#web5-star-odk

For all of the above you can find more info on the OASIS API main repo as well as the Our World site & blogs:

http://www.ourworldthegame.com \
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK


## Donations Welcome! Thank you!

**HoloNET is totally free and Open Sourced to be used anyway you wish as long as we are credited. If you find it helpful, we would REALLY appreciate a donation to our crowd funding page, because this is our full-time job and so have no other income and will help keep us alive so we can continue to improve it for you all (as well as the WEB4 OASIS API, .NET HDK Low Code Generator & WEB5 STAR ODK Ominverse Interoperable Metaverse Low Code Generator), thank you! :)**

**https://www.gofundme.com/ourworldthegame**

<a name="get-involved"></a>
## Do You Want To Get Involved?

We are always looking for people to jump in and get involved, you do not need to be an existing coder, we can help you with that... you just need a willingness to learn and to have an open heart, we are always more interested with what is in your heart rather than your head! ;-)

The whole world is the Our World team, hence the name... ;-) It is not our project; it is all of humanities...

We also offer FREE training and apprenticeship program with the NextGen Developer Training Programmes.  We will teach you all we know over time and you get to work on this real live commercial codebase rather than wasting time working on throw away dummy apps as most training offers. No previous skills/experience required and is open to everyone, but especially for all disadvantaged people including special needs, homeless, unemployed, prison inmates, kids on the streets etc. We want to help the people the world has forgotten and for people who have stopped believing in themselves, we **BELIEVE IN YOU** and in time you will again too. Everyone has a gift for the world, and we will help you find yours… Find out more by checking out the links below:

<a href="https://c8119036-8b0a-4498-ab07-331841f19b4b.filesusr.com/ugd/4280d8_ad8787bd42b1471bae73003bfbf111f7.pdf">NextGen Developer Training Programme</a><br>
<a href="https://c8119036-8b0a-4498-ab07-331841f19b4b.filesusr.com/ugd/4280d8_999d98ba615e4fa6ab4383a415ee24c5.pdf">NextGen Junior Developer Training Programme</a>

We are looking for Web Devs (with any of these: react, angular, vue, js, html, css), Unity Devs & C# Devs.

If anyone is interested in developing this game/platform, then we would LOVE to hear from you! 😊 There will be opportunities for people to own shares and/or cryptocurrency (as well as other unique perks such as premium locations in both the geolocation and VR versions, personal or business service spotlights, free lifetime access to all premium paid services, massive karma points (allowing your avatar to progress to more advanced stages in the game unlocking new exciting quests, areas to explore & new special abilities/superpowers) plus lots more!) based on the input they are willing to provide.  

Thank you and we hope we find interest from people to join us on this exciting incredible journey.

**Want to make a difference in the world?**

**What will be your legacy?**

**Ready to be a hero?**

If the answer is YES, then please [proceed to here](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/wiki/So-You-Want-To-Get-Involved%3F-Ready-To-Be-A-Hero%3F)...


The Future Is Bright, \
The Future Is Holochain...

In Love, Light & Hope, \
The Our World Tribe.

**Our World Smartphone AR Prototype**

https://github.com/NextGenSoftwareUK/Our-World-Smartphone-Prototype-AR

**Sites**

http://www.ourworldthegame.com \
https://oasisplatform.world \
https://api.oasisplatform.world \
https://opensea.io/collection/theoasisandourworld \
http://www.nextgensoftware.co.uk \
http://www.yoga4autism.com \
https://www.thejusticeleagueaccademy.icu 

**Social**

|Type  |Link  |
|--|--|
|Facebook| http://www.facebook.com/ourworldthegame  |
|Twitter | http://www.twitter.com/ourworldthegame |
|YouTube| https://www.youtube.com/channel/UC0_O4RwdY3lq1m3-K-njUxA | 
|Discord| https://discord.gg/q9gMKU6 |
|Hylo| https://www.hylo.com/c/ourworld |
|Telegram| https://t.me/ourworldthegamechat (General Chat) |
|| https://t.me/ourworldthegame (Announcements) |
|| https://t.me/ourworldtechupdate (Tech Updates) |
|| https://t.me/oasisapihackalong (OASIS API Weekly Hackalongs) | 

**Blog/Forum**

[Blog](http://www.ourworldthegame.com/blog) \
[Forum](http://www.ourworldthegame.com/forum)

**Misc**

[The POWER Of The OASIS API](https://drive.google.com/file/d/1nnhGpXcprr6kota1Y85HDDKsBfJHN6sn/view?usp=sharing) \
[Dev Plan/Roadmap](https://drive.google.com/file/d/1QPgnb39fsoXqcQx_YejdIhhoPbmSuTnF/view?usp=sharing) \
[Join The Our World Tribe (Dev Requirements](https://drive.google.com/file/d/1b_G08UTALUg4H3jPlBdElZAFvyRcVKj1/view) \
[Mission/Summary](https://drive.google.com/file/d/12pCk20iLw_uA1yIfojcP6WwvyOT4WRiO/view?usp=sharing) \
[OASIS API & SEEDS API Integration Proposal](https://drive.google.com/file/d/1G8jJ2aMFU9lObddgHJVwcOKRZlpz12xJ/view?usp=sharing) \
[Our World & Game Of SEEDS Proposal](https://drive.google.com/file/d/1tFSK54mHxuUP1Z1Zc7p3ZxK5gQpoUjKW/view?usp=sharing) \
[SEEDS Camppaign Proposal](https://drive.google.com/file/d/1_UFi37UvDPaqW6g8WGJ7SyBPpbSXLfUV/view?usp=sharing) \
[Holochain Forum](https://forum.holochain.org/c/projects/our-world)

**NextGen Developer Training  Programmes**

[NextGen Developer Training Programme](https://docs.wixstatic.com/ugd/4280d8_ad8787bd42b1471bae73003bfbf111f7.pdf) \
[Junior NextGen Developer Training Programme](https://docs.wixstatic.com/ugd/4280d8_999d98ba615e4fa6ab4383a415ee24c5.pdf)

**Business Plan**

[Executive Summary](https://docs.wixstatic.com/ugd/4280d8_8b62b661334c43af8e4476d1a1b2afcb.pdf) \
[Business Plan Summary](https://docs.wixstatic.com/ugd/4280d8_9f8ed61eaf904905a6f94fcebf8650ef.pdf) \
[Business Plan Detailed](https://docs.wixstatic.com/ugd/4280d8_cb55d40e7e1b457c879383561e051fff.pdf) \
[Financials](https://docs.wixstatic.com/ugd/4280d8_698b48f342804534ac73829628799d33.xlsx?dn=NextGen%20Software%20Financials.xlsx) \
[Pitch Deck](https://d4de5c45-0ca1-451c-86a7-ce397b9225cd.filesusr.com/ugd/4280d8_50d17252aa3247eaae80013d0e0bf70d.pptx?dn=NextGen%20Software%20PitchDeck%20Lite.pptx)

**Funding**

**https://www.gofundme.com/ourworldthegame** \
**https://www.patreon.com/davidellams** \

**Key Videos**

[Our World Introduction](https://www.youtube.com/watch?v=wdYa5wQUfrg)  
[OASIS API DEMO SESSION 1 (Overview, Avatar & Karma API)](https://www.youtube.com/watch?v=Zy2QyoYwOAI&t=1072s)  
[OASIS API DEMO With David Ellams (James Halliday)](https://www.youtube.com/watch?v=DB75ldfPzlg&t=7s) \
[Latest preview for the Our World AR Geolocation game](https://www.youtube.com/watch?v=KtaGUxNQu4o&t=11s) \
[Latest prototype for the Our World Smartphone version... :)](https://www.youtube.com/watch?v=2oY4_LZBW4M) \
[Founders Introduction To Our World May 2017 (Remastered Nov 2017)](https://www.youtube.com/watch?v=SB97mvzJiRg&t=1s)  
[Our World Smartphone Prototype AR Mode Part 1](https://www.youtube.com/watch?v=rvNJ6poMduo)  
[Our World Smartphone Prototype AR Mode Part 2](https://www.youtube.com/watch?v=zyVmciqD9rs)  
[Our World - Smartphone Version Prototype In AR Mode](https://www.youtube.com/watch?v=3KIW3wlkUs0)  
[Our World Smartphone Version Preview](https://www.youtube.com/watch?v=U1IEfQQQeLc&t=1s)  
[Games Without Borders Ep 03 ft David Ellams from Our World](https://www.youtube.com/watch?v=3VFp5ltvPEM&t=611s)  
[AWAKEN DREAM SYNERGY DREAM # 19 Our World & The OASIS API By David Ellams - (Presentation Only)](https://www.youtube.com/watch?v=2ntJCTEihnw&t=1s)  
[Interview Between Moving On TV & Our World Founder David Ellams - Part 1](https://www.youtube.com/watch?v=kqTNINBFNV4&t=1s)  
[Interview Between Moving On TV & Our World Founder David Ellams - Part 2](https://www.youtube.com/watch?v=HxZixdkc-Ns&t=1s)  
[Our World Interviews With David Atkinson, Commercial Director of Holochain – Part 1](https://www.youtube.com/watch?v=UICajpltv1Y)  
[Our World Interviews With David Atkinson, Commercial Director of Holochain – Part 2](https://www.youtube.com/watch?v=SsNsEDPglos)  
[ThreeFold, Our World, Protocol Love, Soulfie API Meeting](https://www.youtube.com/watch?v=H5JJyLxGFe0)  

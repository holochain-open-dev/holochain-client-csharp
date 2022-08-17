# HoloNET Holochain .NET/Unity Client

# Table of contents

  - [Overview](#overview)
  - [HoloNET Code Has Migrated](#holonet-code-has-migrated)
  - [Background](#background)
  - [Initial RSM Version](#initial-rsm-version)
  - [How To Use HoloNET](#how-to-use-holonet)
    - [Quick Start](#quick-start)
    - [The Power of .NET Async Methods](#the-power-of-net-async-methods)
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
      - [CallZomeFunctionAsync](#callzomefunctionasync)
        - [Overload 1](#overload-1)
        - [Overload 2](#overload-2)
        - [Overload 3](#overload-3)
        - [Overload 4](#overload-4)
      - [ClearCache](#clearcache)
      - [Disconnect](#disconnect)
      - [SendMessageAsync](#sendmessageasync)
    - [Properties](#properties)
      - [Config](#config)
      - [Logger](#logger)
      - [NetworkServiceProvider](#networkserviceprovider)
      - [NetworkServiceProviderMode](#networkserviceprovidermode)
  - [HoloOASIS](#holooasis)
    - [Using HoloOASIS](#using-holooasis)
    - [Events](#events)
      - [OnInitialized](#oninitialized)
      - [OnPlayerProfileSaved](#onplayerprofilesaved)
      - [OnPlayerProfileLoaded](#onplayerprofileloaded)
      - [OnHoloOASISError](#onholooasiserror)
    - [Methods](#methods)
    - [Properties](#properties)
  - [OASIS API Core](#oasis-api-core)
      - [Using The OASIS API Core](#using-the-oasis-api-core)
    - [Interfaces](#interfaces)
      - [IOASISStorage](#ioasisstorage)
      - [IOASISNET](#ioasisnet)
    - [Events](#events)
    - [Methods](#methods)
    - [Properties](#properties)
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

## Overview

The world's first .NET & Unity client for [Holochain](http://holochain.org).

This library will allow you to connect any .NET or Unity client to Holochain and enjoy the power of a fully de-centralised distributed P2P multi-network agent-centric architecture.

This will help massively turbo charge the holochain ecosystem by opening it up to the massive .NET and Unity communities and open up many more possibilities of the things that can be built on top of Holochain. You can build almost anything you can imagine with .NET and/or Unity from websites, desktop apps, smartphone apps, services, AAA Games and lots more! They can target every device and platform out there from XBox, PS4, Wii, PC, Linux, Mac, iOS, Android, Windows Phone, iPad, Tablets, SmartTV, VR/AR/XR, MagicLeap, etc

**We are a BIG fan of Holochain and are very passionate about it and see a BIG future for it! We feel this is the gateway to taking Holochain mainstream! ;-)**


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


### Events
<a name="events"></a>

You can subscribe to a number of different events:

| Event                                                 | Description                                                                                                                                                                                                                                 |
| ------------------------------------------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| [OnConnected](#onconnected)                           | Fired when the client has successfully connected to the Holochain conductor.                                                                                                                                                                |
| [OnAppInfoCallBack](#onappinfocallback)               | Fired when the client receives AppInfo from the conductor containing the cell id for the running hApp (which in itself contains the AgentPubKey & DnaHash). It also contains the AppId and other info.                                      |
| [OnReadyForZomeCalls](#onreadyforzomecalls)           | Fired when the client has successfully connected and reteived the AgentPubKey & DnaHash, meaning it is ready to make zome calls to the Holochain conductor.                                                                                 |
| [OnDataReceived](#ondatareceived)                     | Fired when any data is received from the Holochain conductor. This returns the raw data.                                                                                                                                                    |
| [OnZomeFunctionCallBack](#onzomefunctioncallback)     | Fired when the Holochain conductor returns the response from a zome function call. This returns the raw data as well as the parsed data returned from the zome function. It also returns the id, zome and zome function that made the call. |
| [OnSignalsCallBack](#onsignalscallback)               | Fired when the Holochain conductor sends signals data. NOTE: This is still waiting for hc to flresh out the details for how this will work. Currently this returns the raw signals data.                                                    | 
| [OnConductorDebugCallBack](#onconductordebugcallback) | Fired when the Holochain conductor sends debug info.                                                                                                                                                                                        |
| [OnDisconnected](#ondisconnected)                     | Fired when the client disconnected from the Holochain conductor.                                                                                                                                                                            |
| [OnError](#onerror)                                   | Fired when an error occurs, check the params for the cause of the error.                                                                                                                                                                    |

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

| Parameter          | Description                                                |
|--------------------|------------------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.               |
| Id                 | The id that made the request.                              |
| AgentPubKey        | The AgentPubKey for the hApp.                              |
| DnaHash            | The DnaHash for the hApp.                                  |
| InstalledAppId     | The InstalledAppId for the hApp.                           |
| RawBinaryData      | The raw binary data returned from the Holochain conductor. |
| RawJSONData        | The raw JSON data returned from the Holochain conductor.   |
| WebSocketResult    | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info.


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

| Parameter          | Description                                                |
|--------------------|------------------------------------------------------------|
| EndPoint           | The URI EndPoint of the Holochain conductor.               |
| RawBinaryData      | The raw binary data returned from the Holochain conductor. |
| RawJSONData        | The raw JSON data returned from the Holochain conductor.   |
| WebSocketResult    | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info.

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
            string zomeData = "";

            foreach (string key in args.ZomeReturnData.Keys)
                zomeData = string.Concat(zomeData, key, "=", args.ZomeReturnData[key], "\n");

            return string.Concat("\nEndPoint: ", args.EndPoint, "\nId: ", args.Id, "\nZome: ", args.Zome, "\nZomeFunction: ", args.ZomeFunction, "\n\nRaw Data: ", args.RawData, "\n\nZomeReturnData: ", args.ZomeReturnData, "\nRaw Zome Return Data: ", args.RawZomeReturnData, "\nRaw Binary Daya: ", args.RawBinaryData, "\nRaw JSON Data: ", args.RawJSONData, "\nIsCallSuccessful: ", args.IsCallSuccessful ? "true" : "false", "\n\nProcessed Zome Return Data:\n", zomeData);
        }
````             

 | Parameter          | Description                                                                                                                                                                                                 |
 |--------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
 | EndPoint           | The URI EndPoint of the Holochain conductor.                                                                                                                                                                |
 | Zome               | The zome that made the request.                                                                                                                                                                             |
 | ZomeFunction       | The zome function that made the request.                                                                                                                                                                    |
 | ZomeReturnData     | The parsed data that the zome function returned. HoloNET will parse & convert the best it can from the Rust Holochain Conductor format to a C# friendly one such as converting from base64 encoding, etc.   |
 | RawZomeReturnData  | The raw binary data that the zome function returned.                                                                                                                                                        |
 | RawBinaryData      | The raw binary data returned from the Holochain conductor.                                                                                                                                                  |
 | RawJSONData        | The raw JSON data returned from the Holochain conductor.                                                                                                                                                    |
 | WebSocketResult    | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info. |


 
#### OnSignalsCallBack
Fired when the Holochain conductor sends signals data. NOTE: This is still waiting for Holochain to flesh out the details for how this will work. Currently this returns the raw signals data.

````c#
holoNETClient.OnSignalsCallBack += HoloNETClient_OnSignalsCallBack;

private static void HoloNETClient_OnSignalsCallBack(object sender, SignalsCallBackEventArgs e)
        {
            Console.WriteLine(string.Concat("TEST HARNESS: SIGINALS CALLBACK EVENT HANDLER: EndPoint: ", e.EndPoint, ", Id: ", e.Id , ", Data: ", e.RawJSONData, "Name: ", e.Name, "SignalType: ", Enum.GetName(typeof(SignalsCallBackEventArgs.SignalTypes), e.SignalType), "Arguments: ", e.SignalData));
            Console.WriteLine("");
        }
````   

 | Parameter          | Description                                                                                                                                                                                                 |
 |--------------------|-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
 | EndPoint           | The URI EndPoint of the Holochain conductor.                                                                                                                                                                |                                                                                                                                                      
 | SignalType         | An enum containing the SignalType, can be either User or Admin.                                                                                                                                             |
 | SignalData         | The Signal Data.                                                                                                                                                                                            |
 | RawBinaryData      | The raw binary data returned from the Holochain conductor.                                                                                                                                                  |
 | RawJSONData        | The raw JSON data returned from the Holochain conductor.                                                                                                                                                    |
 | WebSocketResult    | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info. |

 **NOTE: This is from the previous version of HoloNET running against the previous version of Holochain (Redux) & needs to be updated for the new RSM version, coming soon...**

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
 | NumberRunningZomeCalls   | The number of running zome calls.                                                                                                                                                                           |
 | Offline                  | Whether offline or not.                                                                                                                                                                                     |
 | Type                     | Type of conductor running.                                                                                                                                                                                  |
 | RawBinaryData            | The raw binary data returned from the Holochain conductor.                                                                                                                                                  |
 | RawJSONData              | The raw JSON data returned from the Holochain conductor.                                                                                                                                                    |
 | WebSocketResult          | Contains more detailed technical information of the underlying websocket. This includes the number of bytes received, whether the message was fully received & whether the message is UTF-8 or binary. Please [see here](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocketreceiveresult?view=netframework-4.8) for more info. |

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


### Methods

HoloNETClient contains the following methods:
<br>

|Method|Description  |
|--|--|
|[Connect](#connect)  | This method simply connects to the Holochain conductor. It raises the [OnConnected](#onconnected) event once it is has successfully established a connection. Please see the [Events](#events) section above for more info on how to use this event.
|[CallZomeFunctionAsync](#callzomefunctionasync)| This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data. This method raises the [OnZomeFunctionCallBack](#onzomefunctioncallback) event once it has received a response from the Holochain conductor. Please see the [Events](#events) section above for more info on how to use this event.
|[ClearCache](#clearcache) | Call this method to clear all of HoloNETClient's internal cache. This includes the JSON responses that have been cached using the [GetHolochainInstancesAsync](#getholochaininstancesasync) & [CallZomeFunctionAsync](#callzomefunctionasync) methods if the `cacheData` parm was set to true for any of the calls. |
|[Disconnect](#disconnect) | This method disconnects the client from Holochain conductor. It raises the [OnDisconnected](#ondisconnected) event once it is has successfully disconnected. Please see the [Events](#events) section above for more info on how to use this event. |
|[SendMessageAsync](#sendmessageasync) |This method allows you to send your own raw JSON request to holochain. This method raises the [OnDataReceived](#ondatareceived) event once it has received a response from the Holochain conductor. Please see the [Events](#events) section above for more info on how to use this event. You would rarely need to use this and we highly recommend you use the [CallZomeFunctionAsync](#callzomefunctionasync) method instead.

<br>

#### Connect

This method simply connects to the Holochain conductor. It raises the [OnConnected](#onconnected) event once it is has successfully established a connection. Please see the [Events](#events) section above for more info on how to use this event.

```c#
public async Task Connect(bool getAgentPubKeyAndDnaHashFromConductor = true, bool getAgentPubKeyAndDnaHashFromSandbox = false)
```
| Parameter                             | Description                                                                                    
| ------------------------------------- | ---------------------------------------------------------------------------------------------|
| getAgentPubKeyAndDnaHashFromConductor | Set this to true for HoloNET to automatically retreive the AgentPubKey & DnaHash from the Holochain Conductor after it has connected. This defaults to true. |
| getAgentPubKeyAndDnaHashFromSandbox   | Set this to true if you wish HoloNET to automatically retreive the AgentPubKey & DnaHash from the hc sandbox after it has connected. This defaults to true.  |

**NOTE: If both params are set to true it will first attempt to retreive the AgentPubKey & DnaHash from the Conductor, if that fails it will then attempt to retreive them from the hc sandbox command (it will still do this even if getAgentPubKeyAndDnaHashFromSandbox is set to false).**


#### CallZomeFunctionAsync

This is the main method you will be using to invoke zome functions on your given zome. It has a number of handy overloads making it easier and more powerful to call your zome functions and manage the returned data.

This method raises the [OnZomeFunctionCallBack](#onzomefunctioncallback) event once it has received a response from the Holochain conductor. Please see the [Events](#events) section above for more info on how to use this event.

##### Overload 1

````c#
public async Task CallZomeFunctionAsync(string id, string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false)
````

| Parameter                           | Description                                                                                    
| ----------------------------------- | ---------------------------------------------------------------------------------------------- |
| id                                  | The unique id you wish to assign for this call (NOTE: There is an overload that omits this param, use this overload if you wish HoloNET to auto-generate and manage the id's for you). |
| zome                                | The name of the zome you wish to target.                                                       |
| function                            | The name of the zome function you wish to call.                                                |
| delegate                            | A delegate to call once the zome function returns. This delegate contains the same signature as the one used for the OnZomeFunctionCallBack event.                                             |
| paramsObject                        | A basic CLR object containing the params the zome function is expecting.                       |
| matchIdToInstanceZomeFuncInCallback | This is an optional param, which defaults to true. Set this to true if you wish HoloNET to give the instance, zome  zome function that made the call in the callback/event. If this is false then only the id will be given in the callback. This uses a small internal cache to match up the id to the given zome/function. Set this to false if you wish to save a tiny amount of memory by not utilizing this cache. If it is false then the `Zome` and `ZomeFunction` params will be missing in the ZomeCallBack, you will need to manually match the `id` to the call yourself.  |
| cachReturnData                      | This is an optional param, which defaults to false. Set this to true if you wish HoloNET to cache the response retrieved from holochain. Subsequent calls will return this cached data rather than calling the Holochain conductor again. Use this for static data that is not going to change for performance gains.                                                         


#####  Overload 2

````c#
 public async Task CallZomeFunctionAsync(string zome, string function, ZomeFunctionCallBack callback, object paramsObject, bool cachReturnData = false)
 ````

This overload is similar to the one above except it omits the `id` and `matchIdToInstanceZomeFuncInCallback` param's forcing HoloNET to auto-generate and manage the id's itself. 


##### Overload 3

````c#
public async Task CallZomeFunctionAsync(string id, string zome, string function, object paramsObject, bool matchIdToInstanceZomeFuncInCallback = true, bool cachReturnData = false)
 ````

This overload is similar to the first one, except it is missing the `callback` param. For this overload you would subscribe to the `OnZomeFunctionCallBack` event. You can of course subscribe to this event for the other overloads too, it just means you will then get two callbacks, one for the event handler for `OnZomeFunctionalCallBack` and one for the callback delegate you pass in as a param to this method. The choice is yours on how you wish to use this method...


##### Overload 4

````c#
public async Task CallZomeFunctionAsync(string zome, string function, object paramsObject, bool cachReturnData = false)
 ````

This overload is similar to the one above except it omits the `id` and `matchIdToInstanceZomeFuncInCallback` param's forcing HoloNET to auto-generate and manage the id's itself. It is also missing the `callback` param. For this overload you would subscribe to the `OnZomeFunctionCallBack` event. You can of course subscribe to this event for the other overloads too, it just means you will then get two callbacks, one for the event handler for `OnZomeFunctionalCallBack` and one for the callback delegate you pass in as a param to this method. The choice is yours on how you wish to use this method...


#### ClearCache

Call this method to clear all of HoloNETClient's internal cache. This includes the responses that have been cached using the [CallZomeFunctionAsync](#callzomefunctionasync) methods if the `cacheData` parm was set to true for any of the calls.

````c#
public void ClearCache()
````

#### Disconnect

This method disconnects the client from the Holochain conductor. It raises the [OnDisconnected](#ondisconnected) event once it is has
 successfully disconnected. Please see the [Events](#events) section above for more info on how to use this event.

```c#
public async Task Disconnect()
```
NOTE: Currently when you call this method, you will receive the follow error:

> "The remote party closed the WebSocket connection without completing
> the close handshake."

This looks like an issue with the Holochain conductor and we will be raising this bug with them to see if it is something they need to address...


#### SendMessageAsync

This method allows you to send your own raw JSON request to holochain. This method raises the [OnDataRecived](#ondatareceived) event once it has received a response from the Holochain conductor. Please see the [Events](#events) section above for more info on how to use this event.

You would rarely need to use this and we highly recommend you use the [CallZomeFunctionAsync](#callzomefunctionasync) method instead.

````c#
public async Task SendMessageAsync(string jsonMessage)
 ````
<br>

| Paramameter |Description  |
|--|--|
| jsonMessage | The raw JSON message you wish to send to the Holochain conductor.  |
<br>

### Properties

HoloNETClient contains the following properties:

<br>

| Property | Description  |
|--|--|
| [Config](#config)  | This property contains a struct called `HoloNETConfig` containing the sub-properties: TimeOutSeconds, NeverTimeOut, KeepAliveSeconds, ReconnectionAttempts, ReconnectionIntervalSeconds, SendChunkSize, ReceiveChunkSizeDefault, ErrorHandlingBehaviour, FullPathToExternalHolochainConductor, FullPathToHolochainAppDNA, SecondsToWaitForHolochainConductorToStart, AutoStartConductor & AutoShutdownConductor
| [Logger](#logger) | Property to inject in a [ILogger](#ilogger) implementation. |
| [NetworkServiceProvider](#networkserviceprovider) | This is a property where the network service provider can be injected. The provider needs to implement the `IHoloNETClientNET` interface.  |
| [NetworkServiceProviderMode](#networkserviceprovidermode) |This is a simple enum, which currently has these values: Websockets, HTTP & External. |

<br>

#### Config

This property contains a struct called `HoloNETConfig` containing the following sub-properties:
<br>

|Property|Description  |
|--|--|
|TimeOutSeconds  | The time in seconds before the connection times out when calling either method `SendMessage` or `CalLZomeFunction`. This defaults to 30 seconds.|
|NeverTimeOut|Set this to true if you wish the connection to never time out when making a call from methods 'SendMessage' and `CallZomeFunction`. This defaults to false.
|KeepAliveSeconds| This is the time to keep the connection alive in seconds. This defaults to 30 seconds.
|ReconnectionAttempts| The number of times HoloNETClient will attempt to re-connect if the connection is dropped. The default is 5.|
|ReconnectionIntervalSeconds|The time to wait between each re-connection attempt. The default is 5 seconds.|
|SendChunkSize| The size of the buffer to use when sending data to the Holochain conductor. The default is 1024 bytes.
|ReceiveChunkSizeDefault| The size of the buffer to use when receiving data from the Holochain conductor. The default is 1024 bytes. |
| ErrorHandlingBehaviour | An enum that specifies what to do when anm error occurs. The options are: `AlwaysThrowExceptionOnError`, `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` & `NeverThrowExceptions`). The default is `OnlyThrowExceptionIfNoErrorHandlerSubscribedToOnErrorEvent` meaning it will only throw an error if the `OnError` event has not been subscribed to. This delegates error handling to the caller. If no event has been subscribed then HoloNETClient will throw an error. `AlwaysThrowExceptionOnError` will always throw an error even if the `OnError` event has been subscribed to. The `NeverThrowException` enum option will never throw an error even if the `OnError` event has not been subscribed to. Regardless of what enum is selected, the error will always be logged using whatever `ILogger` has been injected into the [Logger]("#logger") property. 
| FullPathToExternalHolochainConductor| The full path to the conductor exe (hc.exe) that HoloNET will auto-start.|
| FullPathToHolochainAppDNA | The full path to the hApp (Holochain App) DNA file that is the compiled WASM (compiled byt the conductor build options). |
| SecondsToWaitForHolochainConductorToStart | The seconds to wait for the Holochain Conductor to start before attempting to [connect](#connect) to it.|
| AutoStartConductor | Set this to true if you with HoloNET to auto-start the Holochain Conductor defined in the `FullPathToExternalHolochainConductor` parameter. Default is true. |
| AutoShutdownConductor | Set this to true if you wish HoloNET to auto-shutdown the Holochain Conductor after it [disconnects](#disconnect). Default is true.


 

<br>

#### Logger

Property to inject in a `ILogger` implementation.

`HoloNETClientBase` is an abstract class meaning it cannot be instantiated directly. You must inherit from it to use it.  This is where all the code for the HoloNETClient is.
 
`NextGenSoftware.Holochain.HoloNET.Client.Desktop` and `NextGenSoftware.Holochain.HoloNET.Client.Unity` projects both contain a `HoloNETClient` class that do just this.

They contain very little code. All they do is inject into the `Logger` property the logger implementation they wish to use. The implementation must implement the `ILogger` interface. 

````c#

using NextGenSoftware.Holochain.HoloNET.Client.Core;

namespace NextGenSoftware.Holochain.HoloNET.Client.Desktop
{
    public class HoloNETClient : HoloNETClientBase
    {
        public HoloNETClient(string holochainURI) : base(holochainURI)
        {
            this.Logger = new NLogger();
        }
    }
}

````

````c#

using NextGenSoftware.Holochain.HoloNET.Client.Core;

namespace NextGenSoftware.Holochain.HoloNET.Client.Unity
{
    public class HoloNETClient : HoloNETClientBase
    {
        public HoloNETClient(string holochainURI) : base(holochainURI)
        {
            //TODO: Add Unity Compat Logger Here (hopefully the Unity NLogger Download/Asset I found)
            // this.Logger = new NLogger();
            this.Logger = new DumbyLogger();
        }
    }
}
````

The desktop version uses a wrapper around the popular `NLog` logging framework, but unfortunately Unity does not support NLog so this is why this has had to be split out. We are currently looking into a good Logging Solution for Unity. We have found a possible port of NLog for Unity that so far is looking promising but this is still a different dll/library so the code must still remain as it is. This is also good practice to decouple the code as much as possible especially external dependencies such as logging.

<a name="ilogger"></a>
The ILogger interface is very simple:

````c#
namespace NextGenSoftware.Holochain.HoloNET.Client.Core
{
    public interface ILogger
    {
        void Log(string message, LogType type);
    }

    public enum LogType
    {
        Debug,
        Info,
        Warn,
        Error
    }
}
````

<br>

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

<br>

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

**More to come soon...**

<br>

## HoloOASIS

`HoloOASIS` uses the [HoloNETClient](#holonet) to implement a Storage Provider ([IOASISStorage](#ioasisstorage)) for the OASIS System. It will soon also implement a Network Provider ([IOASISNET](#ioasisnet))
 for the OASIS System that will leverage Holochain to create it's own private de-centralised distributed network called `ONET` (as seen on the [OASIS Architecture Diagram](#the-oasis-architecture) below).

This is a good example to see how to use [HoloNETClient](#holonet) in a real world game/platform (OASIS/Our World).

### Using HoloOASIS

You start by instantiating a new HoloOASIS class from either the [NextGenSoftware.OASIS.API.Providers.HoloOASIS.Desktop](#project-structure) project or the [NextGenSoftware.OASIS.API.Providers.HoloOASIS.Unity](#project-structure) project.

````c#
Desktop.HoloOASIS _holoOASIS = new Desktop.HoloOASIS("ws://localhost:8888");
````

You pass into the constructor the URI to the Holochain conductor.

Next, wire up the events:

````c#
_holoOASIS.HoloNETClient.OnConnected += HoloNETClient_OnConnected;
_holoOASIS.OnInitialized += _holoOASIS_OnInitialized;
_holoOASIS.OnPlayerProfileLoaded += _holoOASIS_OnPlayerProfileLoaded;
_holoOASIS.OnPlayerProfileSaved += _holoOASIS_OnPlayerProfileSaved;
_holoOASIS.OnHoloOASISError += _holoOASIS_OnHoloOASISError;
````
<br>
Once HoloOASIS has finished initializing after the `OnInitialzed` event has fired you can create a new user Profile by creating a new Profile object and populating it with the required properties.

You can also add karma by calling the `AddKarma` method on the `Profile` object.

Finally you call the `SaveProfileAsync` method passing in the `Profile` object to save the profile to your local chain on Holochain.

````c#
private static void _holoOASIS_OnInitialized(object sender, EventArgs e)
{
            Console.WriteLine("Initialized.");
            Console.WriteLine("Saving Profile...");

            _savedProfile = new Profile { Username = "dellams", Email = "david@nextgensoftware.co.uk", Password = "1234", FirstName = "David", LastName = "Ellams", DOB = "11/04/1980", Id = Guid.NewGuid(), Title = "Mr", PlayerAddress = "blahahahaha" };
            _savedProfile.AddKarma(999);

            _holoOASIS.SaveProfileAsync(_savedProfile);
}
````
<br>
To load the `Profile` object back from your local chain on Holochain you simply call the `LoadProfileAsync` method passing in the desired profiles Holochain hash, which is returned as a param in the `OnPlayerProfileSaved` event handler.

````c#
private static void _holoOASIS_OnPlayerProfileSaved(object sender, ProfileSavedEventArgs e)
{
            Console.WriteLine("Profile Saved.");
            Console.WriteLine("Profile Entry Hash: " + e.Profile.HcAddressHash);
            Console.WriteLine("Loading Profile...");
            //_savedProfile.Id = new Guid(e.ProfileEntryHash);
            _holoOASIS.LoadProfileAsync(e.Profile.HcAddressHash);
}
````

### Events

HoloOASIS contains the following events:

|Event|Description |
|--|--|
| OnInitialized |Fired when the HoloOASIS Provider has initialized. This is after the embedded [HoloNETClient](#holonet) has finished connecting to the Holochain Conductor.  |
| OnPlayerProfileSaved|Fired when the users profile has finished saving. |
| OnPlayerProfileLoaded|Fired when the users profile has finished loading. |
| OnHoloOASISError|Fired when an error occurs within the provider. 
| OnStorageProviderError|This implements part of the [IOASISStorage](#ioasisstorage) interface. This is a way for the OASIS Providers to bubble up any errors to the ProfileManager contained in the [NextGenSoftware.OASIS.API.Core](#oasisapi) |

#### OnInitialized 

Fired when the HoloOASIS Provider has initialized. This is after the embedded [HoloNETClient](#holonet) has finished connecting to the Holochain Conductor.

````c#
_holoOASIS.OnInitialized += _holoOASIS_OnInitialized;

private static void _holoOASIS_OnInitialized(object sender, EventArgs e)
{
            Console.WriteLine("Initialized.");
            Console.WriteLine("Saving Profile...");

            _savedProfile = new Profile { Username = "dellams", Email = "david@nextgensoftware.co.uk", Password = "1234", FirstName = "David", LastName = "Ellams", DOB = "11/04/1980", Id = Guid.NewGuid(), Title = "Mr", PlayerAddress = "blahahahaha" };
            _savedProfile.AddKarma(999);

            _holoOASIS.SaveProfileAsync(_savedProfile);
}
````
<br>

#### OnPlayerProfileSaved

Fired when the users profile has finished saving.

````c#
_holoOASIS.OnPlayerProfileSaved += _holoOASIS_OnPlayerProfileSaved;

private static void _holoOASIS_OnPlayerProfileSaved(object sender, ProfileSavedEventArgs e)
{
            Console.WriteLine("Profile Saved.");
            Console.WriteLine("Profile Entry Hash: " + e.Profile.HcAddressHash);
            Console.WriteLine("Loading Profile...");
            //_savedProfile.Id = new Guid(e.ProfileEntryHash);
            _holoOASIS.LoadProfileAsync(e.Profile.HcAddressHash);
}
````
<br>

|Parameter|Description |
|--|--|
|Profile  | The profile object that has just been saved.  |
<br>

#### OnPlayerProfileLoaded
Fired when the users profile has finished loading.

````c#
 _holoOASIS.OnPlayerProfileLoaded += _holoOASIS_OnPlayerProfileLoaded;

 private static void _holoOASIS_OnPlayerProfileLoaded(object sender, ProfileLoadedEventArgs e)
        {
            Console.WriteLine("Profile Loaded.");
            Console.WriteLine(string.Concat("Id: ", e.Profile.Id));
            Console.WriteLine(string.Concat("HC Address Hash: ", e.Profile.HcAddressHash));
            Console.WriteLine(string.Concat("Name: ", e.Profile.Title, " ", e.Profile.FirstName, " ", e.Profile.LastName));
            Console.WriteLine(string.Concat("Username: ", e.Profile.Username));
            Console.WriteLine(string.Concat("Password: ", e.Profile.Password));
            Console.WriteLine(string.Concat("Email: ", e.Profile.Email));
            Console.WriteLine(string.Concat("DOB: ", e.Profile.DOB));
            Console.WriteLine(string.Concat("Address: ", e.Profile.PlayerAddress));
            Console.WriteLine(string.Concat("Karma: ", e.Profile.Karma));
            Console.WriteLine(string.Concat("Level: ", e.Profile.Level));
        }
````
<br>

#### OnHoloOASISError

Fired when an error occurs within the provider.

````c#
 _holoOASIS.OnHoloOASISError += _holoOASIS_OnHoloOASISError;

private static void _holoOASIS_OnHoloOASISError(object sender, HoloOASISErrorEventArgs e)
        {
            Console.WriteLine(string.Concat("Error Occured. Reason: ", e.Reason, (e.HoloNETErrorDetails != null ? string.Concat(", HoloNET Reason: ", e.HoloNETErrorDetails.Reason) : ""), (e.HoloNETErrorDetails != null ? string.Concat(", HoloNET Details: ", e.HoloNETErrorDetails.ErrorDetails.ToString()) : ""), "\n"));
        }
````
<br>

|Parameter|Description  |
|--|--|
| EndPoint | The URI EndPoint of the Holochain conductor.
| Reason | The reason for the error.  |
| ErrorDetails| More detailed technical details including stack trace.
| HoloNETErrorDetails| If the error was caused by HoloNET, then the error details returned from HoloNET will appear here.


### Methods

HoloOASIS contains the following methods:

| Method |Description  |
|--|--|
| AddKarmaToProfileAsync |This implements part of the [IOASISStorage](#ioasisstorage) interface. Call this method to add karma to the users profile/avatar.  |
|ConvertProfileToHoloOASISProfile | Internal utility method that converts a `OASIS.API.Core.Profile` object to a `HoloOASIS.Profile` object. The `HoloOASIS.Profile` object extends the `OASIS.API.Core.Profile` object by adding the `HcAddressHash` property to store the address hash returned from Holochain when adding new entries to the chain.
| GetHolonsNearMe | This implements part of the IOASISNET interface. This has not been implemented yet and is just a stub. This method will get a list of the Holons (items/objects) near the user/avatar.
|GetPlayersNearMe|This implements part of the IOASISNET interface. This has not been implemented yet and is just a stub. This method will get a list of the players/avatars near the player's user/avatar.
|HandleError| This is a private method where all errors are funnelled and handled.
|Initialize| Call this method to initilize the provider. Internally this will call the [Connect](#connect) method on the [HoloNETClient](#holonet) class.
|LoadProfileAsync| Call this method to load the users profile/avatar data and return it in a `Profile` object. This has 3 overloads.
|RemoveKarmaFromProfileAsync| Call this method to remove karma from the users profile/avatar.
| SaveProfileAsync | Call this method to save the user's profile/avatar.

### Properties

HoloOASIS contains the following properties:

|Property|Description  |
|--|--|
| HoloNETClient | This contains a ref to the [HoloNETClient](#holonet). You can use this property to access the underlying [HoloNETClient](#holonet) including all [events](#events), [methods](#methods) & [properties](#properties). |


**More to come soon...**

## OASIS API Core

This is where the main OASIS API is located and contains all of the interfaces that the various providers implement along with the base objects & managers to power the OASIS API.

It is located in the `NextGenSoftware.OASIS.API.Core` project.

#### Using The OASIS API Core

The API is still being developed so at the time of writing,  only the`ProfileManager` is available.

You start by instantiating the `ProfileManager` class:

````c#
// Inject in the HoloOASIS Storage Provider (this could be moved to a config file later so the 
// providers can be sweapped without having to re-compile.
ProfileManager = new ProfileManager(new HoloOASIS("ws://localhost:8888"));
````

The `ProfileManager` takes one param for the constructor of type [IOASISStorage](#ioasisstorage). This is where you inject in a Provider that implements the [IOASISStorage](#ioasisstorage) interface. Currently the only provider that has implemented this is the [HoloOASIS](#holooasis) provider. but expect more to follow soon...

````c#
public ProfileManager(IOASISStorage
 OASISStorageProvider)
{
            this.OASISStorageProvider = OASISStorageProvider;
            this.OASISStorageProvider.OnStorageProviderError += OASISStorageProvider_OnStorageProviderError;
}
````

Part of the [IOASISStorage](#ioasisstorage) interface has an event called OnStorageProviderError, which the provider fires to send errors back to the `ProfileManager`.

Once the `ProfileManager` has been instantiated. you can load the users Profile using the `LoadProfileAsync` method:

````c#
IProfile profile = await ProfileManager.LoadProfileAsync(username, password);

if (profile != null)
{
	//TODO: Bind profile info to Unity Avatar UI here.
}
````

### Interfaces

The OASIS API currently has the following interfaces defined:

|Interface|Description  |
|--|--|
|[IOASISStorage](#ioasisstorage)  | This is what a Storage Provider implements so the OASIS API can read & write the users profile/avatar to the storage medium/network. Currently only the HoloOASIS provider exists but more will follow soon...the first will be EthereumOASIS & SOLIDOASIS so the API can talk to both Ethereum & SOLID.  |
|[IOASISNET](#ioasisnet)| This is what a Network Provider implements so the OASIS API can share the users profile/avatar as well as fine Holons and players near them. 

**NOTE: Currently the interfaces are pretty basic, but expect a LOT more to be added in the future...  Additional interfaces will also be added such as the IOASISRenderer interface.**

#### IOASISStorage  

This is what a Storage Provider implements so the OASIS API can read & write the users profile/avatar to the storage medium/network. Currently only the [HoloOASIS](#holooasis) provider exists but more will follow soon...the first will be EthereumOASIS & SOLIDOASIS so the API can talk to both Ethereum & SOLID.

````c#
namespace NextGenSoftware.OASIS.API.Core
{
    // This interface is responsible for persisting data/state to storage, this could be a local DB or other local 
    // storage or through a distributed/decentralised provider such as IPFS or Holochain (these two implementations 
    // will be implemented soon (IPFSOASIS & HoloOASIS).
    public interface IOASISStorage
    {
        Task<IProfile> LoadProfileAsync(string providerKey);
        Task<IProfile> LoadProfileAsync(Guid Id);
        Task<IProfile> LoadProfileAsync(string username, string password);

        //Task<bool> SaveProfileAsync(IProfile profile);
        Task<IProfile> SaveProfileAsync(IProfile profile);
        Task<bool> AddKarmaToProfileAsync(IProfile profile, int karma);
        Task<bool> RemoveKarmaFromProfileAsync(IProfile profile, int karma);

        event StorageProviderError OnStorageProviderError;

        //TODO: Lots more to come! ;-)
    }
}
````
<br>

| Item|Description  |
|--|--|
| LoadProfileAsync | Loads the users profile/avatar. This has 3 overloads, one takes a providerKey (a unique key that the provider can use to identify a profile, [HoloOASIS](#holooasis) uses the address hash for this), one takes a username & password and the final one takes a Guid for the profileID, which is a unique id for the profile irrespective of which provider is providing it.  |
| SaveProfileAsync | Saves the users profile/avatar. 
| AddKarmaToProfileAsync | Add karma to the users profile/avatar.
| RemoveKarmaFromProfileAsync | Remove karma from the users profile/avatar.
| StorageProviderError | An event the provider fires when an error occurs that the `ProfileManager` can then handle.

#### IOASISNET

This is what a Network Provider implements so the OASIS API can share the users profile/avatar as well as find Holon's and players near them. 

````c#
namespace NextGenSoftware.OASIS.API.Core
{
    // This interface provides methods to discover and interact with other nodes/peers on the distributed/decentralised network (ONET)
    // This will involve peer to peer communication.
    public interface IOASISNET
    {
        List<IPlayer> GetPlayersNearMe();
        List<IHolon> GetHolonsNearMe(HolonType Type);
    }
}
````

Currently the [HoloOASIS](#holooasis) Provider defines some stubs for this interface, which will be fully implemented soon...

| Item | Description  |
|--|--|
| GetPlayersNearMe  | Gets a list of players near the user's current location.  |
| GetHolonsNearMe | Get a list of holon's near the user's current location.


### Events

### Methods

### Properties

**More to come soon...**


## HoloUnity

We will soon be creating a Asset for the Unity Asset Store that will include [HoloNET](#holonet) along with Unity wrappers and examples of how to use [HoloNET](#holonet) inside Unity.

In the codebase you will find a project called [NextGenSoftware.OASIS.API.FrontEnd.Unity](#project-structure), which shows how the `ProfileManager` found inside the `OASIS API Core` ([NextGenSoftware.OASIS.API.Core](#project-structure)) is used. When you instantiate the `ProfileManager` you inject into a Storage Provider that implements the [IOASISStorage](#ioasisstorage) interface. Currently the only provider implemented is the [HoloOASIS](#holooasis) Provider.

The actual Our World Unity code is not currently stored in this repo due to size restrictions but we may consider using GitHub LFS (Large File Storage) later on. We are also looking at GitLab and other alternatives to see if they allow greater storage capabilities free out of the box (since we are currently working on a very tight budget but you could change that by donating below! ;-) ).

![alt text](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/blob/master/Images/HolochainTalkingToUnity.jpg "Holochain talking to Unity")

Here is a preview of the OASIS API/Avatar/Karma System... more to come soon... ;-)

**As with the rest of the project, if you have any suggestions we would love to hear from you! :)**

### Using HoloUnity

You start by instantiating the `ProfileManager` class found within the [NextGenSoftware.OASIS.API.Core](#project-structure) project.

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

# HoloNET Holochain .NET/Unity Client

The world's first .NET & Unity client for <a href="holochain.org">Holochain</a>.

This is for the Unity version, which uses a basic built-in lightweight ConsoleLogger and will soon support a Unity Only Logger.

Check out the Desktop verion here:
https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/NextGenSoftware.Holochain.HoloNET.Client.Desktop
Uses NLog (which does not work in Unity) & a basic built-in lightweight ConsoleLogger.

Check out the core abstract base class here:
https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/NextGenSoftware.Holochain.HoloNET.Client.Core
This is the core abstract base class that must be extended/inherited from to use for the target platform/enviroment, etc. It uses Constructor DI (Dependency Injection) to allow the extended class to inject in the Logger they wish to use.

You are free to create your own versions for your target platform/enviroment and custom logger (must implement the ILogger Interface).


## HoloNET Code Has Migrated

This code was migrated from the main OASIS API/STAR Metaverse/HoloNET/.NET HDK code found here:
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

### Background

Original HoloNET code was written back in 2019 and was fully operational with the previous version of Holochain (Redux), but unfortuntley had issues getting it working with RSM. 

https://www.ourworldthegame.com/single-post/2019/08/14/world-exclusive-holochain-talking-to-unity

The previous version also came bundled with the holochain conductor so it could auto-start/shutdown the conductor and be fully integrated with any .NET or Unity application. This code/fuctionaility is still in there and will now work again that we have a Windows binary again (NixOS broke this feature previously).

It was featured on Dev Pulse 44 here:<br>
https://medium.com/holochain/updated-quick-start-guide-the-gift-of-holonet-and-conversations-that-matter-on-the-holochain-8e08efde1f58

https://www.ourworldthegame.com/single-post/2019/09/10/holonet-was-featured-in-the-latest-holochain-dev-pulse

### Initial RSM Version

We are pleased that after nearly 2 years we have now finally got this upgraded to work with RSM thanks to Connors help, who we are eternally grateful to! :)

https://www.ourworldthegame.com/single-post/holonet-rsm-breakthrough-at-long-last

Please check out the above link, there you will find more details on what has changed from the previous Redux HoloNET version as well as some documentation on how to use it... :)

We will also add it here soon...

We can now complete HOLOOASIS provider meaning we can bridge holochain to all web2 (dbs, clouds, etc) and web3 blockchains etc giving a easy migration path to holochain. 

It also allows the STAR Omniverse Low Code Generator to dynamically generate rust and c# code allowing people to focus on their idea rather than the lower level implementations and allow them to build metaverse experiences on top of holochain. 

### Why this is important & vital to the holochain community

This is really vital and important to the wonderful holochain commnity because it will open the doors to the massive .NET, Unity and Enterprise sectors bringing the flood of devs, resources and exposure we all really want holochain to see... :)

So I hope now this has been split out into it's own indepenent repo and been officially handed over to the holochain open source community others can now jump in and get involved...


### What's Next? 

#### NuGet Package & Unity Asset

We plan to get it packaged up onto the .NET NuGet Package store as well as create a Unity Asset making it easier for other .NET & Unity devs to get involved with Holochain and get building hApps... 

#### .NET HDK Low Code Generator

We can then get back to the .NET HDK Low Code Generator (will migrate this <a href='https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/tree/master/NextGenSoftware.Holochain.HoloNET.HDK.Core'>repo</a> over to here soon...), this uses HoloNET to call into the dynamically generated rust and c# code. The rust code acts as a DAL (Data Access Layer) and the C# code acts as the BLL (Business Logic Layer). The generated C# code wraps around calls to HoloNET. This code has also already been written and is working with the previous version of Holochain (Redux).

This also allows devs to make use of all the libraries and resources available to them in Rust, .NET & Unity! 😊

So it will not take long to get this working with RSM (we just need to create the RSM CRUD templates for it to use).

Then, we can add the WASM compilation option to allow it to directly generate the .NET code to WASM so it can be used directly with the conductor so it will not even need the rust code at all then! :)

All of this helps bring yet more C# devs to Holochain who do not have time to learn Rust... :)

#### Restore Holochain Support For The OASIS API

Now HoloNET is working again we can add support for the OASIS API, which will then support Holochain as well as allow bridging to other Blockchains, DB's etc such as Ethereum, EOS, Solana, MongoDB, SQLLite, Neo4j, ThreeFold, ActivityPub, SOLID, IPFS, etc...

Read more here:
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

#### WEB5 STAR Omniverse Interoperable Metaverse Low Code Generator

We can also now add Holochain support to the STAR Metaverse/Omniverse, which the back-end is very close to completion and the front-end is in progress... :)

This is an evolution of the .NET HDK Low Code Generator so works the same way in that is generates dynamic rust and c# code, except it now also generates moons, planets, stars, galaxies, universes, etc and allows them to run across any blockchain, web2 cloud/db, IPFS, Holochain, etc. Everything that the OASIS API supports. STAR integrates both the .NET HDK (containing HoloNET) as well as the OASIS API and then expresses it in a cyberspace ontolgy and soooooo much more! ;-) It also allows other metaverses to be integrated in the Open Omniverse, which is the game of and simulation of life...

Read more here:
https://www.ourworldthegame.com/single-post/announcing-star-odk-hdk-cosmic

For all of the above you can find more info on the OASIS API main repo as well as the Our World site & blogs:

http://www.ourworldthegame.com <br>
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

The Future Is Bright, The Future Is Holochain...
<br><br>
In Love, Light & Hope, <br>
The Our World Tribe.


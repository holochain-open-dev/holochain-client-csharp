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

This code was migrated from the main OASIS API/STAR Metaverse/HoloNET/.NET HDK code found here:
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

### Background

Original HoloNET code (world's first .NET & Unity client for Holochain) written back in 2019 and was fully operational with the previous version of Holochain (Redux), but unfortuntley had issues getting it working with RSM. 

https://www.ourworldthegame.com/single-post/2019/08/14/world-exclusive-holochain-talking-to-unity

The previous version also came bundled with the holochain conductor so it could auto-start/shutdown the conductor and be fully integrated with any .NET or Unity application. This code/fuctionaility is still in there and will now work again that we have a Windows binary again (NixOS broke this feature previously).

It was featured on Dev Pulse 44 here:
https://medium.com/holochain/updated-quick-start-guide-the-gift-of-holonet-and-conversations-that-matter-on-the-holochain-8e08efde1f58

https://www.ourworldthegame.com/single-post/2019/09/10/holonet-was-featured-in-the-latest-holochain-dev-pulse

### Current Issues

You can view the full discussion going on since Aug to get this to work here:
https://github.com/holochain/holochain/issues/905

It actually goes back further than this but this is the latest thread regarding this... :)

### Rewards

If anyone can crack this we will gift £3000+ worth of OLAND (virtual land in Our World) NFTs and earn 7777 karma points and tokens plus maybe more... please share around... thanks 🙏

https://opensea.io/collection/theoasisandourworld

This is urgent and a top priority now, because we can then complete the HOLOOASIS provider meaning we can bridge holochain to all web2 (dbs, clouds, etc) and web3 blockchains etc giving a easy migration path to holochain. 

It also allows the STAR Omniverse Low Code Generator to dynamically generate rust and c# code allowing people to focus on their idea rather than the lower level implementations and allow them to build metaverse experiences on top of holochain. 

### Why this is important & vital to the holochain community

This is really vital and important to the wonderful holochain commnity because it will open the doors to the massive .NET, Unity and Enterprise sectors bringing the flood of devs, resources and exposure we all really want holochain to see... :)

So I hope now this has been split out into it's own indepenent repo and been officially handed over to the holochain open source community others can now jump in and get involved...


### What's Next? 

#### NuGet Package & Unity Asset

Once we crack this nuget we can get it packaged up onto the .NET NuGet Package store as well as create a Unity Asset making it easier for other .NET & Unity devs to get involved with Holochain and get building hApps... 

#### .NET HDK Low Code Generator

We can then get back to the .NET HDK Low Code Generator (will migrate this <a href='https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/tree/master/NextGenSoftware.Holochain.HoloNET.HDK.Core'>repo</a> over to here soon...), this uses HoloNET to call into the dynamically generated rust and c# code. The rust code acts as a DAL (Data Access Layer) and the C# code acts as the BLL (Business Logic Layer). The generated C# code wraps around calls to HoloNET. This code has also already been written and is working with the previous version of Holochain (Redux).

This also allows devs to make use of all the libraries and resources available to them in Rust, .NET & Unity! 😊

So it will not take long to get this working with RSM (we just need to create the RSM CRUD templates for it to use).

Then, we can add the WASM compilation option to allow it to directly generate the .NET code to WASM so it can be used directly with the conductor so it will not even need the rust code at all then! :)

All of this helps bring yet more C# devs to Holochain who do not have time to learn Rust... :)

#### Restore Holochain Support For The OASIS API

Once HoloNET is working the OASIS API will then once again support Holochain, which will allow bridging to other Blockchains, DB's etc such as Ethereum, EOS, Solana, MongoDB, SQLLite, Neo4j, ThreeFold, ActivityPub, SOLID, IPFS, etc...

Read more here:
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

#### STAR Metaverse

We can also then add Holochain support to the STAR Metaverse/Omniverse, which the back-end is very close to completion and the front-end is in progress... :)

Read more here:
https://www.ourworldthegame.com/single-post/announcing-star-odk-hdk-cosmic

All of the above you can find more info on the OASIS API main repo as well as the Our World site & blogs:

http://www.ourworldthegame.com <br>
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

The Future Is Bright, The Future Is Holochain...
<br><br>
In Love, Light & Hope, <br>
The Our World Tribe.


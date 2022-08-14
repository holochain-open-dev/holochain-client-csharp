# HoloNET Holochain .NET/Unity Client

The world's first .NET & Unity client for [Holochain](http://holochain.org).

This library will allow you to connect any .NET or Unity client to Holochain and enjoy the power of a fully de-centralised distributed P2P multi-network agent-centric architecture.
## HoloNET Code Has Migrated

This code was migrated from the main OASIS API/STAR Metaverse/HoloNET/.NET HDK code found here:
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

## Background

Original HoloNET code was written back in 2019 and was fully operational with the previous version of Holochain (Redux), but unfortuntley had issues getting it working with RSM. 

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

We can now complete HOLOOASIS provider meaning we can bridge holochain to all web2 (dbs, clouds, etc) and web3 blockchains etc giving a easy migration path to holochain. 

It also allows the STAR Omniverse Low Code Generator to dynamically generate rust and c# code allowing people to focus on their idea rather than the lower level implementations and allow them to build metaverse experiences on top of holochain. 

## Why this is important & vital to the holochain community

This is really vital and important to the wonderful holochain commnity because it will open the doors to the massive .NET, Unity and Enterprise sectors bringing the flood of devs, resources and exposure we all really want holochain to see... :)

So I hope now this has been split out into it's own indepenent repo and been officially handed over to the holochain open source community others can now jump in and get involved...


## What's Next? 

### NuGet Package & Unity Asset

We plan to get it packaged up onto the .NET NuGet Package store as well as create a Unity Asset making it easier for other .NET & Unity devs to get involved with Holochain and get building hApps... 

This is the NuGet Package, we will release the Unity Asset very soon...

### .NET HDK Low Code Generator

We can then get back to the .NET HDK Low Code Generator (will migrate this [repo](https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK/tree/master/NextGenSoftware.Holochain.HoloNET.HDK.Core) over to here soon...), this uses HoloNET to call into the dynamically generated rust and c# code. The rust code acts as a DAL (Data Access Layer) and the C# code acts as the BLL (Business Logic Layer). The generated C# code wraps around calls to HoloNET. This code has also already been written and is working with the previous version of Holochain (Redux).

This also allows devs to make use of all the libraries and resources available to them in Rust, .NET & Unity! 😊

So it will not take long to get this working with RSM (we just need to create the RSM CRUD templates for it to use).

Then, we can add the WASM compilation option to allow it to directly generate the .NET code to WASM so it can be used directly with the conductor so it will not even need the rust code at all then! :)

All of this helps bring yet more C# devs to Holochain who do not have time to learn Rust... :)

### Restore Holochain Support For The OASIS API

Now HoloNET is working again, we can add support for the OASIS API, which will then support Holochain as well as allow bridging to other Blockchains, DB's etc such as Ethereum, EOS, Solana, MongoDB, SQLLite, Neo4j, ThreeFold, ActivityPub, SOLID, IPFS, etc...

Read more here: \
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

### WEB5 STAR Omniverse Interoperable Metaverse Low Code Generator

We can also now add Holochain support to the STAR Metaverse/Omniverse, which the back-end is very close to completion and the front-end is in progress... :)

This is an evolution of the .NET HDK Low Code Generator so works the same way in that is generates dynamic rust and c# code, except it now also generates moons, planets, stars, galaxies, universes, etc and allows them to run across any blockchain, web2 cloud/db, IPFS, Holochain, etc. Everything that the OASIS API supports. STAR integrates both the .NET HDK (containing HoloNET) as well as the OASIS API and then expresses it in a cyberspace ontolgy and soooooo much more! ;-) It also allows other metaverses to be integrated in the Open Omniverse, which is the game of and simulation of life...

Read more here: \
https://www.ourworldthegame.com/single-post/announcing-star-odk-hdk-cosmic \
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK#web5-star-odk

For all of the above you can find more info on the OASIS API main repo as well as the Our World site & blogs:

http://www.ourworldthegame.com \
https://github.com/NextGenSoftwareUK/Our-World-OASIS-API-HoloNET-HoloUnity-And-.NET-HDK

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

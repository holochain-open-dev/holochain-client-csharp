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

One class which needs an instance of that websocket class passed to it, which is an abstraction over a Zome. So it is a Zome client. That way, one only needs to pass in an argument to make a zome call telling it which 'function' you wish to call in holochain. 

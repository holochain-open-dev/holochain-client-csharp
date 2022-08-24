# HoloNET Test Harness

Test Harness for HoloNET Holochain Client.

https://github.com/holochain-open-dev/holochain-client-csharp
https://github.com/NextGenSoftwareUK/holochain-client-csharp

You need to clone the following repo:
https://github.com/holochain/happ-build-tutorial

And follow the instructions here:
https://github.com/holochain-open-dev/wiki/wiki/Installing-Holochain-Natively-On-Windows

Once you have Holochain setup on your machine and got the example hApp ready above, you need to copy it into a hApps folder in the root of the output folder (Debug or Release) where you installed this Test Harness package. HoloNET will be looking for it there.

The Test Harness sets the paths to the test hApp you compiled above using the following lines:

````c#
_holoNETClient.Config.FullPathToRootHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop");
_holoNETClient.Config.FullPathToCompiledHappFolder = string.Concat(Environment.CurrentDirectory, @"\hApps\happ-build-tutorial-develop\workdir\happ");
````
Currently the Embedded option does not work properly (looks like hc needs more than just the hc.exe and holochain.exe to work properly), so the Test Harness uses the following:

````c#
_holoNETClient.Config.HolochainConductorMode = HolochainConductorModeEnum.UseSystemGlobal;
````

This is the default for HoloNET anyway so for people wishing to use HoloNET there is no need to set this.

Once we can get hc.exe and holochain.exe to work without having to install Holochain with the instructions above we can use the following:

````c#
_holoNETClient.Config.HolochainConductorMode = HolochainConductorModeEnum.UseEmbedded;
````

Finally, from within your app simply call the following method:

````c#
using NextGenSoftware.Holochain.HoloNET.Client.TestHarness;

await HoloNETTestHarness.TestHoloNETClientAsync();
````

You should then see an output something like this:



You can also view the full source and run the Test Harness (and edit to suit your needs etc) here:
https://github.com/holochain-open-dev/holochain-client-csharp/tree/main/NextGenSoftware.Holochain.HoloNET.Client.TestHarness
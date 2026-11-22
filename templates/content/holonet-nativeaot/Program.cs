// Runs the HoloNET demo (HoloNETDemo.cs) and prints what happened. Publish a native, self-contained executable with: dotnet publish -c Release -r win-x64 (or linux-x64, osx-arm64)
Console.WriteLine(await HoloNETDemos.HoloNETDemo.RunAsync());

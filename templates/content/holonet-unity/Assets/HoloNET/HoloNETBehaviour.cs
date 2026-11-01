using UnityEngine;

// Add this component to any GameObject. It shows a button that runs the HoloNET demo (HoloNETDemo.cs) and the text it returns.
public class HoloNETBehaviour : MonoBehaviour
{
    private string output = "";
    private bool busy;

    private async void Run()
    {
        busy = true;
        output = "Running...";
        // NuGetForUnity keeps the HoloNET API package's oasis.happ under Assets/Packages: use it.
        var happ = System.IO.Directory.GetFiles(System.IO.Path.Combine(Application.dataPath, "Packages"), "oasis.happ", System.IO.SearchOption.AllDirectories);
        if (happ.Length > 0)
            HoloNETDemos.HoloNETDemo.HappPath = happ[0];
        output = await HoloNETDemos.HoloNETDemo.RunAsync();
        busy = false;
    }

    private void OnGUI()
    {
        GUI.enabled = !busy;
        if (GUI.Button(new Rect(20, 20, 260, 40), "Run the HoloNET demo"))
            Run();
        GUI.enabled = true;
        GUI.Label(new Rect(20, 70, Screen.width - 40, Screen.height - 90), output);
    }
}

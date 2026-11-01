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

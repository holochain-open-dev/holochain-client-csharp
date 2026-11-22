namespace HoloNETApp;

/// <summary>Drop this control onto any Windows Forms form. It runs the HoloNET demo (HoloNETDemo.cs) and shows what happened.</summary>
public class HoloNETControl : UserControl
{
    private readonly Button runButton = new() { Text = "Run the HoloNET demo", Dock = DockStyle.Top, Height = 32 };
    private readonly TextBox output = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9F) };

    public HoloNETControl()
    {
        Controls.Add(output);
        Controls.Add(runButton);
        runButton.Click += async (s, e) =>
        {
            runButton.Enabled = false;
            output.Text = "Running...";
            output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
            runButton.Enabled = true;
        };
    }
}

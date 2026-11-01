namespace HoloNETApp;

public partial class Form1 : Form
{
    private readonly Button runButton = new() { Text = "Run the HoloNET demo", AutoSize = true, Dock = DockStyle.Top, Padding = new Padding(8) };
    private readonly TextBox output = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, Font = new Font("Consolas", 10) };

    public Form1()
    {
        InitializeComponent();
        Text = "HoloNET";
        Controls.Add(output);
        Controls.Add(runButton);
        runButton.Click += async (s, e) =>
        {
            // Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened.
            runButton.Enabled = false;
            output.Text = "Running...";
            output.Text = (await HoloNETDemos.HoloNETDemo.RunAsync()).Replace("\n", "\r\n");
            runButton.Enabled = true;
        };
    }
}

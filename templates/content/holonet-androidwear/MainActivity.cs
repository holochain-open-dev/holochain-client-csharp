using Android.Widget;

namespace HoloNETApp;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        // A button that runs the HoloNET demo (HoloNETDemo.cs) and a text view for what happened.
        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
        layout.SetPadding(32, 64, 32, 32);
        var button = new Button(this) { Text = "Run the HoloNET demo" };
        var output = new TextView(this);
        layout.AddView(button);
        layout.AddView(output);
        SetContentView(layout);

        button.Click += async (s, e) =>
        {
            button.Enabled = false;
            output.Text = "Running...";
            output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
            button.Enabled = true;
        };
    }
}

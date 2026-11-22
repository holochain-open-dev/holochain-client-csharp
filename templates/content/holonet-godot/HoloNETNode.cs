using Godot;

// Attached to the Main scene. A button that runs the HoloNET demo (HoloNETDemo.cs) and a label for what happened.
public partial class HoloNETNode : Control
{
    public override void _Ready()
    {
        var box = new VBoxContainer();
        box.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(box);

        var button = new Button { Text = "Run the HoloNET demo" };
        var output = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, SizeFlagsVertical = SizeFlags.ExpandFill };
        box.AddChild(button);
        box.AddChild(output);

        button.Pressed += async () =>
        {
            button.Disabled = true;
            output.Text = "Running...";
            output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
            button.Disabled = false;
        };
    }
}

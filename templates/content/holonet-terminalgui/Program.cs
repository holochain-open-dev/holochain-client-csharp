using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

// A terminal UI: a button that runs the HoloNET demo (HoloNETDemo.cs) and a text area for what happened.
using IApplication app = Application.Create().Init();

var window = new Window { Title = "HoloNET (Esc to quit)" };
var button = new Button { Text = "Run the HoloNET demo", X = 1, Y = 1 };
var output = new TextView { X = 1, Y = 3, Width = Dim.Fill(1), Height = Dim.Fill(1), ReadOnly = true, WordWrap = true };

button.Accepting += async (s, e) =>
{
    e.Handled = true;
    output.Text = "Running...";
    output.Text = await HoloNETDemos.HoloNETDemo.RunAsync();
};

window.Add(button, output);
app.Run(window);

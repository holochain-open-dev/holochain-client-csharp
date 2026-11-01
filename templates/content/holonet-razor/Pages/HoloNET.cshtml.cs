using Microsoft.AspNetCore.Mvc.RazorPages;

namespace HoloNETApp.Pages;

public class HoloNETModel : PageModel
{
    public string Result { get; private set; } = "";

    // Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened.
    public async Task OnGetAsync() => Result = await HoloNETDemos.HoloNETDemo.RunAsync();
}

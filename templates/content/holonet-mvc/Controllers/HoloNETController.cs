using Microsoft.AspNetCore.Mvc;

namespace HoloNETApp.Controllers;

public class HoloNETController : Controller
{
    // Runs the HoloNET demo (HoloNETDemo.cs) and shows what happened.
    public async Task<IActionResult> Index() => View((object)await HoloNETDemos.HoloNETDemo.RunAsync());
}

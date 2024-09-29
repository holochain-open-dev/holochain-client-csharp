using NextGenSoftware.CLI.Engine;

namespace NextGenSoftware.Holochain.HoloNET.API.TestHarness
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("NextGenSoftware HoloNET API Test Harness v1.0");
            Console.WriteLine("");
            Task.Run(RunHoloNETAPITests);
        }

        static async Task RunHoloNETAPITests()
        {
            CLIEngine.ShowWorkingMessage("Saving Data...");
            HoloNETAPIResult<bool> saveResult = await HoloNETAPI.SaveKeyValuePairAsync("testkey", "testvalue");

            if (saveResult != null && saveResult.IsSuccess)
                CLIEngine.ShowSuccessMessage("Data Saved Successfully.");
            else
                CLIEngine.ShowErrorMessage($"Error Saving Data. Reason: {saveResult.Message}");


            CLIEngine.ShowWorkingMessage("Loading Data...");
            HoloNETAPIResult<string> loadResult = await HoloNETAPI.LoadDataAsync("testkey");

            if (loadResult != null && loadResult.IsSuccess)
                CLIEngine.ShowSuccessMessage($"Data Loaded Successfully. Data: {loadResult.Result}");
            else
                CLIEngine.ShowErrorMessage($"Error Loading Data. Reason: {loadResult.Message}");


            TestObject testObject = new TestObject() { FirstName = "David", LastName = "Ellams" };

            CLIEngine.ShowWorkingMessage("Saving Object...");
            saveResult = await HoloNETAPI.SaveObjectAsync("testobjectkey", testObject);

            if (saveResult != null && saveResult.IsSuccess)
                CLIEngine.ShowSuccessMessage("Object Saved Successfully.");
            else
                CLIEngine.ShowErrorMessage($"Error Saving Object. Reason: {saveResult.Message}");


            CLIEngine.ShowWorkingMessage("Loading Object...");
            HoloNETAPIResult<TestObject> loadObjectResult = await HoloNETAPI.LoadObjectAsync<TestObject>("testobjectkey");

            if (loadObjectResult != null && loadObjectResult.IsSuccess)
                CLIEngine.ShowSuccessMessage($"Object Loaded Successfully. FirstName: {loadObjectResult.Result.FirstName}, LastName: {loadObjectResult.Result.LastName}");
            else
                CLIEngine.ShowErrorMessage($"Error Loading Object. Reason: {loadObjectResult.Message}");
        }
    }
}

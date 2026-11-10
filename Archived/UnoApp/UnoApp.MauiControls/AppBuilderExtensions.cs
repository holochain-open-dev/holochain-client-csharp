namespace UnoApp;

public static class AppBuilderExtensions
{
    public static MauiAppBuilder UseMauiControls(this MauiAppBuilder builder) =>
        builder
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("UnoApp/Assets/Fonts/OpenSansRegular.ttf", "OpenSansRegular");
                fonts.AddFont("UnoApp/Assets/Fonts/OpenSansSemibold.ttf", "OpenSansSemibold");
            });
}

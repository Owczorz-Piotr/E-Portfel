using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Portfel
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }

    public class DatabaseService
    {
        public string connectionString = "Server=DESKTOP-153MQTB\\SQLEXPRESS;Database=Portfel;Integrated Security=True;TrustServerCertificate=True;";
        

    } 
}
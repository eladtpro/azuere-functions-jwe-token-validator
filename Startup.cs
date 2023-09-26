using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Logging;

[assembly: FunctionsStartup(typeof(MyNamespace.Startup))]

namespace MyNamespace;
public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        //builder.Services.AddHttpClient();

        //builder.Services.AddSingleton<IMyService>((s) => {
        //    return new MyService();
        //});

        //builder.Services.AddSingleton<ILoggerProvider, MyLoggerProvider>();

        bool isDeveloopment = builder.GetContext().Configuration["Environment"] == "Development";

        if (isDeveloopment)
            IdentityModelEventSource.ShowPII = true;

        //builder.Services.AddMvc()
        //.AddJsonOptions(
        //    options => options.SerializerSettings.ReferenceLoopHandling =
        //    Newtonsoft.Json.ReferenceLoopHandling.Ignore);
    }
}
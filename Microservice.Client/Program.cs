using Microservice.Client;
using Microservice.Client.Bootstrap;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Single composition root. Each concern registers itself via an extension method so
// Program.cs stays declarative and feature modules are added in one place.
builder.Services
    .AddClientCore(builder.Configuration, builder.HostEnvironment.BaseAddress)
    .AddClientInfrastructure(builder.Configuration)
    .AddClientFeatures();

await builder.Build().RunAsync();

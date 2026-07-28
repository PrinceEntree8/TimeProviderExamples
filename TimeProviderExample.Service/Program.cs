using Microsoft.AspNetCore.Mvc;
using TimeProviderExample.Service;
using TimeProviderExample.Service.Serializers;
using TimeProviderExample.Service.Services;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializer.Default);
});

builder.Services.AddSingleton<TimeProviderService>();
builder.Services.AddSingleton<UdpClientService>();

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.MapGet("/time", (TimeProviderService timeProviderService) => timeProviderService.GetUtcNow());
app.MapPost("/time", ([FromBody]DateTimeOffset dateTime, TimeProviderService timeProviderService) =>
{
    timeProviderService.SetStartTime(dateTime);
});

app.MapGet("/time/scale", (TimeProviderService timeProviderService) => timeProviderService.Scale);
app.MapPost("/time/scale", ([FromBody]double scale,TimeProviderService timeProviderService) => timeProviderService.Scale = scale);

app.Run();
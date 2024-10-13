using System.Net;
using Contracts;
using MassTransit;
using Polly;
using Polly.Extensions.Http;
using SearchService_controllers.Consumers;
using SearchService_controllers.Data;
using SearchService_controllers.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpClient<AuctionSvcHttpClient>().AddPolicyHandler(GetPolicy());
builder.Services.AddMassTransit(x =>
{
    x.AddConsumersFromNamespaceContaining<AuctionCreatedConsumer>();

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("search", false));

    x.UsingRabbitMq((contex, configuration) =>
    {
        configuration.ReceiveEndpoint("search-auction-created", e => 
        {
            e.UseMessageRetry(r => r.Interval(5, 5));

            e.ConfigureConsumer<AuctionCreatedConsumer>(contex);
        });

        configuration.ConfigureEndpoints(contex);
    });
});

var app = builder.Build();

//Configure the HTTP request pipeline
app.UseAuthorization();

app.MapControllers();

app.Lifetime.ApplicationStarted.Register(async () =>
{
    try
    {
        await DbInitializer.InitDb(app);
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex);
    }
});

app.Run();

static IAsyncPolicy<HttpResponseMessage> GetPolicy()
    => HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound)
    .WaitAndRetryForeverAsync(_ => TimeSpan.FromSeconds(3));

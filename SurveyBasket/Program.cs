using Hangfire;
using Hangfire.Dashboard.BasicAuthorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
//using Serilog;
using SurveyBasket;
using SurveyBasket.MiddleWare;
using SurveyBasket.Persistence;
using SurveyBasket.Services.Implementation;
using SurveyBasket.Services.OptionsPattern;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
//builder.Services.AddIdentityApiEndpoints<Application_User>()
//    .AddEntityFrameworkStores<ApplicationDbContext>();
//    .AddDefaultTokenProviders();


builder.Services.AddDistributedMemoryCache();
//builder.Host.UseSerilog((context,  configuration) =>
//{
//    configuration.ReadFrom.Configuration(context.Configuration);


//});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[]
    {
        new BasicAuthAuthorizationFilter(new BasicAuthAuthorizationFilterOptions
        {
            Users = new[]
            {
                new BasicAuthAuthorizationUser
                {
                    Login = app.Configuration.GetValue<string>("HangFireSettings:UserName"),
                    PasswordClear = app.Configuration.GetValue<string>("HangFireSettings:Password")
                }
            }
        })
    },
    DashboardTitle = "Survey Basket Hangfire Dashboard",
});
RecurringJob.AddOrUpdate<INotifcationService>(
    "SendPollNotifications",
    service => service.SendNewPollNotficationsAsync(null),
    Cron.Daily
);


app.UseAuthentication();
app.UseAuthorization();
//app.MapIdentityApi<Application_User>();
app.MapControllers();
//app.UseMiddleware<ExecptionHandlingMiddleWare>();
app.UseExceptionHandler();
app.Run();

// Make Program class accessible for testing

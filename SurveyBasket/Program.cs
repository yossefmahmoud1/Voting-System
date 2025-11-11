using Microsoft.Extensions.DependencyInjection;
using SurveyBasket;
using SurveyBasket.Persistence;
using SurveyBasket.Services.OptionsPattern;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);
//builder.Services.AddIdentityApiEndpoints<Application_User>()
//    .AddEntityFrameworkStores<ApplicationDbContext>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
//app.MapIdentityApi<Application_User>();
app.MapControllers();

app.Run();

// Make Program class accessible for testing
public partial class Program { }
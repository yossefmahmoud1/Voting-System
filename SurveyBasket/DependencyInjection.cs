using FluentValidation.AspNetCore;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SurveyBasket.Entities;
using SurveyBasket.Persistence;
using SurveyBasket.Repositeryes.Implementation;
using SurveyBasket.Repositeryes.Interfaces;
using SurveyBasket.Services.Implementation;
using SurveyBasket.Services.Interfaces;
using SurveyBasket.Services.OptionsPattern;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Options;

namespace SurveyBasket;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddControllers();
        services.AddAuthConf(configuration);
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        services
            .AddSwaggerServices()
            .AddMapsterConf()
            .AddFluentValidationConf();


        services.AddOptions<JwtOptions>()
             .Bind(configuration.GetSection("JwtOptions"))
             .ValidateDataAnnotations()
             .ValidateOnStart();





        // Register Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // Register Services
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddExceptionHandler<GlobalExecptionHandler>();
        services.AddProblemDetails();   
        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }

    public static IServiceCollection AddMapsterConf(this IServiceCollection services) 
    {
        var mappingConfig = TypeAdapterConfig.GlobalSettings;
        mappingConfig.Scan(Assembly.GetExecutingAssembly());

        services.AddSingleton<IMapper>(new Mapper(mappingConfig));

        return services;
    }

    public static IServiceCollection AddFluentValidationConf(this IServiceCollection services)
    {
        services
            .AddFluentValidationAutoValidation()
            .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        return services;
    }
    private static IServiceCollection AddAuthConf(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<Application_User, IdentityRole>(options =>
        {
            // Configure password rules explicitly
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>();

        // Configure JWT Authentication
        var jwtOptionsSection = configuration.GetSection("JwtOptions");
        var secretKey = jwtOptionsSection["secretKey"] 
            ?? throw new InvalidOperationException("JWT secretKey is missing!");
        var issuer = jwtOptionsSection["issuer"] 
            ?? throw new InvalidOperationException("JWT issuer is missing!");
        var audience = jwtOptionsSection["audience"] 
            ?? throw new InvalidOperationException("JWT audience is missing!");

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });

        return services;
    }
}
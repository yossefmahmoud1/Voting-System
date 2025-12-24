using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;
using FluentValidation.AspNetCore;
using Hangfire;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SurveyBasket.Entities;
using SurveyBasket.PermissionsAuth;
using SurveyBasket.Persistence;
using SurveyBasket.PremisonsAuth;
using SurveyBasket.Repositeryes.Implementation;
using SurveyBasket.Repositeryes.Interfaces;
using SurveyBasket.Services.Implementation;
using SurveyBasket.Services.Interfaces;
using SurveyBasket.Services.OptionsPattern;
using SurveyBasket.Settings;

namespace SurveyBasket;

public static class DependencyInjection
{
    public static IServiceCollection AddDependencies(this IServiceCollection services,
        IConfiguration configuration)
    {
        
        services.AddControllers();
        // Add Authentication and Authorization
        services.AddAuthConf(configuration);
        var connectionString = configuration.GetConnectionString("DefaultConnection") ??
            throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));
        // Add Swagger Services
        services
            .AddSwaggerServices()
            .AddMapsterConf()
            .AddFluentValidationConf();

        // JWT Options Configuration
        services.AddOptions<JwtOptions>()
             .Bind(configuration.GetSection("JwtOptions"))
             .ValidateDataAnnotations()
             .ValidateOnStart();
        // Rate Limiting Configuration
        services.AddRateLimiter(ratelimitoptions =>
        {
            ratelimitoptions.RejectionStatusCode = 429;
            ratelimitoptions.AddConcurrencyLimiter("concurrency", Options =>
            {
                Options.PermitLimit = 100;
                Options.QueueLimit = 50;
                Options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            });

        });

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        services.AddHangfireservice(configuration);
        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>()
            .AddHangfire(options =>
            {
                options.MinimumAvailableServers = 1;
            });

        // Register Repositories
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped<IUserRepository, UserRepository>();

        // Register Services
        services.AddScoped<IPollService, PollService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IVoteService, VoteService>();
        services.AddScoped<IResultService, ResultService>();
        services.AddScoped<ICasheService, CasheService>();
        services.AddScoped<IEmailSender, EmailService>();
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<INotifcationService, NotifcationService>();
        services.AddHttpContextAccessor();
        services.AddScoped<IQuestionService, QuestionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddExceptionHandler<GlobalExecptionHandler>();
        services.AddScoped<ITokenClaimsBuilder, TokenClaimsBuilder>();
        services.AddScoped<IUsersServices, UsersServices>();

        services.AddProblemDetails();   
        services.Configure<MailSettings>(configuration.GetSection(nameof(MailSettings)));
        return services;
    }

    public static IServiceCollection AddSwaggerServices(this IServiceCollection services)
    {
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "Voting System Api",
                Description = "An ASP.NET Core Web API for Voting",
                Contact = new OpenApiContact
                {
                    Name = " Email",
                    Url = new Uri("https://example.com/contact")
                },
                License = new OpenApiLicense
                {
                    Name = "Example License",
                    Url = new Uri("https://example.com/license")
                }
            });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "Enter: Bearer {your JWT token}"
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
        });
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
        services.AddIdentity<Application_User, ApplicationRole>(options =>
        {
            // Configure password rules explicitly
            options.Password.RequiredLength = 6;
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

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

        services.Configure<IdentityOptions>(options =>
        {
            options.Password.RequiredLength = 8;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = true;
        });

        services.Configure<IdentityOptions>(options =>
        {
            options.Lockout.AllowedForNewUsers = true;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
        });

        return services;
    }
    private static IServiceCollection AddHangfireservice(this IServiceCollection services, IConfiguration configuration)
    {
        // Add Hangfire services.
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection")));

        // Add the processing server as IHostedService
        services.AddHangfireServer();
        return services;

    }
    }
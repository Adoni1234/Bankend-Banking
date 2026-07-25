using System.Text;
using ArtemisBanking.Core.Application.Interfaces;
using ArtemisBanking.Core.Domain.Interfaces;
using ArtemisBanking.Core.Domain.Settings;
using ArtemisBanking.Infrastructure.Identity.Context;
using ArtemisBanking.Infrastructure.Identity.Entities;
using ArtemisBanking.Infrastructure.Identity.Seeds;
using ArtemisBanking.Infrastructure.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;


// Para que la siguiente clase funcionase, tuve que agregar en el CSPROJ de esta biblioteca de clases lo siguiente
//  <FrameworkReference Include="Microsoft.AspNetCore.App" />
namespace ArtemisBanking.Infrastructure.Identity
{
    public static class ServiceRegistration
    {
        // Extension method - Decorator pattern
        public static void AddIdentityLayerIocForWebApp(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services, config);

            // Identity options configuration
            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;

                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opt.Lockout.MaxFailedAccessAttempts = 5;

                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
            });

            // Identity setup
            services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddSignInManager<SignInManager<AppUser>>() // Especificar tipo aquí
                .AddEntityFrameworkStores<IdentityContext>()
                .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

            // Token lifespan
            services.Configure<DataProtectionTokenProviderOptions>(opt =>
            {
                opt.TokenLifespan = TimeSpan.FromHours(12);
            });

            // Authentication setup
            services.AddAuthentication(opt =>
            {
                opt.DefaultScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
                opt.DefaultSignInScheme = IdentityConstants.ApplicationScheme;
            })
            .AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(180);
                opt.LoginPath = "/Login";
                opt.AccessDeniedPath = "/Login/AccessDenied";
            });

            services.AddScoped<IBaseAccountService, AccountServiceForWebApp>();
            services.AddScoped<IAccountServiceForWebApp, AccountServiceForWebApp>();
        }
        
        public static async Task RunIdentitySeedAsync(this IServiceProvider service)
        {
            using var scope = service.CreateScope();
            var services = scope.ServiceProvider;
            
            var userManager =  services.GetRequiredService<UserManager<AppUser>>();
            var roleManager =  services.GetRequiredService<RoleManager<IdentityRole>>();
            var commerceRepository = services.GetService<ICommerceRepository>();
            var savingAccountRepository = services.GetService<ISavingAccountRepository>();
            
            await DefaultRoles.SeedAsync(roleManager);
            await DefaultAdminUser.SeedAsync(userManager);
            await DefaultAtmUser.SeedAsync(userManager);
            await DefaultClientUser.SeedAsync(userManager, savingAccountRepository);
            await DefaultCommerceUser.SeedAsync(userManager, commerceRepository, savingAccountRepository);
        }

        // Extension method para API con JWT
        public static void AddIdentityLayerIocForWebApi(this IServiceCollection services, IConfiguration config)
        {
            GeneralConfiguration(services, config);

            // Identity options configuration
            services.Configure<IdentityOptions>(opt =>
            {
                opt.Password.RequiredLength = 8;
                opt.Password.RequireDigit = true;
                opt.Password.RequireNonAlphanumeric = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;

                opt.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                opt.Lockout.MaxFailedAccessAttempts = 5;

                opt.User.RequireUniqueEmail = true;
                opt.SignIn.RequireConfirmedEmail = true;
            });

            // Identity setup
            services.AddIdentityCore<AppUser>()
                .AddRoles<IdentityRole>()
                .AddSignInManager<SignInManager<AppUser>>()
                .AddEntityFrameworkStores<IdentityContext>()
                .AddTokenProvider<DataProtectorTokenProvider<AppUser>>(TokenOptions.DefaultProvider);

            // JWT Settings
            var jwtSettings = new JwtSettings
            {
                SecretKey = null,
                Issuer = null,
                Audience = null,
                DurationInMinutes = 0
            };
            config.GetSection("JwtSettings").Bind(jwtSettings);
            services.Configure<JwtSettings>(config.GetSection("JwtSettings"));

            // JWT Authentication
            services.AddAuthentication(opt =>
            {
                opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.RequireHttpsMetadata = false;
                opt.SaveToken = true;
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = jwtSettings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtSettings.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    // El JWT emitido en GenerateJwtToken mete los roles en un claim custom "roles"
                    // (no en ClaimTypes.Role), asi que sin esto [Authorize(Roles = "...")] nunca autoriza a nadie.
                    RoleClaimType = "roles"
                };
                
                opt.Events = new JwtBearerEvents()
                {
                    OnAuthenticationFailed = af =>
                    {
                        af.NoResult();
                        return Task.CompletedTask;
                    },
                    OnChallenge = c =>
                    {
                        c.HandleResponse();
                        c.Response.StatusCode = 401;
                        c.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new { HasError = true, Error = "You are not Authorized" });
                        return c.Response.WriteAsync(result);
                    },
                    OnForbidden = c =>
                    {
                        c.Response.StatusCode = 403;
                        c.Response.ContentType = "application/json";
                        var result = JsonConvert.SerializeObject(new { HasError = true, Error = "You are not Authorized to access this resource" });
                        return c.Response.WriteAsync(result);
                    }
                };
            }).AddCookie(IdentityConstants.ApplicationScheme, opt =>
            {
                opt.ExpireTimeSpan = TimeSpan.FromMinutes(180);           
            });
            

            services.AddScoped<IBaseAccountService, AccountServiceForWebApi>();
            services.AddScoped<IAccountServiceForWebApi, AccountServiceForWebApi>();
        }

        private static void GeneralConfiguration(IServiceCollection services, IConfiguration config)
        {
            var connectionString = config.GetConnectionString("IdentityConnection");
            services.AddDbContext<IdentityContext>(
                opt =>
                {
                    opt.EnableSensitiveDataLogging();
                    opt.UseSqlServer(connectionString,
                        m => m.MigrationsAssembly(typeof(IdentityContext).Assembly.FullName));
                },
                contextLifetime: ServiceLifetime.Scoped,
                optionsLifetime: ServiceLifetime.Scoped
            );
        }
    }
}
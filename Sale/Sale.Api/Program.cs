using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CoffeeCode.DataBase.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Sale.Database.Entities;
using Sale.Security;
using Sale.ServiceLayer;

namespace Sale.Api
{
    public class Program
    {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.InitializeSqlServerDatabase<SaleDbContext>(connectionString);

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => {
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.SignIn.RequireConfirmedPhoneNumber = false;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 4;
                options.Password.RequiredUniqueChars = 0;
                options.User.RequireUniqueEmail = false;
            }).AddEntityFrameworkStores<SaleDbContext>()
                 .AddDefaultTokenProviders();

            builder.Services.AddHttpContextAccessor();
            ServiceLayerInitializer.Initialize(builder.Services);

            builder.Services.AddControllers();

            builder.Services.AddCors(options => {
                options.AddPolicy(name: "AllowSpecificOrigins",
                    policyBuilder => {
                        policyBuilder.WithOrigins("http://localhost:4200")
                            //.AllowAnyOrigin()
                            .AllowAnyHeader()
                            .AllowAnyMethod()
                            .AllowCredentials();
                    });
            });

            builder.Services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options => {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = builder.Configuration["JWT:ValidAudience"],
                    ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"] ?? ""))
                };
                options.Events = new JwtBearerEvents() {
                    OnMessageReceived = (context) => {
                        if (context.Request.Cookies.ContainsKey("token")) {
                            context.Token = context.Request.Cookies["token"];
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization(PolicyClaim.RegisterPolicies);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Sales API", Version = "v1" });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme() {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
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
                        new string[] {}
                    }
                });
            });

            var app = builder.Build();

            using (var scope = app.Services.CreateScope()) {
                var db = scope.ServiceProvider.GetRequiredService<SaleDbContext>();
                db.Database.Migrate();

                var adminUser = db.Users.FirstOrDefault(e => e.UserName == "admin");
                if (adminUser == null) {
                    var identityUser = new ApplicationUser {
                        UserName = "admin",
                        Email = "admin@mail.com"
                    };

                    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                    var result = userManager.CreateAsync(identityUser, "Admin@123").Result;
                    if (result.Succeeded) {
                        var claims = Claims.GetAll();
                        _ = userManager.AddClaimsAsync(identityUser, claims.Select(e => new System.Security.Claims.Claim(e.ClaimType, e.ClaimValue))).Result;
                    }
                }
            }

            app.UseCors("AllowSpecificOrigins");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment()) {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}

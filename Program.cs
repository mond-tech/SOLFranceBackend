using SOLFranceBackend.Data;
using SOLFranceBackend.Models;
using SOLFranceBackend.Service;
using SOLFranceBackend.Service.IService;
//using EventBus.Messages.MassTransit;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using SOLFranceBackend;
using Microsoft.OpenApi.Models;
using System.Text;
using SOLFranceBackend.Repository;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using SOLFranceBackend.CommandHandlers;
using FluentValidation.AspNetCore;
using FluentValidation;
//using Steeltoe.Discovery.Client;
//using Steeltoe.Discovery.Eureka;

var builder = WebApplication.CreateBuilder(args);

//Application Services
var assembly = typeof(Program).Assembly;
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
});

// Register FluentValidation
//builder.Services.AddValidatorsFromAssemblyContaining<CheckoutBasketCommandValidator>();
//builder.Services.AddFluentValidationAutoValidation();

// Add services to the container.
builder.Services.AddDbContext<AppDbContext>(option =>
{
    option.UseSqlServer(builder?.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddAutoMapper(typeof(MappingProfile));

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductDetailService, ProductDetailService>();
builder.Services.AddScoped<IShoppingCartRepository, ShoppingCartRepository>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection("ApiSettings:JwtOptions"));
builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultTokenProviders();
//builder.Services.AddServiceDiscovery(options => options.UseEureka());
builder.Services.AddControllers();
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(option =>
{
    option.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SOL France API",
        Version = "v1"
    });
    option.AddSecurityDefinition(name: JwtBearerDefaults.AuthenticationScheme, securityScheme: new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Enter the Bearer Authorization string as following: `Bearer Generated-JWT-Token`",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    option.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference= new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id=JwtBearerDefaults.AuthenticationScheme
                }
            }, new string[]{}
        }
    });
});

AppAuthentication();

builder.Services.AddAuthorization();

//Async Communication Services
//builder.Services.AddMessageBroker(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsProduction())
{

    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Auth API");
        c.RoutePrefix = string.Empty;
    });
}
    app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
ApplyMigration();
app.Run();

void ApplyMigration()
{
    using (var scope = app.Services.CreateScope())
    {
        var _db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        if (_db.Database.GetPendingMigrations().Count() > 0)
        {
            _db.Database.Migrate();
        }
    }
}

void AppAuthentication()
{

    var settingsSection = builder.Configuration.GetSection("ApiSettings2");

    if (settingsSection == null)
    {
        // Design-time / tooling execution (Swagger, EF, etc.)
        return;
    }

    var secret = settingsSection.GetValue<string>("Secret");
    if (string.IsNullOrWhiteSpace(secret))
    {
        // Design-time / tooling execution (Swagger, EF, etc.)
        return;
    }

    var issuer = settingsSection.GetValue<string>("Issuer");
    var audience = settingsSection.GetValue<string>("Audience");

    var key = Encoding.ASCII.GetBytes(secret);


    builder.Services.AddAuthentication(x =>
    {
        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).AddJwtBearer(x =>
    {
        x.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            ValidateAudience = true
        };
    });

}
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Product.Application.Features.Products.Commands.CreateProduct;
using Product.Application.Interfaces.Repositories;
using Product.Infrastructure.Context;
using Product.Infrastructure.Repositories;
using MassTransit;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host("localhost", "/", h =>
        {
            h.Username("guest");
            h.Password("guest");
        });
    });
});
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, 
            ValidateAudience = true,
            ValidateLifetime = true, 
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// Policy-Based ve Role-Based Authorization Kurallarý
builder.Services.AddAuthorization(options =>
{
    // Kural 1: Sadece kartýnda "Admin" yazanlar bu kuraldan geçebilir.
    options.AddPolicy("AdminOnlyPolicy", policy =>
        policy.RequireRole("Admin"));

    // Kural 2: Kartýnda "Admin" VEYA "Manager" (Yönetici) yazanlar bu kuraldan geçebilir.
    // (Farklý yetki seviyeleri oluþturma maddesini tam karþýlar)
    options.AddPolicy("ManagerOrAdminPolicy", policy =>
        policy.RequireRole("Admin", "Manager"));
});
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetSection("RedisCacheSettings:ConnectionString").Value;
    options.InstanceName = "Product_";
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // Swagger'a "Ben JWT Bearer þemasý kullanýyorum, bana bir buton ver" diyoruz
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Lütfen JWT Token'ýnýzý þu formatta girin: Bearer [boþluk] [token_metniniz]"
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

builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IProductRepository, ProductRepository>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(CreateProductCommand).Assembly));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
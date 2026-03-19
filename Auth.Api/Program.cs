using Auth.Application.Features.Auth.Commands.Register;
using Auth.Application.Interfaces;
using Auth.Infrastructure.Context;
using Auth.Infrastructure.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddIdentity<Auth.Domain.Entities.User, Microsoft.AspNetCore.Identity.IdentityRole<Guid>>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<Auth.Infrastructure.Context.AuthDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllers();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(RegisterCommand).Assembly));
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<Auth.Infrastructure.Context.AuthDbContext>();
    var userManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.UserManager<Auth.Domain.Entities.User>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<Microsoft.AspNetCore.Identity.RoleManager<Microsoft.AspNetCore.Identity.IdentityRole<Guid>>>();

    context.Database.Migrate();

    if (!await roleManager.RoleExistsAsync("Admin"))
        await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole<Guid>("Admin"));

    if (!await roleManager.RoleExistsAsync("User"))
        await roleManager.CreateAsync(new Microsoft.AspNetCore.Identity.IdentityRole<Guid>("User"));

    if (await userManager.FindByEmailAsync("admin@sirket.com") == null)
    {
        var adminUser = new Auth.Domain.Entities.User
        {
            UserName = "admin_user",
            Email = "admin@sirket.com",
            CreatedDate = DateTime.UtcNow
        };
        var result = await userManager.CreateAsync(adminUser, "admin123");

        if (result.Succeeded)
        {
            
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
    }
}

app.Run();

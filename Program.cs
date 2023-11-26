//Program.cs
using System.Reflection;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using netbusters.Data;
using netbusters.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<TokenService>();

// Configuring Entity Framework Core with Npgsql as the database provider.
// Connects to the database using connection string from the configuration.
builder.Services.AddDbContext<DatabaseContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adding controllers to the service collection.
builder.Services.AddControllers();

// Configure Swagger for API documentation with XML comments.
var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "NetBusters", Version = "v1" });
    c.IncludeXmlComments(xmlPath);

    // Define the OAuth2.0 scheme that Swagger will use
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

// Configure JWT Bearer Authentication using settings directly from appsettings.json.
var secretKey = builder.Configuration["JwtSettings:SecretKey"];
var issuer = builder.Configuration["JwtSettings:Issuer"];
var audience = builder.Configuration["JwtSettings:Audience"];

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidIssuer = issuer,
            ValidAudience = audience,
            NameClaimType = JwtRegisteredClaimNames.NameId, // Use 'sub' for the username
            RoleClaimType = "role"
        };
    });

// Adding MVC Controllers with Views (if used in the application).
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Apply pending database migrations at application startup.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Apply authentication and authorization middleware.
app.UseAuthentication();
app.UseAuthorization();

// Configure default route for MVC.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

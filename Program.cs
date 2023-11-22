using Microsoft.OpenApi.Models;
using Microsoft.EntityFrameworkCore;
using netbusters.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using netbusters.Models;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Bind JWT settings from the configuration to JwtSettings class.
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);

// Register JwtSettings as a singleton for dependency injection.
builder.Services.AddSingleton(jwtSettings);

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
});

// Configure JWT Bearer Authentication using settings from JwtSettings.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var secretKey = Encoding.UTF8.GetBytes(jwtSettings.SecretKey);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(secretKey),
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience
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

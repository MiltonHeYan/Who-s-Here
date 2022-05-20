using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WhosHereServer;
using WhosHereServer.Data;
using WhosHereServer.Services;

var builder = WebApplication.CreateBuilder(args);

#region Configure Services
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add database context
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    Console.WriteLine("Configuring db");
    string dbType = builder.Configuration.GetValue<string>("DbType");
    switch (dbType)
    {
        case "SqlServer":
            string connectionStringSqlS = builder.Configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(connectionStringSqlS, options =>
            {
                options.UseNetTopologySuite();
            });
            break;
        case "MySql":
            string connectionStringMy = builder.Configuration.GetConnectionString("DefaultConnection");
            var serverVersionMy = MySqlServerVersion.AutoDetect(connectionStringMy);
            options.UseMySql(connectionStringMy, serverVersionMy, options =>
            {
                options.UseNetTopologySuite();
            });
            break;
        case "MariaDb":
            string connectionStringMaria = builder.Configuration.GetConnectionString("DefaultConnection");
            var serverVersionMaria = MariaDbServerVersion.AutoDetect(connectionStringMaria);
            options.UseMySql(connectionStringMaria, serverVersionMaria, options =>
            {
                options.UseNetTopologySuite();
            });
            break;
        case "Sqlite":
            options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"), options =>
            {
                options.UseNetTopologySuite();
            });
            break;
        default:
            throw new InvalidAppSettingsException($"Not supported DbType: {dbType}");
    }
});
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Add identity system
builder.Services.AddIdentity<AppUser, IdentityRole>(opt =>
{
    opt.SignIn.RequireConfirmedAccount = true;
    opt.User.RequireUniqueEmail = true;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(opt =>
    {
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters()
        {
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JWT:Secret"])),
            ValidAudience = builder.Configuration["JWT:ValidAudience"],
            ValidIssuer = builder.Configuration["JWT:ValidIssuer"]
        };
        if (builder.Environment.IsDevelopment())
        {
            opt.TokenValidationParameters.ValidateIssuer = false;
            opt.TokenValidationParameters.ValidateAudience = false;
        }
        else
        {
            opt.TokenValidationParameters.ValidateIssuer = true;
            opt.TokenValidationParameters.ValidateAudience = true;
        }
    });

builder.Services.AddSingleton<IChatRelayService, ChatRelayService>();
#endregion

var app = builder.Build();

#region Configure HTTP request pipeline
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.UseWebSockets();
#endregion

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<ApplicationDbContext>();
    context.Database.EnsureCreated();
}
    

app.Run();

using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Todo_App.Configuration;
using Todo_App.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Register the JwtConfig instance
var _JwtConfig = builder.Configuration.GetSection("JwtConfig");
builder.Services.Configure<JwtConfig>(_JwtConfig);

// Register the database context
builder.Services.AddDbContext<ApiDbContext>(options =>
	options.UseSqlite(
		builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Getting jwt secret from appsetting.json
// and create the tokenValidation parameters
var _JwtSecret = builder.Configuration.GetValue<string>("JwtConfig:Secret");

var tokenValidationParameters = new TokenValidationParameters
{
	ValidateIssuerSigningKey = true,
	IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSecret)),
	ValidateIssuer = false,
	ValidateAudience = false,
	RequireExpirationTime = false,
	ValidateLifetime = true
};

// Inject tokenValidationParameters into our DI container
builder.Services.AddSingleton(tokenValidationParameters);

// Add or Register JWT Authentication
builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(jwt =>
{
	jwt.SaveToken = true;
	jwt.RequireHttpsMetadata = true;
	jwt.TokenValidationParameters = tokenValidationParameters;
});

builder.Services.AddDefaultIdentity<IdentityUser>(
	options => options.SignIn.RequireConfirmedAccount = true)
.AddEntityFrameworkStores<ApiDbContext>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

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

app.Run();

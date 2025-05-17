using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using System.Reflection;
using InnoSupabaseAuthentication.Services;
using InnoTrains.Models.Game;
using InnoTrains.Models.Game.Networking;
using InnoTrains.Models.Data;

namespace InnoTrains
{
	public class Program
	{
		private static IConfiguration _config;

		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);
			_config = builder.Configuration;
			ConfigureServices(builder.Services);

			var app = builder.Build();
			Configure(app, app.Environment);

			app.Run();
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			services.Configure<EngineOptions>(_config.GetSection("InnoTrainsEngine"));
			services.Configure<WebsocketEngineOptions>(_config.GetSection("WebsocketEngine"));

			services.Configure<JSONGameDataOptions>(_config.GetSection("JSONGameDataProvider"));
			services.Configure<JSONLobbyDataOptions>(_config.GetSection("JSONLobbyDataProvider"));

			services.AddControllers();

			Assembly authEndpointAssembly = Assembly.Load("SupabaseAuthentication.Endpoints");
			services.AddMvc().AddApplicationPart(authEndpointAssembly).AddControllersAsServices();

			services.AddAuthorization();

			byte[] bytes = Encoding.UTF8.GetBytes(_config["Authentication:JwtSecret"]!);
			services.AddAuthentication().AddJwtBearer(options =>
			{
				options.TokenValidationParameters = new TokenValidationParameters()
				{
					IssuerSigningKey = new SymmetricSecurityKey(bytes),
					ValidAudience = _config["Authentication:Audience"],
					ValidIssuer = _config["Authentication:ValidIssuer"]
				};
			});
			services.AddTransient<IAuthProvider, SupabaseAuthProvider>();
			services.AddSingleton<AuthenticationService>();
		}

		private static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{ 
			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(config =>
			{
				config.MapControllerRoute("Api", "api/{controller}/{action}/{id?}");
			});

			app.UseHsts();
		}
	}
}

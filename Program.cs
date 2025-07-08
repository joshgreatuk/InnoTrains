using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Reflection;
using System.Text.Json;
using InnoSupabaseAuthentication.Services;
using InnoTrains.Models.Game;
using InnoTrains.Models.Game.Networking;
using InnoTrains.Models.Data;
using InnoTrains.Services;
using InnoTrains.Services.Data;

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

			Initialize(app.Services);

			Configure(app, app.Environment);

			app.Run();

			Stop(app.Services);
		}

		private static void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton(new JsonSerializerOptions() { WriteIndented = true });
			
			services.Configure<EngineOptions>(_config.GetSection("InnoTrainsEngine"));
			services.Configure<WebsocketEngineOptions>(_config.GetSection("WebsocketEngine"));

			services.Configure<JSONGameDataOptions>(_config.GetSection("JSONGameDataProvider"));
			services.Configure<JSONLobbyDataOptions>(_config.GetSection("JSONLobbyDataProvider"));

			services.AddSingleton<ILobbyDataProvider, JsonLobbyDataProvider>();
			services.AddSingleton<LobbyService>();

			services.AddControllers();

			Assembly authEndpointAssembly = Assembly.GetAssembly(typeof(InnoSupabaseAuthentication.Services.AuthenticationService));
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

		private static void Initialize(IServiceProvider services)
		{
			foreach (IInitializable initService in services.GetServices<IInitializable>())
			{
				try
				{
					initService.Initialize();
				}
				catch (Exception)
				{
					Console.WriteLine($"There was an exception while initializing service: {initService.GetType().FullName}");
					throw;
				}
			}
		}

		private static void Stop(IServiceProvider services)
		{
			foreach (IStoppable stopService in services.GetServices<IStoppable>())
			{
				try
				{
					stopService.Stop();
				}
				catch (Exception exception)
				{
					Console.WriteLine($"There was an exception while stopping service: {stopService.GetType().FullName}");
					Console.WriteLine(exception);
				}
			}
		}
	}
}

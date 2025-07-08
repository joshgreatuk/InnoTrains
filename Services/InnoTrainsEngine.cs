using InnoTrains.Models.Game;
using InnoTrains.Services.Game;
using InnoTrains.Services.Game.Networking;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

namespace InnoTrains.Services
{
	public delegate void GameStatusChanged(GameStatus newStats, GameStatus oldStatus);

	/// <summary>
	/// The back-end game engine behind InnoTrains, this will drive the game, main events will include:
	/// - Init (Initialize all services and data handlers, every service should be of IGameService)
	/// - Start (After Init)
	/// - Update (Each tick)
	/// - Pause (stop active processing)
	/// - Unpause (resume active processing)
	/// - Exit (Shutdown method, after pause)
	/// 
	/// Since this is a multiplayer-focused engine there will always be a readonly reference to INetworkEngine
	/// This will also be in the service collection but will need to be initialized and handled separately from Init and Start
	/// 
	/// Other services will be contained in an IServiceProvider
	/// 
	/// Services must use the 'lock' keyword when accessing game data as to ensure multi-threading safety
	/// </summary>
	public class InnoTrainsEngine
	{
		public event GameStatusChanged OnStatusChanged;

		public GameOptions GameOptions { get; private set; }

		public INetworkEngine NetworkEngine { get; private set; }
		public IServiceProvider GameServices { get; private set; }

		private readonly EngineOptions EngineOptions;

		private GameStatus status;
		private Task UpdateLoopTask;

		public GameStatus Status 
		{
			get
			{
				return status;
			}
			set 
			{
				GameStatus oldValue = status;
				status = value;
				//If there is an error with the event invocation, game status still has to be changed
				OnStatusChanged.Invoke(status, oldValue);
			} 
		}

		public InnoTrainsEngine(EngineOptions options)
		{
			EngineOptions = options;
		}

		public void Start(INetworkEngine networkEngine, GameOptions options)
		{
			NetworkEngine = networkEngine;
			GameOptions = options;

			GameServices = BuildServiceCollection();

			NetworkEngine.Start(GameServices);

			foreach (BaseGameService service in GameServices.GetServices<BaseGameService>())
			{
				service.Init();
			}

			foreach (BaseGameService service in GameServices.GetServices<BaseGameService>())
			{
				service.Start();
			}

			StartUpdateLoop();
		}

		public IServiceProvider BuildServiceCollection()
		{
			return new ServiceCollection()
				.AddSingleton(NetworkEngine)

				//Add game services here


				//Build
				.BuildServiceProvider();
		}

		public void Pause()
		{
			Status = GameStatus.PAUSED;
			foreach (BaseGameService service in GameServices.GetServices<BaseGameService>())
			{
				service.Pause();
			}
		}

		public void Unpause()
		{
			Status = GameStatus.LOADING;
			foreach (BaseGameService service in GameServices.GetServices<BaseGameService>())
			{
				service.Unpause();
			}
			Status = GameStatus.RUNNING;
			StartUpdateLoop();
		}

		public void Shutdown()
		{
			Pause();

			NetworkEngine.InteractionsUpdate(0d);

			Status = GameStatus.STOPPED;
			foreach (BaseGameService service in GameServices.GetServices<BaseGameService>())
			{
				service.Exit();
			}

			NetworkEngine.EventsUpdate(0d);
			NetworkEngine.Exit(GameServices);
		}

		public void StartUpdateLoop()
		{
			if (UpdateLoopTask != null && !UpdateLoopTask.IsCompleted)
			{
				return;
			}

			UpdateLoopTask = Task.Run(UpdateLoop);
		}

		public async Task UpdateLoop()
		{
			//TODO: Add logging
			double targetTickMsecs = 1000d / Convert.ToDouble(EngineOptions.TargetTPS);
			Stopwatch counter = new Stopwatch();
			double endMsecs = targetTickMsecs;

			//Cache list of gameservices for performance
			BaseGameService[] gameServices = GameServices.GetServices<BaseGameService>().ToArray();

			while (Status is GameStatus.RUNNING)
			{
				counter.Restart();

				double delta = endMsecs <= targetTickMsecs ? targetTickMsecs : endMsecs;
				//Do update

				NetworkEngine.InteractionsUpdate(delta);

				for (int i=0; i < gameServices.Length; i++)
				{
					gameServices[i].Update(delta);
				}

				NetworkEngine.EventsUpdate(delta);

				//Delay to next tick
				endMsecs = counter.Elapsed.TotalMilliseconds;
				await Task.Delay((int)Math.Abs(targetTickMsecs - endMsecs));
			}
		}
	}
}


using System;
using System.Diagnostics;
using System.IO;
using System.Security.Policy;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace iRacingTVController
{
	public static class Program
	{
		public const string IpcNameSettings = "iRacing-TV IPC Settings";
		public const string IpcNameLiveData = "iRacing-TV IPC Live Data";

		public const string MutexNameSettings = "iRacing-TV Mutex Settings";
		public const string MutexNameLiveData = "iRacing-TV Mutex Live Data";

		public const string AppName = "iRacing-TV";

		public static readonly string documentsFolder = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + $"\\{AppName}\\";

		public static DispatcherTimer dispatcherTimer = new( DispatcherPriority.Render );
		public static Stopwatch stopwatch = new();

		public static bool keepRunning = true;

		public static int tickMutex = 0;
		public static long elapsedMilliseconds = 0;
		public static float deltaTime = 0;

		public static Random random = new Random();

		public static void Initialize()
		{
			if ( !Directory.Exists( documentsFolder ) )
			{
				Directory.CreateDirectory( documentsFolder );
			}

			Directory.SetCurrentDirectory( documentsFolder );

			LogFile.Initialize();
			Settings.Initialize();
			IPC.Initialize();
			DataApi.Initialize( false );
			WebPage.Initialize();
			Controller.Initialize();
			SpeechToText.Initialize();
			PushToTalk.Initialize();

			Task.Run( () => ProgramAsync() );
		}

		private static void ProgramAsync()
		{
			try
			{
				LogFile.Write( "Starting async thread...\r\n" );

				stopwatch.Start();

				dispatcherTimer.Tick += ( sender, e ) => Tick( sender, e );
				dispatcherTimer.Interval = TimeSpan.FromSeconds( 1 / 60.0f );
				dispatcherTimer.Start();

				while ( keepRunning )
				{
					Thread.Sleep( 250 );
				}

				dispatcherTimer.Stop();

				stopwatch.Stop();

				LogFile.Write( "Async thread finished.\r\n" );
			}
			catch ( Exception exception )
			{
				LogFile.WriteException( exception );

				throw;
			}
		}

		private static void Tick( object? sender, EventArgs e )
		{
			int tickMutex = Interlocked.Increment( ref Program.tickMutex );

			if ( tickMutex == 1 )
			{
				if ( elapsedMilliseconds == 0 )
				{
					elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
				}
				else
				{
					deltaTime = Math.Min( 0.1f, ( stopwatch.ElapsedMilliseconds - elapsedMilliseconds ) / 1000.0f );

					elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

					Controller.Update();
					Settings.Update();
					IRSDK.Update();
					LiveData.Instance.Update();
					WebPage.Update();

					IPC.UpdateSettings();
					IPC.UpdateLiveData();

					Director.Update();
					SpeechToText.Update();
					PushToTalk.Update();

					SessionFlagsPlayback.Update();
					IncidentPlayback.Update();
					SubtitlePlayback.Update();

					MainWindow.Instance.ControlPanel_Update();

					IRSDK.SendMessages();
				}
			}

			Interlocked.Decrement( ref Program.tickMutex );
		}

		public static string GetTimeString( double timeInSeconds, bool includeMilliseconds )
		{
			TimeSpan time = TimeSpan.FromSeconds( timeInSeconds );

			if ( time.Hours > 0 )
			{
				return time.ToString( @"h\:mm\:ss" );
			}
			else if ( includeMilliseconds )
			{
				if ( time.Minutes > 0 )
				{
					return time.ToString( @"m\:ss\.fff" );
				}
				else
				{
					return time.ToString( @"ss\.fff" );
				}
			}
			else
			{
				return time.ToString( @"m\:ss" );
			}
		}
	}
}

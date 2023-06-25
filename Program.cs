
using System;
using System.IO;
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
		public const string AppNameSTT = "iRacing-STT-VR";

		public static readonly string documentsFolder = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + $"\\{AppName}\\";
		public static readonly string documentsFolderSTT = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + $"\\{AppNameSTT}\\";

		public static DispatcherTimer dispatcherTimer = new( DispatcherPriority.Render );

		public static bool keepRunning = true;

		public static int tickMutex = 0;

		public static void Initialize()
		{
			if ( !Directory.Exists( documentsFolder ) )
			{
				Directory.CreateDirectory( documentsFolder );
			}

			LogFile.Initialize();

			LogFile.Write( $"{MainWindow.Instance.Title} is starting up!\r\n\r\n" );

			Settings.Initialize();

			IPC.Initialize();

			Task.Run( () => ProgramAsync() );
		}

		private static void ProgramAsync()
		{
			try
			{
				dispatcherTimer.Tick += ( sender, e ) => Tick( sender, e );
				dispatcherTimer.Interval = TimeSpan.FromSeconds( 1 / 60.0f );
				dispatcherTimer.Start();

				while ( keepRunning )
				{
					Thread.Sleep( 250 );
				}

				dispatcherTimer.Stop();
			}
			catch ( Exception exception )
			{
				LogFile.WriteException( exception );
			}
		}

		private static void Tick( object? sender, EventArgs e )
		{
			int tickMutex = Interlocked.Increment( ref Program.tickMutex );

			if ( tickMutex == 1 )
			{
				IRSDK.Update();

				LiveData.Instance.Update();

				IPC.UpdateSettings();
				IPC.UpdateLiveData();

				Director.Update();
				IncidentScan.Update();

				MainWindow.Instance.ControlPanel_Update();

				IRSDK.SendMessages();
			}

			Interlocked.Decrement( ref Program.tickMutex );
		}
	}
}

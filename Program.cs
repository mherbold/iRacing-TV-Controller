
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace iRacingTVController
{
	public static class Program
	{
		public const string IpcName = "iRacing-TV IPC";
		public const string MutexName = "iRacing-TV Mutex";
		public const string AppName = "iRacing-TV";

		public static readonly string documentsFolder = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + $"\\{AppName}\\";
		public static readonly string fontsFolder = System.Environment.GetFolderPath( Environment.SpecialFolder.Fonts );

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
				dispatcherTimer.Tick += async ( sender, e ) => await TickAsync( sender, e );
				dispatcherTimer.Interval = TimeSpan.FromSeconds( 1 / 10.0f );
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

		private static async Task TickAsync( object? sender, EventArgs e )
		{
			int tickMutex = Interlocked.Increment( ref Program.tickMutex );

			if ( tickMutex == 1 )
			{
				IPC.Update();
			}

			Interlocked.Decrement( ref Program.tickMutex );
		}
	}
}

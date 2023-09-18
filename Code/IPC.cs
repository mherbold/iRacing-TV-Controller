
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Xml.Serialization;

namespace iRacingTVController
{
	public static class IPC
	{
		public const int MaxMemoryMappedFileSize = 1 * 1024 * 1024;

		public static Mutex? mutexSettings;
		public static Mutex? mutexLiveData;

		public static MemoryMappedFile? memoryMappedFileSettings = null;
		public static MemoryMappedFile? memoryMappedFileLiveData = null;

		public static MemoryMappedViewAccessor? memoryMappedViewAccessorSettings = null;
		public static MemoryMappedViewAccessor? memoryMappedViewAccessorLiveData = null;

		public static long indexSettings = DateTime.Now.Ticks;
		public static long indexLiveData = DateTime.Now.Ticks;

		public static bool readyToSendSettings = false;
		public static bool readyToSendLiveData = false;

		public static void Initialize()
		{
			LogFile.Write( "Initializing settings IPC...\r\n" );

			mutexSettings = new Mutex( false, Program.MutexNameSettings );
			memoryMappedFileSettings = MemoryMappedFile.CreateOrOpen( Program.IpcNameSettings, MaxMemoryMappedFileSize );
			memoryMappedViewAccessorSettings = memoryMappedFileSettings.CreateViewAccessor( 0, MaxMemoryMappedFileSize, MemoryMappedFileAccess.Write );

			LogFile.Write( "Initializing live data IPC...\r\n" );

			mutexLiveData = new Mutex( false, Program.MutexNameLiveData );
			memoryMappedFileLiveData = MemoryMappedFile.CreateOrOpen( Program.IpcNameLiveData, MaxMemoryMappedFileSize );
			memoryMappedViewAccessorLiveData = memoryMappedFileLiveData.CreateViewAccessor( 0, MaxMemoryMappedFileSize, MemoryMappedFileAccess.Write );
		}

		public static void Shutdown()
		{
			memoryMappedViewAccessorSettings?.Dispose();
			memoryMappedFileSettings?.Dispose();
			mutexSettings?.Dispose();

			memoryMappedViewAccessorLiveData?.Dispose();
			memoryMappedFileLiveData?.Dispose();
			mutexLiveData?.Dispose();
		}

		public static void UpdateSettings()
		{
			if ( readyToSendSettings && ( mutexSettings != null ) && ( memoryMappedViewAccessorSettings != null ) )
			{
				var xmlSerializer = new XmlSerializer( typeof( SettingsOverlay ) );

				var memoryStream = new MemoryStream();

				xmlSerializer.Serialize( memoryStream, Settings.overlay );

				var buffer = memoryStream.ToArray();

				var signalReceived = mutexSettings.WaitOne( 250 );

				if ( signalReceived )
				{
					indexSettings++;

					memoryMappedViewAccessorSettings.Write( 0, indexSettings );
					memoryMappedViewAccessorSettings.Write( 8, buffer.Length );
					memoryMappedViewAccessorSettings.WriteArray( 12, buffer, 0, buffer.Length );

					mutexSettings.ReleaseMutex();
				}

				readyToSendSettings = false;
			}
		}

		public static void UpdateLiveData()
		{
			if ( readyToSendLiveData && ( mutexLiveData != null ) && ( memoryMappedViewAccessorLiveData != null ) )
			{
				var xmlSerializer = new XmlSerializer( typeof( LiveData ) );

				var memoryStream = new MemoryStream();

				xmlSerializer.Serialize( memoryStream, LiveData.Instance );

				var buffer = memoryStream.ToArray();

				var signalReceived = mutexLiveData.WaitOne( 250 );

				if ( signalReceived )
				{
					indexLiveData++;

					memoryMappedViewAccessorLiveData.Write( 0, indexLiveData );
					memoryMappedViewAccessorLiveData.Write( 8, buffer.Length );
					memoryMappedViewAccessorLiveData.WriteArray( 12, buffer, 0, buffer.Length );

					mutexLiveData.ReleaseMutex();
				}

				readyToSendLiveData = false;
			}
		}
	}
}

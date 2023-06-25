
using System;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Xml.Serialization;

namespace iRacingTVController
{
	public static class IPC
	{
		public const int MAX_MEMORY_MAPPED_FILE_SIZE = 1 * 1024 * 1024;

		public static MemoryMappedFile? memoryMappedFileSettings = null;
		public static MemoryMappedFile? memoryMappedFileLiveData = null;

		public static long indexSettings = DateTime.Now.Ticks;
		public static long indexLiveData = DateTime.Now.Ticks;

		public static bool readyToSendSettings = false;
		public static bool readyToSendLiveData = false;

		public static void Initialize()
		{
			memoryMappedFileSettings = MemoryMappedFile.CreateOrOpen( Program.IpcNameSettings, MAX_MEMORY_MAPPED_FILE_SIZE );
			memoryMappedFileLiveData = MemoryMappedFile.CreateOrOpen( Program.IpcNameLiveData, MAX_MEMORY_MAPPED_FILE_SIZE );
		}

		public static void UpdateSettings()
		{
			if ( readyToSendSettings && ( memoryMappedFileSettings != null ) )
			{
				var xmlSerializer = new XmlSerializer( typeof( SettingsOverlay ) );

				var memoryStream = new MemoryStream();

				xmlSerializer.Serialize( memoryStream, Settings.overlay );

				var buffer = memoryStream.ToArray();

				if ( Mutex.TryOpenExisting( Program.MutexNameSettings, out var mutex ) )
				{
					mutex.WaitOne();
				}
				else
				{
					mutex = new Mutex( true, Program.MutexNameSettings, out var createdNew );

					if ( !createdNew )
					{
						mutex.WaitOne();
					}
				}

				indexSettings++;

				var viewAccessor = memoryMappedFileSettings.CreateViewAccessor( 0, MAX_MEMORY_MAPPED_FILE_SIZE );

				viewAccessor.Write( 0, indexSettings );
				viewAccessor.Write( 8, buffer.Length );
				viewAccessor.WriteArray( 12, buffer, 0, buffer.Length );
				viewAccessor.Dispose();

				mutex.ReleaseMutex();

				readyToSendSettings = false;
			}
		}

		public static void UpdateLiveData()
		{
			if ( readyToSendLiveData && ( memoryMappedFileLiveData != null ) )
			{
				var xmlSerializer = new XmlSerializer( typeof( LiveData ) );

				var memoryStream = new MemoryStream();

				xmlSerializer.Serialize( memoryStream, LiveData.Instance );

				var buffer = memoryStream.ToArray();

				if ( Mutex.TryOpenExisting( Program.MutexNameLiveData, out var mutex ) )
				{
					mutex.WaitOne();
				}
				else
				{
					mutex = new Mutex( true, Program.MutexNameLiveData, out var createdNew );

					if ( !createdNew )
					{
						mutex.WaitOne();
					}
				}

				indexLiveData++;

				var viewAccessor = memoryMappedFileLiveData.CreateViewAccessor( 0, MAX_MEMORY_MAPPED_FILE_SIZE );

				viewAccessor.Write( 0, indexLiveData );
				viewAccessor.Write( 8, buffer.Length );
				viewAccessor.WriteArray( 12, buffer, 0, buffer.Length );
				viewAccessor.Dispose();

				mutex.ReleaseMutex();

				readyToSendLiveData = false;
			}
		}
	}
}

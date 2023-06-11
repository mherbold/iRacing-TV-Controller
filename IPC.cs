
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Threading;
using System.Xml.Serialization;

namespace iRacingTVController
{
	public static class IPC
	{
		public static MemoryMappedFile? memoryMappedFile = null;

		public static uint index = 0;

		public static bool readyToSend = false;

		public static void Initialize()
		{
			memoryMappedFile = MemoryMappedFile.CreateOrOpen( Program.IpcName, 1 * 1024 * 1024 );
		}

		public static void Update()
		{
			if ( readyToSend && ( memoryMappedFile != null ) )
			{
				var combined = Settings.GetCombinedOverlay();

				if ( Mutex.TryOpenExisting( Program.MutexName, out var mutex ) )
				{
					mutex.WaitOne();
				}
				else
				{
					mutex = new Mutex( true, Program.MutexName, out var createdNew );

					if ( !createdNew )
					{
						mutex.WaitOne();
					}
				}

				index++;

				var viewAccessor = memoryMappedFile.CreateViewAccessor( 0, 4 );

				viewAccessor.Write( 0, index );

				viewAccessor.Dispose();

				var xmlSerializer = new XmlSerializer( typeof( SettingsOverlay ) );

				var memoryStream = new MemoryStream();

				xmlSerializer.Serialize( memoryStream, combined );

				var buffer = memoryStream.ToArray();

				viewAccessor = memoryMappedFile.CreateViewAccessor( 4, 4 );

				viewAccessor.Write( 0, buffer.Length );

				viewAccessor.Dispose();

				viewAccessor = memoryMappedFile.CreateViewAccessor( 8, buffer.Length );

				viewAccessor.WriteArray( 0, buffer, 0, buffer.Length );

				viewAccessor.Dispose();

				mutex.ReleaseMutex();

				readyToSend = false;
			}
		}
	}
}

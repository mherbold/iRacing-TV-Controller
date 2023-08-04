
using System;
using System.IO;
using System.Threading;

namespace iRacingTVController
{
	public static class LogFile
	{
		public static ReaderWriterLock readerWriterLock = new();

		public static string logFilePath = $"{Program.documentsFolder}{Program.AppName}.log";

		public static void Initialize()
		{
			if ( File.Exists( logFilePath ) )
			{
				File.Delete( logFilePath );
			}
		}

		public static void Write( string message )
		{
			try
			{
				readerWriterLock.AcquireWriterLock( 250 );

				File.AppendAllText( logFilePath, $"{DateTime.Now}   {message}" );
			}
			finally
			{
				readerWriterLock.ReleaseWriterLock();
			}
		}

		public static void WriteException( Exception exception )
		{
			Write( $"Exception caught!\r\n\r\n{exception.Message}\r\n\r\n{exception.StackTrace}\r\n\r\n" );
		}
	}
}

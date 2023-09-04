
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;

namespace iRacingTVController
{
	public static class LogFile
	{
		public static ReaderWriterLock readerWriterLock = new();

		public static string logFilePath = $"{Program.documentsFolder}{Program.AppName}.log";

		public static FileStream? fileStream = null;

		public static void Initialize()
		{
			fileStream = new FileStream( logFilePath, FileMode.Append, FileAccess.Write, FileShare.Read );

			Write( "\r\n" );
			Write( $"Log file opened.\r\n" );
		}

		public static void Write( string message )
		{
			if ( fileStream != null )
			{
				try
				{
					readerWriterLock.AcquireWriterLock( 250 );

					try
					{
						var bytes = new UTF8Encoding( true ).GetBytes( $"{DateTime.Now}   {message}" );

						fileStream.Write( bytes, 0, bytes.Length );
						fileStream.Flush();
					}
					finally
					{
						readerWriterLock.ReleaseWriterLock();
					}
				}
				catch ( ApplicationException )
				{
					Debug.WriteLine( "Could not acquire writer lock!" );
				}
			}
		}

		public static void WriteException( Exception exception )
		{
			Write( $"Exception caught!\r\n\r\n{exception.Message}\r\n\r\n{exception.StackTrace}\r\n\r\n" );
		}
	}
}

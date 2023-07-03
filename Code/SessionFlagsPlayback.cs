using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace iRacingTVController
{
	public static class SessionFlagsPlayback
	{
		public static string sessionFlagsPath = Program.documentsFolder + "SessionFlags";

		public static List<SessionFlagsData> sessionFlagsDataList = new();

		public static string sessionFlagsFilePath = string.Empty;
		public static StreamWriter? streamWriter = null;

		public static uint sessionFlags = 0;

		public static string GetSessionFlagsFilePath()
		{
			return $"{sessionFlagsPath}\\{IRSDK.normalizedSession.sessionId}-{IRSDK.normalizedSession.subSessionId}.csv";
		}

		public static void OpenForRecording()
		{
			var newSessionFlagsFilePath = GetSessionFlagsFilePath();

			if ( newSessionFlagsFilePath != sessionFlagsFilePath )
			{
				sessionFlagsFilePath = newSessionFlagsFilePath;

				sessionFlagsDataList = new List<SessionFlagsData>();

				if ( streamWriter != null )
				{
					streamWriter.Close();

					streamWriter = null;
				}

				Directory.CreateDirectory( sessionFlagsPath );

				streamWriter = File.AppendText( sessionFlagsFilePath );

				sessionFlags = 0;
			}
		}

		public static void LoadRecording()
		{
			var newSessionFlagsFilePath = GetSessionFlagsFilePath();

			if ( newSessionFlagsFilePath != sessionFlagsFilePath )
			{
				sessionFlagsFilePath = newSessionFlagsFilePath;

				sessionFlagsDataList.Clear();

				if ( File.Exists( sessionFlagsFilePath ) )
				{
					var streamReader = File.OpenText( sessionFlagsFilePath );

					while ( true )
					{
						var line = streamReader.ReadLine();

						if ( line == null )
						{
							break;
						}

						var match = Regex.Match( line, "(.*),(.*),0x(.*)" );

						if ( match.Success )
						{
							sessionFlagsDataList.Add( new SessionFlagsData( int.Parse( match.Groups[ 1 ].Value ), float.Parse( match.Groups[ 2 ].Value, CultureInfo.InvariantCulture.NumberFormat ), uint.Parse( match.Groups[ 3 ].Value, NumberStyles.HexNumber ) ) );
						}
					}
				}

				sessionFlagsDataList.Reverse();
			}
		}

		public static void Close()
		{
			sessionFlagsFilePath = string.Empty;

			sessionFlagsDataList.Clear();
		}

		public static void Record( int sessionNumber, double sessionTime, uint sessionFlags )
		{
			if ( SessionFlagsPlayback.sessionFlags != sessionFlags )
			{
				SessionFlagsPlayback.sessionFlags = sessionFlags;

				if ( streamWriter != null )
				{
					var sessionFlagsAsHex = sessionFlags.ToString( "X8" );

					streamWriter.WriteLine( $"{sessionNumber},{sessionTime:0.000},0x{sessionFlagsAsHex}" );

					streamWriter.Flush();
				}
			}
		}

		public static uint Playback( int sessionNumber, double sessionTime )
		{
			if ( sessionFlagsDataList != null )
			{
				foreach ( var sessionFlagData in sessionFlagsDataList )
				{
					if ( ( sessionNumber == sessionFlagData.sessionNumber ) && ( sessionTime >= sessionFlagData.sessionTime ) )
					{
						return sessionFlagData.sessionFlags;
					}
				}
			}

			return 0;
		}
	}
}

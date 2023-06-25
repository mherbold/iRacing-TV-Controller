
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace iRacingTVController
{
	public static class ChatLogPlayback
	{
		public static List<ChatLogData> chatLogList = new();

		public static string chatLogFilePath = string.Empty;

		public static void LoadRecording()
		{
			var newChatLogFilePath = $"{Program.documentsFolderSTT}ChatLogs\\{IRSDK.normalizedSession.sessionId}-{IRSDK.normalizedSession.subSessionId}.csv";

			if ( newChatLogFilePath != chatLogFilePath )
			{
				chatLogFilePath = newChatLogFilePath;

				chatLogList.Clear();

				if ( File.Exists( chatLogFilePath ) )
				{
					var streamReader = File.OpenText( chatLogFilePath );

					var startSessionTime = 0.0;

					while ( true )
					{
						var line = streamReader.ReadLine();

						if ( line == null )
						{
							break;
						}

						var match = Regex.Match( line, "([^,]*),([^,]*),([^,]*),([^,]*)(,([^,]*))?(,\"([^\"]*)\")?" );

						if ( match.Success )
						{
							var sessionNumber = int.Parse( match.Groups[ 1 ].Value );
							var sessionTime = float.Parse( match.Groups[ 2 ].Value, CultureInfo.InvariantCulture.NumberFormat );
							var eventId = int.Parse( match.Groups[ 3 ].Value );

							if ( eventId == 5 )
							{
								if ( startSessionTime == 0 )
								{
									startSessionTime = sessionTime;
								}
							}
							else if ( eventId == 6 )
							{
								if ( startSessionTime > 0 )
								{
									chatLogList.Add( new ChatLogData( sessionNumber, startSessionTime - 1.6, sessionTime + 2, match.Groups[ 8 ].Value ) );

									startSessionTime = 0;
								}
							}
						}
					}
				}

				chatLogList.Reverse();
			}
		}

		public static void Close()
		{
			chatLogFilePath = string.Empty;

			chatLogList.Clear();
		}

		public static string? Playback( int sessionNumber, double sessionTime )
		{
			if ( chatLogList != null )
			{
				foreach ( var chatLogData in chatLogList )
				{
					if ( ( sessionNumber == chatLogData.sessionNumber ) && ( sessionTime >= chatLogData.startSessionTime ) && ( sessionTime < chatLogData.endSessionTime ) )
					{
						return chatLogData.text;
					}
				}
			}

			return null;
		}
	}
}

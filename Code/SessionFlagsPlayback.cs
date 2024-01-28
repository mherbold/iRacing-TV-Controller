
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController
{
	public static class SessionFlagsPlayback
	{
		public const int SaveToFileIntervalTime = 3;

		public static List<SessionFlagsData> sessionFlagsDataList = new();

		public static string filePath = string.Empty;

		public static float saveToFileTimeRemaining = 0;
		public static bool saveToFileQueued = false;

		public static uint sessionFlags = 0;

		public static string GetFilePath()
		{
			return $"{Program.documentsFolder}SessionFlags\\{IRSDK.normalizedSession.sessionID}-{IRSDK.normalizedSession.subSessionID}.csv";
		}

		public static SessionFlagsData? GetCurrentSessionFlagsData()
		{
			SessionFlagsData? currentSessionFlagsData = null;

			foreach ( var sessionFlagsData in sessionFlagsDataList )
			{
				if ( sessionFlagsData.SessionNumber > IRSDK.normalizedSession.sessionNumber )
				{
					break;
				}

				if ( sessionFlagsData.SessionNumber == IRSDK.normalizedSession.sessionNumber )
				{
					if ( sessionFlagsData.SessionTime > IRSDK.normalizedData.sessionTime )
					{
						break;
					}

					currentSessionFlagsData = sessionFlagsData;
				}
			}

			return currentSessionFlagsData;
		}

		public static SessionFlagsData? FindFirstGreenFlagDropInCurrentSession()
		{
			foreach ( var sessionFlagsData in sessionFlagsDataList )
			{
				if ( sessionFlagsData.SessionNumber > IRSDK.normalizedSession.sessionNumber )
				{
					break;
				}

				if ( sessionFlagsData.SessionNumber == IRSDK.normalizedSession.sessionNumber )
				{
					if ( ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Green ) != 0 )
					{
						return sessionFlagsData;
					}
				}
			}

			return null;
		}

		public static void Update()
		{
			if ( IRSDK.isConnected )
			{
				if ( !IRSDK.normalizedSession.isReplay )
				{
					if ( IRSDK.normalizedData.sessionFlags != sessionFlags )
					{
						sessionFlags = IRSDK.normalizedData.sessionFlags;

						AddAtCurrentFrame( sessionFlags );

						saveToFileQueued = true;
					}
				}

				saveToFileTimeRemaining = Math.Max( 0, saveToFileTimeRemaining - Program.deltaTime );

				if ( saveToFileQueued && ( saveToFileTimeRemaining == 0 ) )
				{
					saveToFileQueued = false;
					saveToFileTimeRemaining = SaveToFileIntervalTime;

					Save();
				}
			}
			else
			{
				filePath = string.Empty;
				saveToFileQueued = false;

				sessionFlagsDataList.Clear();

				MainWindow.Instance.SessionFlags_ListView.Items.Refresh();
			}
		}

		public static void Save()
		{
			filePath = GetFilePath();

			try
			{
				var streamWriter = new StreamWriter( filePath, false );

				foreach ( var sessionFlagsData in sessionFlagsDataList )
				{
					var sessionFlagsAsHex = sessionFlagsData.SessionFlags.ToString( "X8" );

					streamWriter.WriteLine( $"{sessionFlagsData.SessionNumber},{sessionFlagsData.SessionTime:0.000},0x{sessionFlagsAsHex}" );
				}

				streamWriter.Close();
			}
			catch ( IOException )
			{
				saveToFileQueued = true;
				saveToFileTimeRemaining = SaveToFileIntervalTime;
			}
		}

		public static void Load()
		{
			var newFilePath = GetFilePath();

			if ( filePath != newFilePath )
			{
				filePath = newFilePath;
				saveToFileQueued = false;

				sessionFlagsDataList.Clear();

				MainWindow.Instance.SessionFlags_ListView.Items.Refresh();

				if ( File.Exists( filePath ) )
				{
					LogFile.Write( $"Loading session flags from {filePath}...\r\n" );

					var streamReader = File.OpenText( filePath );

					while ( true )
					{
						var line = streamReader.ReadLine();

						if ( line == null )
						{
							break;
						}

						var match = Regex.Match( line, @"(\d{1}),([0-9.]+),0x([0-9a-fA-F]{8})" );

						if ( match.Success )
						{
							sessionFlags = uint.Parse( match.Groups[ 3 ].Value, NumberStyles.HexNumber );

							var sessionFlagsData = new SessionFlagsData()
							{
								SessionNumber = int.Parse( match.Groups[ 1 ].Value ),
								SessionTime = double.Parse( match.Groups[ 2 ].Value, CultureInfo.InvariantCulture ),
								SessionFlags = uint.Parse( match.Groups[ 3 ].Value, NumberStyles.HexNumber ),
								SessionFlagsAsString = ( (SessionFlags) sessionFlags ).ToString()
							};

							sessionFlagsDataList.Add( sessionFlagsData );
						}
					}
				}
				else
				{
					LogFile.Write( $"Session flags file {filePath} does not exist.\r\n" );
				}

				Refresh();
			}
		}

		public static void Refresh()
		{
			sessionFlagsDataList.Sort( SessionFlagsDataComparison );

			var index = 1;

			foreach ( var sessionFlagsData in sessionFlagsDataList )
			{
				sessionFlagsData.Index = index++;
			}

			MainWindow.Instance.SessionFlags_ListView.ItemsSource = sessionFlagsDataList;

			MainWindow.Instance.SessionFlags_ListView.Items.Refresh();
		}

		public static void AddAtCurrentFrame( uint sessionFlags )
		{
			var sessionFlagsData = new SessionFlagsData()
			{
				SessionNumber = IRSDK.normalizedSession.sessionNumber,
				SessionTime = Math.Round( IRSDK.normalizedData.sessionTime, 3 ),
				SessionFlags = sessionFlags,
				SessionFlagsAsString = ( (SessionFlags) sessionFlags ).ToString()
			};

			sessionFlagsDataList.Add( sessionFlagsData );

			Refresh();

			saveToFileQueued = true;

			MainWindow.Instance.SessionFlags_ListView.ScrollIntoView( sessionFlagsData );
		}

		public static Comparison<SessionFlagsData> SessionFlagsDataComparison = delegate ( SessionFlagsData a, SessionFlagsData b )
		{
			if ( a.SessionNumber == b.SessionNumber )
			{
				return a.SessionTime.CompareTo( b.SessionTime );
			}

			return a.SessionNumber.CompareTo( b.SessionNumber );
		};
	}
}

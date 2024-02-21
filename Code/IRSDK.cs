
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

using irsdkSharp;
using irsdkSharp.Enums;
using irsdkSharp.Serialization;
using irsdkSharp.Serialization.Models.Data;
using irsdkSharp.Serialization.Models.Session;

using CsvHelper;
using System.Globalization;
using System.Diagnostics;
using System.Reflection;
using System.Dynamic;
using System.Windows;

namespace iRacingTVController
{
	public static class IRSDK
	{
		public static readonly IRacingSDK iRacingSdk = new();

		public static bool isConnected = false;
		public static bool wasConnected = false;

		public static int sessionInfoUpdate = -1;

		public static IRacingSessionModel? session = null;
		public static DataModel? data = null;

		public static NormalizedSession normalizedSession = new();
		public static NormalizedData normalizedData = new();

		public static float sendMessageWaitTimeRemaining = 0;
		public static float cameraSwitchWaitTimeRemaining = 0;

		public static int camCarIdx = 0;
		public static int camGroupNumber = 0;
		public static int camCameraNumber = 0;
		public static SettingsDirector.CameraType currentCameraType = SettingsDirector.CameraType.Medium;

		public static bool targetCamEnabled = false;
		public static bool targetCamFastSwitchEnabled = false;
		public static bool targetCamSlowSwitchEnabled = false;
		public static int targetCamCarIdx = 0;
		public static int targetCamGroupNumber = 0;
		public static string targetCamReason = string.Empty;
		public static SettingsDirector.CameraType targetCameraType = SettingsDirector.CameraType.Medium;

		public static bool targetReplayStartFrameNumberEnabled = false;
		public static int targetReplayStartFrameNumber = 0;
		public static bool targetReplayStartPlaying = false;

		public static bool targetReplayStopFrameNumberEnabled = false;
		public static int targetReplayStopFrameNumber = 0;

		public static readonly List<Message> messageBuffer = new();

		public static AiRoster? aiRoster = null;

		public static string driverCsvFilePath = string.Empty;
		public static Dictionary<int, IDictionary<string, object>>? driverCsvFile = null;
		public static bool driverCsvFileNeedsToBeReloaded = false;
		public static FileSystemWatcher? driverCsvFileWatcher = null;

		public static string stringsCsvFilePath = string.Empty;
		public static Dictionary<string, string>? stringsCsvFile = null;
		public static bool stringsCsvFileNeedsToBeReloaded = false;
		public static FileSystemWatcher? stringsCsvFileWatcher = null;

		public static void Update()
		{
			isConnected = iRacingSdk.IsConnected();

			if ( isConnected )
			{
				data = iRacingSdk.GetSerializedData().Data;

				if ( ( session == null ) || ( iRacingSdk.Header.SessionInfoUpdate != sessionInfoUpdate ) )
				{
					LogFile.Write( "Getting updated session information...\r\n" );

					sessionInfoUpdate = iRacingSdk.Header.SessionInfoUpdate;

					session = iRacingSdk.GetSerializedSessionInfo();

					normalizedSession.SessionUpdate();
					normalizedData.SessionUpdate();

					EventLog.SessionUpdate();

					TrackMap.Initialize();
				}

				if ( data.SessionNum != normalizedSession.sessionNumber )
				{
					targetCamSlowSwitchEnabled = false;

					normalizedSession.SessionNumberChange();
					normalizedData.SessionNumberChange();

					EventLog.Reset();
				}

				if ( data.SessionNum >= 0 )
				{
					normalizedData.Update();

					EventLog.Update();

					WebPage.saveToFileQueued = true;
				}
			}
			else if ( wasConnected )
			{
				sessionInfoUpdate = -1;

				normalizedSession.Reset();
				normalizedData.Reset();

				EventLog.Reset();
			}

			wasConnected = isConnected;

			if ( driverCsvFilePath != Settings.overlayLocal.driverCsvFilePath )
			{
				driverCsvFilePath = Settings.overlayLocal.driverCsvFilePath;
				driverCsvFile = null;
				driverCsvFileWatcher = null;
				driverCsvFileNeedsToBeReloaded = true;
			}

			if ( driverCsvFileNeedsToBeReloaded )
			{
				driverCsvFileNeedsToBeReloaded = false;

				if ( driverCsvFilePath != string.Empty )
				{
					ReadDriverCsvFileIntoDictionary();

					var fullPath = Settings.GetFullPath( driverCsvFilePath );

					var directory = Path.GetDirectoryName( fullPath );
					var fileName = Path.GetFileName( fullPath );

					if ( directory != null )
					{
						driverCsvFileWatcher = new()
						{
							Path = directory,
							NotifyFilter = NotifyFilters.LastWrite,
							Filter = fileName,
							EnableRaisingEvents = true,
							IncludeSubdirectories = false
						};

						driverCsvFileWatcher.Changed += OnDriverCsvFileChanged;
					}
				}
			}

			if ( stringsCsvFilePath != Settings.overlayLocal.stringsCsvFilePath )
			{
				stringsCsvFilePath = Settings.overlayLocal.stringsCsvFilePath;
				stringsCsvFile = null;
				stringsCsvFileWatcher = null;
				stringsCsvFileNeedsToBeReloaded = true;
			}

			if ( stringsCsvFileNeedsToBeReloaded )
			{
				stringsCsvFileNeedsToBeReloaded = false;

				if ( stringsCsvFilePath != string.Empty )
				{
					ReadStringsCsvFileIntoDictionary();

					var fullPath = Settings.GetFullPath( stringsCsvFilePath );

					var directory = Path.GetDirectoryName( fullPath );
					var fileName = Path.GetFileName( fullPath );

					if ( directory != null )
					{
						stringsCsvFileWatcher = new()
						{
							Path = directory,
							NotifyFilter = NotifyFilters.LastWrite,
							Filter = fileName,
							EnableRaisingEvents = true,
							IncludeSubdirectories = false
						};

						stringsCsvFileWatcher.Changed += OnStringsCsvFileChanged;
					}
				}
			}
		}

		private static void OnDriverCsvFileChanged( object sender, FileSystemEventArgs e )
		{
			driverCsvFileNeedsToBeReloaded = true;
		}

		private static void ReadDriverCsvFileIntoDictionary()
		{
			try
			{
				driverCsvFile = new();

				using var reader = new StreamReader( driverCsvFilePath );
				using var csv = new CsvReader( reader, CultureInfo.InvariantCulture );

				var records = csv.GetRecords<dynamic>();

				foreach ( IDictionary<string, object> dictionary in records )
				{
					if ( dictionary.ContainsKey( "ID" ) )
					{
						var id = dictionary[ "ID" ];

						if ( id != null )
						{
							driverCsvFile.Add( int.Parse( (string) id ), dictionary );
						}
					}
					else
					{
						throw new Exception( "The 'ID' column does not exist!" );
					}
				}
			}
			catch ( Exception exception )
			{
				driverCsvFile = null;

				MessageBox.Show( MainWindow.Instance, $"We could not load the driver CSV file '{driverCsvFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		private static void OnStringsCsvFileChanged( object sender, FileSystemEventArgs e )
		{
			stringsCsvFileNeedsToBeReloaded = true;
		}

		private static void ReadStringsCsvFileIntoDictionary()
		{
			try
			{
				stringsCsvFile = new();

				using var reader = new StreamReader( stringsCsvFilePath );
				using var csv = new CsvReader( reader, CultureInfo.InvariantCulture );

				var records = csv.GetRecords<dynamic>();

				foreach ( IDictionary<string, object> dictionary in records )
				{
					if ( dictionary.ContainsKey( "Target" ) && dictionary.ContainsKey( "Replacement" ) )
					{
						var target = (string) dictionary[ "Target" ];
						var replacement = (string) dictionary[ "Replacement" ];

						if ( target != null && replacement != null )
						{
							stringsCsvFile.Add( target, replacement );
						}
					}
					else
					{
						throw new Exception( "The 'Target' column or the 'Replacement' column does not exist!" );
					}
				}
			}
			catch ( Exception exception )
			{
				stringsCsvFile = null;

				MessageBox.Show( MainWindow.Instance, $"We could not load the strings CSV file '{stringsCsvFilePath}'.\r\n\r\nThe error message is as follows:\r\n\r\n{exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
		}

		public static void AddMessage( BroadcastMessageTypes msg, int var1, int var2, int var3 )
		{
			messageBuffer.Add( new Message( msg, var1, var2, var3 ) );
		}

		public static void SendMessage( Message message )
		{
			LogFile.Write( $"Sending message to iRacing: {message.msg}, {message.var1}, {message.var2}, {message.var3}\r\n" );

			iRacingSdk.BroadcastMessage( message.msg, message.var1, message.var2, message.var3 );

			sendMessageWaitTimeRemaining = 1.0f / Settings.editor.iracingGeneralCommandRateLimit;

			if ( message.msg == BroadcastMessageTypes.CamSwitchNum )
			{
				cameraSwitchWaitTimeRemaining = Settings.director.switchDelayDirector;
			}
		}

		public static void SendMessages()
		{
			if ( isConnected )
			{
				// update wait timers

				sendMessageWaitTimeRemaining = Math.Max( 0, sendMessageWaitTimeRemaining - Program.deltaTime );

				if ( !Director.isHolding )
				{
					cameraSwitchWaitTimeRemaining = Math.Max( 0, cameraSwitchWaitTimeRemaining - Program.deltaTime );
				}

				// if iracing has switched the camera then we need to reset the camera switch wait ticks

				if ( ( normalizedData.camCarIdx != camCarIdx ) || ( normalizedData.camGroupNumber != camGroupNumber ) || ( normalizedData.camCameraNumber != camCameraNumber ) )
				{
					camCarIdx = normalizedData.camCarIdx;
					camGroupNumber = normalizedData.camGroupNumber;
					camCameraNumber = normalizedData.camCameraNumber;

					if ( cameraSwitchWaitTimeRemaining < Settings.director.switchDelayIracing )
					{
						cameraSwitchWaitTimeRemaining = Settings.director.switchDelayIracing;
					}
				}

				// send the next message in the queue

				if ( sendMessageWaitTimeRemaining <= 0 )
				{
					if ( messageBuffer.Count > 0 )
					{
						var message = messageBuffer.First();

						messageBuffer.RemoveAt( 0 );

						SendMessage( message );
					}
				}

				// send message to pause the replay if target frame number is enabled and we are not paused

				if ( sendMessageWaitTimeRemaining <= 0 )
				{
					if ( targetReplayStartFrameNumberEnabled )
					{
						if ( normalizedData.replaySpeed != 0 )
						{
							var message = new Message( BroadcastMessageTypes.ReplaySetPlaySpeed, 0, 0, 0 );

							SendMessage( message );
						}
					}
				}

				// send message to change the frame number if target frame number is enabled and we are not on it (also auto-start playing if enabled)

				if ( sendMessageWaitTimeRemaining <= 0 )
				{
					if ( targetReplayStartFrameNumberEnabled )
					{
						if ( normalizedData.replayFrameNum != targetReplayStartFrameNumber )
						{
							var message = new Message( BroadcastMessageTypes.ReplaySetPlayPosition, (int) ReplayPositionModeTypes.Begin, IRacingSDK.LoWord( targetReplayStartFrameNumber ), IRacingSDK.HiWord( targetReplayStartFrameNumber ) );

							SendMessage( message );
						}
						else
						{
							targetReplayStartFrameNumberEnabled = false;

							if ( targetReplayStartPlaying )
							{
								targetReplayStartPlaying = false;

								var message = new Message( BroadcastMessageTypes.ReplaySetPlaySpeed, 1, 0, 0 );

								SendMessage( message );
							}
						}
					}
				}

				// send message to auto-stop replay if enabled

				if ( sendMessageWaitTimeRemaining <= 0 )
				{
					if ( targetReplayStopFrameNumberEnabled )
					{
						if ( normalizedData.replaySpeed != 0 )
						{
							if ( normalizedData.replayFrameNum >= targetReplayStopFrameNumber )
							{
								var message = new Message( BroadcastMessageTypes.ReplaySetPlaySpeed, 0, 0, 0 );

								SendMessage( message );
							}
						}
						else
						{
							targetReplayStopFrameNumberEnabled = false;
						}
					}
				}

				// send message to switch the camera if target camera is enabled and the current camera is not the target camera

				if ( sendMessageWaitTimeRemaining <= 0 )
				{
					if ( targetCamEnabled )
					{
						if ( ( camCarIdx != targetCamCarIdx ) || ( camGroupNumber != targetCamGroupNumber ) )
						{
							if ( !Director.isHolding )
							{
								if ( ( cameraSwitchWaitTimeRemaining <= 0 ) || targetCamFastSwitchEnabled )
								{
									var normalizedCar = normalizedData.FindNormalizedCarByCarIdx( targetCamCarIdx );

									if ( normalizedCar != null )
									{
										var message = new Message( BroadcastMessageTypes.CamSwitchNum, normalizedCar.carNumberRaw, targetCamGroupNumber, 0 );

										SendMessage( message );

										if ( camCarIdx != targetCamCarIdx )
										{
											Director.chyronTimer = 0;
										}

										if ( targetCamSlowSwitchEnabled )
										{
											targetCamSlowSwitchEnabled = false;

											cameraSwitchWaitTimeRemaining = Settings.director.switchDelayNotInRace;
										}

										currentCameraType = targetCameraType;
									}
								}
							}
						}
						else
						{
							targetCamFastSwitchEnabled = false;
						}
					}
				}
			}
			else
			{
				sendMessageWaitTimeRemaining = 0;
				cameraSwitchWaitTimeRemaining = 0;

				targetReplayStartFrameNumberEnabled = false;
				targetReplayStopFrameNumberEnabled = false;

				messageBuffer.Clear();
			}
		}

		public static int GetCamGroupNumber( SettingsDirector.CameraType cameraType )
		{
			string? cameraGroupNames = null;

			switch ( cameraType )
			{
				case SettingsDirector.CameraType.Practice:
					cameraGroupNames = Settings.director.camerasPractice;
					break;

				case SettingsDirector.CameraType.Qualifying:
					cameraGroupNames = Settings.director.camerasQualifying;
					break;

				case SettingsDirector.CameraType.Intro:
					cameraGroupNames = Settings.director.camerasIntro;
					break;

				case SettingsDirector.CameraType.Scenic:
					cameraGroupNames = Settings.director.camerasScenic;
					break;

				case SettingsDirector.CameraType.Pits:
					cameraGroupNames = Settings.director.camerasPits;
					break;

				case SettingsDirector.CameraType.StartFinish:
					cameraGroupNames = Settings.director.camerasStartFinish;
					break;

				case SettingsDirector.CameraType.Inside:
					cameraGroupNames = Settings.director.camerasInside;
					break;

				case SettingsDirector.CameraType.Close:
					cameraGroupNames = Settings.director.camerasClose;
					break;

				case SettingsDirector.CameraType.Medium:
					cameraGroupNames = Settings.director.camerasMedium;
					break;

				case SettingsDirector.CameraType.Far:
					cameraGroupNames = Settings.director.camerasFar;
					break;

				case SettingsDirector.CameraType.VeryFar:
					cameraGroupNames = Settings.director.camerasVeryFar;
					break;

				case SettingsDirector.CameraType.Custom1:
					cameraGroupNames = Settings.director.camerasCustom1;
					break;

				case SettingsDirector.CameraType.Custom2:
					cameraGroupNames = Settings.director.camerasCustom2;
					break;

				case SettingsDirector.CameraType.Custom3:
					cameraGroupNames = Settings.director.camerasCustom3;
					break;

				case SettingsDirector.CameraType.Custom4:
					cameraGroupNames = Settings.director.camerasCustom4;
					break;

				case SettingsDirector.CameraType.Custom5:
					cameraGroupNames = Settings.director.camerasCustom5;
					break;

				case SettingsDirector.CameraType.Custom6:
					cameraGroupNames = Settings.director.camerasCustom6;
					break;

				case SettingsDirector.CameraType.Reverse:
					cameraGroupNames = Settings.director.camerasReverse;
					break;
			}

			bool shuffleCamerasInList = ( ( cameraType != SettingsDirector.CameraType.Pits ) && ( cameraType != SettingsDirector.CameraType.StartFinish ) );

			return ( cameraGroupNames == null ) ? 0 : GetCamGroupNumber( cameraGroupNames, shuffleCamerasInList );
		}

		public static int GetCamGroupNumber( string cameraGroupNames, bool shuffleCamerasInList = true )
		{
			if ( session != null )
			{
				var selectedCameraGroupList = cameraGroupNames.Split( "," ).ToList().Select( s => s.Trim().ToLower() ).ToList();

				var shuffledSelectedCameraGroupList = shuffleCamerasInList ? selectedCameraGroupList.OrderBy( s => Program.random.Next() ).ToList() : selectedCameraGroupList;

				foreach ( var selectedCameraGroup in shuffledSelectedCameraGroupList )
				{
					foreach ( var group in session.CameraInfo.Groups )
					{
						if ( group.GroupName.ToLower() == selectedCameraGroup )
						{
							return group.GroupNum;
						}
					}
				}
			}

			return 0;
		}

		public static string GetCamGroupName( int camGroupNumber )
		{
			if ( session != null )
			{
				foreach ( var group in session.CameraInfo.Groups )
				{
					if ( group.GroupNum == camGroupNumber )
					{
						return group.GroupName;
					}
				}
			}

			return string.Empty;
		}

		public static void ReloadAiRoster()
		{
			if ( Settings.editor.iracingCustomPaintsAiRosterFile == string.Empty )
			{
				aiRoster = null;
			}
			else
			{
				string aiRosterText = File.ReadAllText( Settings.editor.iracingCustomPaintsAiRosterFile );

				aiRoster = JsonSerializer.Deserialize<AiRoster>( aiRosterText );
			}
		}

		public class AiRoster
		{
			public class AiDriver
			{
				[JsonInclude] public string driverName = "";
				[JsonInclude] public string carTgaName = "";
			}

			[JsonInclude] public AiDriver[] drivers = Array.Empty<AiDriver>();
		}
	}
}

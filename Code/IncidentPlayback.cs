
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

using irsdkSharp.Enums;

namespace iRacingTVController
{
	public static class IncidentPlayback
	{
		public const int SaveToFileIntervalTime = 3;

		public static List<IncidentData> incidentDataList = new();

		public static string filePath = string.Empty;

		public static float saveToFileTimeRemaining = 0;
		public static bool saveToFileQueued = false;

		public static int currentSession = 0;

		public static int settleStartingFrameNumber = 0;
		public static int settleTargetFrameNumber = 0;
		public static int settleLastFrameNumber = 0;
		public static float settleTimer = 0;

		public enum IncidentScanStateEnum
		{
			RewindToStartOfReplay,
			FindStartOfRace,
			LookAtPaceCarWithScenicCamera,
			SearchForNextIncident,
			AddIncidentToList,
			RewindToStartOfReplayAgain,
			FindStartOfRaceAgain,
			Complete,
			Idle,
			WaitForFrameNumberToSettle
		}

		public static IncidentScanStateEnum currentIncidentScanState = IncidentScanStateEnum.Idle;
		public static IncidentScanStateEnum nextIncidentScanState = IncidentScanStateEnum.Idle;

		public static string GetFilePath()
		{
			return $"{Program.documentsFolder}Incidents\\{IRSDK.normalizedSession.sessionID}-{IRSDK.normalizedSession.subSessionID}.xml";
		}

		public static IncidentData? GetCurrentIncidentData()
		{
			IncidentData? currentIncidentData = null;

			foreach ( var incidentData in incidentDataList )
			{
				if ( !incidentData.Ignore )
				{
					if ( ( IRSDK.normalizedData.replayFrameNum >= incidentData.StartFrame ) && ( IRSDK.normalizedData.replayFrameNum <= incidentData.EndFrame ) )
					{
						currentIncidentData = incidentData;
						break;
					}
				}
			}

			return currentIncidentData;
		}

		public static void Update()
		{
			if ( IRSDK.isConnected )
			{
				switch ( currentIncidentScanState )
				{
					case IncidentScanStateEnum.RewindToStartOfReplay:

						LogFile.Write( "Rewinding to the start of the replay...\r\n" );

						IRSDK.AddMessage( BroadcastMessageTypes.ReplaySetPlaySpeed, 0, 0, 0 );
						IRSDK.AddMessage( BroadcastMessageTypes.ReplaySetPlayPosition, (int) ReplayPositionModeTypes.Begin, 1, 0 );

						currentSession = 1;

						LogFile.Write( "Finding the start of the race event...\r\n" );

						WaitForFrameNumberToSettleState( IncidentScanStateEnum.FindStartOfRace, 1 );

						break;

					case IncidentScanStateEnum.FindStartOfRace:

						if ( currentSession < IRSDK.normalizedSession.sessionCount )
						{
							LogFile.Write( "Jumping to the next session...\r\n" );

							currentSession++;

							IRSDK.AddMessage( BroadcastMessageTypes.ReplaySearch, (int) ReplaySearchModeTypes.NextSession, 0, 0 );

							WaitForFrameNumberToSettleState( IncidentScanStateEnum.FindStartOfRace, 0 );
						}
						else
						{
							LogFile.Write( $"Start of race found at frame {IRSDK.normalizedData.replayFrameNum}.\r\n" );

							LogFile.Write( "Switching to the scenic camera, looking at the pace car...\r\n" );

							currentIncidentScanState = IncidentScanStateEnum.LookAtPaceCarWithScenicCamera;
						}

						break;

					case IncidentScanStateEnum.LookAtPaceCarWithScenicCamera:

						var scenicCameraGroupNumber = IRSDK.GetCamGroupNumber( Settings.editor.editorIncidentsScenicCameras, false );

						IRSDK.AddMessage( BroadcastMessageTypes.CamSwitchNum, 0, scenicCameraGroupNumber, 0 );

						currentIncidentScanState = IncidentScanStateEnum.SearchForNextIncident;

						break;

					case IncidentScanStateEnum.SearchForNextIncident:

						IRSDK.AddMessage( BroadcastMessageTypes.ReplaySearch, (int) ReplaySearchModeTypes.NextIncident, 0, 0 );

						WaitForFrameNumberToSettleState( IncidentScanStateEnum.AddIncidentToList, 0 );

						break;

					case IncidentScanStateEnum.AddIncidentToList:

						var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.camCarIdx );

						if ( normalizedCar != null )
						{
							LogFile.Write( $"New incident found at frame {IRSDK.normalizedData.replayFrameNum} involving car #{normalizedCar.carNumber}.\r\n" );

							var overlapFound = false;

							foreach ( var incidentData in incidentDataList )
							{
								var incidentOverlapMergeFrames = (int) Math.Ceiling( 60 * Settings.editor.editorIncidentsOverlapMergeTime );

								if ( ( IRSDK.normalizedData.camCarIdx == incidentData.CarIdx ) && ( IRSDK.normalizedData.replayFrameNum >= ( incidentData.StartFrame - incidentOverlapMergeFrames ) ) && ( IRSDK.normalizedData.replayFrameNum <= ( incidentData.EndFrame + incidentOverlapMergeFrames ) ) )
								{
									overlapFound = true;

									if ( IRSDK.normalizedData.replayFrameNum <= incidentData.StartFrame )
									{
										incidentData.StartFrame = IRSDK.normalizedData.replayFrameNum;
									}
									else
									{
										incidentData.EndFrame = IRSDK.normalizedData.replayFrameNum;
									}

									MainWindow.Instance.Incidents_ListView.Items.Refresh();

									break;
								}
							}

							if ( !overlapFound )
							{
								var incidentData = new IncidentData()
								{
									Index = incidentDataList.Count + 1,
									CarIdx = normalizedCar.carIdx,
									FrameNumber = IRSDK.normalizedData.replayFrameNum,
									CarNumber = normalizedCar.carNumber,
									DriverName = normalizedCar.userName,
									StartFrame = IRSDK.normalizedData.replayFrameNum,
									EndFrame = IRSDK.normalizedData.replayFrameNum,
									Ignore = false
								};

								incidentDataList.Add( incidentData );

								MainWindow.Instance.Incidents_ListView.Items.Add( incidentData );

								MainWindow.Instance.Incidents_ListView.ScrollIntoView( incidentData );
							}


							saveToFileQueued = true;
						}

						currentIncidentScanState = IncidentScanStateEnum.SearchForNextIncident;

						break;

					case IncidentScanStateEnum.RewindToStartOfReplayAgain:

						LogFile.Write( $"Done with finding incidents, rewinding to the start of the replay again.\r\n" );

						IRSDK.AddMessage( BroadcastMessageTypes.ReplaySetPlayPosition, (int) ReplayPositionModeTypes.Begin, 1, 0 );

						currentSession = 1;

						LogFile.Write( "Finding the start of the race event again...\r\n" );

						WaitForFrameNumberToSettleState( IncidentScanStateEnum.FindStartOfRaceAgain, 1 );

						break;

					case IncidentScanStateEnum.FindStartOfRaceAgain:

						if ( currentSession < IRSDK.normalizedSession.sessionCount )
						{
							LogFile.Write( "Jumping to the next session...\r\n" );

							currentSession++;

							IRSDK.AddMessage( BroadcastMessageTypes.ReplaySearch, (int) ReplaySearchModeTypes.NextSession, 0, 0 );

							WaitForFrameNumberToSettleState( IncidentScanStateEnum.FindStartOfRaceAgain, 0 );
						}
						else
						{
							LogFile.Write( $"Start of race found at frame {IRSDK.normalizedData.replayFrameNum}.\r\n" );

							currentIncidentScanState = IncidentScanStateEnum.Complete;
						}

						break;

					case IncidentScanStateEnum.Complete:

						LogFile.Write( "Incident scan is all done!\r\n" );

						currentIncidentScanState = IncidentScanStateEnum.Idle;

						MessageBox.Show( MainWindow.Instance, "Incident scan is complete!", "All Done", MessageBoxButton.OK, MessageBoxImage.Information );

						break;

					case IncidentScanStateEnum.WaitForFrameNumberToSettle:

						if ( settleTargetFrameNumber != 0 )
						{
							if ( IRSDK.normalizedData.replayFrameNum == settleTargetFrameNumber )
							{
								currentIncidentScanState = nextIncidentScanState;
							}
						}
						else
						{
							if ( ( IRSDK.normalizedData.replayFrameNum != 0 ) && ( IRSDK.normalizedData.replayFrameNum != settleStartingFrameNumber ) )
							{
								if ( ( settleLastFrameNumber == 0 ) || ( settleLastFrameNumber != IRSDK.normalizedData.replayFrameNum ) )
								{
									settleLastFrameNumber = IRSDK.normalizedData.replayFrameNum;
									settleTimer = 0;
								}
								else
								{
									var targetTime = 1.0f / Settings.editor.iracingGeneralCommandRateLimit;

									settleTimer += Program.deltaTime;

									if ( settleTimer >= targetTime )
									{
										currentIncidentScanState = nextIncidentScanState;
									}
								}
							}
							else
							{
								var targetTime = Settings.editor.editorIncidentsTimeout;

								settleTimer += Program.deltaTime;

								if ( settleTimer >= targetTime )
								{
									currentIncidentScanState = IncidentScanStateEnum.RewindToStartOfReplayAgain;
								}
							}
						}

						break;
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

				currentIncidentScanState = IncidentScanStateEnum.Idle;

				saveToFileQueued = false;

				incidentDataList.Clear();

				MainWindow.Instance.Incidents_ListView.Items.Clear();
			}
		}

		public static void Save()
		{
			if ( incidentDataList.Count > 0 )
			{
				filePath = GetFilePath();

				var xmlSerializer = new XmlSerializer( incidentDataList.GetType() );

				try
				{
					var streamWriter = new StreamWriter( filePath );

					xmlSerializer.Serialize( streamWriter, incidentDataList );

					streamWriter.Close();
				}
				catch ( IOException )
				{
					saveToFileQueued = true;
					saveToFileTimeRemaining = SaveToFileIntervalTime;
				}
			}
		}

		public static void Load()
		{
			var newFilePath = GetFilePath();

			if ( filePath != newFilePath )
			{
				filePath = newFilePath;
				saveToFileQueued = false;

				incidentDataList.Clear();

				MainWindow.Instance.Incidents_ListView.Items.Clear();

				if ( File.Exists( filePath ) )
				{
					LogFile.Write( $"Loading incidents from {filePath}...\r\n" );

					var xmlSerializer = new XmlSerializer( incidentDataList.GetType() );

					var fileStream = new FileStream( filePath, FileMode.Open );

					incidentDataList = (List<IncidentData>) ( xmlSerializer.Deserialize( fileStream ) ?? throw new Exception() );

					fileStream.Close();

					foreach ( var incidentData in incidentDataList )
					{
						MainWindow.Instance.Incidents_ListView.Items.Add( incidentData );
					}
				}
				else
				{
					LogFile.Write( $"Incidents file {filePath} does not exist.\r\n" );
				}
			}
		}

		public static void Clear()
		{
			saveToFileQueued = false;

			incidentDataList.Clear();

			MainWindow.Instance.Incidents_ListView.Items.Clear();

			var incidentFilePath = GetFilePath();

			File.Delete( incidentFilePath );
		}

		public static void Start()
		{
			IRSDK.targetCamEnabled = false;
			IRSDK.targetCamFastSwitchEnabled = false;

			Clear();

			currentIncidentScanState = IncidentScanStateEnum.RewindToStartOfReplay;

			Director.isEnabled = false;
		}

		public static bool IsRunning()
		{
			return currentIncidentScanState != IncidentScanStateEnum.Idle;
		}

		public static void WaitForFrameNumberToSettleState( IncidentScanStateEnum nextIncidentScanState, int targetFrameNumber )
		{
			settleStartingFrameNumber = IRSDK.normalizedData.replayFrameNum;
			settleTargetFrameNumber = targetFrameNumber;
			settleLastFrameNumber = 0;
			settleTimer = 0;

			currentIncidentScanState = IncidentScanStateEnum.WaitForFrameNumberToSettle;

			IncidentPlayback.nextIncidentScanState = nextIncidentScanState;
		}
	}
}

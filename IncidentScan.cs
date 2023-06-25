﻿
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Xml.Serialization;

using irsdkSharp.Enums;

namespace iRacingTVController
{
	public static class IncidentScan
	{
		public static List<IncidentData> incidentDataList = new();

		public static string incidentScanFilePath = string.Empty;

		public static int saveIncidentsTick = 0;
		public static bool saveIncidentsQueued = false;

		public static int currentSession = 0;

		public static int settleStartingFrameNumber = 0;
		public static int settleTargetFrameNumber = 0;
		public static int settleLastFrameNumber = 0;
		public static int settleLoopCount = 0;

		public enum IncidentScanStateEnum
		{
			RewindToStartOfReplay,
			FindStartOfRace,
			LookAtPaceCarWithScenicCamera,
			WaitForLookAtPaceCarToComplete,
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

		public static IncidentData? GetCurrentIncident()
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

		public static void Start()
		{
			IRSDK.targetCamEnabled = false;
			IRSDK.targetCamFastSwitchEnabled = false;

			incidentDataList.Clear();

			MainWindow.Instance.Incidents_ListView.Items.Clear();

			var incidentScanFilePath = GetIncidentScanFilePath();

			File.Delete( incidentScanFilePath );

			currentIncidentScanState = IncidentScanStateEnum.RewindToStartOfReplay;

			Director.isEnabled = false;

			//if ( Overlay.isVisible )
			//{
			//Overlay.ToggleVisibility();
			//}

			//MainWindow.instance?.Update();
		}

		public static bool IsRunning()
		{
			return currentIncidentScanState != IncidentScanStateEnum.Idle;
		}

		public static void Update()
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

					var scenicCameraGroupNumber = IRSDK.GetCamGroupNumber( Settings.editor.incidentsScenicCameras );

					IRSDK.AddMessage( BroadcastMessageTypes.CamSwitchNum, 0, scenicCameraGroupNumber, 0 );

					currentIncidentScanState = IncidentScanStateEnum.WaitForLookAtPaceCarToComplete;

					break;

				case IncidentScanStateEnum.WaitForLookAtPaceCarToComplete:

					scenicCameraGroupNumber = IRSDK.GetCamGroupNumber( Settings.editor.incidentsScenicCameras );

					if ( ( IRSDK.camCarIdx == 0 ) && ( IRSDK.camGroupNumber == scenicCameraGroupNumber ) )
					{
						currentIncidentScanState = IncidentScanStateEnum.SearchForNextIncident;
					}

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
							var incidentOverlapMergeFrames = (int) Math.Ceiling( 60 * Settings.editor.incidentsOverlapMergeTime );

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


						saveIncidentsQueued = true;
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
								settleLoopCount = 0;
							}
							else
							{
								var targetLoopCount = (int) Math.Ceiling( 60.0f / Settings.editor.iracingCommandRateLimit );

								settleLoopCount++;

								if ( settleLoopCount == targetLoopCount )
								{
									currentIncidentScanState = nextIncidentScanState;
								}
							}
						}
						else
						{
							var targetLoopCount = (int) Math.Ceiling( Settings.editor.incidentsTimeout * 60 );

							settleLoopCount++;

							if ( settleLoopCount == targetLoopCount )
							{
								currentIncidentScanState = IncidentScanStateEnum.RewindToStartOfReplayAgain;
							}
						}
					}

					break;
			}

			saveIncidentsTick--;

			if ( saveIncidentsQueued && ( saveIncidentsTick <= 0 ) )
			{
				saveIncidentsQueued = false;
				saveIncidentsTick = 60;

				SaveIncidents();
			}
		}

		public static void WaitForFrameNumberToSettleState( IncidentScanStateEnum nextIncidentScanState, int targetFrameNumber )
		{
			settleStartingFrameNumber = IRSDK.normalizedData.replayFrameNum;
			settleTargetFrameNumber = targetFrameNumber;
			settleLastFrameNumber = 0;
			settleLoopCount = 0;

			currentIncidentScanState = IncidentScanStateEnum.WaitForFrameNumberToSettle;

			IncidentScan.nextIncidentScanState = nextIncidentScanState;
		}

		public static string GetIncidentScanFilePath()
		{
			return $"{Program.documentsFolder}IncidentScans\\{IRSDK.normalizedSession.sessionId}-{IRSDK.normalizedSession.subSessionId}.xml";
		}

		public static void SaveIncidents()
		{
			if ( incidentDataList.Count > 0 )
			{
				incidentScanFilePath = GetIncidentScanFilePath();

				var xmlSerializer = new XmlSerializer( incidentDataList.GetType() );

				var streamWriter = new StreamWriter( incidentScanFilePath );

				xmlSerializer.Serialize( streamWriter, incidentDataList );

				streamWriter.Close();
			}
		}

		public static void LoadIncidents()
		{
			var newIncidentsScanFilePath = GetIncidentScanFilePath();

			if ( incidentScanFilePath != newIncidentsScanFilePath )
			{
				incidentScanFilePath = newIncidentsScanFilePath;

				incidentDataList.Clear();

				MainWindow.Instance.Incidents_ListView.Items.Clear();

				if ( File.Exists( incidentScanFilePath ) )
				{
					var xmlSerializer = new XmlSerializer( incidentDataList.GetType() );

					var fileStream = new FileStream( incidentScanFilePath, FileMode.Open );

					incidentDataList = (List<IncidentData>) ( xmlSerializer.Deserialize( fileStream ) ?? throw new Exception() );

					fileStream.Close();

					foreach ( var incidentData in incidentDataList )
					{
						MainWindow.Instance.Incidents_ListView.Items.Add( incidentData );
					}
				}
			}
		}
	}
}
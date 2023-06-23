
using System;
using System.Collections.Generic;

using irsdkSharp.Enums;

namespace iRacingTVController
{
	public static class IncidentScan
	{
		public const int IncidentFrameCount = 180;
		public const int IncidentPrerollFrameCount = 60;

		public static readonly List<IncidentData> incidentList = new();

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
			IncidentData? currentIncident = null;

			foreach ( var incident in incidentList )
			{
				var incidentStartFrame = incident.StartFrame;
				var incidentEndFrame = incident.EndFrame;

				if ( ( IRSDK.normalizedData.replayFrameNum >= incidentStartFrame ) && ( IRSDK.normalizedData.replayFrameNum < incidentEndFrame ) )
				{
					currentIncident = incident;

					break;
				}
			}

			return currentIncident;
		}

		public static void Start()
		{
			MainWindow.Instance.Incidents_ListView.Items.Clear();

			currentIncidentScanState = IncidentScanStateEnum.RewindToStartOfReplay;

			//Director.isEnabled = false;

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

					incidentList.Clear();

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

					IRSDK.AddMessage( BroadcastMessageTypes.CamSwitchNum, 0, 10, 0 );

					currentIncidentScanState = IncidentScanStateEnum.WaitForLookAtPaceCarToComplete;

					break;

				case IncidentScanStateEnum.WaitForLookAtPaceCarToComplete:

					if ( ( IRSDK.currentCameraCarIdx == 0 ) && ( IRSDK.currentCameraGroupNumber == 10 ) )
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

						var incidentData = new IncidentData()
						{
							CarIdx = normalizedCar.carIdx,
							FrameNumber = IRSDK.normalizedData.replayFrameNum,
							CarNumber = normalizedCar.carNumber,
							DriverName = normalizedCar.userName,
							StartFrame = IRSDK.normalizedData.replayFrameNum - 60, // TODO convert to setting
							EndFrame = IRSDK.normalizedData.replayFrameNum + 60, // TODO convert to setting
							Ignore = false
						};

						incidentList.Add( incidentData );

						MainWindow.Instance.Incidents_ListView.Items.Add( incidentData );
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
							var targetLoopCount = (int) Math.Ceiling( 60.0f / Settings.editor.iracingCommandRateLimit ) * 10;

							settleLoopCount++;

							if ( settleLoopCount == targetLoopCount ) // TODO change to setting
							{
								currentIncidentScanState = IncidentScanStateEnum.RewindToStartOfReplayAgain;
							}
						}
					}

					break;
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
	}
}

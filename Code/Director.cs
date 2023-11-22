
using System;
using System.Collections.Generic;

using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController
{
	public static class Director
	{
		public static bool isEnabled = true;
		public static bool isHolding = false;
		public static bool driverWasTalking = false;
		public static bool showChyron = false;
		public static float chyronTimer = 0;

		public static int targetCamCarIdx = 0;
		public static SettingsDirector.CameraType targetCamType = SettingsDirector.CameraType.Intro;
		public static SettingsDirector.CameraType autoCamType = SettingsDirector.CameraType.Close;

		public static List<NormalizedCar> preferredCarList = new();

		public static void Initialize()
		{
			if ( Settings.editor.editorStartupEnableDirector )
			{
				IRSDK.targetCamEnabled = true;

				isEnabled = true;
			}
		}

		public static void Update()
		{
			if ( !IRSDK.isConnected || !IRSDK.targetCamEnabled )
			{
				return;
			}

			if ( !isEnabled )
			{
				showChyron = true;

				var targetNormalizedCar = IRSDK.normalizedData.normalizedCars[ targetCamCarIdx ];

				var previousAutoCamType = autoCamType;

				UpdateAutoCamType( targetNormalizedCar );

				if ( ( targetCamType == SettingsDirector.CameraType.AutoCam ) && ( autoCamType != previousAutoCamType ) )
				{
					IRSDK.targetCamGroupNumber = GetCamGroupNumber( targetNormalizedCar, targetCamType );
				}
			}
			else
			{
				showChyron = false;
				chyronTimer += Program.deltaTime;

				IncidentData? currentIncident = IncidentPlayback.GetCurrentIncidentData();

				NormalizedCar? firstPlaceCar = null;
				NormalizedCar? leadingOnTrackCar = null;
				NormalizedCar? leadingPittedCar = null;
				NormalizedCar? hottestCar = null;
				NormalizedCar? incidentCar = ( currentIncident != null ) ? IRSDK.normalizedData.FindNormalizedCarByCarIdx( currentIncident.CarIdx ) : null;
				NormalizedCar? talkingCar = ( IRSDK.normalizedData.radioTransmitCarIdx != -1 ) ? IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.radioTransmitCarIdx ) : null;
				NormalizedCar? randomCar = GetRandomNormalizedCar();

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard )
					{
						firstPlaceCar ??= normalizedCar;

						if ( ( leadingOnTrackCar == null ) && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar )
						{
							leadingOnTrackCar = normalizedCar;
						}

						if ( ( leadingPittedCar == null ) && normalizedCar.isOnPitRoad )
						{
							leadingPittedCar = normalizedCar;
						}

						if ( ( normalizedCar.heatTotal > 0 ) && ( ( hottestCar == null ) || ( normalizedCar.heatTotal > hottestCar.heatTotal ) ) )
						{
							hottestCar = normalizedCar;
						}

						if ( normalizedCar.isPreferredCar )
						{
							preferredCarList.Add( normalizedCar );
						}
					}
				}

				NormalizedCar? preferredCar = null;

				if ( preferredCarList.Count > 0 )
				{
					var preferredCarIndex = (int) Math.Floor( IRSDK.normalizedData.sessionTime / 10.0f ) % preferredCarList.Count;

					preferredCar = preferredCarList[ preferredCarIndex ];
				}

				var targetCamCarIdx = preferredCar?.carIdx ?? leadingOnTrackCar?.carIdx ?? leadingPittedCar?.carIdx ?? 0;
				var targetCamType = SettingsDirector.CameraType.Far;
				var targetCamFastSwitchEnabled = false;
				var targetCamSlowSwitchEnabled = false;
				var targetCamReason = "No matching rule, looking at the preferred or leading car.";

				if ( Settings.director.rule1_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateCoolDown ) && ( firstPlaceCar != null ) && !firstPlaceCar.isOutOfCar ) )
				{
					targetCamCarIdx = firstPlaceCar.carIdx;
					targetCamType = Settings.director.rule1_Camera;
					targetCamReason = "Rule 1: Post-race cool down, first place car still connected, look at first place car.";
				}
				else if ( Settings.director.rule2_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateRacing ) && Settings.director.preferredCarLockOnEnabled && ( preferredCar != null ) && ( preferredCar.heatTotal >= Settings.director.preferredCarLockOnMinimumHeat ) ) )
				{
					showChyron = true;

					targetCamCarIdx = preferredCar.carIdx;
					targetCamType = Settings.director.rule2_Camera;
					targetCamReason = "Rule 2: Racing, preferred car lock-on enabled, and preferred car heat >= minimum, look at preferred car.";
				}
				else if ( Settings.director.rule3_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateCheckered ) ) )
				{
					var highestLapPosition = 0.0f;

					foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
					{
						if ( normalizedCar.includeInLeaderboard )
						{
							if ( normalizedCar.lapPosition < IRSDK.normalizedData.sessionLapsTotal + ( 20.0f / IRSDK.normalizedSession.trackLengthInMeters ) )
							{
								if ( normalizedCar.lapPosition > highestLapPosition )
								{
									highestLapPosition = normalizedCar.lapPosition;

									targetCamFastSwitchEnabled = true;
									targetCamCarIdx = normalizedCar.carIdx;
									targetCamType = Settings.director.rule3_Camera;
									targetCamReason = "Rule 3: Racing, checkered flag, look at car nearest to the finish line.";

									driverWasTalking = false;
								}
							}
						}
					}
				}
				else if ( Settings.director.rule4_Enabled && ( ( currentIncident != null ) && ( incidentCar != null ) ) )
				{
					targetCamFastSwitchEnabled = true;
					targetCamCarIdx = currentIncident.CarIdx;
					targetCamType = Settings.director.rule4_Camera;
					targetCamReason = "Rule 4: Incident.";

					driverWasTalking = false;
				}
				else if ( Settings.director.rule5_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState >= SessionState.StateRacing ) && ( leadingOnTrackCar != null ) && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.GreenHeld | (uint) SessionFlags.StartReady | (uint) SessionFlags.StartSet | (uint) SessionFlags.StartGo ) ) != 0 ) ) )
				{
					targetCamFastSwitchEnabled = true;
					targetCamCarIdx = leadingOnTrackCar.carIdx;
					targetCamType = Settings.director.rule5_Camera;
					targetCamReason = "Rule 5: Racing, green flag is about to be shown or is waving.";

					driverWasTalking = false;
				}
				else if ( Settings.director.rule6_Enabled && ( talkingCar != null ) )
				{
					targetCamFastSwitchEnabled = true;
					targetCamCarIdx = IRSDK.normalizedData.radioTransmitCarIdx;
					targetCamType = Settings.director.rule6_Camera;
					targetCamReason = "Rule 6: Driver is talking";

					driverWasTalking = true;
				}
				else if ( driverWasTalking )
				{
					driverWasTalking = false;

					if ( !isHolding )
					{
						IRSDK.cameraSwitchWaitTimeRemaining = Settings.director.switchDelayRadioChatter;
					}
				}
				else if ( Settings.director.rule7_Enabled && ( IRSDK.normalizedSession.isInPracticeSession ) )
				{
					if ( randomCar != null )
					{
						showChyron = true;

						targetCamSlowSwitchEnabled = true;
						targetCamCarIdx = randomCar.carIdx;
						targetCamType = Settings.director.rule7_Camera;
						targetCamReason = "Rule 7: Practice session.";
					}
					else
					{
						targetCamCarIdx = 0;
						targetCamType = SettingsDirector.CameraType.Intro;
						targetCamReason = "Rule 7: Practice session (no cars on track).";
					}
				}
				else if ( Settings.director.rule8_Enabled && ( IRSDK.normalizedSession.isInQualifyingSession ) )
				{
					if ( randomCar != null )
					{
						showChyron = true;

						targetCamSlowSwitchEnabled = true;
						targetCamCarIdx = randomCar.carIdx;
						targetCamType = Settings.director.rule8_Camera;
						targetCamReason = "Rule 8: Qualifying session.";
					}
					else
					{
						targetCamCarIdx = 0;
						targetCamType = SettingsDirector.CameraType.Intro;
						targetCamReason = "Rule 8: Qualifying session (no cars on track).";
					}
				}
				else if ( Settings.director.rule9_Enabled && ( ( ( leadingOnTrackCar != null ) || ( leadingPittedCar != null ) ) && ( IRSDK.normalizedData.sessionState == SessionState.StateWarmup ) ) )
				{
					var normalizedCar = leadingOnTrackCar ?? leadingPittedCar;

					if ( normalizedCar != null )
					{
						targetCamCarIdx = normalizedCar.carIdx;
						targetCamType = Settings.director.rule9_Camera;
						targetCamReason = "Rule 9: Cars are warming up.";
					}
				}
				else if ( Settings.director.rule10_Enabled && ( ( ( leadingOnTrackCar != null ) || ( leadingPittedCar != null ) ) && ( IRSDK.normalizedData.sessionState == SessionState.StateParadeLaps ) ) )
				{
					if ( ( IRSDK.normalizedData.paceCar != null ) && !IRSDK.normalizedData.paceCar.isOnPitRoad )
					{
						targetCamCarIdx = 0;
						targetCamType = Settings.director.rule13_Camera;
						targetCamReason = "Rule 10: Cars are doing parade laps (pace car on track).";
					}
					else
					{
						var normalizedCar = leadingOnTrackCar ?? leadingPittedCar;

						if ( normalizedCar != null )
						{
							targetCamCarIdx = normalizedCar.carIdx;
							targetCamType = Settings.director.rule10_Camera;
							targetCamReason = "Rule 10: Cars are doing parade laps (pace car off track).";
						}
					}
				}
				else if ( Settings.director.rule11_Enabled && ( ( ( leadingOnTrackCar != null ) || ( leadingPittedCar != null ) ) && ( IRSDK.normalizedData.sessionState == SessionState.StateGetInCar ) ) )
				{
					var normalizedCar = leadingOnTrackCar ?? leadingPittedCar;

					if ( normalizedCar != null )
					{
						targetCamCarIdx = normalizedCar.carIdx;
						targetCamType = Settings.director.rule11_Camera;
						targetCamReason = "Rule 11: Drivers are getting into their cars.";
					}
				}
				else if ( Settings.director.rule12_Enabled && ( ( ( leadingOnTrackCar != null ) || ( leadingPittedCar != null ) ) && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 ) ) )
				{
					var normalizedCar = leadingOnTrackCar ?? leadingPittedCar;

					if ( normalizedCar != null )
					{
						targetCamCarIdx = normalizedCar.carIdx;
						targetCamType = Settings.director.rule12_Camera;
						targetCamReason = "Rule 12: Caution flag waving.";
					}
				}
				else if ( Settings.director.rule13_Enabled && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.OneLapToGreen ) ) != 0 ) )
				{
					targetCamCarIdx = 0;
					targetCamType = SettingsDirector.CameraType.Reverse;
					targetCamReason = "Rule 13: One lap to green (pace car).";
				}
				else if ( Settings.director.rule14_Enabled )
				{
					if ( hottestCar != null )
					{
						showChyron = true;

						targetCamCarIdx = hottestCar.carIdx;
						targetCamType = Settings.director.rule14_Camera;
						targetCamReason = "Rule 13: Hottest car.";
					}
				}

				var targetNormalizedCar = IRSDK.normalizedData.normalizedCars[ targetCamCarIdx ];
				var previousAutoCamType = autoCamType;

				UpdateAutoCamType( targetNormalizedCar );

				if ( showChyron == false )
				{
					chyronTimer = 0;
				}

				if ( chyronTimer < 2 )
				{
					showChyron = false;
				}

				if ( ( Director.targetCamCarIdx != targetCamCarIdx ) || ( Director.targetCamType != targetCamType ) || ( ( targetCamType == SettingsDirector.CameraType.AutoCam ) && ( autoCamType != previousAutoCamType ) ) )
				{
					Director.targetCamCarIdx = targetCamCarIdx;
					Director.targetCamType = targetCamType;

					IRSDK.targetCamFastSwitchEnabled = targetCamFastSwitchEnabled;
					IRSDK.targetCamSlowSwitchEnabled = targetCamSlowSwitchEnabled;
					IRSDK.targetCamCarIdx = targetCamCarIdx;
					IRSDK.targetCamGroupNumber = GetCamGroupNumber( targetNormalizedCar, targetCamType );
					IRSDK.targetCamReason = targetCamReason;

					MainWindow.Instance.cameraType = targetCamType;
				}
			}
		}

		public static void UpdateAutoCamType( NormalizedCar normalizedCar )
		{
			if ( normalizedCar.isOnPitRoad )
			{
				autoCamType = SettingsDirector.CameraType.Pits;
			}
			else
			{
				var inCautionButNotWaving = IRSDK.normalizedData.isUnderCaution && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) == 0 );

				if ( !inCautionButNotWaving && ( normalizedCar.distanceToCarInFrontInMeters >= Settings.director.autoCamInsideMinimum ) && ( normalizedCar.distanceToCarInFrontInMeters <= Settings.director.autoCamInsideMaximum ) && ( IRSDK.camCarIdx == normalizedCar.carIdx ) )
				{
					autoCamType = SettingsDirector.CameraType.Inside;
				}
				else
				{
					var nearestCarDistance = Math.Min( normalizedCar.distanceToCarInFrontInMeters, normalizedCar.distanceToCarBehindInMeters );

					if ( nearestCarDistance <= Settings.director.autoCamCloseMaximum )
					{
						autoCamType = SettingsDirector.CameraType.Close;
					}
					else if ( nearestCarDistance <= Settings.director.autoCamMediumMaximum )
					{
						autoCamType = SettingsDirector.CameraType.Medium;
					}
					else if ( nearestCarDistance <= Settings.director.autoCamFarMaximum )
					{
						autoCamType = SettingsDirector.CameraType.Far;
					}
					else
					{
						autoCamType = SettingsDirector.CameraType.VeryFar;
					}
				}
			}
		}

		public static int GetCamGroupNumber( NormalizedCar normalizedCar, SettingsDirector.CameraType cameraType )
		{
			if ( cameraType == SettingsDirector.CameraType.AutoCam )
			{
				return GetAutoCamGroupNumber( normalizedCar );
			}
			else
			{
				return IRSDK.GetCamGroupNumber( cameraType );
			}
		}

		private static int GetAutoCamGroupNumber( NormalizedCar normalizedCar )
		{
			int camGroupNumber;

			switch ( autoCamType )
			{
				case SettingsDirector.CameraType.Pits:
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasPits, false );
					break;

				case SettingsDirector.CameraType.Inside:
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasInside );
					break;

				case SettingsDirector.CameraType.Close:
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasClose );
					break;

				case SettingsDirector.CameraType.Medium:
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasMedium );
					break;

				case SettingsDirector.CameraType.Far:
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasFar );
					break;

				default:
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasVeryFar );
					break;
			}

			return camGroupNumber;
		}

		private static NormalizedCar? GetRandomNormalizedCar()
		{
			var numCarsOnTrack = 0;

			foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
			{
				if ( normalizedCar.includeInLeaderboard && !normalizedCar.isOutOfCar && !normalizedCar.isOnPitRoad )
				{
					numCarsOnTrack++;
				}
			}

			if ( numCarsOnTrack > 0 )
			{
				var targetIndex = (int) Math.Floor( IRSDK.normalizedData.sessionTime / 10.0f ) % numCarsOnTrack;

				var currentIndex = 0;

				foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard && !normalizedCar.isOutOfCar && !normalizedCar.isOnPitRoad )
					{
						if ( currentIndex == targetIndex )
						{
							return normalizedCar;
						}

						currentIndex++;
					}
				}
			}

			return null;
		}
	}
}

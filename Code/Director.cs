
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
		public static SettingsDirector.CameraType resolvedCamType = SettingsDirector.CameraType.Scenic;
		public static SettingsDirector.CameraType targetCamType = SettingsDirector.CameraType.Scenic;
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

			var allowShowChyron = false;

			if ( !isEnabled )
			{
				allowShowChyron = true;

				var targetNormalizedCar = IRSDK.normalizedData.normalizedCars[ targetCamCarIdx ];

				var previousAutoCamType = autoCamType;

				UpdateAutoCamType( targetNormalizedCar );

				if ( ( targetCamType == SettingsDirector.CameraType.AutoCam ) && ( autoCamType != previousAutoCamType ) )
				{
					IRSDK.targetCamGroupNumber = GetCamGroupNumber( targetCamType );
				}
			}
			else
			{
				IncidentData? currentIncident = IncidentPlayback.GetCurrentIncidentData();

				NormalizedCar? firstPlaceCar = null;
				NormalizedCar? leadingOnTrackCar = null;
				NormalizedCar? leadingPittedCar = null;
				NormalizedCar? hottestCar = null;
				NormalizedCar? slowestOnTrackCar = null;
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

						if ( !normalizedCar.isOutOfCar )
						{
							if ( ( slowestOnTrackCar == null ) || ( ( !normalizedCar.isOnPitRoad || ( normalizedCar.speedInMetersPerSecond > 2 ) ) && ( normalizedCar.speedInMetersPerSecond < slowestOnTrackCar.speedInMetersPerSecond ) ) )
							{
								slowestOnTrackCar = normalizedCar;
							}
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
					targetCamReason = "Rule 1: Post-race cool down";
				}
				else if ( Settings.director.rule2_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateRacing ) && Settings.director.preferredCarLockOnEnabled && ( preferredCar != null ) && ( ( preferredCar.heatTotal >= Settings.director.preferredCarLockOnMinimumHeat ) || ( ( preferredCar.normalizedCarBehind?.heatTotal ?? 0 ) >= Settings.director.preferredCarLockOnMinimumHeat ) ) ) )
				{
					allowShowChyron = true;

					targetCamCarIdx = preferredCar.carIdx;
					targetCamType = Settings.director.rule2_Camera;
					targetCamReason = "Rule 2: Preferred car lock-on";
				}
				else if ( Settings.director.rule3_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateCheckered ) ) )
				{
					var maxLapDistPctDelta = 120.0f / IRSDK.normalizedSession.trackLengthInMeters;
					var minLapDistPct = 1.0 - maxLapDistPctDelta;
					var maxLapDistPct = maxLapDistPctDelta;

					var highestLapPosition = 0.0f;

					foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
					{
						if ( normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar )
						{
							if ( ( normalizedCar.lapDistPct > minLapDistPct ) || ( normalizedCar.lapDistPct < maxLapDistPct ) )
							{
								if ( normalizedCar.lapPosition > highestLapPosition )
								{
									highestLapPosition = normalizedCar.lapPosition;

									targetCamFastSwitchEnabled = true;
									targetCamCarIdx = normalizedCar.carIdx;
									targetCamType = Settings.director.rule3_Camera;
									targetCamReason = "Rule 3: Checkered flag";

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
					targetCamReason = "Rule 4: Incident";

					driverWasTalking = false;
				}
				else if ( Settings.director.rule5_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState >= SessionState.StateRacing ) && ( leadingOnTrackCar != null ) && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.GreenHeld | (uint) SessionFlags.StartReady | (uint) SessionFlags.StartSet | (uint) SessionFlags.StartGo ) ) != 0 ) ) )
				{
					targetCamFastSwitchEnabled = true;
					targetCamCarIdx = leadingOnTrackCar.carIdx;
					targetCamType = Settings.director.rule5_Camera;
					targetCamReason = "Rule 5: Green flag is about to be shown or is waving";

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
						allowShowChyron = true;

						targetCamSlowSwitchEnabled = true;
						targetCamCarIdx = randomCar.carIdx;
						targetCamType = Settings.director.rule7_Camera;
						targetCamReason = "Rule 7: Practice session";
					}
					else
					{
						targetCamCarIdx = IRSDK.normalizedData.paceCar?.carIdx ?? 0;
						targetCamType = SettingsDirector.CameraType.Scenic;
						targetCamReason = "Rule 7: Practice session (no cars on track)";
					}
				}
				else if ( Settings.director.rule8_Enabled && ( IRSDK.normalizedSession.isInQualifyingSession ) )
				{
					if ( randomCar != null )
					{
						allowShowChyron = true;

						targetCamSlowSwitchEnabled = true;
						targetCamCarIdx = randomCar.carIdx;
						targetCamType = Settings.director.rule8_Camera;
						targetCamReason = "Rule 8: Qualifying session";
					}
					else
					{
						targetCamCarIdx = IRSDK.normalizedData.paceCar?.carIdx ?? 0;
						targetCamType = SettingsDirector.CameraType.Scenic;
						targetCamReason = "Rule 8: Qualifying session (no cars on track)";
					}
				}
				else if ( Settings.director.rule9_Enabled && ( ( ( leadingOnTrackCar != null ) || ( leadingPittedCar != null ) || ( IRSDK.normalizedData.paceCar != null ) ) && ( IRSDK.normalizedData.sessionState == SessionState.StateWarmup ) ) )
				{
					var normalizedCar = leadingOnTrackCar ?? leadingPittedCar ?? IRSDK.normalizedData.paceCar;

					if ( normalizedCar != null )
					{
						targetCamCarIdx = normalizedCar.carIdx;
						targetCamType = Settings.director.rule9_Camera;
						targetCamReason = "Rule 9: Cars are warming up";
					}
				}
				else if ( Settings.director.rule10_Enabled && ( ( ( IRSDK.normalizedData.paceCar != null ) && !IRSDK.normalizedData.paceCar.isOnPitRoad ) && ( IRSDK.normalizedData.sessionState == SessionState.StateParadeLaps ) ) )
				{
					targetCamCarIdx = IRSDK.normalizedData.paceCar.carIdx;
					targetCamType = Settings.director.rule10_Camera;
					targetCamReason = "Rule 10: Cars are doing parade laps";
				}
				else if ( Settings.director.rule11_Enabled && ( ( ( leadingOnTrackCar != null ) || ( leadingPittedCar != null ) || ( IRSDK.normalizedData.paceCar != null ) ) && ( IRSDK.normalizedData.sessionState == SessionState.StateGetInCar ) ) )
				{
					var normalizedCar = leadingOnTrackCar ?? leadingPittedCar ?? IRSDK.normalizedData.paceCar;

					if ( normalizedCar != null )
					{
						targetCamCarIdx = normalizedCar.carIdx;
						targetCamType = Settings.director.rule11_Camera;
						targetCamReason = "Rule 11: Drivers are getting into their cars";
					}
				}
				else if ( Settings.director.rule12_Enabled && ( ( ( slowestOnTrackCar != null ) || ( firstPlaceCar != null ) ) && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving ) ) != 0 ) ) )
				{
					var normalizedCar = slowestOnTrackCar ?? firstPlaceCar;

					if ( normalizedCar != null )
					{
						targetCamCarIdx = normalizedCar.carIdx;
						targetCamType = Settings.director.rule12_Camera;
						targetCamReason = "Rule 12: Caution flag is waving";
					}
				}
				else if ( Settings.director.rule13_Enabled && ( ( IRSDK.normalizedData.paceCar != null ) && !IRSDK.normalizedData.paceCar.isOnPitRoad ) && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.OneLapToGreen ) ) != 0 ) && ( IRSDK.normalizedData.sessionState >= SessionState.StateParadeLaps ) )
				{
					targetCamCarIdx = IRSDK.normalizedData.paceCar.carIdx;
					targetCamType = Settings.director.rule13_Camera;
					targetCamReason = "Rule 13: One lap to green";
				}
				else if ( Settings.director.rule14_Enabled )
				{
					if ( hottestCar != null )
					{
						allowShowChyron = true;

						targetCamCarIdx = hottestCar.carIdx;
						targetCamType = Settings.director.rule14_Camera;
						targetCamReason = "Rule 14: Hottest car";
					}
				}

				var targetNormalizedCar = IRSDK.normalizedData.normalizedCars[ targetCamCarIdx ];
				var previousAutoCamType = autoCamType;

				UpdateAutoCamType( targetNormalizedCar );

				if ( ( Director.targetCamCarIdx != targetCamCarIdx ) || ( Director.targetCamType != targetCamType ) || ( ( targetCamType == SettingsDirector.CameraType.AutoCam ) && ( autoCamType != previousAutoCamType ) ) )
				{
					Director.targetCamCarIdx = targetCamCarIdx;
					Director.targetCamType = targetCamType;

					IRSDK.targetCamFastSwitchEnabled = targetCamFastSwitchEnabled;
					IRSDK.targetCamSlowSwitchEnabled = targetCamSlowSwitchEnabled;
					IRSDK.targetCamCarIdx = targetCamCarIdx;
					IRSDK.targetCamGroupNumber = GetCamGroupNumber( targetCamType );
					IRSDK.targetCamReason = targetCamReason;

					MainWindow.Instance.cameraType = targetCamType;
				}
			}

			showChyron = false;

			if ( allowShowChyron )
			{
				if ( ( resolvedCamType == SettingsDirector.CameraType.Pits ) || ( resolvedCamType == SettingsDirector.CameraType.Inside ) || ( resolvedCamType == SettingsDirector.CameraType.Close ) || ( resolvedCamType == SettingsDirector.CameraType.Medium ) )
				{
					showChyron = true;
				}
			}

			chyronTimer += Program.deltaTime;

			if ( showChyron == false )
			{
				chyronTimer = 0;
			}

			if ( chyronTimer < 2 )
			{
				showChyron = false;
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
				var inCautionButNotWaving = IRSDK.normalizedData.isUnderCaution && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving ) ) == 0 );

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

		public static int GetCamGroupNumber( SettingsDirector.CameraType cameraType )
		{
			if ( cameraType == SettingsDirector.CameraType.AutoCam )
			{
				resolvedCamType = autoCamType;

				return GetAutoCamGroupNumber();
			}
			else
			{
				resolvedCamType = cameraType;

				return IRSDK.GetCamGroupNumber( cameraType );
			}
		}

		private static int GetAutoCamGroupNumber()
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

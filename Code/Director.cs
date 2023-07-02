
using System;

using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController
{
	public static class Director
	{
		public static bool isEnabled = false;
		public static bool isOverridden = false;
		public static bool driverWasTalking = false;

		public static int targetCamCarIdx = 0;
		public static SettingsDirector.CameraType targetCamType = SettingsDirector.CameraType.Intro;

		public static void Update()
		{
			if ( !IRSDK.isConnected )
			{
				IRSDK.targetCamReason = "Not connected to iRacing.";

				return;
			}

			if ( !isEnabled )
			{
				IRSDK.targetCamReason = "Director is not enabled.";

				return;
			}

			if ( isOverridden )
			{
				return;
			}

			IncidentData? currentIncident = IncidentScan.GetCurrentIncident();

			NormalizedCar? firstPlaceCar = null;
			NormalizedCar? leadingOnTrackCar = null;
			NormalizedCar? leadingPittedCar = null;
			NormalizedCar? preferredCar = null;
			NormalizedCar? hottestCar = null;
			NormalizedCar? incidentCar = ( currentIncident != null ) ? IRSDK.normalizedData.FindNormalizedCarByCarIdx( currentIncident.CarIdx ) : null;
			NormalizedCar? talkingCar = ( IRSDK.normalizedData.radioTransmitCarIdx != -1 ) ? IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.radioTransmitCarIdx ) : null;
			NormalizedCar? randomCar = GetRandomNormalizedCar();

			foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardPositionSortedNormalizedCars )
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

					if ( normalizedCar.carNumber == Settings.director.preferredCarNumber )
					{
						preferredCar = normalizedCar;
					}

					if ( ( normalizedCar.attackingHeat > 0 ) && ( ( hottestCar == null ) || ( ( normalizedCar.attackingHeat + normalizedCar.heatBias ) > ( hottestCar.attackingHeat + hottestCar.heatBias ) ) ) )
					{
						hottestCar = normalizedCar;
					}
				}
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
			else if ( Settings.director.rule2_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateRacing ) && Settings.director.preferredCarLockOnEnabled && ( preferredCar != null ) && ( ( preferredCar.attackingHeat >= Settings.director.preferredCarLockOnMinimumHeat ) || ( preferredCar.defendingHeat >= Settings.director.preferredCarLockOnMinimumHeat ) ) ) )
			{
				targetCamCarIdx = preferredCar.carIdx;
				targetCamType = Settings.director.rule2_Camera;
				targetCamReason = "Rule 2: Racing, preferred car lock-on enabled, and preferred car heat >= minimum, look at preferred car.";
			}
			else if ( Settings.director.rule3_Enabled && ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateCheckered ) ) )
			{
				var highestLapPosition = 0.0f;

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardPositionSortedNormalizedCars )
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

				IRSDK.cameraSwitchWaitTimeRemaining = Settings.director.switchDelayRadioChatter;
			}
			else if ( Settings.director.rule7_Enabled && ( IRSDK.normalizedSession.isInPracticeSession ) )
			{
				if ( randomCar != null )
				{
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
				var normalizedCar = leadingOnTrackCar ?? leadingPittedCar;

				if ( normalizedCar != null )
				{
					targetCamCarIdx = normalizedCar.carIdx;
					targetCamType = Settings.director.rule10_Camera;
					targetCamReason = "Rule 10: Cars are doing parade laps.";
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
			else if ( Settings.director.rule13_Enabled )
			{
				if ( hottestCar != null )
				{
					targetCamCarIdx = hottestCar.carIdx;
					targetCamType = Settings.director.rule13_Camera;
					targetCamReason = "Rule 13: Hottest car.";
				}
			}

			if ( ( Director.targetCamCarIdx != targetCamCarIdx ) || ( Director.targetCamType != targetCamType ) )
			{
				Director.targetCamCarIdx = targetCamCarIdx;
				Director.targetCamType = targetCamType;

				IRSDK.targetCamEnabled = true;
				IRSDK.targetCamFastSwitchEnabled = targetCamFastSwitchEnabled;
				IRSDK.targetCamSlowSwitchEnabled = targetCamSlowSwitchEnabled;
				IRSDK.targetCamCarIdx = targetCamCarIdx;
				IRSDK.targetCamGroupNumber = GetCamGroupNumber( IRSDK.normalizedData.normalizedCars[ targetCamCarIdx ], targetCamType );
				IRSDK.targetCamReason = targetCamReason;
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

			var inCautionButNotWaving = IRSDK.normalizedData.isUnderCaution && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) == 0 );

			if ( !inCautionButNotWaving && ( normalizedCar.distanceToCarInFrontInMeters >= Settings.director.autoCamInsideMinimum ) && ( normalizedCar.distanceToCarInFrontInMeters <= Settings.director.autoCamInsideMaximum ) && ( IRSDK.camCarIdx == normalizedCar.carIdx ) )
			{
				camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasInside );
			}
			else
			{
				var nearestCarDistance = Math.Min( normalizedCar.distanceToCarInFrontInMeters, normalizedCar.distanceToCarBehindInMeters );

				if ( nearestCarDistance <= Settings.director.autoCamCloseMaximum )
				{
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasClose );
				}
				else if ( nearestCarDistance <= Settings.director.autoCamMediumMaximum )
				{
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasMedium );
				}
				else if ( nearestCarDistance <= Settings.director.autoCamFarMaximum )
				{
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasFar );
				}
				else
				{
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasVeryFar );
				}
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

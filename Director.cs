
using System;

using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController
{
	public static class Director
	{
		public static bool isEnabled = false;
		public static bool driverWasTalking = false;

		public static void Update()
		{
			if ( !isEnabled )
			{
				return;
			}

			IncidentData? currentIncident = IncidentScan.GetCurrentIncident();

			IRSDK.targetCamEnabled = true;
			IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasFar );
			IRSDK.targetCamReason = string.Empty;

			NormalizedCar? firstPlaceCar = null;
			NormalizedCar? firstVisibleCar = null;
			NormalizedCar? preferredCar = null;
			NormalizedCar? hottestCar = null;

			foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
			{
				if ( normalizedCar.includeInLeaderboard )
				{
					firstPlaceCar ??= normalizedCar;

					if ( ( firstVisibleCar == null ) && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar )
					{
						firstVisibleCar = normalizedCar;
					}

					if ( normalizedCar.carNumber == Settings.director.preferredCarNumber )
					{
						preferredCar = normalizedCar;
					}

					if ( ( normalizedCar.attackingHeat > 0 ) && ( ( hottestCar == null ) || ( normalizedCar.attackingHeat > hottestCar.attackingHeat ) ) )
					{
						hottestCar = normalizedCar;
					}
				}
			}

			if ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateCoolDown ) && ( firstPlaceCar != null ) && !firstPlaceCar.isOutOfCar )
			{
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasClose );

				IRSDK.targetCamCarIdx = firstPlaceCar.carIdx;
				IRSDK.targetCamReason = $"Race is cooling down and this car is the winner";
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && Settings.director.preferredCarLockOnHeatEnabled && ( IRSDK.normalizedData.sessionState == SessionState.StateRacing ) && ( preferredCar != null ) && ( ( preferredCar.attackingHeat >= Settings.director.preferredCarLockOnMinimumHeat ) || ( preferredCar.defendingHeat >= Settings.director.preferredCarLockOnMinimumHeat ) ) )
			{
				IRSDK.targetCamReason = $"Preferred car heat is >= {Settings.director.preferredCarLockOnMinimumHeat}";

				IRSDK.targetCamGroupNumber = ChooseCamGroupNumber( preferredCar );
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState == SessionState.StateCheckered ) )
			{
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasMedium );

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

								IRSDK.targetCamCarIdx = normalizedCar.carIdx;
								IRSDK.cameraSwitchWaitTicksRemaining = IRSDK.sendMessageWaitTicksRemaining;
								IRSDK.targetCamReason = "Checkered flag and this car is closest to the finish line";
							}
						}
					}
				}

				driverWasTalking = false;
			}
			else if ( currentIncident != null )
			{
				IRSDK.targetCamFastSwitchEnabled = true;
				IRSDK.targetCamCarIdx = currentIncident.CarIdx;
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasMedium );
				IRSDK.targetCamReason = $"Incident at frame {currentIncident.FrameNumber} involving this car";

				driverWasTalking = false;
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState >= SessionState.StateRacing ) && ( firstVisibleCar != null ) && ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.GreenHeld | (uint) SessionFlags.StartReady | (uint) SessionFlags.StartSet | (uint) SessionFlags.StartGo ) ) != 0 ) )
			{
				IRSDK.targetCamCarIdx = firstVisibleCar.carIdx;
				IRSDK.targetCamReason = "Green flag is about to be shown or is waving";

				driverWasTalking = false;
			}
			else if ( Settings.director.switchToTalkingDrivers && ( IRSDK.normalizedData.radioTransmitCarIdx != -1 ) )
			{
				IRSDK.targetCamFastSwitchEnabled = true;
				IRSDK.targetCamCarIdx = IRSDK.normalizedData.radioTransmitCarIdx;
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasClose );
				IRSDK.targetCamReason = "Driver of this car is talking";

				driverWasTalking = true;
			}
			else if ( driverWasTalking )
			{
				driverWasTalking = false;

				IRSDK.cameraSwitchWaitTicksRemaining = (int) Math.Ceiling( Settings.director.switchDelayRadioChatter * 60 );
			}
			else if ( IRSDK.normalizedSession.isInPracticeSession )
			{
				// IRSDK.targetCamCarIdx = 0; TODO
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasPractice );
				IRSDK.targetCamReason = "In practice session";
			}
			else if ( IRSDK.normalizedSession.isInQualifyingSession )
			{
				// IRSDK.targetCamCarIdx = 0; TODO
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasQualifying );
				IRSDK.targetCamReason = "In qualifying session";
			}
			else if ( ( firstVisibleCar != null ) && ( IRSDK.normalizedData.sessionState == SessionState.StateWarmup ) )
			{
				IRSDK.targetCamCarIdx = firstVisibleCar.carIdx;
				IRSDK.targetCamReason = "Cars are warming up and this is the lead car";

				driverWasTalking = false;
			}
			else if ( ( firstVisibleCar != null ) && ( IRSDK.normalizedData.sessionState == SessionState.StateParadeLaps ) )
			{
				IRSDK.targetCamCarIdx = firstVisibleCar.carIdx;
				IRSDK.targetCamReason = "Doing parade laps and this is the lead car";

				driverWasTalking = false;
			}
			else if ( IRSDK.normalizedData.sessionState == SessionState.StateGetInCar )
			{
				IRSDK.targetCamCarIdx = 0;
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasIntro );
				IRSDK.targetCamReason = "The race has not started yet";
			}
			else
			{
				IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasMedium );

				if ( hottestCar != null )
				{
					IRSDK.targetCamReason = "This is the hottest car";

					IRSDK.targetCamGroupNumber = ChooseCamGroupNumber( hottestCar );
				}
				else if ( firstVisibleCar != null )
				{
					IRSDK.targetCamReason = "There are no hot cars and this is the lead car";

					IRSDK.targetCamGroupNumber = ChooseCamGroupNumber( firstVisibleCar );
				}

				if ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 )
				{
					IRSDK.targetCamGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasFar );
				}
				else if ( ( preferredCar != null ) && ( ( preferredCar.distanceToCarInFrontInMeters < 50 ) || ( preferredCar.distanceToCarBehindInMeters < 50 ) ) )
				{
					IRSDK.targetCamReason = "This is the preferred car";

					IRSDK.targetCamGroupNumber = ChooseCamGroupNumber( preferredCar );
				}
			}
		}

		private static int ChooseCamGroupNumber( NormalizedCar normalizedCar )
		{
			IRSDK.targetCamCarIdx = normalizedCar.carIdx;

			int camGroupNumber;

			if ( IRSDK.normalizedData.isUnderCaution )
			{
				camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasFar );

				IRSDK.targetCamReason += " (under caution)";
			}
			else if ( ( normalizedCar.distanceToCarInFrontInMeters > 2 ) && ( normalizedCar.distanceToCarInFrontInMeters < 12 ) && ( IRSDK.camCarIdx == normalizedCar.carIdx ) )
			{
				camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasInside );

				IRSDK.targetCamReason += " (car in front within 12m)";
			}
			else
			{
				var nearestCarDistance = Math.Min( normalizedCar.distanceToCarInFrontInMeters, normalizedCar.distanceToCarBehindInMeters );

				if ( nearestCarDistance < 10 )
				{
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasClose );

					IRSDK.targetCamReason += " (car within 10m)";
				}
				else if ( nearestCarDistance < 20 )
				{
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasMedium );

					IRSDK.targetCamReason += " (car within 20m)";
				}
				else if ( nearestCarDistance < 30 )
				{
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasFar );

					IRSDK.targetCamReason += " (car within 30m)";
				}
				else
				{
					camGroupNumber = IRSDK.GetCamGroupNumber( Settings.director.camerasVeryFar );

					IRSDK.targetCamReason += " (car within 50m)";
				}
			}

			return camGroupNumber;
		}
	}
}

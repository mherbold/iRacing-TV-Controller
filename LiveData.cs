
using System;

using irsdkSharp.Serialization.Enums.Fastest;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveData
	{
		public static LiveData Instance { get; private set; }

		public LiveDataRaceStatus liveDataRaceStatus = new();
		public LiveDataLeaderboard liveDataLeaderboard = new();
		public LiveDataVoiceOf liveDataVoiceOf = new();
		public LiveDataSubtitle liveDataSubtitle = new();

		public string seriesLogoTextureUrl = string.Empty;

		static LiveData()
		{
			Instance = new LiveData();
		}

		private LiveData()
		{
			Instance = this;
		}

		public void Update()
		{
			if ( !IRSDK.isConnected )
			{
				return;
			}

			Settings.UpdateCombinedOverlay();

			UpdateRaceStatus();
			UpdateLeaderboard();
			UpdateVoiceOf();
			UpdateSubtitle();

			seriesLogoTextureUrl = IRSDK.normalizedSession.seriesLogoTextureUrl;

			IPC.readyToSendLiveData = true;
		}

		public void UpdateRaceStatus()
		{
			// session name

			liveDataRaceStatus.sessionNameText = IRSDK.normalizedSession.sessionName;

			// laps remaining

			if ( IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
			{
				liveDataRaceStatus.lapsRemainingText = GetTimeString( IRSDK.normalizedData.sessionTimeRemaining, false );
			}
			else if ( IRSDK.normalizedData.sessionLapsRemaining == 0 )
			{
				liveDataRaceStatus.lapsRemainingText = Settings.combined.translationDictionary[ "FinalLap" ].translation;
			}
			else
			{
				var lapsRemaining = Math.Min( IRSDK.normalizedData.sessionLapsTotal, IRSDK.normalizedData.sessionLapsRemaining + 1 );

				liveDataRaceStatus.lapsRemainingText = lapsRemaining.ToString() + " " + Settings.combined.translationDictionary[ "ToGo" ].translation;
			}

			// lights

			liveDataRaceStatus.showBlackLight = false;
			liveDataRaceStatus.showGreenLight = false;
			liveDataRaceStatus.showWhiteLight = false;
			liveDataRaceStatus.showYellowLight = false;

			if ( IRSDK.normalizedData.isUnderCaution )
			{
				liveDataRaceStatus.showYellowLight = true;
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && ( IRSDK.normalizedData.sessionState != SessionState.StateRacing ) )
			{
				liveDataRaceStatus.showBlackLight = true;
			}
			else if ( IRSDK.normalizedSession.isInRaceSession && ( ( IRSDK.normalizedData.sessionLapsRemaining == 0 ) || ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.White ) != 0 ) ) )
			{
				liveDataRaceStatus.showWhiteLight = true;
			}
			else
			{
				liveDataRaceStatus.showGreenLight = true;
			}

			// lap / time string

			if ( IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
			{
				liveDataRaceStatus.unitsText = Settings.combined.translationDictionary[ "Time" ].translation;
			}
			else
			{
				liveDataRaceStatus.unitsText = Settings.combined.translationDictionary[ "Lap" ].translation;
			}

			// current lap

			if ( IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
			{
				liveDataRaceStatus.currentLapText = GetTimeString( IRSDK.normalizedData.sessionTimeTotal - IRSDK.normalizedData.sessionTimeRemaining, false ) + " | " + GetTimeString( IRSDK.normalizedData.sessionTimeTotal, false );
			}
			else
			{
				liveDataRaceStatus.currentLapText = IRSDK.normalizedData.currentLap.ToString() + " | " + IRSDK.normalizedData.sessionLapsTotal.ToString();
			}

			// flags

			liveDataRaceStatus.showGreenFlag = false;
			liveDataRaceStatus.showYellowFlag = false;
			liveDataRaceStatus.showCheckeredFlag = false;

			if ( IRSDK.normalizedSession.isInRaceSession )
			{
				if ( IRSDK.normalizedData.sessionState >= SessionState.StateCheckered )
				{
					liveDataRaceStatus.showCheckeredFlag = true;
				}
				else if ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.YellowWaving ) ) != 0 )
				{
					liveDataRaceStatus.showYellowFlag = true;
				}
				else if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartGo ) != 0 )
				{
					liveDataRaceStatus.showGreenFlag = true;
				}
			}
		}

		public void UpdateLeaderboard()
		{
			// leaderboard splits

			var bottomSplitCount = Settings.combined.leaderboardPlaceCount / 2;
			var bottomSplitLastPosition = Settings.combined.leaderboardPlaceCount;

			if ( bottomSplitCount > 0 )
			{
				if ( !IRSDK.normalizedSession.isInQualifyingSession )
				{
					foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
					{
						if ( !normalizedCar.includeInLeaderboard )
						{
							break;
						}

						if ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx )
						{
							if ( normalizedCar.leaderboardPosition > bottomSplitLastPosition )
							{
								while ( bottomSplitLastPosition < normalizedCar.leaderboardPosition )
								{
									bottomSplitLastPosition += bottomSplitCount;
								}

								if ( bottomSplitLastPosition > IRSDK.normalizedData.numLeaderboardCars )
								{
									bottomSplitLastPosition = IRSDK.normalizedData.numLeaderboardCars;
								}

								break;
							}
						}
					}
				}
			}

			var topSplitFirstPosition = 1;
			var topSplitLastPosition = Settings.combined.leaderboardPlaceCount - bottomSplitCount;
			var bottomSplitFirstPosition = bottomSplitLastPosition - bottomSplitCount + 1;

			// leaderboard

			liveDataLeaderboard.show = false;

			NormalizedCar? normalizedCarInFront = null;

			var carInFrontLapPosition = 0.0f;
			var leadCarF2Time = 0.0f;
			var carsShown = 0;

			foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
			{
				// stop when we run out of cars to show

				if ( !normalizedCar.includeInLeaderboard )
				{
					break;
				}

				// lead car f2 time

				if ( leadCarF2Time == 0 )
				{
					leadCarF2Time = normalizedCar.f2Time;
				}

				// check if the car is visible on the leaderboard

				liveDataLeaderboard.liveDataPlaces[ normalizedCar.carIdx ].show = ( ( ( normalizedCar.leaderboardPosition >= topSplitFirstPosition ) && ( normalizedCar.leaderboardPosition <= topSplitLastPosition ) ) || ( ( normalizedCar.leaderboardPosition >= bottomSplitFirstPosition ) && ( normalizedCar.leaderboardPosition <= bottomSplitLastPosition ) ) );

				// hide cars that have not qualified yet (only during qualifying)

				if ( IRSDK.normalizedSession.isInQualifyingSession )
				{
					if ( normalizedCar.f2Time == 0 )
					{
						liveDataLeaderboard.liveDataPlaces[ normalizedCar.carIdx ].show = false;
					}
				}

				// skip cars not visible on the leaderboard

				if ( !liveDataLeaderboard.liveDataPlaces[ normalizedCar.carIdx ].show )
				{
					normalizedCar.wasVisibleOnLeaderboard = false;

					continue;
				}

				// at least one car is visible so we want to show the leaderboard

				liveDataLeaderboard.show = true;

				carsShown++;

				// place index

				var placeIndex = normalizedCar.leaderboardPosition - ( ( normalizedCar.leaderboardPosition >= bottomSplitFirstPosition ) ? bottomSplitFirstPosition - topSplitLastPosition : topSplitFirstPosition );

				// get place

				var liveDataPlace = liveDataLeaderboard.liveDataPlaces[ normalizedCar.carIdx ];

				// compute place position

				var targetPlacePosition = new Vector2( Settings.combined.leaderboardPlaceSpacing.x, -Settings.combined.leaderboardPlaceSpacing.y ) * placeIndex + new Vector2( Settings.combined.leaderboardFirstPlacePosition.x, -Settings.combined.leaderboardFirstPlacePosition.y );

				if ( normalizedCar.wasVisibleOnLeaderboard )
				{
					normalizedCar.placePosition += ( targetPlacePosition - normalizedCar.placePosition ) * 0.15f;
				}
				else
				{
					normalizedCar.placePosition = targetPlacePosition;
				}

				liveDataPlace.position = normalizedCar.placePosition;

				// update place text

				liveDataPlace.placeText = normalizedCar.leaderboardPosition.ToString();

				// car number texture url

				liveDataPlace.carNumberTextureUrl = normalizedCar.carNumberTextureUrl;

				// car texture url

				liveDataPlace.carTextureUrl = normalizedCar.carTextureUrl;

				// helmet texture url

				liveDataPlace.helmetTextureUrl = normalizedCar.helmetTextureUrl;

				// driver name

				liveDataPlace.driverNameText = normalizedCar.abbrevName;

				// driver name color

				if ( Settings.combined.leaderboardUseClassColors )
				{
					var tintColor = Settings.combined.textSettingsDataDictionary[ "DriverName" ].tintColor;

					liveDataPlace.driverNameColor = Color.Lerp( tintColor, normalizedCar.classColor, Settings.combined.leaderboardClassColorStrength );
				}
				else
				{
					liveDataPlace.driverNameColor = Settings.combined.textSettingsDataDictionary[ "DriverName" ].tintColor;
				}

				// telemetry

				liveDataPlace.telemetryText = string.Empty;
				liveDataPlace.telemetryColor = Settings.combined.textSettingsDataDictionary[ "Telemetry" ].tintColor;

				if ( IRSDK.normalizedSession.isInQualifyingSession )
				{
					if ( leadCarF2Time == normalizedCar.f2Time )
					{
						liveDataPlace.telemetryText = $"{leadCarF2Time:0.000}";
					}
					else
					{
						var deltaTime = normalizedCar.f2Time - leadCarF2Time;

						liveDataPlace.telemetryText = $"-{deltaTime:0.000}";
					}
				}
				else if ( normalizedCar.isOnPitRoad )
				{
					liveDataPlace.telemetryText = Settings.combined.translationDictionary[ "Pit" ].translation;
					liveDataPlace.telemetryColor = Settings.combined.telemetryPitColor;
				}
				else if ( normalizedCar.isOutOfCar )
				{
					liveDataPlace.telemetryText = Settings.combined.translationDictionary[ "Out" ].translation;
					liveDataPlace.telemetryColor = Settings.combined.telemetryOutColor;
				}
				else if ( IRSDK.normalizedSession.isInRaceSession )
				{
					if ( ( IRSDK.normalizedData.sessionState == SessionState.StateRacing ) && normalizedCar.hasCrossedStartLine )
					{
						if ( !Settings.combined.telemetryIsBetweenCars && normalizedCar.lapPositionRelativeToLeader >= 1.0f )
						{
							var wholeLapsDown = Math.Floor( normalizedCar.lapPositionRelativeToLeader );

							liveDataPlace.telemetryText = $"-{wholeLapsDown:0} {Settings.combined.translationDictionary[ "LapsAbbreviation" ].translation}";
						}
						else if ( !IRSDK.normalizedData.isUnderCaution )
						{
							var lapPosition = Settings.combined.telemetryIsBetweenCars ? carInFrontLapPosition - normalizedCar.lapPosition : normalizedCar.lapPositionRelativeToLeader;

							if ( lapPosition > 0 )
							{
								if ( Settings.combined.telemetryMode == 0 )
								{
									liveDataPlace.telemetryText = $"-{lapPosition:0.000} {Settings.combined.translationDictionary[ "LapsAbbreviation" ].translation}";
								}
								else if ( Settings.combined.telemetryMode == 1 )
								{
									var distance = lapPosition * IRSDK.normalizedSession.trackLengthInMeters;

									if ( IRSDK.normalizedData.displayIsMetric )
									{
										var distanceString = $"{distance:0}";

										if ( distanceString != "0" )
										{
											liveDataPlace.telemetryText = $"-{distanceString} {Settings.combined.translationDictionary[ "MetersAbbreviation" ].translation}";
										}
									}
									else
									{
										distance *= 3.28084f;

										var distanceString = $"{distance:0}";

										if ( distanceString != "0" )
										{
											liveDataPlace.telemetryText = $"-{distanceString} {Settings.combined.translationDictionary[ "FeetAbbreviation" ].translation}";
										}
									}
								}
								else
								{
									if ( ( normalizedCarInFront != null ) && ( normalizedCarInFront.checkpoints[ normalizedCar.checkpointIdx ] > 0 ) )
									{
										var checkpointTime = (float) ( normalizedCarInFront.checkpoints[ normalizedCar.checkpointIdx ] - normalizedCar.checkpoints[ normalizedCar.checkpointIdx ] );

										var deltaCheckpointTime = checkpointTime - normalizedCar.checkpointTime;

										if ( Math.Abs( deltaCheckpointTime ) < 0.05 )
										{
											normalizedCar.checkpointTime += deltaCheckpointTime * 0.05f;
										}
										else
										{
											normalizedCar.checkpointTime = checkpointTime;
										}

										liveDataPlace.telemetryText = $"{normalizedCar.checkpointTime:0.00}";
									}
								}
							}
						}
					}
				}

				carInFrontLapPosition = normalizedCar.lapPosition;

				// current target and speed

				if ( !IRSDK.normalizedSession.isInQualifyingSession )
				{
					liveDataPlace.showHighlight = ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx );

					liveDataPlace.speedText = $"{normalizedCar.speedInMetersPerSecond * ( IRSDK.normalizedData.displayIsMetric ? 3.6f : 2.23694f ):0} {( IRSDK.normalizedData.displayIsMetric ? Settings.combined.translationDictionary[ "KPH" ].translation : Settings.combined.translationDictionary[ "MPH" ].translation )}";
				}
				else
				{
					liveDataPlace.showHighlight = false;
				}

				//

				normalizedCar.wasVisibleOnLeaderboard = true;

				if ( normalizedCarInFront == null || Settings.combined.telemetryIsBetweenCars )
				{
					normalizedCarInFront = normalizedCar;
				}
			}

			// leaderboard background and splitter

			liveDataLeaderboard.backgroundSize = Settings.combined.leaderboardPlaceSpacing * Math.Min( carsShown, Settings.combined.leaderboardPlaceCount );
			liveDataLeaderboard.showSplitter = ( ( topSplitLastPosition + 1 ) != bottomSplitFirstPosition );
			liveDataLeaderboard.splitterPosition = Settings.combined.leaderboardPlaceSpacing * topSplitLastPosition;
		}

		public void UpdateVoiceOf()
		{
			liveDataVoiceOf.show = ( IRSDK.normalizedData.radioTransmitCarIdx != -1 );

			liveDataVoiceOf.voiceOfText = Settings.combined.translationDictionary[ "VoiceOf" ].translation;

			if ( IRSDK.normalizedData.radioTransmitCarIdx != -1 )
			{
				var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.radioTransmitCarIdx );

				if ( normalizedCar != null )
				{
					liveDataVoiceOf.driverNameText = normalizedCar.userName;

					liveDataVoiceOf.carIdx = IRSDK.normalizedData.radioTransmitCarIdx;
				}
			}
		}

		public void UpdateSubtitle()
		{
			liveDataSubtitle.text = ChatLogPlayback.Playback( IRSDK.normalizedSession.sessionNumber, IRSDK.normalizedData.sessionTime ) ?? string.Empty;
		}

		public static string GetTimeString( double timeInSeconds, bool includeMilliseconds )
		{
			TimeSpan time = TimeSpan.FromSeconds( timeInSeconds );

			if ( time.Hours > 0 )
			{
				return time.ToString( @"h\:mm\:ss" );
			}
			else if ( includeMilliseconds )
			{
				if ( time.Minutes > 0 )
				{
					return time.ToString( @"m\:ss\.fff" );
				}
				else
				{
					return time.ToString( @"ss\.fff" );
				}
			}
			else
			{
				return time.ToString( @"m\:ss" );
			}
		}
	}
}

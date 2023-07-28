
using System;

using irsdkSharp.Serialization.Enums.Fastest;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveData
	{
		public static LiveData Instance { get; private set; }

		public bool isConnected = false;

		public LiveDataControlPanel liveDataControlPanel = new();
		public LiveDataRaceStatus liveDataRaceStatus = new();
		public LiveDataLeaderboard liveDataLeaderboard = new();
		public LiveDataVoiceOf liveDataVoiceOf = new();
		public LiveDataSubtitle liveDataSubtitle = new();
		public LiveDataIntro liveDataIntro = new();
		public LiveDataStartLights liveDataStartLights = new();
		public LiveDataTrackMap liveDataTrackMap = new();

		public string seriesLogoTextureUrl = string.Empty;

		[NonSerialized] public int lastFrameBottomSplitFirstPosition = 0;

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
			isConnected = IRSDK.isConnected;

			Settings.UpdateCombinedOverlay();

			UpdateControlPanel();
			UpdateRaceStatus();
			UpdateLeaderboard();
			UpdateTrackMap();
			UpdateVoiceOf();
			UpdateSubtitle();
			UpdateIntro();
			UpdateStartLights();

			seriesLogoTextureUrl = IRSDK.normalizedSession.seriesLogoTextureUrl;

			IPC.readyToSendLiveData = true;
		}

		public void UpdateControlPanel()
		{
			liveDataControlPanel.masterOn = MainWindow.Instance.masterOn;
			liveDataControlPanel.raceStatusOn = MainWindow.Instance.raceStatusOn;
			liveDataControlPanel.leaderboardOn = MainWindow.Instance.leaderboardOn;
			liveDataControlPanel.trackMapOn = MainWindow.Instance.trackMapOn;
			liveDataControlPanel.startLightsOn = MainWindow.Instance.startLightsOn;
			liveDataControlPanel.voiceOfOn = MainWindow.Instance.voiceOfOn;
			liveDataControlPanel.subtitlesOn = MainWindow.Instance.subtitlesOn;
			liveDataControlPanel.introOn = MainWindow.Instance.introOn;
			liveDataControlPanel.customLayerOn = MainWindow.Instance.customLayerOn;
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
				liveDataRaceStatus.lapsRemainingText = Settings.overlay.translationDictionary[ "FinalLap" ].translation;
			}
			else
			{
				var lapsRemaining = Math.Min( IRSDK.normalizedData.sessionLapsTotal, IRSDK.normalizedData.sessionLapsRemaining + 1 );

				liveDataRaceStatus.lapsRemainingText = lapsRemaining.ToString() + " " + Settings.overlay.translationDictionary[ "ToGo" ].translation;
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
				liveDataRaceStatus.unitsText = Settings.overlay.translationDictionary[ "Time" ].translation;
			}
			else
			{
				liveDataRaceStatus.unitsText = Settings.overlay.translationDictionary[ "Lap" ].translation;
			}

			// current lap

			if ( IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
			{
				liveDataRaceStatus.currentLapText = GetTimeString( Math.Ceiling( IRSDK.normalizedData.sessionTimeTotal - IRSDK.normalizedData.sessionTimeRemaining ), false ) + " | " + GetTimeString( IRSDK.normalizedData.sessionTimeTotal, false );
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

			var bottomSplitSlotCount = Settings.overlay.leaderboardSlotCount / 2;
			var bottomSplitLastSlotIndex = Settings.overlay.leaderboardSlotCount;

			if ( !IRSDK.normalizedSession.isInQualifyingSession )
			{
				if ( bottomSplitSlotCount > 0 )
				{
					foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardIndexSortedNormalizedCars )
					{
						if ( !normalizedCar.includeInLeaderboard )
						{
							break;
						}

						if ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx )
						{
							if ( normalizedCar.leaderboardIndex > bottomSplitLastSlotIndex )
							{
								while ( bottomSplitLastSlotIndex < normalizedCar.leaderboardIndex )
								{
									bottomSplitLastSlotIndex += bottomSplitSlotCount;
								}

								if ( bottomSplitLastSlotIndex > IRSDK.normalizedData.numLeaderboardCars )
								{
									bottomSplitLastSlotIndex = IRSDK.normalizedData.numLeaderboardCars;
								}

								break;
							}
						}
					}
				}
			}

			var topSplitFirstSlotIndex = 1;
			var topSplitLastSlotIndex = Settings.overlay.leaderboardSlotCount - bottomSplitSlotCount;
			var bottomSplitFirstSlotIndex = bottomSplitLastSlotIndex - bottomSplitSlotCount + 1;

			// leaderboard

			liveDataLeaderboard.show = false;

			NormalizedCar? carInFront = null;

			var leadCarBestLapTime = 0.0f;
			var carsShown = 0;

			foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardIndexSortedNormalizedCars )
			{
				// get slot

				var liveDataLeaderboardSlot = liveDataLeaderboard.liveDataLeaderboardSlots[ normalizedCar.carIdx ];

				// car number texture url

				liveDataLeaderboardSlot.carNumberTextureUrl = normalizedCar.carNumberTextureUrl;

				// car texture url

				liveDataLeaderboardSlot.carTextureUrl = normalizedCar.carTextureUrl;

				// helmet texture url

				liveDataLeaderboardSlot.helmetTextureUrl = normalizedCar.helmetTextureUrl;

				// driver texture url

				liveDataLeaderboardSlot.driverTextureUrl = normalizedCar.driverTextureUrl;

				// skip pace car and spectators

				if ( !normalizedCar.includeInLeaderboard )
				{
					liveDataLeaderboardSlot.show = false;
				}
				else
				{
					// reset car in front if different class

					if ( ( carInFront != null ) && ( carInFront.classID != normalizedCar.classID ) )
					{
						carInFront = null;
					}

					// lead car best lap time

					if ( carInFront == null )
					{
						leadCarBestLapTime = normalizedCar.bestLapTime;
					}

					// check if the car is visible on the leaderboard

					liveDataLeaderboardSlot.show = ( ( ( normalizedCar.leaderboardIndex >= topSplitFirstSlotIndex ) && ( normalizedCar.leaderboardIndex <= topSplitLastSlotIndex ) ) || ( ( normalizedCar.leaderboardIndex >= bottomSplitFirstSlotIndex ) && ( normalizedCar.leaderboardIndex <= bottomSplitLastSlotIndex ) ) );

					// hide cars that have not qualified yet (only during qualifying)

					if ( IRSDK.normalizedSession.isInQualifyingSession )
					{
						if ( normalizedCar.bestLapTime == 0 )
						{
							liveDataLeaderboardSlot.show = false;
						}
					}
				}

				// skip cars not visible on the leaderboard

				if ( !liveDataLeaderboardSlot.show )
				{
					normalizedCar.wasVisibleOnLeaderboard = false;
				}
				else
				{
					// at least one car is visible so we want to show the leaderboard

					liveDataLeaderboard.show = true;

					carsShown++;

					// slot index

					var slotIndex = normalizedCar.leaderboardIndex - ( ( normalizedCar.leaderboardIndex >= bottomSplitFirstSlotIndex ) ? bottomSplitFirstSlotIndex - topSplitLastSlotIndex : topSplitFirstSlotIndex );

					var resetSlotOffset = ( ( lastFrameBottomSplitFirstPosition != bottomSplitFirstSlotIndex ) && ( normalizedCar.leaderboardIndex >= bottomSplitFirstSlotIndex ) );

					// compute slot offset

					var targetSlotOffset = new Vector2( Settings.overlay.leaderboardSlotSpacing.x, -Settings.overlay.leaderboardSlotSpacing.y ) * slotIndex + new Vector2( Settings.overlay.leaderboardFirstSlotPosition.x, -Settings.overlay.leaderboardFirstSlotPosition.y );

					if ( normalizedCar.wasVisibleOnLeaderboard && !resetSlotOffset )
					{
						normalizedCar.leaderboardSlotOffset += ( targetSlotOffset - normalizedCar.leaderboardSlotOffset ) * 0.15f;
					}
					else
					{
						normalizedCar.leaderboardSlotOffset = targetSlotOffset;
					}

					liveDataLeaderboardSlot.offset = normalizedCar.leaderboardSlotOffset;

					// position text

					liveDataLeaderboardSlot.positionText = ( normalizedCar.displayedPosition >= 1 ) ? normalizedCar.displayedPosition.ToString() : "";

					// position text color

					var tintColor = Settings.overlay.textSettingsDataDictionary[ "LeaderboardPosition" ].tintColor;

					if ( Settings.overlay.leaderboardUseClassColors )
					{
						liveDataLeaderboardSlot.positionColor = Color.Lerp( tintColor, normalizedCar.classColor, Settings.overlay.leaderboardClassColorStrength );
					}
					else
					{
						liveDataLeaderboardSlot.positionColor = tintColor;
					}

					// driver name

					liveDataLeaderboardSlot.driverNameText = normalizedCar.abbrevName;

					// driver name color

					tintColor = Settings.overlay.textSettingsDataDictionary[ "LeaderboardPositionDriverName" ].tintColor;

					if ( Settings.overlay.leaderboardUseClassColors )
					{
						liveDataLeaderboardSlot.driverNameColor = Color.Lerp( tintColor, normalizedCar.classColor, Settings.overlay.leaderboardClassColorStrength );
					}
					else
					{
						liveDataLeaderboardSlot.driverNameColor = tintColor;
					}

					// telemetry

					var negativeSign = Settings.overlay.telemetryShowAsNegativeNumbers ? "-" : "";

					liveDataLeaderboardSlot.telemetryText = string.Empty;
					liveDataLeaderboardSlot.telemetryColor = Settings.overlay.textSettingsDataDictionary[ "LeaderboardPositionTelemetry" ].tintColor;

					if ( IRSDK.normalizedSession.isInPracticeSession || IRSDK.normalizedSession.isInQualifyingSession )
					{
						if ( normalizedCar.bestLapTime > 0 )
						{
							if ( leadCarBestLapTime == normalizedCar.bestLapTime )
							{
								liveDataLeaderboardSlot.telemetryText = GetTimeString( leadCarBestLapTime, true );
							}
							else
							{
								var deltaTime = normalizedCar.bestLapTime - leadCarBestLapTime;

								liveDataLeaderboardSlot.telemetryText = $"{negativeSign}{deltaTime:0.000}";
							}
						}
					}
					else if ( normalizedCar.isOnPitRoad )
					{
						liveDataLeaderboardSlot.telemetryText = Settings.overlay.translationDictionary[ "Pit" ].translation;
						liveDataLeaderboardSlot.telemetryColor = Settings.overlay.telemetryPitColor;
					}
					else if ( normalizedCar.isOutOfCar )
					{
						liveDataLeaderboardSlot.telemetryText = Settings.overlay.translationDictionary[ "Out" ].translation;
						liveDataLeaderboardSlot.telemetryColor = Settings.overlay.telemetryOutColor;
					}
					else if ( IRSDK.normalizedSession.isInRaceSession )
					{
						if ( ( IRSDK.normalizedData.sessionState == SessionState.StateRacing ) && normalizedCar.hasCrossedStartLine )
						{
							if ( normalizedCar.lapPositionRelativeToClassLeader >= 1.0f )
							{
								var wholeLapsDown = Math.Floor( normalizedCar.lapPositionRelativeToClassLeader );

								liveDataLeaderboardSlot.telemetryText = $"-{wholeLapsDown:0} {Settings.overlay.translationDictionary[ "LapsAbbreviation" ].translation}";
							}
							else if ( !IRSDK.normalizedData.isUnderCaution )
							{
								var lapPosition = Settings.overlay.telemetryIsBetweenCars ? ( ( carInFront == null ) ? 0 : ( carInFront.lapPosition - normalizedCar.lapPosition ) ) : normalizedCar.lapPositionRelativeToClassLeader;

								if ( lapPosition > 0 )
								{
									if ( Settings.overlay.telemetryMode == 0 )
									{
										liveDataLeaderboardSlot.telemetryText = $"{negativeSign}{lapPosition:0.000} {Settings.overlay.translationDictionary[ "LapsAbbreviation" ].translation}";
									}
									else if ( Settings.overlay.telemetryMode == 1 )
									{
										var distance = lapPosition * IRSDK.normalizedSession.trackLengthInMeters;

										if ( IRSDK.normalizedData.displayIsMetric )
										{
											var distanceString = $"{distance:0}";

											if ( distanceString != "0" )
											{
												liveDataLeaderboardSlot.telemetryText = $"{negativeSign}{distanceString} {Settings.overlay.translationDictionary[ "MetersAbbreviation" ].translation}";
											}
										}
										else
										{
											distance *= 3.28084f;

											var distanceString = $"{distance:0}";

											if ( distanceString != "0" )
											{
												liveDataLeaderboardSlot.telemetryText = $"{negativeSign}{distanceString} {Settings.overlay.translationDictionary[ "FeetAbbreviation" ].translation}";
											}
										}
									}
									else
									{
										if ( ( carInFront != null ) && ( carInFront.checkpoints[ normalizedCar.checkpointIdx ] > 0 ) )
										{
											var checkpointTime = Math.Abs( (float) ( normalizedCar.checkpoints[ normalizedCar.checkpointIdx ] - carInFront.checkpoints[ normalizedCar.checkpointIdx ] ) );

											var deltaCheckpointTime = checkpointTime - normalizedCar.checkpointTime;

											if ( Math.Abs( deltaCheckpointTime ) < 0.1 )
											{
												normalizedCar.checkpointTime += deltaCheckpointTime * 0.05f;
											}
											else
											{
												normalizedCar.checkpointTime = checkpointTime;
											}

											liveDataLeaderboardSlot.telemetryText = $"{negativeSign}{normalizedCar.checkpointTime:0.00}";
										}
									}
								}
							}
						}
					}

					// current target and speed

					if ( !IRSDK.normalizedSession.isInQualifyingSession && !normalizedCar.isOutOfCar )
					{
						liveDataLeaderboardSlot.showHighlight = ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx );

						liveDataLeaderboardSlot.speedText = $"{Math.Abs( normalizedCar.speedInMetersPerSecond ) * ( IRSDK.normalizedData.displayIsMetric ? 3.6f : 2.23694f ):0} {( IRSDK.normalizedData.displayIsMetric ? Settings.overlay.translationDictionary[ "KPH" ].translation : Settings.overlay.translationDictionary[ "MPH" ].translation )}";
					}
					else
					{
						liveDataLeaderboardSlot.showHighlight = false;
					}

					//

					normalizedCar.wasVisibleOnLeaderboard = true;

					if ( ( carInFront == null ) || Settings.overlay.telemetryIsBetweenCars )
					{
						carInFront = normalizedCar;
					}
				}
			}

			// leaderboard background and splitter

			liveDataLeaderboard.backgroundSize = Settings.overlay.leaderboardSlotSpacing * Math.Min( carsShown, Settings.overlay.leaderboardSlotCount );
			liveDataLeaderboard.showSplitter = ( ( topSplitLastSlotIndex + 1 ) != bottomSplitFirstSlotIndex );
			liveDataLeaderboard.splitterPosition = Settings.overlay.leaderboardSlotSpacing * topSplitLastSlotIndex;

			// remember the bottom split first position

			lastFrameBottomSplitFirstPosition = bottomSplitFirstSlotIndex;
		}

		public void UpdateTrackMap()
		{
			if ( TrackMap.initialized )
			{
				liveDataTrackMap.trackID = TrackMap.trackID;
				liveDataTrackMap.width = TrackMap.width;
				liveDataTrackMap.height = TrackMap.height;
				liveDataTrackMap.drawVectorList = TrackMap.drawVectorList;

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardIndexSortedNormalizedCars )
				{
					// get car

					var liveDataTrackMapCar = liveDataTrackMap.liveDataTrackMapCars[ normalizedCar.carIdx ];

					// skip pace car and spectators

					liveDataTrackMapCar.show = normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar;

					// skip cars not visible on the leaderboard

					if ( liveDataTrackMapCar.show )
					{
						liveDataTrackMapCar.offset = TrackMap.GetPosition( normalizedCar.lapDistPct );
					}
				}
			}
			else
			{
				liveDataTrackMap.trackID = 0;
				liveDataTrackMap.width = 0;
				liveDataTrackMap.height = 0;
				liveDataTrackMap.drawVectorList = null;
			}
		}

		public void UpdateVoiceOf()
		{
			liveDataVoiceOf.show = ( IRSDK.normalizedData.radioTransmitCarIdx != -1 );

			liveDataVoiceOf.voiceOfText = Settings.overlay.translationDictionary[ "VoiceOf" ].translation;

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
			var subtitleData = SubtitlePlayback.GetCurrentSubtitleData();

			liveDataSubtitle.text = ( subtitleData == null ) ? string.Empty : subtitleData.Text;
		}

		public void UpdateIntro()
		{
			if ( IRSDK.normalizedData.sessionTimeDelta < 0 )
			{
				liveDataIntro.show = false;
			}
			else if ( IRSDK.normalizedData.sessionTimeDelta > 0 )
			{
				liveDataIntro.show = false;

				if ( IRSDK.normalizedSession.isInRaceSession )
				{
					var numRows = (int) Math.Ceiling( IRSDK.normalizedData.numLeaderboardCars / 2.0 );

					var animationDuration = Settings.overlay.introInTime + Settings.overlay.introHoldTime + Settings.overlay.introOutTime;

					var introStartTime = Math.Min( Settings.overlay.introLeftStartTime, Settings.overlay.introRightStartTime );
					var introEndTime = Math.Max( Settings.overlay.introLeftStartTime, Settings.overlay.introRightStartTime ) + ( numRows - 1 ) * Settings.overlay.introStartInterval + animationDuration;

					if ( ( IRSDK.normalizedData.sessionTime >= introStartTime ) && ( IRSDK.normalizedData.sessionTime < introEndTime ) )
					{
						liveDataIntro.show = true;

						for ( var driverIndex = 0; driverIndex < liveDataIntro.liveDataIntroDrivers.Length; driverIndex++ )
						{
							var liveDataIntroDriver = liveDataIntro.liveDataIntroDrivers[ driverIndex ];

							var normalizedCar = IRSDK.normalizedData.leaderboardIndexSortedNormalizedCars[ driverIndex ];

							if ( normalizedCar.includeInLeaderboard )
							{
								var rowNumber = Math.Floor( driverIndex / 2.0 );
								var rowStartTime = ( ( ( driverIndex & 1 ) == 0 ) ? Settings.overlay.introLeftStartTime : Settings.overlay.introRightStartTime ) + rowNumber * Settings.overlay.introStartInterval;
								var rowEndTime = rowStartTime + animationDuration;

								liveDataIntroDriver.show = ( IRSDK.normalizedData.sessionTime >= rowStartTime ) && ( IRSDK.normalizedData.sessionTime < rowEndTime );
								liveDataIntroDriver.carIdx = normalizedCar.carIdx;
								liveDataIntroDriver.positionText = $"P{normalizedCar.displayedPosition}";
								liveDataIntroDriver.driverNameText = normalizedCar.userName;

								if ( normalizedCar.qualifyingTime == -1 )
								{
									liveDataIntroDriver.qualifyingTimeText = Settings.overlay.translationDictionary[ "DidNotQualify" ].translation;
								}
								else
								{
									liveDataIntroDriver.qualifyingTimeText = GetTimeString( normalizedCar.qualifyingTime, true );
								}
							}
							else
							{
								liveDataIntroDriver.show = false;
							}
						}
					}
				}
			}
		}

		public void UpdateStartLights()
		{
			liveDataStartLights.showReady = false;
			liveDataStartLights.showSet = false;
			liveDataStartLights.showGo = false;

			if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartGo ) != 0 )
			{
				liveDataStartLights.showGo = true;
			}
			else if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartSet ) != 0 )
			{
				liveDataStartLights.showSet = true;
			}
			else if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartReady ) != 0 )
			{
				if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.OneLapToGreen ) != 0 )
				{
					if ( ( IRSDK.normalizedData.paceCar == null ) || ( IRSDK.normalizedData.paceCar.isOnPitRoad && ( IRSDK.normalizedData.paceCar.lapDistPct > 0.5f ) ) )
					{
						liveDataStartLights.showReady = true;
					}
				}
				else if ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.StartHidden ) == 0 )
				{
					liveDataStartLights.showReady = true;
				}
			}
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

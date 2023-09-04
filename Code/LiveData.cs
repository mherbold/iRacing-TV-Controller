
using System;
using System.Text.Json.Serialization;
using System.Windows.Media.TextFormatting;
using System.Xml.Serialization;
using irsdkSharp.Serialization.Enums.Fastest;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveData
	{
		public const int MaxNumDrivers = 64;
		public const int MaxNumClasses = 8;

		public static LiveData Instance { get; private set; }

		[JsonInclude] public bool isConnected = false;
		public string systemMessage = string.Empty;

		public LiveDataSteamVr liveDataSteamVr = new();
		public LiveDataControlPanel liveDataControlPanel = new();
		public LiveDataDriver[] liveDataDrivers = new LiveDataDriver[ MaxNumDrivers ];
		[JsonInclude] public LiveDataRaceStatus liveDataRaceStatus = new();
		[JsonInclude, XmlIgnore] public LiveDataLeaderboard[]? liveDataLeaderboardsWebPage = null;
		public LiveDataLeaderboard[]? liveDataLeaderboards = null;
		public LiveDataVoiceOf liveDataVoiceOf = new();
		public LiveDataSubtitle liveDataSubtitle = new();
		public LiveDataIntro liveDataIntro = new();
		public LiveDataStartLights liveDataStartLights = new();
		[JsonInclude] public LiveDataTrackMap liveDataTrackMap = new();
		[JsonInclude, XmlIgnore] public LiveDataEventLog liveDataEventLog = new();
		public LiveDataHud liveDataHud = new();

		public string seriesLogoTextureUrl = string.Empty;

		[NonSerialized] public int[] lastFrameBottomSplitFirstPosition = new int[ MaxNumClasses ];

		[NonSerialized] public float hudGapTimeFront = 0;
		[NonSerialized] public float hudGapTimeBack = 0;

		[NonSerialized] public float speechToTextTimer = 0;

		static LiveData()
		{
			Instance = new LiveData();
		}

		private LiveData()
		{
			Instance = this;

			for ( var driverIndex = 0; driverIndex < liveDataDrivers.Length; driverIndex++ )
			{
				liveDataDrivers[ driverIndex ] = new LiveDataDriver();
			}
		}

		public void Update()
		{
			isConnected = IRSDK.isConnected;

			if ( Controller.currentMode == Controller.Mode.None )
			{
				systemMessage = string.Empty;
			}
			else
			{
				switch ( Controller.currentMode )
				{
					case Controller.Mode.Width:
						systemMessage = "Adjusting SteamVR Overlay Width";
						break;

					case Controller.Mode.PositionXY:
						systemMessage = "Adjusting SteamVR Overlay Position (X/Y)";
						break;

					case Controller.Mode.PositionZ:
						systemMessage = "Adjusting SteamVR Overlay Position (Z)";
						break;

					case Controller.Mode.Curvature:
						systemMessage = "Adjusting SteamVR Overlay Curvature";
						break;
				}
			}

			Settings.UpdateCombinedOverlay();

			UpdateSteamVr();
			UpdateControlPanel();
			UpdateDrivers();
			UpdateRaceStatus();
			UpdateLeaderboard( ref liveDataLeaderboardsWebPage, false );
			UpdateLeaderboard( ref liveDataLeaderboards, true );
			UpdateTrackMap();
			UpdateVoiceOf();
			UpdateSubtitle();
			UpdateIntro();
			UpdateStartLights();
			UpdateEventLog();
			UpdateHud();

			seriesLogoTextureUrl = IRSDK.normalizedSession.seriesLogoTextureUrl;

			IPC.readyToSendLiveData = true;
		}

		public void UpdateSteamVr()
		{
			liveDataSteamVr.enabled = Settings.editor.editorSteamVrEnabled;
			liveDataSteamVr.width = Settings.editor.editorSteamVrWidth;
			liveDataSteamVr.position = Settings.editor.editorSteamVrPosition;
			liveDataSteamVr.curvature = Settings.editor.editorSteamVrCurvature;
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
			liveDataControlPanel.hudOn = MainWindow.Instance.hudOn;
			liveDataControlPanel.customLayerOn = MainWindow.Instance.customLayerOn;
		}

		public void UpdateDrivers()
		{
			foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
			{
				liveDataDrivers[ normalizedCar.carIdx ].carNumberTextureUrl = normalizedCar.carNumberTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].carTextureUrl = normalizedCar.carTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].helmetTextureUrl = normalizedCar.helmetTextureUrl;
				liveDataDrivers[ normalizedCar.carIdx ].driverTextureUrl = normalizedCar.driverTextureUrl;
			}
		}

		public void UpdateRaceStatus()
		{
			// session name

			if ( Settings.overlay.translationDictionary.ContainsKey( IRSDK.normalizedSession.sessionName ) )
			{
				liveDataRaceStatus.sessionNameText = Settings.overlay.translationDictionary[ IRSDK.normalizedSession.sessionName ].translation;
			}
			else
			{
				liveDataRaceStatus.sessionNameText = IRSDK.normalizedSession.sessionName;
			}

			// laps remaining

			if ( IRSDK.normalizedData.isInTimedRace || !IRSDK.normalizedSession.isInRaceSession )
			{
				liveDataRaceStatus.lapsRemainingText = Program.GetTimeString( IRSDK.normalizedData.sessionTimeRemaining, false );
			}
			else if ( IRSDK.normalizedData.sessionLapsRemaining == 1 )
			{
				liveDataRaceStatus.lapsRemainingText = Settings.overlay.translationDictionary[ "FinalLap" ].translation;
			}
			else
			{
				var lapsRemaining = Math.Min( IRSDK.normalizedData.sessionLapsTotal, IRSDK.normalizedData.sessionLapsRemaining );

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
			else if ( IRSDK.normalizedSession.isInRaceSession && ( ( IRSDK.normalizedData.sessionLapsRemaining == 1 ) || ( ( IRSDK.normalizedData.sessionFlags & (uint) SessionFlags.White ) != 0 ) ) )
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
				liveDataRaceStatus.currentLapText = Program.GetTimeString( Math.Ceiling( IRSDK.normalizedData.sessionTimeTotal - IRSDK.normalizedData.sessionTimeRemaining ), false ) + " | " + Program.GetTimeString( IRSDK.normalizedData.sessionTimeTotal, false );
			}
			else
			{
				liveDataRaceStatus.currentLapText = IRSDK.normalizedData.lapNumber.ToString() + " | " + IRSDK.normalizedData.sessionLapsTotal.ToString();
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

		public void UpdateLeaderboard( ref LiveDataLeaderboard[]? liveDataLeaderboards, bool splitLeaderboard )
		{
			// allocate leaderboards

			if ( ( liveDataLeaderboards == null ) || ( liveDataLeaderboards.Length != IRSDK.normalizedData.numLeaderboardClasses ) )
			{
				liveDataLeaderboards = new LiveDataLeaderboard[ IRSDK.normalizedData.numLeaderboardClasses ];

				for ( var leaderboardIndex = 0; leaderboardIndex < liveDataLeaderboards.Length; leaderboardIndex++ )
				{
					liveDataLeaderboards[ leaderboardIndex ] = new LiveDataLeaderboard();
				}
			}

			var leaderboardOffset = Vector2.zero;

			// go through each car class

			for ( var classIndex = 0; classIndex < IRSDK.normalizedData.numLeaderboardClasses; classIndex++ )
			{
				var currentLiveDataLeaderboard = liveDataLeaderboards[ classIndex ];

				var currentLeaderboardClass = IRSDK.normalizedData.leaderboardClass[ classIndex ];

				var currentClassID = currentLeaderboardClass.classID;

				// leaderboard splits

				var bottomSplitSlotCount = Settings.overlay.leaderboardSlotCount / 2;
				var bottomSplitLastSlotIndex = Settings.overlay.leaderboardSlotCount;

				if ( !IRSDK.normalizedSession.isInQualifyingSession )
				{
					if ( bottomSplitSlotCount > 0 )
					{
						foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
						{
							if ( !normalizedCar.includeInLeaderboard )
							{
								break;
							}

							if ( normalizedCar.classID == currentClassID )
							{
								if ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx )
								{
									if ( normalizedCar.displayedPosition > bottomSplitLastSlotIndex )
									{
										while ( bottomSplitLastSlotIndex < normalizedCar.displayedPosition )
										{
											bottomSplitLastSlotIndex += bottomSplitSlotCount;
										}

										if ( bottomSplitLastSlotIndex > currentLeaderboardClass.numDrivers )
										{
											bottomSplitLastSlotIndex = currentLeaderboardClass.numDrivers;
										}

										break;
									}
								}
							}
						}
					}
				}

				var topSplitFirstSlotIndex = 1;
				var topSplitLastSlotIndex = Settings.overlay.leaderboardSlotCount - bottomSplitSlotCount;
				var bottomSplitFirstSlotIndex = bottomSplitLastSlotIndex - bottomSplitSlotCount + 1;

				if ( !splitLeaderboard )
				{
					topSplitLastSlotIndex = MaxNumDrivers;
					bottomSplitFirstSlotIndex = MaxNumDrivers + 1;
					bottomSplitLastSlotIndex = MaxNumDrivers + 1;
				}

				// leaderboard

				currentLiveDataLeaderboard.show = false;
				currentLiveDataLeaderboard.classColor = currentLeaderboardClass.color;
				currentLiveDataLeaderboard.className = currentLeaderboardClass.name;
				currentLiveDataLeaderboard.classNameShort = currentLeaderboardClass.shortName;

				NormalizedCar? carInFront = null;

				var leadCarBestLapTime = 0.0f;
				var carsShown = 0;

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
				{
					// skip cars with wrong car class

					if ( normalizedCar.classID != currentClassID )
					{
						continue;
					}

					// get slot

					var liveDataLeaderboardSlot = currentLiveDataLeaderboard.liveDataLeaderboardSlots[ normalizedCar.carIdx ];

					// skip pace car and spectators

					if ( !normalizedCar.includeInLeaderboard )
					{
						liveDataLeaderboardSlot.show = false;
					}
					else
					{
						// lead car best lap time

						if ( carInFront == null )
						{
							leadCarBestLapTime = normalizedCar.bestLapTime;
						}

						// check if the car is visible on the leaderboard

						liveDataLeaderboardSlot.show = ( ( ( normalizedCar.displayedPosition >= topSplitFirstSlotIndex ) && ( normalizedCar.displayedPosition <= topSplitLastSlotIndex ) ) || ( ( normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex ) && ( normalizedCar.displayedPosition <= bottomSplitLastSlotIndex ) ) );

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
						if ( splitLeaderboard )
						{
							normalizedCar.wasVisibleOnLeaderboard = false;
						}
					}
					else
					{
						// at least one car is visible so we want to show the leaderboard

						currentLiveDataLeaderboard.show = true;

						carsShown++;

						// slot index

						var slotIndex = normalizedCar.displayedPosition - ( ( normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex ) ? bottomSplitFirstSlotIndex - topSplitLastSlotIndex : topSplitFirstSlotIndex );

						// compute slot offset

						if ( splitLeaderboard )
						{
							var resetSlotOffset = ( ( lastFrameBottomSplitFirstPosition[ classIndex ] != bottomSplitFirstSlotIndex ) && ( normalizedCar.displayedPosition >= bottomSplitFirstSlotIndex ) );

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
						}

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

						// car number text

						liveDataLeaderboardSlot.carNumberText = normalizedCar.carNumber;

						// car number text color

						tintColor = Settings.overlay.textSettingsDataDictionary[ "LeaderboardPositionCarNumber" ].tintColor;

						if ( Settings.overlay.leaderboardUseClassColors )
						{
							liveDataLeaderboardSlot.carNumberColor = Color.Lerp( tintColor, normalizedCar.classColor, Settings.overlay.leaderboardClassColorStrength );
						}
						else
						{
							liveDataLeaderboardSlot.carNumberColor = tintColor;
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
									liveDataLeaderboardSlot.telemetryText = Program.GetTimeString( leadCarBestLapTime, true );
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
												if ( !splitLeaderboard )
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

						// preferred driver

						liveDataLeaderboardSlot.showPreferredCar = normalizedCar.isPreferredCar;

						//

						if ( splitLeaderboard )
						{
							normalizedCar.wasVisibleOnLeaderboard = true;
						}

						if ( ( carInFront == null ) || Settings.overlay.telemetryIsBetweenCars )
						{
							carInFront = normalizedCar;
						}
					}
				}

				if ( splitLeaderboard )
				{
					// leaderboard offset and background and splitter

					currentLiveDataLeaderboard.offset = new Vector2( leaderboardOffset.x, leaderboardOffset.y );
					currentLiveDataLeaderboard.backgroundSize = Settings.overlay.leaderboardSlotSpacing * Math.Min( carsShown, Settings.overlay.leaderboardSlotCount );
					currentLiveDataLeaderboard.showSplitter = ( ( topSplitLastSlotIndex + 1 ) != bottomSplitFirstSlotIndex );
					currentLiveDataLeaderboard.splitterPosition = Settings.overlay.leaderboardFirstSlotPosition + Settings.overlay.leaderboardSlotSpacing * topSplitLastSlotIndex;

					if ( currentLiveDataLeaderboard.show )
					{
						if ( Settings.overlay.leaderboardMultiClassOffsetType == 0 )
						{
							leaderboardOffset.y += currentLiveDataLeaderboard.backgroundSize.y;
						}

						leaderboardOffset += Settings.overlay.leaderboardMultiClassOffset;
					}

					// remember the bottom split first position

					lastFrameBottomSplitFirstPosition[ classIndex ] = bottomSplitFirstSlotIndex;
				}
			}
		}

		public void UpdateTrackMap()
		{
			if ( TrackMap.initialized )
			{
				liveDataTrackMap.show = true;
				liveDataTrackMap.trackID = TrackMap.trackID;
				liveDataTrackMap.width = TrackMap.width;
				liveDataTrackMap.height = TrackMap.height;
				liveDataTrackMap.startFinishLine = TrackMap.fullVectorList[ ( TrackMap.startFinishOffset + Settings.overlay.trackMapStartFinishOffset ) % TrackMap.numVectors ];
				liveDataTrackMap.drawVectorList = TrackMap.drawVectorList;

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
				{
					// get car

					var liveDataTrackMapCar = liveDataTrackMap.liveDataTrackMapCars[ normalizedCar.carIdx ];

					// skip pace car and spectators

					liveDataTrackMapCar.show = normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar;

					// skip cars not visible on the leaderboard

					if ( liveDataTrackMapCar.show )
					{
						liveDataTrackMapCar.offset = TrackMap.GetPosition( normalizedCar.lapDistPct );
						liveDataTrackMapCar.carNumber = normalizedCar.carNumber;
					}

					liveDataTrackMapCar.showHighlight = ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx );
				}
			}
			else
			{
				liveDataTrackMap.show = false;
				liveDataTrackMap.trackID = 0;
				liveDataTrackMap.width = 0;
				liveDataTrackMap.height = 0;
				liveDataTrackMap.startFinishLine = Vector3.zero;
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

							var normalizedCar = IRSDK.normalizedData.classLeaderboardSortedNormalizedCars[ driverIndex ];

							if ( normalizedCar.includeInLeaderboard )
							{
								var rowNumber = Math.Floor( driverIndex / 2.0 );
								var rowStartTime = ( ( ( driverIndex & 1 ) == 0 ) ? Settings.overlay.introLeftStartTime : Settings.overlay.introRightStartTime ) + rowNumber * Settings.overlay.introStartInterval;
								var rowEndTime = rowStartTime + animationDuration;

								liveDataIntroDriver.show = ( IRSDK.normalizedData.sessionTime >= rowStartTime ) && ( IRSDK.normalizedData.sessionTime < rowEndTime );
								liveDataIntroDriver.carIdx = normalizedCar.carIdx;
								liveDataIntroDriver.positionText = $"P{normalizedCar.displayedPosition}";
								liveDataIntroDriver.driverNameText = normalizedCar.userName;
								liveDataIntroDriver.carNumberText = normalizedCar.carNumber;

								if ( normalizedCar.qualifyingTime == -1 )
								{
									liveDataIntroDriver.qualifyingTimeText = Settings.overlay.translationDictionary[ "DidNotQualify" ].translation;
								}
								else if ( normalizedCar.qualifyingTime == 0 )
								{
									liveDataIntroDriver.qualifyingTimeText = "";
								}
								else
								{
									liveDataIntroDriver.qualifyingTimeText = Program.GetTimeString( normalizedCar.qualifyingTime, true );
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

		public void UpdateEventLog()
		{
			liveDataEventLog.messages = EventLog.messages;
		}

		public void UpdateHud()
		{
			if ( IRSDK.data == null )
			{
				return;
			}

			// player car

			var normalizedCar = IRSDK.normalizedData.normalizedCars[ IRSDK.data.PlayerCarIdx ];

			// fuel

			liveDataHud.fuel = "?.??";
			liveDataHud.fuelColor = Settings.overlay.textSettingsDataDictionary[ "HudFuel" ].tintColor;

			if ( IRSDK.normalizedData.highestLapFuelLevelDelta > 0 )
			{
				var fuelLapsRemaining = IRSDK.normalizedData.fuelLevel / IRSDK.normalizedData.highestLapFuelLevelDelta;

				liveDataHud.fuel = $"{fuelLapsRemaining:0.00}";

				if ( ( IRSDK.normalizedData.isInTimedRace && ( fuelLapsRemaining <= 2.0f ) ) || ( !IRSDK.normalizedData.isInTimedRace && ( fuelLapsRemaining < IRSDK.normalizedData.sessionLapsRemaining ) ) )
				{
					liveDataHud.fuelColor = new Color( 1, 0.25f, 0.25f, 1 );
				}
			}

			// laps to leader

			if ( normalizedCar.lapPositionRelativeToClassLeader == 0 )
			{
				liveDataHud.lapsToLeader = "-.---";
			}
			else
			{
				liveDataHud.lapsToLeader = $"{normalizedCar.lapPositionRelativeToClassLeader:0.000}";
			}

			// rpm

			var steppedRPM = (int) Math.Floor( normalizedCar.rpm / 50 ) * 50;

			liveDataHud.rpm = steppedRPM.ToString();
			liveDataHud.rpmColor = Settings.overlay.textSettingsDataDictionary[ "HudRPM" ].tintColor;

			if ( normalizedCar.gear < IRSDK.normalizedSession.numForwardGears )
			{
				if ( normalizedCar.rpm >= IRSDK.normalizedSession.blinkRpm )
				{
					liveDataHud.rpmColor = new Color( 1, 0.2f, 0.2f, 1 );
				}
				else if ( normalizedCar.rpm >= IRSDK.normalizedSession.redlineRpm )
				{
					liveDataHud.rpmColor = new Color( 1, 1, 0.2f, 1 );
				}
				else if ( normalizedCar.rpm >= IRSDK.normalizedSession.shiftRpm )
				{
					liveDataHud.rpmColor = new Color( 0.2f, 1, 0.2f, 1 );
				}
			}

			// speed

			liveDataHud.speed = $"{Math.Abs( normalizedCar.speedInMetersPerSecond ) * ( IRSDK.normalizedData.displayIsMetric ? 3.6f : 2.23694f ):0}";

			// gear

			if ( normalizedCar.gear == -1 )
			{
				liveDataHud.gear = "R";
			}
			else if ( normalizedCar.gear == 0 )
			{
				liveDataHud.gear = "N";
			}
			else
			{
				liveDataHud.gear = normalizedCar.gear.ToString();
			}

			// gap time

			liveDataHud.gapTimeFront = "-.--";
			liveDataHud.gapTimeFrontColor = Settings.overlay.textSettingsDataDictionary[ "HudGapTimeFront" ].tintColor;

			if ( normalizedCar.normalizedCarInFront != null )
			{
				var checkpointTimeHis = normalizedCar.normalizedCarInFront.checkpoints[ normalizedCar.checkpointIdx ];
				var checkpointTimeMine = normalizedCar.checkpoints[ normalizedCar.checkpointIdx ];

				if ( ( checkpointTimeHis > 0 ) && ( checkpointTimeMine > 0 ) && ( checkpointTimeMine >= checkpointTimeHis ) )
				{
					var gapTime = (float) ( checkpointTimeMine - checkpointTimeHis );

					var gapTimeDelta = Math.Abs( gapTime - hudGapTimeFront );

					if ( gapTimeDelta >= 0.5f )
					{
						hudGapTimeFront = gapTime;
					}
					else
					{
						hudGapTimeFront = hudGapTimeFront * 0.95f + gapTime * 0.05f;
					}

					liveDataHud.gapTimeFront = $"{hudGapTimeFront:0.00}";
				}

				var deltaLapPositionRelativeToClassLeader = normalizedCar.normalizedCarInFront.lapPositionRelativeToClassLeader - normalizedCar.lapPositionRelativeToClassLeader;

				if ( deltaLapPositionRelativeToClassLeader < 0.5f )
				{
					liveDataHud.gapTimeFrontColor = new Color( 1.0f, 0.3f, 0.3f, liveDataHud.gapTimeFrontColor.a );
				}
				else if ( deltaLapPositionRelativeToClassLeader > 0.5f )
				{
					liveDataHud.gapTimeFrontColor = new Color( 0.4f, 0.4f, 1.0f, liveDataHud.gapTimeFrontColor.a );
				}
			}

			liveDataHud.gapTimeBack = "-.--";
			liveDataHud.gapTimeBackColor = Settings.overlay.textSettingsDataDictionary[ "HudGapTimeBack" ].tintColor;

			if ( normalizedCar.normalizedCarBehind != null )
			{
				var checkpointTimeMine = normalizedCar.checkpoints[ normalizedCar.normalizedCarBehind.checkpointIdx ];
				var checkpointTimeHis = normalizedCar.normalizedCarBehind.checkpoints[ normalizedCar.normalizedCarBehind.checkpointIdx ];

				if ( ( checkpointTimeHis > 0 ) && ( checkpointTimeMine > 0 ) && ( checkpointTimeHis >= checkpointTimeMine ) )
				{
					var gapTime = (float) ( checkpointTimeHis - checkpointTimeMine );

					var gapTimeDelta = Math.Abs( gapTime - hudGapTimeBack );

					if ( gapTimeDelta >= 0.5f )
					{
						hudGapTimeBack = gapTime;
					}
					else
					{
						hudGapTimeBack = hudGapTimeBack * 0.95f + gapTime * 0.05f;
					}

					liveDataHud.gapTimeBack = $"{hudGapTimeBack:0.00}";
				}

				var deltaLapPositionRelativeToClassLeader = normalizedCar.normalizedCarBehind.lapPositionRelativeToClassLeader - normalizedCar.lapPositionRelativeToClassLeader;

				if ( deltaLapPositionRelativeToClassLeader < 0.5f )
				{
					liveDataHud.gapTimeBackColor = new Color( 1.0f, 0.3f, 0.3f, liveDataHud.gapTimeFrontColor.a );
				}
				else if ( deltaLapPositionRelativeToClassLeader > 0.5f )
				{
					liveDataHud.gapTimeBackColor = new Color( 0.4f, 0.4f, 1.0f, liveDataHud.gapTimeFrontColor.a );
				}
			}

			// speech to text

			var recognizedString = SpeechToText.GetRecognizingString();

			if ( recognizedString == string.Empty )
			{
				if ( speechToTextTimer > 0 )
				{
					speechToTextTimer -= Program.deltaTime;

					if ( speechToTextTimer <= 0.0f )
					{
						speechToTextTimer = 0;

						liveDataHud.speechToText = string.Empty;
					}
				}
			}
			else
			{
				liveDataHud.speechToText = recognizedString;

				speechToTextTimer = 15.0f;
			}

			// spotter indicators

			var carsLeft = 0;
			var carsRight = 0;

			switch ( (CarLeftRight) IRSDK.data.CarLeftRight )
			{
				case CarLeftRight.LRCarLeft:
					carsLeft = 1;
					break;
				case CarLeftRight.LR2CarsLeft:
					carsLeft = 2;
					break;
				case CarLeftRight.LRCarRight:
					carsRight = 1;
					break;
				case CarLeftRight.LR2CarsRight:
					carsRight = 2;
					break;
				case CarLeftRight.LRCarLeftRight:
					carsLeft = 2;
					carsRight = 2;
					break;
			}

			if ( carsLeft == 1 )
			{
				liveDataHud.showLeftSpotterIndicator = true;
			}
			else if ( carsLeft == 2 )
			{
				liveDataHud.showLeftSpotterIndicator = ( Program.elapsedMilliseconds % 250 ) >= 100;
			}
			else
			{
				liveDataHud.showLeftSpotterIndicator = false;
			}

			if ( carsRight == 1 )
			{
				liveDataHud.showRightSpotterIndicator = true;
			}
			else if ( carsRight == 2 )
			{
				liveDataHud.showRightSpotterIndicator = ( Program.elapsedMilliseconds % 250 ) >= 100;
			}
			else
			{
				liveDataHud.showRightSpotterIndicator = false;
			}
		}
	}
}

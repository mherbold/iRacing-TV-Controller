
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
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
		public LiveDataChyron liveDataChyron = new();
		public LiveDataSubtitle liveDataSubtitle = new();
		public LiveDataIntro liveDataIntro = new();
		public LiveDataStartLights liveDataStartLights = new();
		[JsonInclude] public LiveDataTrackMap liveDataTrackMap = new();
		public LiveDataPitLane liveDataPitLane = new();
		[JsonInclude, XmlIgnore] public LiveDataEventLog liveDataEventLog = new();
		public LiveDataHud liveDataHud = new();
		public LiveDataTrainer liveDataTrainer = new();
		public LiveDataWebcamStreaming liveDataWebcamStreaming = new();

		public string seriesLogoTextureUrl = string.Empty;

		[NonSerialized, XmlIgnore] public int[] lastFrameBottomSplitFirstPosition = new int[ MaxNumClasses ];

		[NonSerialized, XmlIgnore] public float hudGapTimeFront = 0;
		[NonSerialized, XmlIgnore] public float hudGapTimeBack = 0;

		[NonSerialized, XmlIgnore] public float speechToTextTimer = 0;

		[NonSerialized, XmlIgnore] public int chyronRandomOffset = 0;
		[NonSerialized, XmlIgnore] public Dictionary<string, string> chyronRandomItemNames;
		[NonSerialized, XmlIgnore] public string[] chyronAvailableItemLabels;
		[NonSerialized, XmlIgnore] public string[] chyronAvailableItemValues;

		[NonSerialized, XmlIgnore] public int trackIdLastFrame = 0;
		[NonSerialized, XmlIgnore] public bool pitLaneTouched = false;
		[NonSerialized, XmlIgnore] public float pitLaneMinLapDistPct = 0;
		[NonSerialized, XmlIgnore] public float pitLaneMaxLapDistPct = 0;

		[NonSerialized, XmlIgnore] public float paceCarDistPct = 0;

		[NonSerialized, XmlIgnore] public double interpolatedDeltaTime = 0;
		[NonSerialized, XmlIgnore] public double interpolatedDeltaInterpolatedDeltaTime = 0;
		[NonSerialized, XmlIgnore] public double lastInterpolatedDeltaTime = 0;
		[NonSerialized, XmlIgnore] public Color red = new Color( 1, 0.35f, 0.35f, 1 );
		[NonSerialized, XmlIgnore] public Color green = new Color( 0.2f, 1, 0.2f, 1 );

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

			chyronRandomItemNames = new Dictionary<string, string>
			{
				{ "FAV_REAL_TRACKS", "Favorite tracks" },
				{ "FAV_REAL_CARS", "Favorite cars" },
				{ "FAV_MOVIES", "Favorite movies" },
				{ "FAV_HOBBIES", "Hobbies" },
				{ "FAV_GAMES", "Favorite games" },
				{ "FAV_MUSIC", "Favorite music" },
				{ "FAV_TV_SHOWS", "Favorite TV shows" },
				{ "FAV_BOOKS", "Favorite books" },
				{ "FAV_QUOTATION", "Favorite saying" },
				{ "FAV_SPORTS", "Favorite sports" }
			};

			chyronAvailableItemLabels = new string[ chyronRandomItemNames.Count ];
			chyronAvailableItemValues = new string[ chyronRandomItemNames.Count ];
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
			UpdatePitLane();
			UpdateVoiceOf();
			UpdateChyron();
			UpdateSubtitle();
			UpdateIntro();
			UpdateStartLights();
			UpdateEventLog();
			UpdateHud();
			UpdateTrainer();
			UpdateWebcamStreaming();

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
			liveDataControlPanel.pitLaneOn = MainWindow.Instance.pitLaneOn;
			liveDataControlPanel.startLightsOn = MainWindow.Instance.startLightsOn;
			liveDataControlPanel.voiceOfOn = MainWindow.Instance.voiceOfOn;
			liveDataControlPanel.chyronOn = MainWindow.Instance.chyronOn;
			liveDataControlPanel.subtitlesOn = MainWindow.Instance.subtitlesOn;
			liveDataControlPanel.introOn = MainWindow.Instance.introOn;
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

				if ( normalizedCar.memberProfile != null )
				{
					liveDataDrivers[ normalizedCar.carIdx ].memberImageUrl = normalizedCar.memberProfile.ImageUrl;
					liveDataDrivers[ normalizedCar.carIdx ].memberClubRegionUrl = $"https://ir-core-sites.iracing.com/members/member_images/world_cup/club_logos/club_{normalizedCar.memberProfile.Info.ClubId:000}_long_0128_web.png";
				}
				else
				{
					liveDataDrivers[ normalizedCar.carIdx ].memberImageUrl = string.Empty;
				}
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
				else if ( ( IRSDK.normalizedData.sessionFlags & ( (uint) SessionFlags.CautionWaving ) ) != 0 )
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

							if ( !Settings.overlay.leaderboardSeparateBoards || ( normalizedCar.classID == currentClassID ) )
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

				NormalizedCar? normalizedCarClassLeader = null;
				NormalizedCar? normalizedCarInFront = null;

				var classLeaderBestLapTime = 0.0f;
				var carsShown = 0;

				// reset leaderboard slots to be hidden

				foreach ( var liveDataLeaderboardSlot in currentLiveDataLeaderboard.liveDataLeaderboardSlots )
				{
					liveDataLeaderboardSlot.show = false;
				}

				// go through cars for this class

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
				{
					// skip cars with wrong car class

					if ( Settings.overlay.leaderboardSeparateBoards && ( normalizedCar.classID != currentClassID ) )
					{
						continue;
					}

					// get slot

					var liveDataLeaderboardSlot = currentLiveDataLeaderboard.liveDataLeaderboardSlots[ normalizedCar.carIdx ];

					// skip pace car and spectators

					if ( normalizedCar.includeInLeaderboard )
					{
						// class leader best lap time

						if ( normalizedCarClassLeader == null )
						{
							normalizedCarClassLeader = normalizedCar;

							classLeaderBestLapTime = normalizedCar.bestLapTime;
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

						var textSettings = Settings.overlay.textSettingsDataDictionary[ "LeaderboardPosition" ];

						var tintColor = textSettings.tintColor;

						if ( textSettings.useClassColors )
						{
							liveDataLeaderboardSlot.positionColor = Color.Lerp( tintColor, normalizedCar.classColor, textSettings.classColorStrength );
						}
						else
						{
							liveDataLeaderboardSlot.positionColor = tintColor;
						}

						// car number text

						liveDataLeaderboardSlot.carNumberText = normalizedCar.carNumber;

						// car number text color

						textSettings = Settings.overlay.textSettingsDataDictionary[ "LeaderboardPositionCarNumber" ];

						tintColor = textSettings.tintColor;

						if ( textSettings.useClassColors )
						{
							liveDataLeaderboardSlot.carNumberColor = Color.Lerp( tintColor, normalizedCar.classColor, textSettings.classColorStrength );
						}
						else
						{
							liveDataLeaderboardSlot.carNumberColor = tintColor;
						}

						// driver name

						liveDataLeaderboardSlot.driverNameText = normalizedCar.displayedName;

						// driver name color

						textSettings = Settings.overlay.textSettingsDataDictionary[ "LeaderboardPositionDriverName" ];

						tintColor = textSettings.tintColor;

						if ( textSettings.useClassColors )
						{
							liveDataLeaderboardSlot.driverNameColor = Color.Lerp( tintColor, normalizedCar.classColor, textSettings.classColorStrength );
						}
						else
						{
							liveDataLeaderboardSlot.driverNameColor = tintColor;
						}

						// telemetry

						var sign = Settings.overlay.telemetryShowAsNegativeNumbers ? "-" : "+";

						liveDataLeaderboardSlot.telemetryText = string.Empty;
						liveDataLeaderboardSlot.telemetryColor = Settings.overlay.textSettingsDataDictionary[ "LeaderboardPositionTelemetry" ].tintColor;

						if ( IRSDK.normalizedSession.isInPracticeSession || IRSDK.normalizedSession.isInQualifyingSession )
						{
							if ( normalizedCar.bestLapTime > 0 )
							{
								if ( classLeaderBestLapTime == normalizedCar.bestLapTime )
								{
									liveDataLeaderboardSlot.telemetryText = Program.GetTimeString( classLeaderBestLapTime, true );
								}
								else
								{
									var deltaTime = normalizedCar.bestLapTime - classLeaderBestLapTime;

									liveDataLeaderboardSlot.telemetryText = $"{sign}{deltaTime:0.000}";
								}
							}

							normalizedCar.checkpointTime = 0;
						}
						else if ( normalizedCar.isOnPitRoad )
						{
							liveDataLeaderboardSlot.telemetryText = Settings.overlay.translationDictionary[ "Pit" ].translation;
							liveDataLeaderboardSlot.telemetryColor = Settings.overlay.telemetryPitColor;

							normalizedCar.checkpointTime = 0;
						}
						else if ( normalizedCar.isOutOfCar )
						{
							liveDataLeaderboardSlot.telemetryText = Settings.overlay.translationDictionary[ "Out" ].translation;
							liveDataLeaderboardSlot.telemetryColor = Settings.overlay.telemetryOutColor;

							normalizedCar.checkpointTime = 0;
						}
						else if ( IRSDK.normalizedSession.isInRaceSession )
						{
							if ( normalizedCar.hasCrossedStartLine )
							{
								if ( normalizedCar.lapPositionRelativeToClassLeader >= 1.0f )
								{
									var wholeLapsDown = Math.Floor( normalizedCar.lapPositionRelativeToClassLeader );

									liveDataLeaderboardSlot.telemetryText = $"-{wholeLapsDown:0} {Settings.overlay.translationDictionary[ "LapsAbbreviation" ].translation}";

									normalizedCar.checkpointTime = 0;
								}
								else if ( !IRSDK.normalizedData.isUnderCaution && ( normalizedCarInFront != null ) )
								{
									if ( !normalizedCar.hasCrossedFinishLine && !normalizedCarInFront.hasCrossedFinishLine )
									{
										var lapPosition = Settings.overlay.telemetryIsBetweenCars ? ( normalizedCarInFront.lapPosition - normalizedCar.lapPosition ) : normalizedCar.lapPositionRelativeToClassLeader;

										if ( Settings.overlay.telemetryMode == 0 )
										{
											liveDataLeaderboardSlot.telemetryText = $"{sign}{lapPosition:0.000} {Settings.overlay.translationDictionary[ "LapsAbbreviation" ].translation}";
										}
										else if ( Settings.overlay.telemetryMode == 1 )
										{
											var distance = lapPosition * IRSDK.normalizedSession.trackLengthInMeters;

											if ( IRSDK.normalizedData.displayIsMetric )
											{
												var distanceString = $"{distance:0}";

												if ( distanceString != "0" )
												{
													liveDataLeaderboardSlot.telemetryText = $"{sign}{distanceString} {Settings.overlay.translationDictionary[ "MetersAbbreviation" ].translation}";
												}
											}
											else
											{
												distance *= 3.28084f;

												var distanceString = $"{distance:0}";

												if ( distanceString != "0" )
												{
													liveDataLeaderboardSlot.telemetryText = $"{sign}{distanceString} {Settings.overlay.translationDictionary[ "FeetAbbreviation" ].translation}";
												}
											}
										}
										else
										{
											if ( Settings.overlay.telemetryIsBetweenCars )
											{
												if ( normalizedCarInFront.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] > 0 )
												{
													if ( !splitLeaderboard )
													{
														var checkpointTime = Math.Abs( (float) ( normalizedCar.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] - normalizedCarInFront.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] ) );

														if ( ( normalizedCar.checkpointTime != 0 ) && ( normalizedCar.normalizedCarForTelemetry != null ) && ( normalizedCarInFront.carIdx == normalizedCar.normalizedCarForTelemetry.carIdx ) )
														{
															normalizedCar.checkpointTime = normalizedCar.checkpointTime * 0.95f + checkpointTime * 0.05f;
														}
														else
														{
															normalizedCar.normalizedCarForTelemetry = normalizedCarInFront;

															normalizedCar.checkpointTime = checkpointTime;
														}
													}

													liveDataLeaderboardSlot.telemetryText = $"{sign}{normalizedCar.checkpointTime:0.00}";
												}
											}
											else if ( normalizedCarClassLeader != null )
											{
												if ( normalizedCarClassLeader.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] > 0 )
												{
													if ( !splitLeaderboard )
													{
														var checkpointTime = Math.Abs( (float) ( normalizedCar.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] - normalizedCarClassLeader.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ] ) );

														if ( ( normalizedCar.checkpointTime != 0 ) && ( normalizedCar.normalizedCarForTelemetry != null ) && ( normalizedCarClassLeader.carIdx == normalizedCar.normalizedCarForTelemetry.carIdx ) )
														{
															normalizedCar.checkpointTime = normalizedCar.checkpointTime * 0.95f + checkpointTime * 0.05f;
														}
														else
														{
															normalizedCar.normalizedCarForTelemetry = normalizedCarClassLeader;

															normalizedCar.checkpointTime = checkpointTime;
														}
													}

													liveDataLeaderboardSlot.telemetryText = $"{sign}{normalizedCar.checkpointTime:0.00}";
												}
											}
										}

										if ( liveDataLeaderboardSlot.telemetryText == string.Empty )
										{
											normalizedCar.checkpointTime = 0;
										}
									}
								}
							}
							else
							{
								normalizedCar.checkpointTime = 0;
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

						normalizedCarInFront = normalizedCar;
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
				liveDataTrackMap.showPaceCar = false;
				liveDataTrackMap.trackID = TrackMap.trackID;
				liveDataTrackMap.width = TrackMap.width;
				liveDataTrackMap.height = TrackMap.height;
				liveDataTrackMap.startFinishLine = TrackMap.fullVectorList[ ( TrackMap.startFinishOffset + Settings.overlay.trackMapStartFinishOffset ) % TrackMap.numVectors ];
				liveDataTrackMap.drawVectorList = TrackMap.drawVectorList;

				foreach ( var normalizedCar in IRSDK.normalizedData.leaderboardSortedNormalizedCars )
				{
					var liveDataTrackMapCar = liveDataTrackMap.liveDataTrackMapCars[ normalizedCar.carIdx ];

					liveDataTrackMapCar.show = normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar;
					liveDataTrackMapCar.offset = TrackMap.GetPosition( normalizedCar.lapDistPct );
					liveDataTrackMapCar.carNumber = normalizedCar.carNumber;
					liveDataTrackMapCar.showHighlight = ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx );

					if ( normalizedCar.isPaceCar )
					{
						if ( !normalizedCar.isOnPitRoad )
						{
							var lapDistPctDelta = normalizedCar.lapDistPct - paceCarDistPct;

							var distanceMovedInMeters = lapDistPctDelta * IRSDK.normalizedSession.trackLengthInMeters;
							var speedInMetersPerSecond = distanceMovedInMeters / (float) IRSDK.normalizedData.sessionTimeDelta;

							if ( speedInMetersPerSecond >= 5 )
							{
								liveDataTrackMap.showPaceCar = true;
								liveDataTrackMap.paceCarOffset = TrackMap.GetPosition( normalizedCar.lapDistPct );
							}
						}

						paceCarDistPct = normalizedCar.lapDistPct;
					}
				}
			}
			else
			{
				liveDataTrackMap.show = false;
				liveDataTrackMap.showPaceCar = false;
				liveDataTrackMap.trackID = 0;
				liveDataTrackMap.width = 0;
				liveDataTrackMap.height = 0;
				liveDataTrackMap.startFinishLine = Vector3.zero;
				liveDataTrackMap.drawVectorList = null;
			}
		}

		public void UpdatePitLane()
		{
			if ( trackIdLastFrame != IRSDK.normalizedSession.trackID )
			{
				trackIdLastFrame = IRSDK.normalizedSession.trackID;

				pitLaneTouched = false;

				pitLaneMinLapDistPct = 0;
				pitLaneMaxLapDistPct = 0;
			}

			if ( !pitLaneTouched )
			{
				foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad )
					{
						pitLaneTouched = true;

						pitLaneMinLapDistPct = normalizedCar.lapDistPct;
						pitLaneMaxLapDistPct = normalizedCar.lapDistPct;

						break;
					}
				}
			}

			liveDataPitLane.show = false;

			if ( pitLaneTouched )
			{
				foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad )
					{
						liveDataPitLane.show = true;

						var lapDistPct = normalizedCar.lapDistPct;

						var deltaLapDistPct = lapDistPct - pitLaneMinLapDistPct;

						if ( deltaLapDistPct <= -0.5 )
						{
							lapDistPct += 1;
						}
						else if ( deltaLapDistPct >= 0.5 )
						{
							lapDistPct -= 1;
						}

						if ( lapDistPct < pitLaneMinLapDistPct )
						{
							pitLaneMinLapDistPct = lapDistPct;
						}

						lapDistPct = normalizedCar.lapDistPct;

						deltaLapDistPct = lapDistPct - pitLaneMaxLapDistPct;

						if ( deltaLapDistPct <= -0.5 )
						{
							lapDistPct += 1;
						}
						else if ( deltaLapDistPct >= 0.5 )
						{
							lapDistPct -= 1;
						}

						if ( lapDistPct > pitLaneMaxLapDistPct )
						{
							pitLaneMaxLapDistPct = lapDistPct;
						}
					}
				}

				var adjustedMaxLapDistPct = pitLaneMaxLapDistPct;

				if ( adjustedMaxLapDistPct < pitLaneMinLapDistPct )
				{
					adjustedMaxLapDistPct += 1;
				}

				var length = adjustedMaxLapDistPct - pitLaneMinLapDistPct;

				foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
				{
					var liveDataPitLaneCar = liveDataPitLane.liveDataPitLaneCars[ normalizedCar.carIdx ];

					if ( normalizedCar.includeInLeaderboard && normalizedCar.isOnPitRoad && ( length > 0 ) )
					{
						liveDataPitLaneCar.show = true;
						liveDataPitLaneCar.showHighlight = ( normalizedCar.carIdx == IRSDK.normalizedData.camCarIdx );

						var lapDistPct = normalizedCar.lapDistPct;

						var deltaLapDistPct = lapDistPct - pitLaneMinLapDistPct;

						if ( deltaLapDistPct <= -0.5 )
						{
							lapDistPct += 1;
						}
						else if ( deltaLapDistPct >= 0.5 )
						{
							lapDistPct -= 1;
						}

						var offset = Settings.overlay.pitLaneLength * ( ( lapDistPct - pitLaneMinLapDistPct ) / length );

						liveDataPitLaneCar.offset = new Vector3( offset, 0, 0 );
					}
					else
					{
						liveDataPitLaneCar.show = false;
					}
				}
			}
		}

		public void UpdateVoiceOf()
		{
			liveDataVoiceOf.show = false;

			liveDataVoiceOf.voiceOfText = Settings.overlay.translationDictionary[ "VoiceOf" ].translation;

			if ( IRSDK.normalizedData.radioTransmitCarIdx != -1 )
			{
				liveDataVoiceOf.show = true;

				var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.radioTransmitCarIdx );

				if ( normalizedCar != null )
				{
					liveDataVoiceOf.driverNameText = normalizedCar.userName;

					liveDataVoiceOf.carIdx = IRSDK.normalizedData.radioTransmitCarIdx;
				}

				if ( liveDataControlPanel.voiceOfOn )
				{
					Director.chyronTimer = 0;
				}
			}
		}

		public void UpdateChyron()
		{
			if ( IRSDK.normalizedData.camCarIdx != IRSDK.normalizedData.camCarIdxLastFrame )
			{
				chyronRandomOffset++;
			}

			var normalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.normalizedData.camCarIdx );

			if ( ( normalizedCar != null ) && normalizedCar.includeInLeaderboard && Director.showChyron && ( !liveDataControlPanel.voiceOfOn || ( IRSDK.normalizedData.radioTransmitCarIdx == -1 ) ) )
			{
				liveDataChyron.show = true;

				liveDataChyron.driverNameText = normalizedCar.userName;
				liveDataChyron.speedLabelText = Settings.overlay.translationDictionary[ "Speed" ].translation;
				liveDataChyron.speedText = $"{Math.Abs( normalizedCar.speedInMetersPerSecond ) * ( IRSDK.normalizedData.displayIsMetric ? 3.6f : 2.23694f ):0} {( IRSDK.normalizedData.displayIsMetric ? Settings.overlay.translationDictionary[ "KPH" ].translation : Settings.overlay.translationDictionary[ "MPH" ].translation )}";
				liveDataChyron.gearLabelText = Settings.overlay.translationDictionary[ "Gear" ].translation;
				liveDataChyron.rpmLabelText = Settings.overlay.translationDictionary[ "RPM" ].translation;
				liveDataChyron.rpmText = $"{normalizedCar.rpm:0}";
				liveDataChyron.ratingLabelText = Settings.overlay.translationDictionary[ "iRating" ].translation;
				liveDataChyron.ratingText = normalizedCar.iRating.ToString();
				liveDataChyron.licenseLabelText = Settings.overlay.translationDictionary[ "License" ].translation;
				liveDataChyron.licenseText = normalizedCar.license;
				liveDataChyron.hometownLabelText = string.Empty;
				liveDataChyron.hometownText = string.Empty;
				liveDataChyron.randomLabelText = string.Empty;
				liveDataChyron.randomText = string.Empty;

				if ( normalizedCar.memberProfile != null )
				{
					var numRandomItems = 0;

					foreach ( var field in normalizedCar.memberProfile.ProfileFields )
					{
						if ( ( field.Value != string.Empty ) && ( field.Value != "undefined" ) && ( field.Value != "N/A" ) )
						{
							if ( field.Name == "HOMETOWN" )
							{
								liveDataChyron.hometownLabelText = Settings.overlay.translationDictionary[ "Hometown" ].translation;
								liveDataChyron.hometownText = field.Value;
							}
							else if ( chyronRandomItemNames.ContainsKey( field.Name ) )
							{
								chyronAvailableItemLabels[ numRandomItems ] = chyronRandomItemNames[ field.Name ];
								chyronAvailableItemValues[ numRandomItems ] = field.Value;

								numRandomItems++;
							}
						}
					}

					if ( numRandomItems > 0 )
					{
						var winner = chyronRandomOffset % numRandomItems;

						liveDataChyron.randomLabelText = chyronAvailableItemLabels[ winner ];
						liveDataChyron.randomText = chyronAvailableItemValues[ winner ];
					}
				}

				liveDataChyron.carIdx = normalizedCar.carIdx;

				if ( normalizedCar.gear == -1 )
				{
					liveDataChyron.gearText = "R";
				}
				else if ( normalizedCar.gear == 0 )
				{
					liveDataChyron.gearText = "N";
				}
				else
				{
					liveDataChyron.gearText = normalizedCar.gear.ToString();
				}
			}
			else
			{
				liveDataChyron.show = false;
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
								liveDataIntroDriver.licenseText = normalizedCar.license;
								liveDataIntroDriver.ratingText = normalizedCar.iRating.ToString();

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

			liveDataHud.fuel = "-.--";
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

			if ( !IRSDK.normalizedSession.isInRaceSession || ( normalizedCar.lapPositionRelativeToClassLeader <= 0 ) )
			{
				liveDataHud.lapsToLeader = "-.---";
			}
			else
			{
				liveDataHud.lapsToLeader = $"{normalizedCar.lapPositionRelativeToClassLeader:0.000}";
			}

			// gap time

			liveDataHud.gapTimeFront = "-.--";
			liveDataHud.gapTimeFrontColor = Settings.overlay.textSettingsDataDictionary[ "HudGapTimeFront" ].tintColor;

			if ( !IRSDK.normalizedSession.isInQualifyingSession && ( normalizedCar.normalizedCarInFront != null ) )
			{
				var checkpointTimeHis = normalizedCar.normalizedCarInFront.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ];
				var checkpointTimeMine = normalizedCar.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ];

				if ( ( checkpointTimeHis > 0 ) && ( checkpointTimeMine > 0 ) && ( checkpointTimeMine >= checkpointTimeHis ) )
				{
					var gapTime = (float) ( checkpointTimeMine - checkpointTimeHis );

					if ( normalizedCar.carIdxInFrontLastFrame == normalizedCar.normalizedCarInFront.carIdx )
					{
						hudGapTimeFront = hudGapTimeFront * 0.95f + gapTime * 0.05f;
					}
					else
					{
						hudGapTimeFront = gapTime;
					}

					liveDataHud.gapTimeFront = $"{hudGapTimeFront:0.00} {normalizedCar.normalizedCarInFront.displayedName} #{normalizedCar.normalizedCarInFront.carNumber} P{normalizedCar.normalizedCarInFront.displayedPosition}";
				}

				var deltaLapPositionRelativeToClassLeader = normalizedCar.normalizedCarInFront.lapPositionRelativeToClassLeader - normalizedCar.lapPositionRelativeToClassLeader;

				if ( deltaLapPositionRelativeToClassLeader < -0.5f )
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

			if ( !IRSDK.normalizedSession.isInQualifyingSession && ( normalizedCar.normalizedCarBehind != null ) )
			{
				var checkpointTimeMine = normalizedCar.sessionTimeCheckpoints[ normalizedCar.normalizedCarBehind.checkpointIdx ];
				var checkpointTimeHis = normalizedCar.normalizedCarBehind.sessionTimeCheckpoints[ normalizedCar.normalizedCarBehind.checkpointIdx ];

				if ( ( checkpointTimeHis > 0 ) && ( checkpointTimeMine > 0 ) && ( checkpointTimeHis >= checkpointTimeMine ) )
				{
					var gapTime = (float) ( checkpointTimeHis - checkpointTimeMine );

					if ( normalizedCar.carIdxBehindLastFrame == normalizedCar.normalizedCarBehind.carIdx )
					{
						hudGapTimeBack = hudGapTimeBack * 0.95f + gapTime * 0.05f;
					}
					else
					{
						hudGapTimeBack = gapTime;
					}

					liveDataHud.gapTimeBack = $"{hudGapTimeBack:0.00} {normalizedCar.normalizedCarBehind.displayedName} #{normalizedCar.normalizedCarBehind.carNumber} P{normalizedCar.normalizedCarBehind.displayedPosition}";
				}

				var deltaLapPositionRelativeToClassLeader = normalizedCar.normalizedCarBehind.lapPositionRelativeToClassLeader - normalizedCar.lapPositionRelativeToClassLeader;

				if ( deltaLapPositionRelativeToClassLeader < -0.5f )
				{
					liveDataHud.gapTimeBackColor = new Color( 1.0f, 0.3f, 0.3f, liveDataHud.gapTimeFrontColor.a );
				}
				else if ( deltaLapPositionRelativeToClassLeader > 0.5f )
				{
					liveDataHud.gapTimeBackColor = new Color( 0.4f, 0.4f, 1.0f, liveDataHud.gapTimeFrontColor.a );
				}
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

			// lap delta

			var tL0 = normalizedCar.sessionTimeCheckpointsLastLap[ 0 ];
			var tL1 = normalizedCar.sessionTimeCheckpointsLastLap[ normalizedCar.checkpointIdx ];

			var tC0 = normalizedCar.sessionTimeCheckpoints[ 0 ];
			var tC1 = normalizedCar.sessionTimeCheckpoints[ normalizedCar.checkpointIdx ];

			if ( normalizedCar.checkpointIdx == 0 )
			{
				interpolatedDeltaTime = 0;
				interpolatedDeltaInterpolatedDeltaTime = 0;
				lastInterpolatedDeltaTime = 0;
			}

			if ( ( tL0 > 0 ) && ( tL1 >= tL0 ) && ( tC0 > 0 ) && ( tC1 >= tC0 ) )
			{
				var lastLapTime = tL1 - tL0;
				var currentLapTime = tC1 - tC0;

				var deltaTime = currentLapTime - lastLapTime;

				interpolatedDeltaTime = interpolatedDeltaTime * 0.97f + deltaTime * 0.03f;

				var deltaInterpolatedDeltaTime = ( interpolatedDeltaTime - lastInterpolatedDeltaTime ) * 1000;

				interpolatedDeltaInterpolatedDeltaTime = interpolatedDeltaInterpolatedDeltaTime * 0.99f + deltaInterpolatedDeltaTime * 0.01f;

				lastInterpolatedDeltaTime = interpolatedDeltaTime;

				liveDataHud.lapDelta = $"{interpolatedDeltaTime:+0.00;-0.00; 0.00} | {interpolatedDeltaInterpolatedDeltaTime:+0.00;-0.00; 0.00}";

				if ( interpolatedDeltaInterpolatedDeltaTime <= 0 )
				{
					liveDataHud.lapDeltaColor = Color.Lerp( Color.white, green, (float) Math.Min( 1, -interpolatedDeltaInterpolatedDeltaTime ) );
				}
				else
				{
					liveDataHud.lapDeltaColor = Color.Lerp( Color.white, red, (float) Math.Min( 1, interpolatedDeltaInterpolatedDeltaTime ) );
				}
			}
			else
			{
				interpolatedDeltaTime = 0;
				interpolatedDeltaInterpolatedDeltaTime = 0;
				lastInterpolatedDeltaTime = 0;

				liveDataHud.lapDelta = " -.-- | -.--";
				liveDataHud.lapDeltaColor = Color.white;
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

		public void UpdateTrainer()
		{
			liveDataTrainer.drawVectorList = Trainer.drawVectorList;

			liveDataTrainer.message = Trainer.message;
		}

		public void UpdateWebcamStreaming()
		{
			liveDataWebcamStreaming.enabled = Settings.editor.editorWebcamStreamingEnabled;
			liveDataWebcamStreaming.webserverURL = Settings.editor.editorWebcamStreamingWebserverURL;
		}
	}
}

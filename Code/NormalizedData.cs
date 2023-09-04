
using System;
using System.Collections.Generic;
using System.Linq;

using irsdkSharp.Serialization.Enums.Fastest;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public class NormalizedData
	{
		public class LeaderboardClass
		{
			public int numDrivers = 0;
			public int classID = -1;
			public Color color = Color.white;
			public string name = string.Empty;
			public string shortName = string.Empty;
		}

		public const int MaxNumCars = 64;
		public const int MaxNumClasses = 8;

		public double sessionTimeDelta;
		public double sessionTime;

		public uint sessionFlags;
		public uint sessionFlagsLastFrame;

		public int replayFrameNum;
		public int replaySpeed;

		public bool displayIsMetric;

		public bool isInTimedRace;
		public bool isUnderCaution;
		public bool isTalking;

		public SessionState sessionState;

		public double sessionTimeTotal;
		public double sessionTimeRemaining;

		public int sessionLapsTotal;
		public int sessionLapsRemaining;

		public int lapNumber;
		public int lapNumberLastFrame;

		public int numLeaderboardCars;
		public int numLeaderboardClasses;
		public LeaderboardClass[] leaderboardClass = new LeaderboardClass[ MaxNumClasses ];

		public int camCarIdx;
		public int camGroupNumber;
		public int camCameraNumber;

		public int radioTransmitCarIdx;

		public float bestLapTime;

		public float fuelLevel;
		public float lastLapFuelLevel;
		public float[] lapFuelLevelDelta = Array.Empty<float>();
		public float highestLapFuelLevelDelta;

		public NormalizedCar? paceCar = null;

		public NormalizedCar[] normalizedCars = new NormalizedCar[ MaxNumCars ];

		public List<NormalizedCar> leaderboardSortedNormalizedCars = new( MaxNumCars );
		public List<NormalizedCar> classLeaderboardSortedNormalizedCars = new( MaxNumCars );
		public List<NormalizedCar> relativeLapPositionSortedNormalizedCars = new( MaxNumCars );

		public NormalizedData()
		{
			for ( var i = 0; i < MaxNumCars; i++ )
			{
				normalizedCars[ i ] = new NormalizedCar( i );

				leaderboardSortedNormalizedCars.Add( normalizedCars[ i ] );
				classLeaderboardSortedNormalizedCars.Add( normalizedCars[ i ] );
				relativeLapPositionSortedNormalizedCars.Add( normalizedCars[ i ] );
			}

			for ( var i = 0; i < MaxNumClasses; i++ )
			{
				leaderboardClass[ i ] = new LeaderboardClass();
			}

			Reset();
		}

		public void Reset()
		{
			sessionTimeDelta = 0;
			sessionTime = 0;

			sessionFlags = 0;
			sessionFlagsLastFrame = 0;

			replayFrameNum = 0;
			replaySpeed = 0;

			displayIsMetric = false;

			isInTimedRace = false;
			isUnderCaution = false;
			isTalking = false;

			sessionState = SessionState.StateInvalid;

			sessionTimeTotal = 0;
			sessionTimeRemaining = 0;

			sessionLapsTotal = 0;
			sessionLapsRemaining = 0;

			lapNumber = 0;
			lapNumberLastFrame = 0;

			numLeaderboardCars = 0;
			numLeaderboardClasses = 1;

			for ( var classIndex = 0; classIndex < MaxNumClasses; classIndex++ )
			{
				leaderboardClass[ classIndex ].numDrivers = 0;
				leaderboardClass[ classIndex ].classID = -1;
				leaderboardClass[ classIndex ].color = Color.white;
				leaderboardClass[ classIndex ].name = string.Empty;
				leaderboardClass[ classIndex ].shortName = string.Empty;
			}

			camCarIdx = 0;
			camGroupNumber = 0;
			camCameraNumber = 0;

			radioTransmitCarIdx = -1;

			bestLapTime = 0;

			paceCar = null;

			fuelLevel = 0;
			lastLapFuelLevel = 0;
			lapFuelLevelDelta = new float[] { 0, 0, 0, 0, 0 };
			highestLapFuelLevelDelta = 0;

			foreach ( var normalizedCar in normalizedCars )
			{
				normalizedCar.Reset();
			}
		}

		public void SessionUpdate()
		{
			numLeaderboardCars = 0;

			if ( IRSDK.session != null )
			{
				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.SessionUpdate();

					if ( normalizedCar.includeInLeaderboard )
					{
						numLeaderboardCars++;
					}

					if ( normalizedCar.isPaceCar )
					{
						paceCar = normalizedCar;
					}
				}

				foreach ( var session in IRSDK.session.SessionInfo.Sessions )
				{
					if ( session.SessionName == "QUALIFY" )
					{
						if ( session.ResultsPositions != null )
						{
							foreach ( var position in session.ResultsPositions )
							{
								normalizedCars[ position.CarIdx ].qualifyingPosition = position.Position;
								normalizedCars[ position.CarIdx ].qualifyingTime = position.Time;
							}
						}

						break;
					}
				}
			}

			for ( var carIdx = 0; carIdx < MaxNumCars; carIdx++ )
			{
				var normalizedCar = normalizedCars[ carIdx ];

				var originalAbbrevName = normalizedCar.abbrevName;

				for ( var otherCarIdx = carIdx + 1; otherCarIdx < MaxNumCars; otherCarIdx++ )
				{
					var otherNormalizedCar = normalizedCars[ otherCarIdx ];

					if ( otherNormalizedCar.abbrevName == originalAbbrevName )
					{
						normalizedCar.GenerateAbbrevName( true );
						otherNormalizedCar.GenerateAbbrevName( true );
					}
				}
			}
		}

		public void SessionNumberChange()
		{
			sessionTime = 0;

			sessionFlags = 0;
			sessionFlagsLastFrame = 0;

			lapNumber = 0;
			lapNumberLastFrame = 0;

			foreach ( var normalizedCar in normalizedCars )
			{
				normalizedCar.SessionNumberChange();
			}
		}

		public void Update()
		{
			if ( IRSDK.data == null )
			{
				return;
			}

			var newSessionTime = Math.Round( IRSDK.data.SessionTime / ( 1.0f / 60.0f ) ) * ( 1.0f / 60.0f );

			sessionTimeDelta = newSessionTime - sessionTime;
			sessionTime = newSessionTime;

			if ( IRSDK.data.ReplayPlaySpeed >= 0 )
			{
				sessionTimeDelta = Math.Max( 0, sessionTimeDelta );
			}

			sessionFlagsLastFrame = sessionFlags;

			if ( IRSDK.normalizedSession.isReplay )
			{
				var sessionFlagsData = SessionFlagsPlayback.GetCurrentSessionFlagsData();

				sessionFlags = sessionFlagsData?.SessionFlags ?? 0;
			}
			else
			{
				sessionFlags = (uint) IRSDK.data.SessionFlags;
			}

			replayFrameNum = IRSDK.data.ReplayFrameNum;
			replaySpeed = IRSDK.data.ReplayPlaySpeed;

			displayIsMetric = IRSDK.data.DisplayUnits == 1;

			isInTimedRace = IRSDK.data.SessionLapsTotal == 32767;
			isUnderCaution = ( sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.Caution | (uint) SessionFlags.YellowWaving | (uint) SessionFlags.Yellow ) ) != 0;
			isTalking = IRSDK.data.PushToTalk;

			sessionState = (SessionState) IRSDK.data.SessionState;

			sessionTimeTotal = IRSDK.data.SessionTimeTotal;
			sessionTimeRemaining = Math.Max( 0, IRSDK.data.SessionTimeRemain );

			sessionLapsTotal = IRSDK.data.SessionLapsTotal;
			sessionLapsRemaining = Math.Min( sessionLapsTotal, Math.Max( 0, IRSDK.data.SessionLapsRemain ) + 1 );

			lapNumberLastFrame = lapNumber;

			if ( isInTimedRace )
			{
				lapNumber = IRSDK.data.Lap + 1;
			}
			else
			{
				lapNumber = sessionLapsTotal - sessionLapsRemaining + 1;
			}

			camCarIdx = IRSDK.data.CamCarIdx;
			camGroupNumber = IRSDK.data.CamGroupNumber;
			camCameraNumber = IRSDK.data.CamCameraNumber;

			var radioTransmitCarIdxLastFrame = radioTransmitCarIdx;

			radioTransmitCarIdx = IRSDK.data.RadioTransmitCarIdx;

			// speech to text

			if ( IRSDK.normalizedData.replaySpeed == 1 )
			{
				if ( radioTransmitCarIdx != radioTransmitCarIdxLastFrame )
				{
					if ( radioTransmitCarIdx == -1 )
					{
						SpeechToText.Stop( IRSDK.normalizedSession.sessionNumber, IRSDK.normalizedData.sessionTime );
					}
					else
					{
						if ( radioTransmitCarIdxLastFrame != -1 )
						{
							SpeechToText.Stop( IRSDK.normalizedSession.sessionNumber, IRSDK.normalizedData.sessionTime );
						}

						var subtitleData = SubtitlePlayback.GetCurrentSubtitleData();

						if ( ( subtitleData == null ) || ( subtitleData.Text == string.Empty ) )
						{
							SpeechToText.Start( IRSDK.normalizedSession.sessionNumber, IRSDK.normalizedData.sessionTime );
						}
					}
				}
			}
			else
			{
				SpeechToText.StopAll();
			}

			// hud - fuel

			fuelLevel = IRSDK.data.FuelLevel;

			if ( lapNumber != lapNumberLastFrame )
			{
				lapFuelLevelDelta[ lapNumberLastFrame % lapFuelLevelDelta.Length ] = lastLapFuelLevel - fuelLevel;

				lastLapFuelLevel = fuelLevel;
				highestLapFuelLevelDelta = 0;

				foreach ( var lapFuelLevelData in lapFuelLevelDelta )
				{
					highestLapFuelLevelDelta = Math.Max( highestLapFuelLevelDelta, lapFuelLevelData );
				}
			}

			// leaderboard and heat stuff

			if ( sessionTimeDelta > 0 )
			{
				var preferredCarUserIdsList = Settings.director.preferredCarUserIds.Split( "," ).ToList().Select( s => s.Trim() ).ToList();
				var preferredCarCarNumbersList = Settings.director.preferredCarCarNumbers.Split( "," ).ToList().Select( s => s.Trim() ).ToList();

				// update each car

				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.Update();

					if ( normalizedCar.includeInLeaderboard )
					{
						normalizedCar.isPreferredCar = preferredCarUserIdsList.Contains( normalizedCar.userId.ToString() ) || preferredCarCarNumbersList.Contains( normalizedCar.carNumber );
					}
				}

				// calculate distances to car in front and back for each car

				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.normalizedCarInFront = null;
					normalizedCar.normalizedCarBehind = null;

					normalizedCar.distanceToCarInFrontInMeters = float.MaxValue;
					normalizedCar.distanceToCarBehindInMeters = float.MaxValue;

					if ( normalizedCar.includeInLeaderboard && !normalizedCar.isOnPitRoad && !normalizedCar.isOutOfCar )
					{
						foreach ( var otherNormalizedCar in normalizedCars )
						{
							if ( normalizedCar != otherNormalizedCar )
							{
								if ( otherNormalizedCar.includeInLeaderboard && !otherNormalizedCar.isOnPitRoad && !otherNormalizedCar.isOutOfCar )
								{
									var signedDistanceToOtherCar = otherNormalizedCar.lapDistPct - normalizedCar.lapDistPct;

									if ( signedDistanceToOtherCar > 0.5f )
									{
										signedDistanceToOtherCar -= 1;
									}
									else if ( signedDistanceToOtherCar < -0.5f )
									{
										signedDistanceToOtherCar += 1;
									}

									var signedDistanceToOtherCarInMeters = signedDistanceToOtherCar * IRSDK.normalizedSession.trackLengthInMeters;

									if ( signedDistanceToOtherCarInMeters >= 0 )
									{
										if ( normalizedCar.distanceToCarInFrontInMeters > signedDistanceToOtherCarInMeters )
										{
											normalizedCar.normalizedCarInFront = otherNormalizedCar;
											normalizedCar.distanceToCarInFrontInMeters = signedDistanceToOtherCarInMeters;
										}
									}
									else
									{
										signedDistanceToOtherCarInMeters = -signedDistanceToOtherCarInMeters;

										if ( normalizedCar.distanceToCarBehindInMeters > signedDistanceToOtherCarInMeters )
										{
											normalizedCar.normalizedCarBehind = otherNormalizedCar;
											normalizedCar.distanceToCarBehindInMeters = signedDistanceToOtherCarInMeters;
										}
									}
								}
							}
						}
					}
				}

				// leaderboard sorting

				if ( IRSDK.normalizedSession.isInPracticeSession || IRSDK.normalizedSession.isInQualifyingSession )
				{
					leaderboardSortedNormalizedCars.Sort( NormalizedCar.BestLapTimeComparison );
				}
				else if ( IRSDK.normalizedSession.isInRaceSession )
				{
					if ( IRSDK.normalizedData.sessionState <= SessionState.StateParadeLaps )
					{
						leaderboardSortedNormalizedCars.Sort( NormalizedCar.QualifyingPositionComparison );
					}
					else if ( isUnderCaution || ( IRSDK.normalizedData.sessionState >= SessionState.StateCheckered ) )
					{
						leaderboardSortedNormalizedCars.Sort( NormalizedCar.OverallPositionComparison );
					}
					else
					{
						leaderboardSortedNormalizedCars.Sort( NormalizedCar.LapPositionComparison );
					}
				}

				// set leaderboard index

				var leaderboardIndex = 1;

				foreach ( var normalizedCar in leaderboardSortedNormalizedCars )
				{
					normalizedCar.leaderboardIndex = leaderboardIndex++;
				}

				// class leaderboard sorting

				classLeaderboardSortedNormalizedCars.Sort( NormalizedCar.ClassLeaderboardIndexComparison );

				// lap position relative to class leader for laps down telemetry, also count number of classes, and set displayed position

				var classLeader = classLeaderboardSortedNormalizedCars[ 0 ];

				leaderboardClass[ 0 ].numDrivers = 0;
				leaderboardClass[ 0 ].classID = classLeader.classID;
				leaderboardClass[ 0 ].color = classLeader.classColor;
				leaderboardClass[ 0 ].name = classLeader.carClass?.Name ?? string.Empty;
				leaderboardClass[ 0 ].shortName = classLeader.carClass?.ShortName ?? string.Empty;

				numLeaderboardClasses = 1;

				var displayedPosition = 1;

				foreach ( var normalizedCar in classLeaderboardSortedNormalizedCars )
				{
					if ( !normalizedCar.includeInLeaderboard )
					{
						break;
					}

					if ( classLeader.classID != normalizedCar.classID )
					{
						classLeader = normalizedCar;

						leaderboardClass[ numLeaderboardClasses ].numDrivers = 0;
						leaderboardClass[ numLeaderboardClasses ].classID = classLeader.classID;
						leaderboardClass[ numLeaderboardClasses ].color = classLeader.classColor;
						leaderboardClass[ numLeaderboardClasses ].name = classLeader.carClass?.Name ?? string.Empty;
						leaderboardClass[ numLeaderboardClasses ].shortName = classLeader.carClass?.ShortName ?? string.Empty;

						numLeaderboardClasses++;

						displayedPosition = 1;
					}

					normalizedCar.lapPositionRelativeToClassLeader = classLeader.lapPosition - normalizedCar.lapPosition;
					normalizedCar.displayedPosition = displayedPosition++;
					normalizedCar.leaderboardClassIndex = numLeaderboardClasses - 1;

					leaderboardClass[ normalizedCar.leaderboardClassIndex ].numDrivers++;
				}

				for ( var i = numLeaderboardClasses; leaderboardIndex < MaxNumClasses; leaderboardIndex++ )
				{
					leaderboardClass[ i ].numDrivers = 0;
				}

				// heat calculations

				foreach ( var normalizedCar in normalizedCars )
				{
					if ( normalizedCar.includeInLeaderboard )
					{
						if ( normalizedCar.isOnPitRoad || ( normalizedCar.outOfCarTimer >= 10 ) )
						{
							normalizedCar.heat = 0;
							normalizedCar.heatBonus = 0;
							normalizedCar.heatBias = 0;
							normalizedCar.heatTotal = 0;
							normalizedCar.heatGapTime = 0;
						}
						else if ( normalizedCar.checkpointIdx != normalizedCar.checkpointIdxLastFrame )
						{
							normalizedCar.heat = 0;

							var heatGapTime = 0.0f;

							if ( normalizedCar.normalizedCarInFront != null )
							{
								var checkpointTimeHis = normalizedCar.normalizedCarInFront.checkpoints[ normalizedCar.checkpointIdx ];
								var checkpointTimeMine = normalizedCar.checkpoints[ normalizedCar.checkpointIdx ];

								if ( ( checkpointTimeHis > 0 ) && ( checkpointTimeMine > 0 ) && ( checkpointTimeMine >= checkpointTimeHis ) )
								{
									heatGapTime = ( (float) ( checkpointTimeMine - checkpointTimeHis ) - 0.1f ) / ( Settings.director.heatMaxGapTime - 0.1f );

									normalizedCar.heat = (float) Math.Pow( Math.Max( 0, Math.Min( 1, 1 - heatGapTime ) ), 2 );
								}
							}

							var deltaHeatGapTime = heatGapTime - normalizedCar.heatGapTime;

							normalizedCar.heatGapTime = heatGapTime;

							var heatBonus = normalizedCar.heatBonus;

							if ( deltaHeatGapTime < 0 )
							{
								heatBonus = Math.Max( 0, Math.Min( 1, 1 - heatGapTime ) ) * Settings.director.heatOvertakeBonus;
							}
							else if ( deltaHeatGapTime > 0 )
							{
								heatBonus = 0;
							}

							normalizedCar.heatBonus = normalizedCar.heatBonus * 0.9f + heatBonus * 0.1f;

							if ( normalizedCar.heatBonus < 0.01f )
							{
								normalizedCar.heatBonus = 0;
							}

							normalizedCar.heatBias = 0;

							if ( ( normalizedCar.heat + normalizedCar.heatBonus ) > 0 )
							{
								var numDrivers = leaderboardClass[ normalizedCar.leaderboardClassIndex ].numDrivers;

								var driverOnScale = normalizedCar.displayedPosition - 1;
								var scaleMidpoint = ( ( numDrivers - 1.0f ) / 2.0f );

								var positionAsSignedPct = ( scaleMidpoint - driverOnScale ) / scaleMidpoint;

								normalizedCar.heatBias = Settings.director.heatBias * positionAsSignedPct + Math.Abs( Settings.director.heatBias );
							}

							normalizedCar.heatTotal = normalizedCar.heat + normalizedCar.heatBonus + normalizedCar.heatBias;
						}
					}
				}

				// sort cars by relative lap position for control panel

				var leader = leaderboardSortedNormalizedCars[ 0 ];

				foreach ( var normalizedCar in leaderboardSortedNormalizedCars )
				{
					normalizedCar.lapDistPctRelativeToLeader = leader.lapDistPct - normalizedCar.lapDistPct;

					if ( normalizedCar.lapDistPctRelativeToLeader < 0 )
					{
						normalizedCar.lapDistPctRelativeToLeader += 1;
					}
				}

				relativeLapPositionSortedNormalizedCars.Sort( NormalizedCar.RelativeLapPositionComparison );
			}
		}

		public NormalizedCar? FindNormalizedCarByCarIdx( int carIdx )
		{
			foreach ( var normalizedCar in normalizedCars )
			{
				if ( normalizedCar.carIdx == carIdx )
				{
					return normalizedCar;
				}
			}

			return null;
		}
	}
}

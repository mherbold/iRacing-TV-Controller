
using System;
using System.Collections.Generic;
using System.Linq;
using irsdkSharp.Serialization.Enums.Fastest;

namespace iRacingTVController
{
	public class NormalizedData
	{
		public const int MaxNumCars = 63;

		public double sessionTimeDelta;
		public double sessionTime;
		public uint sessionFlags;

		public int replayFrameNum;
		public int replaySpeed;

		public bool displayIsMetric;

		public bool isInTimedRace;
		public bool isUnderCaution;

		public SessionState sessionState;

		public double sessionTimeTotal;
		public double sessionTimeRemaining;

		public int sessionLapsTotal;
		public int sessionLapsRemaining;

		public int currentLap;
		public int numLeaderboardCars;

		public int camCarIdx;
		public int camGroupNumber;
		public int camCameraNumber;

		public int radioTransmitCarIdx;

		public NormalizedCar[] normalizedCars = new NormalizedCar[ MaxNumCars ];

		public List<NormalizedCar> absoluteLapPositionSortedNormalizedCars = new( MaxNumCars );
		public List<NormalizedCar> relativeLapPositionSortedNormalizedCars = new( MaxNumCars );
		public List<NormalizedCar> leaderboardPositionSortedNormalizedCars = new( MaxNumCars );

		public NormalizedData()
		{
			for ( var i = 0; i < MaxNumCars; i++ )
			{
				normalizedCars[ i ] = new NormalizedCar( i );

				absoluteLapPositionSortedNormalizedCars.Add( normalizedCars[ i ] );
				relativeLapPositionSortedNormalizedCars.Add( normalizedCars[ i ] );
				leaderboardPositionSortedNormalizedCars.Add( normalizedCars[ i ] );
			}

			Reset();
		}

		public void Reset()
		{
			sessionTimeDelta = 0;
			sessionTime = 0;
			sessionFlags = 0;

			replayFrameNum = 0;
			replaySpeed = 0;

			displayIsMetric = false;

			isInTimedRace = false;
			isUnderCaution = false;

			sessionState = SessionState.StateInvalid;

			sessionTimeTotal = 0;
			sessionTimeRemaining = 0;

			sessionLapsTotal = 0;
			sessionLapsRemaining = 0;

			currentLap = 0;
			numLeaderboardCars = 0;

			camCarIdx = 0;
			camGroupNumber = 0;
			camCameraNumber = 0;

			radioTransmitCarIdx = -1;

			foreach ( var normalizedCar in normalizedCars )
			{
				normalizedCar.Reset();
			}
		}

		public void SessionNumberChange()
		{
			foreach ( var normalizedCar in normalizedCars )
			{
				normalizedCar.SessionNumberChange();
			}
		}

		public void SessionUpdate( bool forceUpdate = false )
		{
			numLeaderboardCars = 0;

			foreach ( var normalizedCar in normalizedCars )
			{
				normalizedCar.SessionUpdate( forceUpdate );

				if ( normalizedCar.includeInLeaderboard )
				{
					numLeaderboardCars++;
				}
			}

			if ( IRSDK.session != null )
			{
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
		}

		public void Update()
		{
			if ( IRSDK.data == null )
			{
				return;
			}

			sessionTimeDelta = Math.Round( ( IRSDK.data.SessionTime - sessionTime ) / ( 1.0f / 60.0f ) ) * ( 1.0f / 60.0f );
			sessionTime = IRSDK.data.SessionTime;

			if ( IRSDK.normalizedSession.isReplay )
			{
				sessionFlags = SessionFlagsPlayback.Playback( IRSDK.normalizedSession.sessionNumber, IRSDK.normalizedData.sessionTime );
			}
			else
			{
				sessionFlags = (uint) IRSDK.data.SessionFlags;

				SessionFlagsPlayback.Record( IRSDK.normalizedSession.sessionNumber, IRSDK.normalizedData.sessionTime, sessionFlags );
			}

			replayFrameNum = IRSDK.data.ReplayFrameNum;
			replaySpeed = IRSDK.data.ReplayPlaySpeed;

			displayIsMetric = IRSDK.data.DisplayUnits == 1;

			isInTimedRace = IRSDK.data.SessionLapsTotal == 32767;
			isUnderCaution = ( sessionFlags & ( (uint) SessionFlags.CautionWaving | (uint) SessionFlags.Caution | (uint) SessionFlags.YellowWaving | (uint) SessionFlags.Yellow ) ) != 0;

			sessionState = (SessionState) IRSDK.data.SessionState;

			sessionTimeTotal = IRSDK.data.SessionTimeTotal;
			sessionTimeRemaining = Math.Max( 0, IRSDK.data.SessionTimeRemain );

			sessionLapsTotal = IRSDK.data.SessionLapsTotal;
			sessionLapsRemaining = Math.Max( 0, IRSDK.data.SessionLapsRemain );

			currentLap = sessionLapsTotal - sessionLapsRemaining;

			camCarIdx = IRSDK.data.CamCarIdx;
			camGroupNumber = IRSDK.data.CamGroupNumber;
			camCameraNumber = IRSDK.data.CamCameraNumber;

			radioTransmitCarIdx = IRSDK.data.RadioTransmitCarIdx;

			if ( sessionTimeDelta > 0 )
			{
				// update each car

				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.Update();
				}

				// reset defending heat and heat bias, calculate attacking heat, and calculate distances to car in front and back, for each car

				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.attackingHeat = 0;
					normalizedCar.defendingHeat = 0;
					normalizedCar.heatBias = 0;

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
										var heat = 1 - Math.Max( 0, Math.Abs( signedDistanceToOtherCarInMeters ) - Settings.director.heatCarLength ) / Math.Max( 1, Settings.director.heatFalloff );

										if ( heat > 0 )
										{
											normalizedCar.attackingHeat += heat;
										}

										normalizedCar.distanceToCarInFrontInMeters = Math.Min( normalizedCar.distanceToCarInFrontInMeters, signedDistanceToOtherCarInMeters );
									}
									else
									{
										normalizedCar.distanceToCarBehindInMeters = Math.Min( normalizedCar.distanceToCarBehindInMeters, -signedDistanceToOtherCarInMeters );
									}
								}
							}
						}
					}
				}

				// sort cars by absolute lap positions

				absoluteLapPositionSortedNormalizedCars.Sort( NormalizedCar.AbsoluteLapPositionComparison );

				// set leaderboard position and lap position relative to leader for each car

				var leaderboardPosition = 1;
				var leaderLapPosition = absoluteLapPositionSortedNormalizedCars[ 0 ].lapPosition;

				foreach ( var normalizedCar in absoluteLapPositionSortedNormalizedCars )
				{
					if ( IRSDK.normalizedSession.isInQualifyingSession )
					{
						if ( ( normalizedCar.f2Time > 0 ) && ( normalizedCar.officialPosition >= 1 ) )
						{
							normalizedCar.leaderboardPosition = normalizedCar.officialPosition;
						}
						else
						{
							normalizedCar.leaderboardPosition = normalizedCar.carIdx + 64;
						}
					}
					else if ( IRSDK.normalizedSession.isInRaceSession )
					{
						if ( IRSDK.normalizedData.sessionState <= SessionState.StateWarmup )
						{
							normalizedCar.leaderboardPosition = normalizedCar.qualifyingPosition;
						}
						else if ( IRSDK.normalizedData.sessionState == SessionState.StateParadeLaps )
						{
							if ( normalizedCar.isOutOfCar )
							{
								normalizedCar.leaderboardPosition = normalizedCar.carIdx + 64;
							}
							else
							{
								normalizedCar.leaderboardPosition = normalizedCar.qualifyingPosition;
							}
						}
						else if ( isUnderCaution || !normalizedCar.hasCrossedStartLine || ( IRSDK.normalizedData.sessionState >= SessionState.StateCheckered ) )
						{
							if ( normalizedCar.officialPosition == 0 )
							{
								normalizedCar.leaderboardPosition = normalizedCar.qualifyingPosition;
							}
							else
							{
								normalizedCar.leaderboardPosition = normalizedCar.officialPosition;
							}
						}
						else
						{
							normalizedCar.leaderboardPosition = leaderboardPosition++;
						}
					}
					else
					{
						normalizedCar.leaderboardPosition = normalizedCar.carIdx;
					}

					normalizedCar.lapPositionRelativeToLeader = leaderLapPosition - normalizedCar.lapPosition;
				}

				// sort cars by relative lap position (relative to leader)

				relativeLapPositionSortedNormalizedCars.Sort( NormalizedCar.RelativeLapPositionComparison );

				// update defending heat for each car

				NormalizedCar normalizedCarInFront = relativeLapPositionSortedNormalizedCars.Last();

				foreach ( var normalizedCar in relativeLapPositionSortedNormalizedCars )
				{
					normalizedCarInFront.defendingHeat = normalizedCar.attackingHeat;

					normalizedCarInFront = normalizedCar;
				}

				// sort cars by leaderboard position

				leaderboardPositionSortedNormalizedCars.Sort( NormalizedCar.LeaderboardPositionComparison );

				// clean up leaderboard positions and set the heat bias of each car

				leaderboardPosition = 1;

				foreach ( var normalizedCar in leaderboardPositionSortedNormalizedCars )
				{
					normalizedCar.leaderboardPosition = leaderboardPosition++;

					if ( normalizedCar.includeInLeaderboard )
					{
						if ( normalizedCar.attackingHeat > 0 )
						{
							var positionAsSignedPct = ( ( numLeaderboardCars / 2.0f ) - normalizedCar.leaderboardPosition ) / ( numLeaderboardCars / 2.0f );

							normalizedCar.heatBias = Settings.director.heatBias * positionAsSignedPct + Math.Abs( Settings.director.heatBias );
						}
					}
				}
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

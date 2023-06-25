
using System;
using System.Collections.Generic;

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
		public List<NormalizedCar> leaderboardSortedNormalizedCars = new( MaxNumCars );
		public List<NormalizedCar> attackingHeatSortedNormalizedCars = new( MaxNumCars );

		public NormalizedData()
		{
			for ( var i = 0; i < MaxNumCars; i++ )
			{
				normalizedCars[ i ] = new NormalizedCar( i );

				leaderboardSortedNormalizedCars.Add( normalizedCars[ i ] );
				attackingHeatSortedNormalizedCars.Add( normalizedCars[ i ] );
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
				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.Update();
				}

				foreach ( var normalizedCar in normalizedCars )
				{
					normalizedCar.attackingHeat = 0;
					normalizedCar.defendingHeat = 0;

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

									var heat = 1 - Math.Max( 0, Math.Abs( signedDistanceToOtherCarInMeters ) - Settings.overlay.directorCarLength ) / Math.Max( 1, Settings.overlay.directorHeatFalloff );

									if ( heat > 0 )
									{
										if ( signedDistanceToOtherCar >= 0 )
										{
											normalizedCar.attackingHeat += heat;
										}
										else
										{
											normalizedCar.defendingHeat += heat;
										}
									}

									if ( signedDistanceToOtherCarInMeters >= 0 )
									{
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

				leaderboardSortedNormalizedCars.Sort( NormalizedCar.LapPositionComparison );

				var leaderboardPosition = 1;
				var leaderLapPosition = leaderboardSortedNormalizedCars[ 0 ].lapPosition;

				foreach ( var normalizedCar in leaderboardSortedNormalizedCars )
				{
					if ( IRSDK.normalizedSession.isInPracticeSession )
					{
						normalizedCar.leaderboardPosition = normalizedCar.carIdx;
					}
					else
					{
						if ( normalizedCar.officialPosition >= 1 )
						{
							normalizedCar.leaderboardPosition = normalizedCar.officialPosition;
						}
						else
						{
							normalizedCar.leaderboardPosition = int.MaxValue;
						}

						if ( IRSDK.normalizedSession.isInRaceSession )
						{
							if ( normalizedCar.hasCrossedStartLine )
							{
								if ( !isUnderCaution && ( sessionState < SessionState.StateCheckered ) )
								{
									normalizedCar.leaderboardPosition = leaderboardPosition++;
								}
							}
							else
							{
								normalizedCar.leaderboardPosition = normalizedCar.qualifyingPosition;
							}
						}
					}

					normalizedCar.lapPositionRelativeToLeader = leaderLapPosition - normalizedCar.lapPosition;
				}

				leaderboardSortedNormalizedCars.Sort( NormalizedCar.LeaderboardPositionComparison );

				leaderboardPosition = 1;

				foreach ( var normalizedCar in leaderboardSortedNormalizedCars )
				{
					normalizedCar.leaderboardPosition = leaderboardPosition++;

					if ( normalizedCar.includeInLeaderboard )
					{
						if ( normalizedCar.attackingHeat > 0 )
						{
							var positionAsSignedPct = ( ( numLeaderboardCars / 2.0f ) - normalizedCar.leaderboardPosition ) / ( numLeaderboardCars / 2.0f );

							var heatBias = Settings.overlay.directorHeatBias * positionAsSignedPct + Math.Abs( Settings.overlay.directorHeatBias );

							normalizedCar.attackingHeat += heatBias;
						}
					}
				}

				attackingHeatSortedNormalizedCars.Sort( NormalizedCar.HeatComparison );
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

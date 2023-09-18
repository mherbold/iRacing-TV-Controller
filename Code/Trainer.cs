
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public static class Trainer
	{
		public const int numVectors = 40;

		public static Vector3[] drawVectorListA = new Vector3[ numVectors ];
		public static Vector3[] drawVectorListB = new Vector3[ numVectors ];

		public static string message = string.Empty;

		public static void Update()
		{
			if ( IRSDK.data != null )
			{
				var playerNormalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.data.PlayerCarIdx );

				if ( IRSDK.data.PlayerCarIdx >= 0 )
				{
					if ( playerNormalizedCar != null )
					{
						if ( IRSDK.normalizedData.replayFrameNum < IRSDK.normalizedData.replayFrameNumLastFrame )
						{
							message = string.Empty;

							for ( var i = 0; i < IRSDK.normalizedSession.numCheckpoints; i++ )
							{
								IRSDK.normalizedData.fastestLapSpeedCheckpoints[ i ] = 0;
							}
						}

						if ( IRSDK.normalizedData.fastestLapTime != double.MaxValue )
						{
							if ( IRSDK.normalizedData.bestLapTime > 0 )
							{
								IRSDK.normalizedData.fastestLapTime += 0.25 * Program.deltaTime / IRSDK.normalizedData.bestLapTime;
							}
						}

						if ( playerNormalizedCar.currentLap != playerNormalizedCar.currentLapLastFrame )
						{
							IRSDK.normalizedData.fastestLapTimeAge++;

							if ( IRSDK.normalizedData.fastestLapTimeAge >= 3 )
							{
								IRSDK.normalizedData.fastestLapTimeAge = 0;

								IRSDK.normalizedData.fastestLapTime = double.MaxValue;
							}
						}

						foreach ( var normalizedCar in IRSDK.normalizedData.normalizedCars )
						{
							if ( ( normalizedCar.currentLap != normalizedCar.currentLapLastFrame ) && !normalizedCar.isOnPitRoad && !normalizedCar.wasOnPitRoad )
							{
								if ( ( normalizedCar.lastLapTime > 0 ) && ( normalizedCar.lastLapTime < IRSDK.normalizedData.fastestLapTime ) )
								{
									IRSDK.normalizedData.fastestLapTime = normalizedCar.lastLapTime;

									message = $"#{playerNormalizedCar.carNumber} {playerNormalizedCar.abbrevName} vs. #{normalizedCar.carNumber} {normalizedCar.abbrevName} " + Program.GetTimeString( normalizedCar.lastLapTime, true );

									for ( var i = 0; i < IRSDK.normalizedSession.numCheckpoints; i++ )
									{
										IRSDK.normalizedData.fastestLapSpeedCheckpoints[ i ] = normalizedCar.speedCheckpoints[ i ];
									}
								}
							}
						}

						var speed = playerNormalizedCar.speedInMetersPerSecond;

						var offset = ( playerNormalizedCar.checkpointIdx + IRSDK.normalizedSession.numCheckpoints - ( numVectors / 2 ) ) % IRSDK.normalizedSession.numCheckpoints;

						Vector3? holdVector = null;

						for ( var i = 0; i < numVectors; i++ )
						{
							drawVectorListA[ i ] = new Vector3( i * ( Settings.overlay.trainerSize.x / numVectors ), ( -( Settings.overlay.trainerSize.y / 2 ) + Settings.overlay.trainerSpeedScale * ( IRSDK.normalizedData.fastestLapSpeedCheckpoints[ offset ] - speed ) ), 0 );

							drawVectorListA[ i ].y = Math.Clamp( drawVectorListA[ i ].y, -Settings.overlay.trainerSize.y, 0 );

							if ( holdVector != null )
							{
								drawVectorListB[ i ] = holdVector;
							}
							else
							{
								drawVectorListB[ i ] = new Vector3( i * ( Settings.overlay.trainerSize.x / numVectors ), ( -( Settings.overlay.trainerSize.y / 2 ) + Settings.overlay.trainerSpeedScale * ( playerNormalizedCar.speedCheckpoints[ offset ] - speed ) ), 0 );
							}

							drawVectorListB[ i ].y = Math.Clamp( drawVectorListB[ i ].y, -Settings.overlay.trainerSize.y, 0 );

							if ( offset == playerNormalizedCar.checkpointIdx )
							{
								holdVector = drawVectorListB[ i ];
							}

							offset = ( offset + 1 ) % IRSDK.normalizedSession.numCheckpoints;
						}
					}
				}
			}
		}
	}
}

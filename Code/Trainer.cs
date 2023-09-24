
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public static class Trainer
	{
		public const int NumLists = 7;
		public const int NumVectors = 40;

		public static NormalizedCar[] fastestCars = new NormalizedCar[ NumLists ];

		public static Vector3[][] drawVectorList = new Vector3[ NumLists ][];

		public static string message = string.Empty;

		public static void Initialize()
		{
			for ( var i = 0; i < drawVectorList.Length; i++ )
			{
				drawVectorList[ i ] = new Vector3[ NumVectors ];
			}
		}

		public static void Update()
		{
			if ( IRSDK.data != null )
			{
				var playerNormalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.data.PlayerCarIdx );

				if ( IRSDK.data.PlayerCarIdx >= 0 )
				{
					if ( playerNormalizedCar != null )
					{
						fastestCars[ 0 ] = playerNormalizedCar;
						fastestCars[ 1 ] = playerNormalizedCar;

						var fastestCarCount = 2;

						foreach ( var normalizedCar in IRSDK.normalizedData.fastestTimeSortedNormalizedCars )
						{
							if ( ( normalizedCar.fastestTime == 0 ) || ( normalizedCar.carIdx == IRSDK.data.PlayerCarIdx ) )
							{
								continue;
							}

							fastestCars[ fastestCarCount++ ] = normalizedCar;

							if ( fastestCarCount == NumLists )
							{
								break;
							}
						}

						var speed = playerNormalizedCar.speedInMetersPerSecond;

						var offset = ( playerNormalizedCar.checkpointIdx + IRSDK.normalizedSession.numCheckpoints - ( NumVectors / 2 ) ) % IRSDK.normalizedSession.numCheckpoints;

						Vector3? holdVector = null;

						for ( var i = 0; i < NumVectors; i++ )
						{
							if ( holdVector != null )
							{
								drawVectorList[ 0 ][ i ] = holdVector;
							}
							else
							{
								drawVectorList[ 0 ][ i ] = new Vector3( i * ( Settings.overlay.trainerSize.x / NumVectors ), ( -( Settings.overlay.trainerSize.y / 2 ) + Settings.overlay.trainerSpeedScale * ( playerNormalizedCar.speedCheckpoints[ offset ] - speed ) ), 0 );
							}

							drawVectorList[ 0 ][ i ].y = Math.Clamp( drawVectorList[ 0 ][ i ].y, -Settings.overlay.trainerSize.y, 0 );

							if ( offset == playerNormalizedCar.checkpointIdx )
							{
								holdVector = drawVectorList[ 0 ][ i ];
							}

							offset = ( offset + 1 ) % IRSDK.normalizedSession.numCheckpoints;
						}

						offset = ( playerNormalizedCar.checkpointIdx + IRSDK.normalizedSession.numCheckpoints - ( NumVectors / 2 ) ) % IRSDK.normalizedSession.numCheckpoints;

						for ( var i = 0; i < NumVectors; i++ )
						{
							for ( var j = 1; j < NumLists; j++ )
							{
								if ( j < fastestCarCount )
								{
									drawVectorList[ j ][ i ] = new Vector3( i * ( Settings.overlay.trainerSize.x / NumVectors ), ( -( Settings.overlay.trainerSize.y / 2 ) + Settings.overlay.trainerSpeedScale * ( fastestCars[ j ].fastestSpeedCheckpoints[ offset ] - speed ) ), 0 );

									drawVectorList[ j ][ i ].y = Math.Clamp( drawVectorList[ j ][ i ].y, -Settings.overlay.trainerSize.y, 0 );
								}
								else
								{
									drawVectorList[ j ][ i ] = Vector3.zero;
								}
							}

							offset = ( offset + 1 ) % IRSDK.normalizedSession.numCheckpoints;
						}
					}
				}
			}
		}
	}
}

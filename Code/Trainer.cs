
using System;
using System.Collections.Generic;

namespace iRacingTVController
{
	public static class Trainer
	{
		public static string message = string.Empty;
		public static string countdown = string.Empty;

		public static void Initialize()
		{
		}

		public static void Update()
		{
			if ( IRSDK.data != null )
			{
				if ( IRSDK.data.PlayerCarIdx >= 0 )
				{
					var playerNormalizedCar = IRSDK.normalizedData.FindNormalizedCarByCarIdx( IRSDK.data.PlayerCarIdx );

					if ( ( playerNormalizedCar != null ) && ( IRSDK.trainerCsvFile != null ) )
					{
						var playerCurrentDistance = IRSDK.data.LapDistPct * IRSDK.normalizedSession.trackLengthInFeet;

						IDictionary<string, object>? recordBehind = null;
						float recordBehindDelta = 0.0f;
						IDictionary<string, object>? recordAhead = null;
						float recordAheadDelta = 0.0f;

						foreach ( var record in IRSDK.trainerCsvFile )
						{
							var recordDistance = record.Key;

							if ( recordDistance < playerCurrentDistance )
							{
								var delta = playerCurrentDistance - recordDistance;

								if ( ( recordBehind == null ) || ( delta < recordBehindDelta ) )
								{
									recordBehind = record.Value;
									recordBehindDelta = delta;
								}

								delta = recordDistance + IRSDK.normalizedSession.trackLengthInFeet - playerCurrentDistance;

								if ( ( recordAhead == null ) || ( delta < recordAheadDelta ) )
								{
									recordAhead = record.Value;
									recordAheadDelta = delta;
								}
							}
							else
							{
								var delta = recordDistance - playerCurrentDistance;

								if ( ( recordAhead == null ) || ( delta < recordAheadDelta ) )
								{
									recordAhead = record.Value;
									recordAheadDelta = delta;
								}

								delta = playerCurrentDistance - ( recordDistance - IRSDK.normalizedSession.trackLengthInFeet );

								if ( ( recordBehind == null ) || ( delta < recordBehindDelta ) )
								{
									recordBehind = record.Value;
									recordBehindDelta = delta;
								}
							}
						}

						if ( ( recordBehind != null ) && ( recordAhead != null ) )
						{
							var recordBehindPct = recordBehindDelta / ( recordBehindDelta + recordAheadDelta );
							var recordAheadPct = recordAheadDelta / ( recordBehindDelta + recordAheadDelta );

							message = (string) recordBehind[ "Message" ];
							countdown = string.Empty;

							if ( int.Parse( (string) recordAhead[ "Countdown" ] ) == 1 )
							{
								if ( playerNormalizedCar.speedInMetersPerSecond > 2.0f )
								{
									var timeToDistance = recordAheadDelta / ( playerNormalizedCar.speedInMetersPerSecond * 3.28084f );

									if ( timeToDistance < 1.5f )
									{
										countdown = Math.Ceiling( timeToDistance * 2.0f ).ToString();
									}
								}
							}
						}
					}
					else
					{
						message = "Trainer can't run.";
						countdown = string.Empty;
					}
				}
			}
		}
	}
}

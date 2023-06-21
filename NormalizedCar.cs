
using System;
using System.IO;
using System.Text.RegularExpressions;

using irsdkSharp.Serialization.Enums.Fastest;
using irsdkSharp.Serialization.Models.Session.DriverInfo;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public class NormalizedCar
	{
		public const int MAX_NUM_CHECKPOINTS = 300;

		public int carIdx = 0;
		public int driverIdx = 0;

		public string userName = string.Empty;
		public string abbrevName = string.Empty;

		public string carNumber = string.Empty;
		public int carNumberRaw = 0;

		public Color classColor = Color.white;

		public bool includeInLeaderboard = false;
		public bool hasCrossedStartLine = false;
		public bool isOnPitRoad = false;
		public bool isOutOfCar = false;

		public int officialPosition = 0;
		public int leaderboardPosition = 0;

		public float lapDistPctDelta = 0;
		public float lapDistPct = 0;

		public int lapPositionErrorCount = 0;
		public float lapPosition = 0;
		public float lapPositionRelativeToLeader = 0;

		public int checkpointIdx = 0;
		public double[] checkpoints = new double[ MAX_NUM_CHECKPOINTS ];

		public float f2Time = 0;
		public float checkpointTime = 0;

		public int qualifyingPosition = 0;
		public float qualifyingTime = 0;

		public float attackingHeat = 0;
		public float defendingHeat = 0;

		public float distanceToCarInFrontInMeters = 0;
		public float distanceToCarBehindInMeters = 0;

		public float distanceMovedInMeters = 0;
		public float speedInMetersPerSecond = 0;

		public string carNumberTextureUrl = string.Empty;
		public string carTextureUrl = string.Empty;
		public string helmetTextureUrl = string.Empty;

		public bool wasVisibleOnLeaderboard = false;
		public Vector2 placePosition = Vector2.zero;

		public NormalizedCar( int carIdx )
		{
			this.carIdx = carIdx;

			Reset();
		}

		public void Reset()
		{
			driverIdx = -1;

			userName = string.Empty;
			abbrevName = string.Empty;

			carNumber = string.Empty;
			carNumberRaw = 0;

			classColor = Color.white;

			includeInLeaderboard = false;
			hasCrossedStartLine = false;
			isOnPitRoad = false;
			isOutOfCar = false;

			officialPosition = int.MaxValue;
			leaderboardPosition = int.MaxValue;

			lapDistPctDelta = 0;
			lapDistPct = 0;

			lapPositionErrorCount = 0;
			lapPosition = 0;
			lapPositionRelativeToLeader = 0;

			checkpointIdx = -1;

			f2Time = 0;
			checkpointTime = 0;

			qualifyingPosition = int.MaxValue;
			qualifyingTime = 0;

			attackingHeat = 0;
			defendingHeat = 0;

			distanceToCarInFrontInMeters = float.MaxValue;
			distanceToCarBehindInMeters = float.MaxValue;

			distanceMovedInMeters = 0;
			speedInMetersPerSecond = 0;

			carNumberTextureUrl = string.Empty;
			carTextureUrl = string.Empty;

			placePosition = Vector2.zero;
			wasVisibleOnLeaderboard = false;

			for ( var i = 0; i < checkpoints.Length; i++ )
			{
				checkpoints[ i ] = 0;
			}
		}

		public void SessionChange()
		{
			if ( IRSDK.data == null )
			{
				return;
			}

			var car = IRSDK.data.Cars[ carIdx ];

			hasCrossedStartLine = false;

			lapPositionErrorCount = 0;
			lapPosition = 0;

			lapDistPct = Math.Max( 0, car.CarIdxLapDistPct );

			checkpointIdx = -1;

			for ( var i = 0; i < checkpoints.Length; i++ )
			{
				checkpoints[ i ] = 0;
			}
		}

		public void SessionUpdate()
		{
			if ( IRSDK.session == null )
			{
				return;
			}

			if ( driverIdx == -1 )
			{
				includeInLeaderboard = false;

				DriverModel? driver = null;

				for ( var driverIdx = 0; driverIdx < IRSDK.session.DriverInfo.Drivers.Count; driverIdx++ )
				{
					driver = IRSDK.session.DriverInfo.Drivers[ driverIdx ];

					if ( driver.CarIdx == carIdx )
					{
						this.driverIdx = driverIdx;
						break;
					}
				}

				if ( ( driver != null ) && ( driverIdx != -1 ) )
				{
					userName = Regex.Replace( driver.UserName, @"[\d]", string.Empty );

					GenerateAbbrevName( false );

					carNumber = driver.CarNumber;
					carNumberRaw = driver.CarNumberRaw;

					classColor = new Color( driver.CarClassColor[ 2.. ] );

					includeInLeaderboard = ( driver.IsSpectator == 0 ) && ( driver.CarIsPaceCar == 0 );

					if ( includeInLeaderboard )
					{
						foreach ( var session in IRSDK.session.SessionInfo.Sessions )
						{
							if ( ( session.SessionName == "QUALIFY" ) && ( session.ResultsPositions != null ) )
							{
								foreach ( var position in session.ResultsPositions )
								{
									if ( position.CarIdx == carIdx )
									{
										qualifyingPosition = position.Position;
										qualifyingTime = position.Time;
										break;
									}
								}
							}
						}

						var numberDesignMatch = Regex.Match( driver.CarNumberDesignStr, "(\\d+),(\\d+),(.{6}),(.{6}),(.{6})" );

						if ( numberDesignMatch.Success )
						{
							var settings = Settings.combined.imageSettingsDataDictionary[ "CarNumber" ];

							var colorA = numberDesignMatch.Groups[ 3 ].Value;
							var colorB = numberDesignMatch.Groups[ 4 ].Value;
							var colorC = numberDesignMatch.Groups[ 5 ].Value;

							var pattern = int.Parse( numberDesignMatch.Groups[ 1 ].Value );
							var slant = int.Parse( numberDesignMatch.Groups[ 2 ].Value );

							if ( Settings.combined.carNumberOverrideEnabled )
							{
								colorA = Settings.combined.carNumberColorOverrideA.ToString();
								colorB = Settings.combined.carNumberColorOverrideB.ToString();
								colorC = Settings.combined.carNumberColorOverrideC.ToString();

								pattern = Settings.combined.carNumberPatternOverride;
								slant = Settings.combined.carNumberSlantOverride;
							}

							carNumberTextureUrl = $"http://localhost:32034/pk_number.png?size={settings.size.y}&view=0&number={carNumber}&numPat={pattern}&numCol={colorA},{colorB},{colorC}&numSlnt={slant}";
						}

						var carDesignMatch = Regex.Match( driver.CarDesignStr, "(\\d+),(.{6}),(.{6}),(.{6}),?(.{6})?" );

						if ( numberDesignMatch.Success && carDesignMatch.Success )
						{
							var licColor = driver.LicColor[ 2.. ];
							var carPath = driver.CarPath.Replace( " ", "%5C" );
							var customCarTgaFilePath = $"{Settings.combined.iracingCustomPaintsDirectory}\\{driver.CarPath}\\car_{driver.UserID}.tga".Replace( " ", "%5C" );

							if ( !File.Exists( customCarTgaFilePath ) )
							{
								customCarTgaFilePath = string.Empty;
							}

							carTextureUrl = $"http://localhost:32034/pk_car.png?size=2&view=1&licCol={licColor}&club={driver.ClubID}&sponsors={driver.CarSponsor_1},{driver.CarSponsor_2}&numPat={numberDesignMatch.Groups[ 1 ].Value}&numCol={numberDesignMatch.Groups[ 3 ].Value},{numberDesignMatch.Groups[ 4 ].Value},{numberDesignMatch.Groups[ 5 ].Value}&numSlnt={numberDesignMatch.Groups[ 2 ].Value}&number={carNumber}&carPath={carPath}&carPat={carDesignMatch.Groups[ 1 ].Value}&carCol={carDesignMatch.Groups[ 2 ].Value},{carDesignMatch.Groups[ 3 ].Value},{carDesignMatch.Groups[ 4 ].Value}&carRimType=2&carRimCol={carDesignMatch.Groups[ 5 ].Value}&carCustPaint={customCarTgaFilePath}";
						}

						var helmetDesignMatch = Regex.Match( driver.HelmetDesignStr, "(\\d+),(.{6}),(.{6}),(.{6})" );

						if ( helmetDesignMatch.Success )
						{
							var licColor = driver.LicColor[ 2.. ];
							var helmetType = driver.HelmetType;
							var customHelmetTgaFileName = $"{Settings.combined.iracingCustomPaintsDirectory}\\helmet_{driver.UserID}.tga".Replace( " ", "%5C" );

							if ( !File.Exists( customHelmetTgaFileName ) )
							{
								customHelmetTgaFileName = string.Empty;
							}

							helmetTextureUrl = $"http://localhost:32034/pk_helmet.png?size=7&hlmtPat={helmetDesignMatch.Groups[ 1 ].Value}&licCol={licColor}&hlmtCol={helmetDesignMatch.Groups[ 2 ].Value},{helmetDesignMatch.Groups[ 3 ].Value},{helmetDesignMatch.Groups[ 4 ].Value}&view=1&hlmtType={helmetType}&hlmtCustPaint={customHelmetTgaFileName}";
						}
					}
				}
			}
		}

		public void Update()
		{
			if ( ( IRSDK.data == null ) || !includeInLeaderboard )
			{
				return;
			}

			var car = IRSDK.data.Cars[ carIdx ];

			isOnPitRoad = car.CarIdxOnPitRoad;
			isOutOfCar = car.CarIdxLapDistPct == -1;

			officialPosition = car.CarIdxPosition;
			f2Time = Math.Max( 0, car.CarIdxF2Time );

			if ( !isOutOfCar )
			{
				var newCarIdxLapDistPct = Math.Max( 0, car.CarIdxLapDistPct );

				lapDistPctDelta = newCarIdxLapDistPct - lapDistPct;
				lapDistPct = newCarIdxLapDistPct;

				if ( lapDistPctDelta > 0.5f )
				{
					lapDistPctDelta -= 1.0f;
				}
				else if ( lapDistPctDelta < -0.5f )
				{
					lapDistPctDelta += 1.0f;
				}

				if ( hasCrossedStartLine )
				{
					if ( IRSDK.normalizedData.sessionState <= SessionState.StateParadeLaps )
					{
						hasCrossedStartLine = false;
					}
				}
				else
				{
					if ( ( car.CarIdxLap >= 2 ) || ( ( car.CarIdxLap >= 1 ) && ( newCarIdxLapDistPct > 0 ) && ( newCarIdxLapDistPct < 0.5f ) ) )
					{
						hasCrossedStartLine = true;
					}
				}

				if ( hasCrossedStartLine )
				{
					var newLapPosition = car.CarIdxLap + car.CarIdxLapDistPct - 1;

					lapPositionErrorCount++;

					if ( ( lapPositionErrorCount >= 10 ) || ( Math.Abs( newLapPosition - lapPosition ) < 0.05f ) )
					{
						lapPositionErrorCount = 0;
						lapPosition = newLapPosition;
					}
					else
					{
						lapPosition += lapDistPctDelta;
					}

					var checkpointIdx = (int) Math.Floor( lapDistPct * Settings.combined.leaderboardTelemetryNumberOfCheckpoints ) % Settings.combined.leaderboardTelemetryNumberOfCheckpoints;

					if ( checkpointIdx != this.checkpointIdx )
					{
						this.checkpointIdx = checkpointIdx;

						checkpoints[ checkpointIdx ] = IRSDK.normalizedData.sessionTime;
					}
				}
				else
				{
					lapPosition = 0;
				}

				distanceMovedInMeters = lapDistPctDelta * IRSDK.normalizedSession.trackLengthInMeters;
				speedInMetersPerSecond = distanceMovedInMeters / (float) IRSDK.normalizedData.sessionTimeDelta;
			}
		}

		public void GenerateAbbrevName( bool includeFirstNameInitial )
		{
			var userNameParts = userName.Split( " " );

			if ( userNameParts.Length == 0 )
			{
				abbrevName = "---";
			}
			else if ( userNameParts.Length == 1 )
			{
				abbrevName = userNameParts[ 0 ];
			}
			else if ( includeFirstNameInitial )
			{
				abbrevName = $"{userNameParts[ 0 ][ ..1 ]}. {userNameParts[ ^1 ]}";
			}
			else
			{
				abbrevName = userNameParts[ ^1 ];
			}
		}

		public static Comparison<NormalizedCar> LapPositionComparison = delegate ( NormalizedCar object1, NormalizedCar object2 )
		{
			if ( object1.includeInLeaderboard && object2.includeInLeaderboard )
			{
				if ( object1.lapPosition == object2.lapPosition )
				{
					return object1.carIdx.CompareTo( object2.carIdx );
				}
				else
				{
					return object2.lapPosition.CompareTo( object1.lapPosition );
				}
			}
			else if ( object1.includeInLeaderboard )
			{
				return -1;
			}
			else if ( object2.includeInLeaderboard )
			{
				return 1;
			}
			else
			{
				return 0;
			}
		};

		public static Comparison<NormalizedCar> HeatComparison = delegate ( NormalizedCar object1, NormalizedCar object2 )
		{
			if ( object1.includeInLeaderboard && object2.includeInLeaderboard )
			{
				if ( object1.attackingHeat == object2.attackingHeat )
				{
					return object1.officialPosition.CompareTo( object2.officialPosition );
				}
				else
				{
					return object2.attackingHeat.CompareTo( object1.attackingHeat );
				}
			}
			else if ( object1.includeInLeaderboard )
			{
				return -1;
			}
			else if ( object2.includeInLeaderboard )
			{
				return 1;
			}
			else
			{
				return 0;
			}
		};

		public static Comparison<NormalizedCar> LeaderboardPositionComparison = delegate ( NormalizedCar object1, NormalizedCar object2 )
		{
			if ( object1.includeInLeaderboard && object2.includeInLeaderboard )
			{
				if ( object1.leaderboardPosition == object2.leaderboardPosition )
				{
					return object1.officialPosition.CompareTo( object2.officialPosition );
				}
				else
				{
					return object1.leaderboardPosition.CompareTo( object2.leaderboardPosition );
				}
			}
			else if ( object1.includeInLeaderboard )
			{
				return -1;
			}
			else if ( object2.includeInLeaderboard )
			{
				return 1;
			}
			else
			{
				return 0;
			}
		};
	}
}

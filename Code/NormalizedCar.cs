﻿
using System;
using System.IO;
using System.Linq;
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

		public float outOfCarTimer = 0;

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
		public float heatBias = 0;

		public float distanceToCarInFrontInMeters = 0;
		public float distanceToCarBehindInMeters = 0;

		public float distanceMovedInMeters = 0;
		public float speedInMetersPerSecond = 0;

		public string carNumberTextureUrl = string.Empty;
		public string carTextureUrl = string.Empty;
		public string helmetTextureUrl = string.Empty;
		public string driverTextureUrl = string.Empty;

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

		public void SessionNumberChange()
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

		public void SessionUpdate( bool forceUpdate = false )
		{
			if ( IRSDK.session == null )
			{
				return;
			}

			if ( ( driverIdx == -1 ) || forceUpdate )
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
						var numberDesignMatch = Regex.Match( driver.CarNumberDesignStr, "(\\d+),(\\d+),(.{6}),(.{6}),(.{6})" );

						if ( numberDesignMatch.Success )
						{
							var colorA = numberDesignMatch.Groups[ 3 ].Value;
							var colorB = numberDesignMatch.Groups[ 4 ].Value;
							var colorC = numberDesignMatch.Groups[ 5 ].Value;

							var pattern = int.Parse( numberDesignMatch.Groups[ 1 ].Value );
							var slant = int.Parse( numberDesignMatch.Groups[ 2 ].Value );

							if ( Settings.overlay.carNumberOverrideEnabled )
							{
								colorA = Settings.overlay.carNumberColorA.ToString();
								colorB = Settings.overlay.carNumberColorB.ToString();
								colorC = Settings.overlay.carNumberColorC.ToString();

								pattern = Settings.overlay.carNumberPattern;
								slant = Settings.overlay.carNumberSlant;
							}

							carNumberTextureUrl = $"http://localhost:32034/pk_number.png?size=64&view=0&number={carNumber}&numPat={pattern}&numCol={colorA},{colorB},{colorC}&numSlnt={slant}";
						}

						var carDesignMatch = Regex.Match( driver.CarDesignStr, "(\\d+),(.{6}),(.{6}),(.{6})[,.]?(.{6})?" );

						if ( numberDesignMatch.Success && carDesignMatch.Success )
						{
							var licColor = driver.LicColor[ 2.. ];
							var carPath = driver.CarPath.Replace( " ", "%5C" );
							var customCarTgaFilePath = $"{Settings.editor.iracingCustomPaintsDirectory}\\{driver.CarPath}\\car_{driver.UserID}.tga";

							if ( !File.Exists( customCarTgaFilePath ) )
							{
								customCarTgaFilePath = string.Empty;
							}
							else
							{
								customCarTgaFilePath = customCarTgaFilePath.Replace( " ", "%20" );
							}

							carTextureUrl = $"http://localhost:32034/pk_car.png?size=2&view=1&licCol={licColor}&club={driver.ClubID}&sponsors={driver.CarSponsor_1},{driver.CarSponsor_2}&numPat={numberDesignMatch.Groups[ 1 ].Value}&numCol={numberDesignMatch.Groups[ 3 ].Value},{numberDesignMatch.Groups[ 4 ].Value},{numberDesignMatch.Groups[ 5 ].Value}&numSlnt={numberDesignMatch.Groups[ 2 ].Value}&number={carNumber}&carPath={carPath}&carPat={carDesignMatch.Groups[ 1 ].Value}&carCol={carDesignMatch.Groups[ 2 ].Value},{carDesignMatch.Groups[ 3 ].Value},{carDesignMatch.Groups[ 4 ].Value}&carRimType=2&carRimCol={carDesignMatch.Groups[ 5 ].Value}&carCustPaint={customCarTgaFilePath}";
						}

						var helmetDesignMatch = Regex.Match( driver.HelmetDesignStr, "(\\d+),(.{6}),(.{6}),(.{6})" );

						if ( helmetDesignMatch.Success )
						{
							var licColor = driver.LicColor[ 2.. ];
							var helmetType = driver.HelmetType;
							var customHelmetTgaFileName = $"{Settings.editor.iracingCustomPaintsDirectory}\\helmet_{driver.UserID}.tga";

							if ( !File.Exists( customHelmetTgaFileName ) )
							{
								customHelmetTgaFileName = string.Empty;
							}
							else
							{
								customHelmetTgaFileName = customHelmetTgaFileName.Replace( " ", "%20" );
							}

							helmetTextureUrl = $"http://localhost:32034/pk_helmet.png?size=7&hlmtPat={helmetDesignMatch.Groups[ 1 ].Value}&licCol={licColor}&hlmtCol={helmetDesignMatch.Groups[ 2 ].Value},{helmetDesignMatch.Groups[ 3 ].Value},{helmetDesignMatch.Groups[ 4 ].Value}&view=1&hlmtType={helmetType}&hlmtCustPaint={customHelmetTgaFileName}";
						}

						var driverDesignMatch = Regex.Match( driver.SuitDesignStr, "(\\d+),(.{6}),(.{6}),(.{6})" );

						if ( driverDesignMatch.Success )
						{
							var suitType = driver.BodyType;
							var helmetType = driver.HelmetType;
							var faceType = driver.FaceType;
							var customSuitTgaFileName = $"{Settings.editor.iracingCustomPaintsDirectory}\\suit_{driver.UserID}.tga";

							if ( !File.Exists( customSuitTgaFileName ) )
							{
								customSuitTgaFileName = string.Empty;
							}
							else
							{
								customSuitTgaFileName = customSuitTgaFileName.Replace( " ", "%20" );
							}

							driverTextureUrl = $"http://localhost:32034/pk_body.png?size=1&view=2&bodyType={suitType}&suitPat={driverDesignMatch.Groups[ 1 ].Value}&suitCol={driverDesignMatch.Groups[ 2 ].Value},{driverDesignMatch.Groups[ 3 ].Value},{driverDesignMatch.Groups[ 4 ].Value}&hlmtType={helmetType}&hlmtPat={helmetDesignMatch.Groups[ 1 ].Value}&hlmtCol={helmetDesignMatch.Groups[ 2 ].Value},{helmetDesignMatch.Groups[ 3 ].Value},{helmetDesignMatch.Groups[ 4 ].Value}&faceType={faceType}&suitCustPaint={customSuitTgaFileName}";
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

			var newCarIdxLapDistPct = Math.Max( 0, car.CarIdxLapDistPct );

			if ( isOutOfCar )
			{
				outOfCarTimer += Program.deltaTime;

				lapDistPctDelta *= 0.99f;
				lapDistPct += lapDistPctDelta;
				lapPosition += lapDistPctDelta;

				if ( lapDistPct >= 1.0f )
				{
					lapDistPct -= 1.0f;
				}
				else if ( lapDistPct < 0.0f )
				{
					lapDistPct += 1.0f;
				}
			}
			else
			{
				outOfCarTimer = 0;

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
				else if ( !isOnPitRoad )
				{
					if ( ( car.CarIdxLap >= 2 ) || ( ( car.CarIdxLap >= 1 ) && ( newCarIdxLapDistPct > 0 ) && ( newCarIdxLapDistPct < 0.5f ) ) )
					{
						hasCrossedStartLine = true;
					}
				}

				if ( !IRSDK.normalizedSession.isInRaceSession || hasCrossedStartLine )
				{
					var newLapPosition = car.CarIdxLap + newCarIdxLapDistPct - 1;

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
				}
				else
				{
					lapPosition = 0 - qualifyingPosition / 200.0f;
				}
			}

			var checkpointIdx = (int) Math.Max( 0, Math.Floor( lapDistPct * Settings.overlay.telemetryNumberOfCheckpoints ) ) % Settings.overlay.telemetryNumberOfCheckpoints;

			if ( checkpointIdx != this.checkpointIdx )
			{
				this.checkpointIdx = checkpointIdx;

				checkpoints[ checkpointIdx ] = IRSDK.normalizedData.sessionTime;
			}

			distanceMovedInMeters = lapDistPctDelta * IRSDK.normalizedSession.trackLengthInMeters;
			speedInMetersPerSecond = distanceMovedInMeters / (float) IRSDK.normalizedData.sessionTimeDelta;
		}

		public void GenerateAbbrevName( bool includeFirstNameInitial )
		{
			var userNameParts = userName.Split( " " );

			if ( ( userNameParts.Length == 0 ) || ( userNameParts[ 0 ] == string.Empty ) )
			{
				abbrevName = "---";

				return;
			}

			var userNameIndex = userNameParts.Length - 1;

			abbrevName = userNameParts[ userNameIndex ];

			var suffixList = Settings.editor.iracingDriverNamesSuffixes.Split( "," ).ToList().Select( s => s.Trim().ToLower() ).ToList();

			if ( suffixList.Contains( abbrevName.ToLower() ) )
			{
				userNameIndex--;

				if ( userNameIndex >= 0 )
				{
					abbrevName = userNameParts[ userNameIndex ];
				}
			}

			if ( Settings.editor.iracingDriverNameCapitalizationOption == 1 )
			{
				if ( abbrevName == abbrevName.ToUpper() )
				{
					abbrevName = $"{abbrevName[ 0 ].ToString().ToUpper()}{abbrevName[ 1.. ].ToLower()}";
				}
			}
			else if ( Settings.editor.iracingDriverNameCapitalizationOption == 2 )
			{
				abbrevName = abbrevName.ToUpper();
			}

			if ( includeFirstNameInitial )
			{
				if ( userNameIndex >= 1 )
				{
					abbrevName = $"{userNameParts[ 0 ][ ..1 ].ToUpper()}. {abbrevName}";
				}
			}
		}

		public static Comparison<NormalizedCar> AbsoluteLapPositionComparison = delegate ( NormalizedCar object1, NormalizedCar object2 )
		{
			int result = 0;

			if ( object1.includeInLeaderboard && object2.includeInLeaderboard )
			{
				if ( object1.lapPosition == object2.lapPosition )
				{
					result = object1.carIdx.CompareTo( object2.carIdx );
				}
				else
				{
					result = object2.lapPosition.CompareTo( object1.lapPosition );
				}
			}
			else if ( object1.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( object2.includeInLeaderboard )
			{
				result = 1;
			}

			return result;
		};

		public static Comparison<NormalizedCar> RelativeLapPositionComparison = delegate ( NormalizedCar object1, NormalizedCar object2 )
		{
			int result = 0;

			if ( object1.includeInLeaderboard && object2.includeInLeaderboard )
			{
				var lprl1 = object1.lapPositionRelativeToLeader % 1;
				var lprl2 = object2.lapPositionRelativeToLeader % 1;

				if ( lprl1 == lprl2 )
				{
					result = object1.carIdx.CompareTo( object2.carIdx );
				}
				else
				{
					result = lprl1.CompareTo( lprl2 );
				}
			}
			else if ( object1.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( object2.includeInLeaderboard )
			{
				result = 1;
			}

			return result;
		};

		public static Comparison<NormalizedCar> LeaderboardPositionComparison = delegate ( NormalizedCar object1, NormalizedCar object2 )
		{
			int result = 0;

			if ( object1.includeInLeaderboard && object2.includeInLeaderboard )
			{
				if ( object1.leaderboardPosition == object2.leaderboardPosition )
				{
					result = object1.officialPosition.CompareTo( object2.officialPosition );
				}
				else
				{
					result = object1.leaderboardPosition.CompareTo( object2.leaderboardPosition );
				}
			}
			else if ( object1.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( object2.includeInLeaderboard )
			{
				result = 1;
			}

			return result;
		};
	}
}
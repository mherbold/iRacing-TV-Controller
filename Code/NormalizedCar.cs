
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

using Aydsko.iRacingData.Common;
using Aydsko.iRacingData.Member;
using irsdkSharp.Serialization.Enums.Fastest;
using irsdkSharp.Serialization.Models.Session.DriverInfo;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	public class NormalizedCar
	{
		public int carIdx = 0;
		public int driverIdx = -1;

		public int userId = 0;

		public string userName = string.Empty;
		public string displayedName = string.Empty;

		public string carNumber = string.Empty;
		public int carNumberRaw = 0;

		public int classID = 0;
		public Color classColor = Color.white;
		public CarClass? carClass = null;
		public float carClassEstLapTime = float.MaxValue;

		public bool includeInLeaderboard = false;
		public bool hasCrossedStartLine = false;
		public bool hasCrossedFinishLine = false;
		public bool isOnPitRoad = false;
		public bool wasOnPitRoad = false;
		public bool isOutOfCar = false;
		public bool wasOutOfCar = false;
		public bool isPaceCar = false;
		public bool isSpectator = false;
		public bool isPreferredCar = false;

		public float outOfCarTimer = 0;

		public int leaderboardIndex = 0;
		public int leaderboardClassIndex = 0;

		public int overallPosition = 0;
		public int classPosition = 0;
		public int displayedPosition = 0;
		public int qualifyingPosition = 0;

		public int currentLap = 0;
		public int currentLapLastFrame = 0;

		public int lapCompletedLastFrame = 0;

		public float lastLapTime = 0;
		public float bestLapTime = 0;
		public float bestLapTimeLastFrame = 0;
		public float qualifyingTime = 0;

		public float lapDistPctDelta = 0;
		public float lapDistPct = 0;

		public int lapPositionErrorCount = 0;
		public float lapPosition = 0;
		public float lapPositionPct = 0;
		public float lapDistPctRelativeToLeader = 0;
		public float lapPositionRelativeToClassLeader = 0;

		public float checkpointTime = 0;
		public int checkpointIdx = 0;
		public int checkpointIdxLastFrame = 0;
		public double[] sessionTimeCheckpoints = new double[ NormalizedSession.MaxNumCheckpoints ];
		public double[] sessionTimeCheckpointsLastLap = new double[ NormalizedSession.MaxNumCheckpoints ];
		public float[] speedCheckpoints = new float[ NormalizedSession.MaxNumCheckpoints ];

		public double fastestTime = 0;
		public float[] fastestSpeedCheckpoints = new float[ NormalizedSession.MaxNumCheckpoints ];

		public float heat = 0;
		public float heatBonus = 0;
		public float heatBias = 0;
		public float heatTotal = 0;
		public float heatGapTime = 0;

		public NormalizedCar? normalizedCarInFront = null;
		public NormalizedCar? normalizedCarBehind = null;

		public NormalizedCar? normalizedCarForTelemetry = null;

		public float distanceToCarInFrontInMeters = 0;
		public float distanceToCarBehindInMeters = 0;

		public int carIdxInFrontLastFrame = -1;
		public int carIdxBehindLastFrame = -1;

		public float distanceMovedInMeters = 0;
		public float speedInMetersPerSecond = 0;

		public string carNumberTextureUrl = string.Empty;
		public string carTextureUrl = string.Empty;
		public string helmetTextureUrl = string.Empty;
		public string driverTextureUrl = string.Empty;
		public string memberIdTextureUrl_A = string.Empty;
		public string memberIdTextureUrl_B = string.Empty;
		public string memberIdTextureUrl_C = string.Empty;

		public bool wasVisibleOnLeaderboard = false;
		public Vector2 leaderboardSlotOffset = Vector2.zero;

		public uint sessionFlags = 0;
		public uint sessionFlagsLastFrame = 0;

		public int currentIncidentPoints = 0;
		public int previousIncidentPoints = 0;
		public int activeIncidentPoints = 0;
		public float activeIncidentTimer = 0;

		public int gear = 0;
		public float rpm = 0;
		public int iRating = 0;
		public string license = string.Empty;

		public bool memberProfileRetrieved = false;
		public MemberProfile? memberProfile = null;

		public NormalizedCar( int carIdx )
		{
			this.carIdx = carIdx;

			Reset();
		}

		public void Reset()
		{
			driverIdx = -1;

			userId = 0;

			userName = string.Empty;
			displayedName = string.Empty;

			carNumber = string.Empty;
			carNumberRaw = 0;

			classID = 0;
			classColor = Color.white;

			includeInLeaderboard = false;

			hasCrossedStartLine = false;
			hasCrossedFinishLine = false;

			isOnPitRoad = false;
			wasOnPitRoad = false;

			isOutOfCar = false;
			wasOutOfCar = false;

			isPaceCar = false;
			isSpectator = false;

			leaderboardIndex = 0;
			leaderboardClassIndex = 0;

			overallPosition = 0;
			classPosition = 0;
			displayedPosition = 0;
			qualifyingPosition = 0;

			currentLap = 0;
			currentLapLastFrame = 0;

			lapCompletedLastFrame = 0;

			lastLapTime = 0;
			bestLapTime = 0;
			bestLapTimeLastFrame = 0;
			qualifyingTime = 0;

			lapDistPctDelta = 0;
			lapDistPct = 0;

			lapPositionErrorCount = 0;
			lapPosition = 0;
			lapDistPctRelativeToLeader = 0;

			checkpointIdx = 0;
			checkpointIdxLastFrame = 0;
			checkpointTime = 0;

			fastestTime = 0;

			heat = 0;
			heatBonus = 0;
			heatBias = 0;
			heatTotal = 0;
			heatGapTime = 0;

			normalizedCarInFront = null;
			normalizedCarBehind = null;

			normalizedCarForTelemetry = null;

			distanceToCarInFrontInMeters = float.MaxValue;
			distanceToCarBehindInMeters = float.MaxValue;

			carIdxInFrontLastFrame = -1;
			carIdxBehindLastFrame = -1;

			distanceMovedInMeters = 0;
			speedInMetersPerSecond = 0;

			carNumberTextureUrl = string.Empty;
			carTextureUrl = string.Empty;

			wasVisibleOnLeaderboard = false;
			leaderboardSlotOffset = Vector2.zero;

			sessionFlags = 0;
			sessionFlagsLastFrame = 0;

			currentIncidentPoints = 0;
			previousIncidentPoints = 0;
			activeIncidentPoints = 0;
			activeIncidentTimer = 0;

			gear = 0;
			rpm = 0;
			iRating = 0;
			license = string.Empty;

			memberProfileRetrieved = false;
			memberProfile = null;

			for ( var i = 0; i < sessionTimeCheckpoints.Length; i++ )
			{
				sessionTimeCheckpoints[ i ] = 0;
				sessionTimeCheckpointsLastLap[ i ] = 0;
				speedCheckpoints[ i ] = 0;
				fastestSpeedCheckpoints[ i ] = 0;
			}
		}

		public void SessionNumberChange()
		{
			if ( ( IRSDK.data == null ) || ( IRSDK.session == null ) )
			{
				return;
			}

			var car = IRSDK.data.Cars[ carIdx ];

			hasCrossedStartLine = false;
			hasCrossedFinishLine = false;

			isOnPitRoad = car.CarIdxOnPitRoad;
			wasOnPitRoad = isOnPitRoad;

			isOutOfCar = car.CarIdxLapDistPct == -1;
			wasOutOfCar = isOutOfCar;

			leaderboardIndex = 0;
			leaderboardClassIndex = 0;

			overallPosition = 0;
			classPosition = 0;
			displayedPosition = 0;

			currentLap = 0;
			currentLapLastFrame = 0;

			lapCompletedLastFrame = 0;

			lastLapTime = 0;
			bestLapTime = 0;
			bestLapTimeLastFrame = 0;

			lapDistPctDelta = 0;
			lapDistPct = Math.Max( 0, car.CarIdxLapDistPct );

			lapPositionErrorCount = 0;
			lapPosition = 0;
			lapDistPctRelativeToLeader = 0;

			checkpointIdx = 0;
			checkpointIdxLastFrame = 0;
			checkpointTime = 0;

			fastestTime = 0;

			heat = 0;
			heatBonus = 0;
			heatBias = 0;
			heatTotal = 0;
			heatGapTime = 0;

			normalizedCarInFront = null;
			normalizedCarBehind = null;

			normalizedCarForTelemetry = null;

			distanceToCarInFrontInMeters = 0;
			distanceToCarBehindInMeters = 0;

			distanceMovedInMeters = 0;
			speedInMetersPerSecond = 0;

			wasVisibleOnLeaderboard = false;
			leaderboardSlotOffset = Vector2.zero;

			sessionFlags = 0;
			sessionFlagsLastFrame = 0;

			if ( driverIdx != -1 )
			{
				var driver = IRSDK.session.DriverInfo.Drivers[ driverIdx ];

				currentIncidentPoints = driver.CurDriverIncidentCount;
			}
			else
			{
				currentIncidentPoints = 0;
			}

			previousIncidentPoints = 0;
			activeIncidentPoints = 0;
			activeIncidentTimer = 0;

			gear = 0;
			rpm = 0;

			for ( var i = 0; i < sessionTimeCheckpoints.Length; i++ )
			{
				sessionTimeCheckpoints[ i ] = 0;
				sessionTimeCheckpointsLastLap[ i ] = 0;
				speedCheckpoints[ i ] = 0;
				fastestSpeedCheckpoints[ i ] = 0;
			}
		}

		public void SessionUpdate()
		{
			if ( IRSDK.session == null )
			{
				return;
			}

			DriverModel? driver = null;

			includeInLeaderboard = false;

			var driverFound = false;

			for ( var driverIdx = 0; driverIdx < IRSDK.session.DriverInfo.Drivers.Count; driverIdx++ )
			{
				driver = IRSDK.session.DriverInfo.Drivers[ driverIdx ];

				if ( driver.CarIdx == carIdx )
				{
					this.driverIdx = driverIdx;

					driverFound = true;

					break;
				}
			}

			if ( ( driver == null ) || !driverFound )
			{
				driverIdx = -1;
				return;
			}

			userId = driver.UserID;

			userName = Regex.Replace( driver.UserName, @"[\d]", string.Empty );

			isPaceCar = driver.CarIsPaceCar == 1;
			isSpectator = driver.IsSpectator == 1;

			if ( isPaceCar )
			{
				displayedName = driver.UserName;
			}
			else
			{
				GenerateDisplayedName( false );
			}

			carNumber = driver.CarNumber;
			carNumberRaw = driver.CarNumberRaw;

			classID = driver.CarClassID;
			classColor = new Color( driver.CarClassColor[ 2.. ] );
			carClass = DataApi.GetCarClass( classID );
			carClassEstLapTime = driver.CarClassEstLapTime;

			iRating = driver.IRating;
			license = driver.LicString;

			includeInLeaderboard = !isSpectator && !isPaceCar;

			if ( includeInLeaderboard )
			{
				var numberDesignMatch = Regex.Match( driver.CarNumberDesignStr, @"(\d+),(\d+),(.{6}),(.{6}),(.{6})" );

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

//					LogFile.Write( $"{displayedName}'s car number texture URL = {carNumberTextureUrl}\r\n" );
				}

				var carDesignMatch = Regex.Match( driver.CarDesignStr, @"(\d+),(.{6}),(.{6}),(.{6})[,.]?(.{6})?" );

				if ( numberDesignMatch.Success && carDesignMatch.Success )
				{
					var licColor = driver.LicColor[ 2.. ];
					var carPath = driver.CarPath.Replace( " ", "%5C" );
					var customCarTgaFilePath = $"{Settings.editor.iracingCustomPaintsDirectory}\\{driver.CarPath}\\car_num_{driver.UserID}.tga";
					var showSimStampedNumber = 0;

					if ( ( driver.CarIsAI == 1 ) && ( IRSDK.aiRoster != null ) )
					{
						foreach ( var aiDriver in IRSDK.aiRoster.drivers )
						{
							if ( aiDriver.driverName == driver.UserName )
							{
								customCarTgaFilePath = Path.GetDirectoryName( Settings.editor.iracingCustomPaintsAiRosterFile ) + $"\\{aiDriver.carTgaName}";
								break;
							}
						}
					}

					if ( !File.Exists( customCarTgaFilePath ) )
					{
						customCarTgaFilePath = $"{Settings.editor.iracingCustomPaintsDirectory}\\{driver.CarPath}\\car_{driver.UserID}.tga";
						showSimStampedNumber = 1;

						if ( !File.Exists( customCarTgaFilePath ) )
						{
							customCarTgaFilePath = string.Empty;
						}
					}

					customCarTgaFilePath = customCarTgaFilePath.Replace( " ", "%20" );

					carTextureUrl = $"http://localhost:32034/pk_car.png?size=2&view=1&licCol={licColor}&club={driver.ClubID}&sponsors={driver.CarSponsor_1},{driver.CarSponsor_2}&numShow={showSimStampedNumber}&numPat={numberDesignMatch.Groups[ 1 ].Value}&numCol={numberDesignMatch.Groups[ 3 ].Value},{numberDesignMatch.Groups[ 4 ].Value},{numberDesignMatch.Groups[ 5 ].Value}&numSlnt={numberDesignMatch.Groups[ 2 ].Value}&number={carNumber}&carPath={carPath}&carPat={carDesignMatch.Groups[ 1 ].Value}&carCol={carDesignMatch.Groups[ 2 ].Value},{carDesignMatch.Groups[ 3 ].Value},{carDesignMatch.Groups[ 4 ].Value}&carRimType=2&carRimCol={carDesignMatch.Groups[ 5 ].Value}&carCustPaint={customCarTgaFilePath}";

//					LogFile.Write( $"{displayedName}'s car texture URL = {carTextureUrl}\r\n" );
				}

				var helmetDesignMatch = Regex.Match( driver.HelmetDesignStr, @"(\d+),(.{6}),(.{6}),(.{6})" );

				if ( helmetDesignMatch.Success )
				{
					var licColor = driver.LicColor[ 2.. ];
					var helmetType = driver.HelmetType;
					var customHelmetTgaFileName = $"{Settings.editor.iracingCustomPaintsDirectory}\\helmet_{driver.UserID}.tga";

					if ( !File.Exists( customHelmetTgaFileName ) )
					{
						customHelmetTgaFileName = string.Empty;
					}

					customHelmetTgaFileName = customHelmetTgaFileName.Replace( " ", "%20" );

					helmetTextureUrl = $"http://localhost:32034/pk_helmet.png?size=7&hlmtPat={helmetDesignMatch.Groups[ 1 ].Value}&licCol={licColor}&hlmtCol={helmetDesignMatch.Groups[ 2 ].Value},{helmetDesignMatch.Groups[ 3 ].Value},{helmetDesignMatch.Groups[ 4 ].Value}&view=1&hlmtType={helmetType}&hlmtCustPaint={customHelmetTgaFileName}";

//					LogFile.Write( $"{displayedName}'s helmet texture URL = {helmetTextureUrl}\r\n" );
				}

				var driverDesignMatch = Regex.Match( driver.SuitDesignStr, @"(\d+),(.{6}),(.{6}),(.{6})" );

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

					customSuitTgaFileName = customSuitTgaFileName.Replace( " ", "%20" );

					driverTextureUrl = $"http://localhost:32034/pk_body.png?size=1&view=2&bodyType={suitType}&suitPat={driverDesignMatch.Groups[ 1 ].Value}&suitCol={driverDesignMatch.Groups[ 2 ].Value},{driverDesignMatch.Groups[ 3 ].Value},{driverDesignMatch.Groups[ 4 ].Value}&hlmtType={helmetType}&hlmtPat={helmetDesignMatch.Groups[ 1 ].Value}&hlmtCol={helmetDesignMatch.Groups[ 2 ].Value},{helmetDesignMatch.Groups[ 3 ].Value},{helmetDesignMatch.Groups[ 4 ].Value}&faceType={faceType}&suitCustPaint={customSuitTgaFileName}";

//					LogFile.Write( $"{displayedName}'s driver texture URL = {driverTextureUrl}\r\n" );
				}

				var memberIdTextureFileName = $"{Program.documentsFolder}MemberImages\\A_{driver.UserID}.png";

				if ( File.Exists( memberIdTextureFileName ) )
				{
					memberIdTextureUrl_A = memberIdTextureFileName;
				}
				else
				{
					memberIdTextureUrl_A = string.Empty;
				}

				memberIdTextureFileName = $"{Program.documentsFolder}\\MemberImages\\B_{driver.UserID}.png";

				if ( File.Exists( memberIdTextureFileName ) )
				{
					memberIdTextureUrl_B = memberIdTextureFileName;
				}
				else
				{
					memberIdTextureUrl_B = string.Empty;
				}

				memberIdTextureFileName = $"{Program.documentsFolder}\\MemberImages\\C_{driver.UserID}.png";

				if ( File.Exists( memberIdTextureFileName ) )
				{
					memberIdTextureUrl_C = memberIdTextureFileName;
				}
				else
				{
					memberIdTextureUrl_C = string.Empty;
				}

				if ( driver.CurDriverIncidentCount > currentIncidentPoints )
				{
					if ( activeIncidentPoints == 0 )
					{
						previousIncidentPoints = currentIncidentPoints;
					}

					activeIncidentPoints = Math.Max( IRSDK.normalizedSession.isDirtTrack ? 2 : 4, driver.CurDriverIncidentCount - previousIncidentPoints );
					activeIncidentTimer = 0;
				}

				currentIncidentPoints = driver.CurDriverIncidentCount;
			}
		}

		public void Update()
		{
			if ( ( IRSDK.data == null ) || ( IRSDK.session == null ) )
			{
				return;
			}

			var car = IRSDK.data.Cars[ carIdx ];

			wasOnPitRoad = isOnPitRoad;
			isOnPitRoad = car.CarIdxOnPitRoad;

			wasOutOfCar = isOutOfCar;
			isOutOfCar = car.CarIdxLapDistPct == -1;

			if ( !includeInLeaderboard )
			{
				lapDistPct = Math.Max( 0, car.CarIdxLapDistPct );
				return;
			}

			overallPosition = car.CarIdxPosition;
			classPosition = car.CarIdxClassPosition;

			float newBestLapTime;

			if ( IRSDK.normalizedSession.isInQualifyingSession )
			{
				newBestLapTime = Math.Max( 0, car.CarIdxF2Time );
			}
			else
			{
				newBestLapTime = Math.Max( 0, car.CarIdxBestLapTime );
			}

			if ( newBestLapTime > 0 )
			{
				bestLapTimeLastFrame = bestLapTime;

				bestLapTime = newBestLapTime;

				if ( ( IRSDK.normalizedData.bestLapTime == 0 ) || ( IRSDK.normalizedData.bestLapTime > bestLapTime ) )
				{
					IRSDK.normalizedData.bestLapTime = bestLapTime;
				}
			}

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

				var justCrossedStartFinishLine = car.CarIdxLapCompleted > lapCompletedLastFrame;

				lapCompletedLastFrame = car.CarIdxLapCompleted;

				if ( IRSDK.normalizedData.sessionState < SessionState.StateRacing )
				{
					hasCrossedStartLine = false;
					hasCrossedFinishLine = false;
				}
				else if ( IRSDK.normalizedData.sessionState == SessionState.StateRacing )
				{
					if ( justCrossedStartFinishLine || ( ( car.CarIdxLap == 1 ) && ( lapDistPct <= 0.5f ) ) )
					{
						hasCrossedStartLine = true;
					}

					hasCrossedFinishLine = false;
				}
				else if ( IRSDK.normalizedData.sessionState == SessionState.StateCheckered )
				{
					if ( justCrossedStartFinishLine )
					{
						hasCrossedStartLine = true;
						hasCrossedFinishLine = true;
					}
				}
				else if ( IRSDK.normalizedData.sessionState == SessionState.StateCoolDown )
				{
					hasCrossedStartLine = true;
					hasCrossedFinishLine = true;
				}

				if ( hasCrossedStartLine )
				{
					lapPosition = Math.Max( 0, lapPosition );
				}

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

			if ( IRSDK.normalizedSession.isInRaceSession && !hasCrossedStartLine )
			{
				lapPosition = qualifyingPosition / -200.0f;
			}

			currentLapLastFrame = currentLap;

			currentLap = (int) Math.Floor( lapPosition ) + 1;

			if ( currentLapLastFrame == 0 )
			{
				currentLapLastFrame = currentLap;
			}

			if ( currentLap == ( currentLapLastFrame + 1 ) )
			{
				if ( sessionTimeCheckpoints[ 0 ] > 0 )
				{
					lastLapTime = (float) ( IRSDK.normalizedData.sessionTime - sessionTimeCheckpoints[ 0 ] );
				}
				else
				{
					lastLapTime = 0;
				}
			}

			distanceMovedInMeters = lapDistPctDelta * IRSDK.normalizedSession.trackLengthInMeters;
			speedInMetersPerSecond = distanceMovedInMeters / (float) IRSDK.normalizedData.sessionTimeDelta;

			checkpointIdxLastFrame = checkpointIdx;

			var targetCheckpointIdx = (int) Math.Max( 0, Math.Floor( lapDistPct * IRSDK.normalizedSession.numCheckpoints ) ) % IRSDK.normalizedSession.numCheckpoints;

			if ( IRSDK.normalizedData.replayFrameNum < IRSDK.normalizedData.replayFrameNumLastFrame )
			{
				for ( var i = 0; i < sessionTimeCheckpoints.Length; i++ )
				{
					sessionTimeCheckpoints[ i ] = 0;
					sessionTimeCheckpointsLastLap[ i ] = 0;
					speedCheckpoints[ i ] = 0;
				}

				checkpointIdx = targetCheckpointIdx;
			}

			if ( checkpointIdx != targetCheckpointIdx )
			{
				if ( ( targetCheckpointIdx < checkpointIdxLastFrame ) && !isOnPitRoad )
				{
					if ( fastestTime > 0 )
					{
						fastestTime += 0.25f;
					}

					if ( speedCheckpoints[ 0 ] != 0 )
					{
						var newFastestTime = IRSDK.normalizedData.sessionTime - sessionTimeCheckpoints[ 0 ];

						if ( ( fastestTime == 0 ) || ( newFastestTime < fastestTime ) )
						{
							fastestTime = newFastestTime;

							for ( var i = 0; i < IRSDK.normalizedSession.numCheckpoints; i++ )
							{
								fastestSpeedCheckpoints[ i ] = speedCheckpoints[ i ];
							}
						}
					}
				}

				var numSteps = targetCheckpointIdx - checkpointIdx;

				if ( numSteps < 0 )
				{
					numSteps += IRSDK.normalizedSession.numCheckpoints;
				}

				if ( numSteps > 10 )
				{
					sessionTimeCheckpoints[ targetCheckpointIdx ] = IRSDK.normalizedData.sessionTime;
					speedCheckpoints[ targetCheckpointIdx ] = speedInMetersPerSecond;
				}
				else
				{
					var startSessionTime = sessionTimeCheckpoints[ checkpointIdx ];
					var startSpeed = speedCheckpoints[ checkpointIdx ];

					var stepSize = 1f / numSteps;

					var nextCheckpointIdx = ( checkpointIdxLastFrame + 1 ) % IRSDK.normalizedSession.numCheckpoints;

					for ( var step = 1; step <= numSteps; step++ )
					{
						var t = stepSize * step;

						sessionTimeCheckpointsLastLap[ nextCheckpointIdx ] = sessionTimeCheckpoints[ nextCheckpointIdx ];
						sessionTimeCheckpoints[ nextCheckpointIdx ] = Program.Lerp( startSessionTime, IRSDK.normalizedData.sessionTime, t );
						speedCheckpoints[ nextCheckpointIdx ] = (float) Program.Lerp( startSpeed, speedInMetersPerSecond, t );

						nextCheckpointIdx++;
					}
				}

				checkpointIdx = targetCheckpointIdx;
			}

			sessionFlagsLastFrame = sessionFlags;
			sessionFlags = (uint) car.CarIdxSessionFlags;

			if ( activeIncidentPoints > 0 )
			{
				activeIncidentTimer += Program.deltaTime;

				if ( activeIncidentTimer > 5 )
				{
					previousIncidentPoints = 0;
					activeIncidentPoints = 0;
					activeIncidentTimer = 0;
				}
			}

			gear = car.CarIdxGear;
			rpm = Math.Max( 0, car.CarIdxRPM );
		}

		public void GenerateDisplayedName( bool multipleDriversHaveTheSameName )
		{
			displayedName = "---";

			var userNameParts = userName.Split( " " );

			if ( ( userNameParts.Length == 0 ) || ( userNameParts[ 0 ] == string.Empty ) )
			{
				return;
			}

			var firstName = userNameParts[ 0 ];
			var lastName = string.Empty;

			if ( userNameParts.Length >= 2 )
			{
				var userNameIndex = userNameParts.Length - 1;

				lastName = userNameParts[ userNameIndex ];

				var suffixList = Settings.editor.iracingDriverNamesSuffixes.Split( "," ).ToList().Select( s => s.Trim().ToLower() ).ToList();

				if ( suffixList.Contains( lastName.ToLower() ) )
				{
					if ( userNameIndex >= 2 )
					{
						lastName = userNameParts[ userNameIndex - 1 ] + " " + lastName;
					}
					else
					{
						lastName = string.Empty;
					}
				}
			}

			switch ( Settings.editor.iracingDriverNameCapitalizationOption )
			{
				case 1:

					if ( firstName.Length >= 2 )
					{
						if ( firstName == firstName.ToUpper() )
						{
							firstName = $"{firstName[ 0 ].ToString().ToUpper()}{firstName[ 1.. ].ToLower()}";
						}
					}

					if ( lastName.Length >= 2 )
					{
						if ( lastName == lastName.ToUpper() )
						{
							lastName = $"{lastName[ 0 ].ToString().ToUpper()}{lastName[ 1.. ].ToLower()}";
						}
					}

					break;

				case 2:

					firstName = firstName.ToUpper();
					lastName = lastName.ToUpper();
					break;
			}

			var option = Settings.editor.iracingDriverNameFormatOption;

			if ( multipleDriversHaveTheSameName )
			{
				switch ( option )
				{
					case 0: option = 6; break;
					case 1: option = 2; break;
					case 3: option = 7; break;
					case 4: option = 5; break;
				}
			}

			switch ( option )
			{
				case 0:

					if ( lastName != string.Empty )
					{
						displayedName = lastName;
					}
					else if ( firstName != string.Empty )
					{
						displayedName = firstName;
					}

					break;

				case 1:

					if ( lastName != string.Empty )
					{
						displayedName = lastName.Substring( 0, Math.Min( 3, lastName.Length ) );
					}
					else if ( firstName != string.Empty )
					{
						displayedName = firstName.Substring( 0, Math.Min( 3, firstName.Length ) );
					}

					break;

				case 2:

					if ( ( firstName != string.Empty ) && ( lastName != string.Empty ) )
					{
						displayedName = $"{firstName[ 0 ]} {lastName.Substring( 0, Math.Min( 3, lastName.Length ) )}";
					}
					else if ( lastName != string.Empty )
					{
						displayedName = lastName.Substring( 0, Math.Min( 3, lastName.Length ) );
					}
					else if ( firstName != string.Empty )
					{
						displayedName = firstName.Substring( 0, Math.Min( 3, firstName.Length ) );
					}

					break;

				case 3:

					if ( firstName != string.Empty )
					{
						displayedName = firstName;
					}
					else if ( lastName != string.Empty )
					{
						displayedName = lastName;
					}

					break;

				case 4:

					if ( firstName != string.Empty )
					{
						displayedName = firstName.Substring( 0, Math.Min( 3, firstName.Length ) );
					}
					else if ( lastName != string.Empty )
					{
						displayedName = lastName.Substring( 0, Math.Min( 3, lastName.Length ) );
					}

					break;

				case 5:

					if ( ( firstName != string.Empty ) && ( lastName != string.Empty ) )
					{
						displayedName = $"{firstName.Substring( 0, Math.Min( 3, firstName.Length ) )} {lastName[ 0 ]}";
					}
					else if ( firstName != string.Empty )
					{
						displayedName = firstName.Substring( 0, Math.Min( 3, firstName.Length ) );
					}
					else if ( lastName != string.Empty )
					{
						displayedName = lastName.Substring( 0, Math.Min( 3, lastName.Length ) );
					}

					break;

				case 6:

					if ( ( firstName != string.Empty ) && ( lastName != string.Empty ) )
					{
						displayedName = $"{firstName[ 0 ]}. {lastName}";
					}
					else if ( lastName != string.Empty )
					{
						displayedName = lastName;
					}
					else if ( firstName != string.Empty )
					{
						displayedName = firstName;
					}

					break;

				case 7:

					if ( ( firstName != string.Empty ) && ( lastName != string.Empty ) )
					{
						displayedName = $"{firstName} {lastName[ 0 ]}.";
					}
					else if ( firstName != string.Empty )
					{
						displayedName = firstName;
					}
					else if ( lastName != string.Empty )
					{
						displayedName = lastName;
					}

					break;

				case 8:

					if ( ( firstName != string.Empty ) && ( lastName != string.Empty ) )
					{
						displayedName = $"{lastName}, {firstName[ 0 ]}";
					}
					else if ( lastName != string.Empty )
					{
						displayedName = lastName;
					}
					else if ( firstName != string.Empty )
					{
						displayedName = firstName;
					}

					break;

				case 9:

					if ( ( firstName != string.Empty ) && ( lastName != string.Empty ) )
					{
						displayedName = $"{firstName} {lastName}";
					}
					else if ( lastName != string.Empty )
					{
						displayedName = lastName;
					}
					else if ( firstName != string.Empty )
					{
						displayedName = firstName;
					}

					break;

			}
		}

		public static Comparison<NormalizedCar> BestLapTimeComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				if ( a.bestLapTime == b.bestLapTime )
				{
					result = a.carIdx.CompareTo( b.carIdx );
				}
				else if ( a.bestLapTime == 0 )
				{
					result = 1;
				}
				else if ( b.bestLapTime == 0 )
				{
					result = -1;
				}
				else
				{
					result = a.bestLapTime.CompareTo( b.bestLapTime );
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};

		public static Comparison<NormalizedCar> QualifyingPositionComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				if ( a.qualifyingPosition == b.qualifyingPosition )
				{
					result = a.carIdx.CompareTo( b.carIdx );
				}
				else
				{
					result = a.qualifyingPosition.CompareTo( b.qualifyingPosition );
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};

		public static Comparison<NormalizedCar> OverallPositionComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				if ( ( a.overallPosition >= 1 ) && ( b.overallPosition >= 1 ) )
				{
					if ( a.overallPosition == b.overallPosition )
					{
						result = a.carIdx.CompareTo( b.carIdx );
					}
					else
					{
						result = a.overallPosition.CompareTo( b.overallPosition );
					}
				}
				else if ( a.overallPosition >= 1 )
				{
					result = -1;
				}
				else if ( b.overallPosition >= 1 )
				{
					result = 1;
				}
				else
				{
					result = a.carIdx.CompareTo( b.carIdx );
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};

		public static Comparison<NormalizedCar> LapPositionComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.hasCrossedFinishLine && b.hasCrossedFinishLine )
			{
				return OverallPositionComparison( a, b );
			}

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				if ( a.lapPosition == b.lapPosition )
				{
					result = a.carIdx.CompareTo( b.carIdx );
				}
				else
				{
					result = b.lapPosition.CompareTo( a.lapPosition );
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};

		public static Comparison<NormalizedCar> LeaderboardIndexComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				if ( a.leaderboardIndex == b.leaderboardIndex )
				{
					result = a.carIdx.CompareTo( b.carIdx );
				}
				else
				{
					result = a.leaderboardIndex.CompareTo( b.leaderboardIndex );
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};

		public static Comparison<NormalizedCar> ClassLeaderboardIndexComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				if ( a.classID == b.classID )
				{
					if ( a.leaderboardIndex == b.leaderboardIndex )
					{
						result = a.carIdx.CompareTo( b.carIdx );
					}
					else
					{
						result = a.leaderboardIndex.CompareTo( b.leaderboardIndex );
					}
				}
				else if ( ( a.carClass != null ) && ( b.carClass != null ) )
				{
					if ( a.carClass.RelativeSpeed == b.carClass.RelativeSpeed )
					{
						result = a.classID.CompareTo( b.classID );
					}
					else
					{
						result = b.carClass.RelativeSpeed.CompareTo( a.carClass.RelativeSpeed );
					}
				}
				else
				{
					result = a.classID.CompareTo( b.classID );
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};

		public static Comparison<NormalizedCar> RelativeLapPositionComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				var lprl1 = a.lapDistPctRelativeToLeader % 1;
				var lprl2 = b.lapDistPctRelativeToLeader % 1;

				if ( lprl1 == lprl2 )
				{
					result = a.carIdx.CompareTo( b.carIdx );
				}
				else
				{
					result = lprl1.CompareTo( lprl2 );
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};

		public static Comparison<NormalizedCar> FastestTimeComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				if ( a.fastestTime == b.fastestTime )
				{
					result = a.carIdx.CompareTo( b.carIdx );
				}
				else
				{
					result = a.fastestTime.CompareTo( b.fastestTime );
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};

		public static Comparison<NormalizedCar> CarNumberComparison = delegate ( NormalizedCar a, NormalizedCar b )
		{
			int result;

			if ( a.includeInLeaderboard && b.includeInLeaderboard )
			{
				if ( a.carNumber == b.carNumber )
				{
					result = a.carIdx.CompareTo( b.carIdx );
				}
				else
				{
					try
					{
						result = int.Parse( a.carNumber ).CompareTo( int.Parse( b.carNumber ) );
					}
					catch
					{
						result = a.carNumber.CompareTo( b.carNumber );
					}
				}
			}
			else if ( a.includeInLeaderboard )
			{
				result = -1;
			}
			else if ( b.includeInLeaderboard )
			{
				result = 1;
			}
			else
			{
				result = a.carIdx.CompareTo( b.carIdx );
			}

			return result;
		};
	}
}

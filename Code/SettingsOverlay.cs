
using System;
using System.IO;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsOverlay
	{
		public const string defaultFilePath = "default overlay settings";

		public string filePath = defaultFilePath;

		public override string ToString()
		{
			return Path.GetFileNameWithoutExtension( filePath );
		}

		[NonSerialized] public const int MaxNumFonts = 4;
		[NonSerialized] public const int MaxNumAnimations = 2;

		public bool showBorders = false;

		public Vector2Int position = new( 0, 0 );
		public Vector2Int size = new( 1920, 1080 );
		public string driverCsvFilePath = string.Empty;
		public string stringsCsvFilePath = string.Empty;
		public string trainerCsvFilePath = string.Empty;

		public bool position_Overridden = false;
		public bool size_Overridden = false;
		public bool driverCsvFilePath_Overridden = false;
		public bool stringsCsvFilePath_Overridden = false;
		public bool trainerCsvFilePath_Overridden = false;

		public string[] fontNames = new string[ MaxNumFonts ];
		public string[] fontPaths = new string[ MaxNumFonts ];

		public bool[] fontNames_Overridden = new bool[ MaxNumFonts ];

		public bool startLightsEnabled = true;
		public Vector2 startLightsPosition = new( 735, -20 );

		public bool startLightsEnabled_Overridden = false;
		public bool startLightsPosition_Overridden = false;

		public bool raceStatusEnabled = true;
		public Vector2 raceStatusPosition = new( 44, 9 );

		public bool raceStatusEnabled_Overridden = false;
		public bool raceStatusPosition_Overridden = false;

		public bool leaderboardEnabled = true;
		public Vector2 leaderboardPosition = new( 44, 244 );
		public Vector2 leaderboardFirstSlotPosition = new( 0, 41 );
		public int leaderboardSlotCount = 19;
		public Vector2 leaderboardSlotSpacing = new( 0, 41 );
		public bool leaderboardSeparateBoards = true;
		public Vector2 leaderboardMultiClassOffset = new( 0, 49 );
		public int leaderboardMultiClassOffsetType = 0;

		public bool leaderboardEnabled_Overridden = false;
		public bool leaderboardPosition_Overridden = false;
		public bool leaderboardFirstSlotPosition_Overridden = false;
		public bool leaderboardSlotCount_Overridden = false;
		public bool leaderboardSlotSpacing_Overridden = false;
		public bool leaderboardSeparateBoards_Overridden = false;
		public bool leaderboardMultiClassOffset_Overridden = false;

		public bool raceResultEnabled = true;
		public Vector2 raceResultPosition = new( 960, 96 );
		public Vector2 raceResultFirstSlotPosition = new( 0, 80 );
		public int raceResultSlotCount = 10;
		public Vector2 raceResultSlotSpacing = new( 0, 64 );
		public float raceResultStartTime = 2;
		public float raceResultInterval = 10;

		public bool raceResultEnabled_Overridden = false;
		public bool raceResultPosition_Overridden = false;
		public bool raceResultFirstSlotPosition_Overridden = false;
		public bool raceResultSlotCount_Overridden = false;
		public bool raceResultSlotSpacing_Overridden = false;
		public bool raceResultStartTime_Overridden = false;
		public bool raceResultInterval_Overridden = false;

		public bool trackMapEnabled = true;
		public bool trackMapReverse = false;
		public Vector2 trackMapPosition = new( 1460, 50 );
		public Vector2 trackMapSize = new( 440, 240 );
		public string trackMapTextureFilePath = Settings.GetRelativePath( Program.documentsFolder + "Assets\\default\\track-map-road.png" );
		public float trackMapLineThickness = 0.025f;
		public Color trackMapLineColor = new( 1.0f, 1.0f, 1.0f, 0.5f );
		public int trackMapStartFinishOffset = 0;

		public bool trackMapEnabled_Overridden = false;
		public bool trackMapReverse_Overridden = false;
		public bool trackMapPosition_Overridden = false;
		public bool trackMapSize_Overridden = false;
		public bool trackMapTextureFilePath_Overridden = false;
		public bool trackMapLineThickness_Overridden = false;
		public bool trackMapLineColor_Overridden = false;
		public bool trackMapStartFinishOffset_Overridden = false;

		public bool pitLaneEnabled = true;
		public Vector2 pitLanePosition = new( 1460, 340 );
		public int pitLaneLength = 440;

		public bool pitLaneEnabled_Overridden = false;
		public bool pitLanePosition_Overridden = false;
		public bool pitLaneLength_Overridden = false;

		public bool voiceOfEnabled = true;
		public Vector2 voiceOfPosition = new( 730, 30 );

		public bool voiceOfEnabled_Overridden = false;
		public bool voiceOfPosition_Overridden = false;

		public bool chyronEnabled = true;
		public bool chyronShowDuringPractice = false;
		public bool chyronShowDuringQualifying = false;
		public bool chyronShowDuringRace = true;
		public Vector2 chyronPosition = new( 1408, 890 );
		public float chyronDelay = 2.0f;

		public bool chyronEnabled_Overridden = false;
		public bool chyronShowDuringPractice_Overridden = false;
		public bool chyronShowDuringQualifying_Overridden = false;
		public bool chyronShowDuringRace_Overridden = false;
		public bool chyronPosition_Overridden = false;
		public bool chyronDelay_Overridden = false;

		public bool battleChyronEnabled = true;
		public Vector2 battleChyronPosition = new( 838, 890 );
		public float battleChyronDistance = 18;
		public float battleChyronDelay = 30;

		public bool battleChyronEnabled_Overridden = false;
		public bool battleChyronPosition_Overridden = false;
		public bool battleChyronDistance_Overridden = false;
		public bool battleChyronDelay_Overridden = false;

		public bool subtitleEnabled = true;
		public Vector2 subtitlePosition = new( 990, 220 );
		public Vector2 subtitleMaxSize = new( 1250, 190 );
		public Vector2Int subtitleTextPadding = new( 24, 12 );

		public bool subtitleEnabled_Overridden = false;
		public bool subtitlePosition_Overridden = false;
		public bool subtitleMaxSize_Overridden = false;
		public bool subtitleTextPadding_Overridden = false;

		public bool carNumberOverrideEnabled = false;
		public Color carNumberColorA = Color.white;
		public Color carNumberColorB = Color.black;
		public Color carNumberColorC = Color.black;
		public int carNumberPattern = 0;
		public int carNumberSlant = 0;

		public bool carNumberOverrideEnabled_Overridden = false;
		public bool carNumberColorA_Overridden = false;
		public bool carNumberColorB_Overridden = false;
		public bool carNumberColorC_Overridden = false;
		public bool carNumberPattern_Overridden = false;
		public bool carNumberSlant_Overridden = false;

		public Color telemetryPitColor = new( 0.875f, 0.816f, 0.137f, 1 );
		public Color telemetryOutColor = new( 0.875f, 0.125f, 0.125f, 1 );
		public bool telemetryIsBetweenCars = true;
		public int telemetryMode = 2;
		public bool telemetryShowAsNegativeNumbers = true;

		public bool telemetryPitColor_Overridden = false;
		public bool telemetryOutColor_Overridden = false;
		public bool telemetryIsBetweenCars_Overridden = false;
		public bool telemetryMode_Overridden = false;
		public bool telemetryShowAsNegativeNumbers_Overridden = false;

		public bool introEnabled = true;
		public Vector2 introLeftPosition = new( 502, 430 );
		public float introLeftScale = 2;
		public Vector2 introRightPosition = new( 1415, 672 );
		public float introRightScale = 2;
		public float introLeftStartTime = 10;
		public float introRightStartTime = 11f;
		public float introStartInterval = 8;
		public int introLeftInAnimationNumber = 5;
		public int introRightInAnimationNumber = 4;
		public int introLeftOutAnimationNumber = 3;
		public int introRightOutAnimationNumber = 2;
		public float introInTime = 2;
		public float introHoldTime = 8;
		public float introOutTime = 1;

		public bool introEnabled_Overridden = false;
		public bool introLeftPosition_Overridden = false;
		public bool introLeftScale_Overridden = false;
		public bool introRightPosition_Overridden = false;
		public bool introRightScale_Overridden = false;
		public bool introStartTime_Overridden = false;
		public bool introStartInterval_Overridden = false;
		public bool introInAnimationNumber_Overridden = false;
		public bool introOutAnimationNumber_Overridden = false;
		public bool introInTime_Overridden = false;
		public bool introHoldTime_Overridden = false;
		public bool introOutTime_Overridden = false;

		public bool hudEnabled = false;
		public Vector2 hudPosition = new( 620, 250 );
		public Vector2 hudSpeechToTextPosition = new( 340, -100 );
		public Vector2 hudSpeechToTextMaxSize = new( 1250, 190 );
		public Vector2Int hudSpeechToTextTextPadding = new( 24, 12 );
		public Vector2 hudLocalWebcamPosition = new( 960, 840 );
		public Vector2 hudLocalWebcamSize = new( 320, 240 );
		public Vector2 hudRemoteWebcamPosition = new( 640, 840 );
		public Vector2 hudRemoteWebcamSize = new( 320, 240 );

		public bool hudEnabled_Overridden = false;
		public bool hudPosition_Overridden = false;
		public bool hudSpeechToTextPosition_Overridden = false;
		public bool hudSpeechToTextMaxSize_Overridden = false;
		public bool hudSpeechToTextTextPadding_Overridden = false;
		public bool hudLocalWebcamPosition_Overridden = false;
		public bool hudLocalWebcamSize_Overridden = false;
		public bool hudRemoteWebcamPosition_Overridden = false;
		public bool hudRemoteWebcamSize_Overridden = false;

		public bool trainerEnabled = false;
		public Vector2 trainerPosition = new( 640, 540 );

		public bool trainerEnabled_Overridden = false;
		public bool trainerPosition_Overridden = false;

		public SerializableDictionary<string, SettingsText> textSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsImage> imageSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsTranslation> translationDictionary = new();
	}
}

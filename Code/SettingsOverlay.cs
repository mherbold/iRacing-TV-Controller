
using System;
using System.IO;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsOverlay
	{
		public string filePath = string.Empty;

		public override string ToString()
		{
			return Path.GetFileNameWithoutExtension( filePath );
		}

		[NonSerialized] public const int MaxNumFonts = 4;
		[NonSerialized] public const int MaxNumAnimations = 2;

		public Vector2Int position = new( 0, 0 );
		public Vector2Int size = new( 1920, 1080 );

		public bool position_Overridden = false;
		public bool size_Overridden = false;

		public string[] fontNames = new string[ MaxNumFonts ];
		public string[] fontPaths = new string[ MaxNumFonts ];

		public bool[] fontNames_Overridden = new bool[ MaxNumFonts ];

		public bool raceStatusEnabled = true;
		public Vector2 raceStatusPosition = new( 44, 9 );

		public bool raceStatusEnabled_Overridden = false;
		public bool raceStatusPosition_Overridden = false;

		public bool leaderboardEnabled = true;
		public Vector2 leaderboardPosition = new( 44, 244 );
		public Vector2 leaderboardFirstSlotPosition = new( 0, 41 );
		public int leaderboardSlotCount = 19;
		public Vector2 leaderboardSlotSpacing = new( 0, 41 );
		public bool leaderboardUseClassColors = true;
		public float leaderboardClassColorStrength = 0.5f;
		public Vector2 leaderboardMultiClassOffset = new( 0, 49 );
		public int leaderboardMultiClassOffsetType = 0;

		public bool leaderboardEnabled_Overridden = false;
		public bool leaderboardPosition_Overridden = false;
		public bool leaderboardFirstSlotPosition_Overridden = false;
		public bool leaderboardSlotCount_Overridden = false;
		public bool leaderboardSlotSpacing_Overridden = false;
		public bool leaderboardUseClassColors_Overridden = false;
		public bool leaderboardClassColorStrength_Overridden = false;
		public bool leaderboardMultiClassOffset_Overridden = false;

		public bool trackMapEnabled = true;
		public bool trackMapReverse = false;
		public Vector2 trackMapPosition = new( 1380, 168 );
		public Vector2 trackMapSize = new( 440, 440 );
		public string trackMapTextureFilePath = Settings.GetRelativePath( Program.documentsFolder + "Assets\\default\\track-map-road.png" );
		public float trackMapLineThickness = 0.025f;
		public Color trackMapLineColor = new( 0.485f, 0.485f, 0.485f, 1.0f );
		public int trackMapStartFinishOffset = 0;

		public bool trackMapEnabled_Overridden = false;
		public bool trackMapReverse_Overridden = false;
		public bool trackMapPosition_Overridden = false;
		public bool trackMapSize_Overridden = false;
		public bool trackMapTextureFilePath_Overridden = false;
		public bool trackMapLineThickness_Overridden = false;
		public bool trackMapLineColor_Overridden = false;
		public bool trackMapStartFinishOffset_Overridden = false;

		public bool voiceOfEnabled = true;
		public Vector2 voiceOfPosition = new( 1920, 41 );

		public bool voiceOfEnabled_Overridden = false;
		public bool voiceOfPosition_Overridden = false;

		public bool subtitleEnabled = true;
		public Vector2 subtitlePosition = new( 1089, 918 );
		public Vector2 subtitleMaxSize = new( 1250, 190 );
		public Color subtitleBackgroundColor = new( 0, 0, 0, 0.9f );
		public Vector2Int subtitleTextPadding = new( 12, 6 );

		public bool subtitleEnabled_Overridden = false;
		public bool subtitlePosition_Overridden = false;
		public bool subtitleMaxSize_Overridden = false;
		public bool subtitleBackgroundColor_Overridden = false;
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
		public int telemetryNumberOfCheckpoints = 150;
		public bool telemetryShowAsNegativeNumbers = true;

		public bool telemetryPitColor_Overridden = false;
		public bool telemetryOutColor_Overridden = false;
		public bool telemetryIsBetweenCars_Overridden = false;
		public bool telemetryMode_Overridden = false;
		public bool telemetryNumberOfCheckpoints_Overridden = false;
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

		public bool startLightsEnabled = true;
		public Vector2 startLightsPosition = new( 903, 130 );

		public bool startLightsEnabled_Overridden = false;
		public bool startLightsPosition_Overridden = false;

		public SerializableDictionary<string, SettingsText> textSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsImage> imageSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsTranslation> translationDictionary = new();
	}
}

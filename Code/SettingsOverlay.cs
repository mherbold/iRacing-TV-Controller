
using System;
using System.Collections.Generic;
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

		public string[] fontPaths = new string[ MaxNumFonts ];

		public bool[] fontNames_Overridden = new bool[ MaxNumFonts ];

		public bool raceStatusEnabled = true;
		public Vector2 raceStatusPosition = new( 44, 9 );

		public bool raceStatusEnabled_Overridden = false;
		public bool raceStatusPosition_Overridden = false;

		public bool leaderboardEnabled = true;
		public Vector2 leaderboardPosition = new( 44, 244 );
		public Vector2 leaderboardFirstPlacePosition = new( 0, 0 );
		public int leaderboardPlaceCount = 20;
		public Vector2 leaderboardPlaceSpacing = new( 0, 41 );
		public bool leaderboardUseClassColors = true;
		public float leaderboardClassColorStrength = 0.5f;

		public bool leaderboardEnabled_Overridden = false;
		public bool leaderboardPosition_Overridden = false;
		public bool leaderboardFirstPlacePosition_Overridden = false;
		public bool leaderboardPlaceCount_Overridden = false;
		public bool leaderboardPlaceSpacing_Overridden = false;
		public bool leaderboardUseClassColors_Overridden = false;
		public bool leaderboardClassColorStrength_Overridden = false;

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

		public bool telemetryPitColor_Overridden = false;
		public bool telemetryOutColor_Overridden = false;
		public bool telemetryIsBetweenCars_Overridden = false;
		public bool telemetryMode_Overridden = false;
		public bool telemetryNumberOfCheckpoints_Overridden = false;

		public bool introEnabled = true;
		public float introStartTime = 10;
		public float introRowInterval = 4;
		public int introAnimationNumber = 1;
		public float introAnimationSpeed = 1;
		public Vector2 introLeftPosition = new( 781, 534 );
		public float introLeftScale = 1.7f;
		public Vector2 introRightPosition = new( 1495, 534 );
		public float introRightScale = 1.7f;

		public bool introEnabled_Overridden = false;
		public bool introStartTime_Overridden = false;
		public bool introRowInterval_Overridden = false;
		public bool introAnimationNumber_Overridden = false;
		public bool introAnimationSpeed_Overridden = false;
		public bool introLeftPosition_Overridden = false;
		public bool introLeftScale_Overridden = false;
		public bool introRightPosition_Overridden = false;
		public bool introRightScale_Overridden = false;

		public SerializableDictionary<string, SettingsText> textSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsImage> imageSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsTranslation> translationDictionary = new();
	}
}

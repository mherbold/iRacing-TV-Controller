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

		public Vector2Int overlayPosition = new( 0, 0 );
		public Vector2Int overlaySize = new( 1920, 1080 );

		public bool overlayPosition_Overridden = false;
		public bool overlaySize_Overridden = false;

		public string[] fontPaths = new string[ MaxNumFonts ];

		public bool[] fontNames_Overridden = new bool[ MaxNumFonts ];

		public bool raceStatusOverlayEnabled = true;
		public Vector2 raceStatusOverlayPosition = new( 44, 9 );

		public bool raceStatusOverlayEnabled_Overridden = false;
		public bool raceStatusOverlayPosition_Overridden = false;

		public bool leaderboardOverlayEnabled = true;
		public Vector2 leaderboardOverlayPosition = new( 44, 244 );
		public Vector2 leaderboardFirstPlacePosition = new( 0, 0 );
		public int leaderboardPlaceCount = 20;
		public Vector2 leaderboardPlaceSpacing = new( 0, 41 );
		public bool leaderboardUseClassColors = true;
		public float leaderboardClassColorStrength = 0.5f;
		public Color leaderboardTelemetryPitColor = new( 0.875f, 0.816f, 0.137f, 1 );
		public Color leaderboardTelemetryOutColor = new( 0.875f, 0.125f, 0.125f, 1 );
		public bool leaderboardTelemetryIsBetweenCars = true;
		public int leaderboardTelemetryMode = 0;
		public int leaderboardTelemetryNumberOfCheckpoints = 150;

		public bool leaderboardOverlayEnabled_Overridden = false;
		public bool leaderboardOverlayPosition_Overridden = false;
		public bool leaderboardFirstPlacePosition_Overridden = false;
		public bool leaderboardPlaceCount_Overridden = false;
		public bool leaderboardPlaceSpacing_Overridden = false;
		public bool leaderboardUseClassColors_Overridden = false;
		public bool leaderboardClassColorStrength_Overridden = false;
		public bool leaderboardTelemetryPitColor_Overridden = false;
		public bool leaderboardTelemetryOutColor_Overridden = false;
		public bool leaderboardTelemetryIsBetweenCars_Overridden = false;
		public bool leaderboardTelemetryMode_Overridden = false;
		public bool leaderboardTelemetryNumberOfCheckpoints_Overridden = false;

		public bool voiceOfOverlayEnabled = true;
		public Vector2 voiceOfOverlayPosition = new( 1920, 41 );

		public bool voiceOfOverlayEnabled_Overridden = false;
		public bool voiceOfOverlayPosition_Overridden = false;

		public bool subtitleOverlayEnabled = true;
		public Vector2 subtitleOverlayPosition = new( 1089, 918 );
		public Vector2 subtitleOverlayMaxSize = new( 1250, 190 );
		public Color subtitleOverlayBackgroundColor = new( 0, 0, 0, 0.9f );
		public Vector2Int subtitleTextPadding = new( 12, 6 );

		public bool subtitleOverlayEnabled_Overridden = false;
		public bool subtitleOverlayPosition_Overridden = false;
		public bool subtitleOverlayMaxSize_Overridden = false;
		public bool subtitleOverlayBackgroundColor_Overridden = false;
		public bool subtitleTextPadding_Overridden = false;

		public bool carNumberOverrideEnabled = false;
		public Color carNumberColorOverrideA = Color.white;
		public Color carNumberColorOverrideB = Color.white;
		public Color carNumberColorOverrideC = Color.white;
		public int carNumberPatternOverride = 0;
		public int carNumberSlantOverride = 0;

		public bool carNumberOverrideEnabled_Overridden = false;
		public bool carNumberColorOverrideA_Overridden = false;
		public bool carNumberColorOverrideB_Overridden = false;
		public bool carNumberColorOverrideC_Overridden = false;
		public bool carNumberPatternOverride_Overridden = false;
		public bool carNumberSlantOverride_Overridden = false;

		public float directorCarLength = 4.91f;
		public float directorHeatFalloff = 20.0f;
		public float directorHeatBias = 0.5f;

		public bool directorCarLength_Overridden = false;
		public bool directorHeatFalloff_Overridden = false;
		public bool directorHeatBias_Overridden = false;

		public string iracingCustomPaintsDirectory = string.Empty;

		public bool iracingCustomPaintsDirectory_Overridden = false;

		public SerializableDictionary<string, SettingsText> textSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsImage> imageSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsTranslation> translationDictionary = new();
	}
}

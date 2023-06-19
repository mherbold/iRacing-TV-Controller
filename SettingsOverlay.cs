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

		public bool leaderboardOverlayEnabled = true;
		public Vector2 leaderboardOverlayPosition = new( 44, 244 );
		public Vector2 leaderboardFirstPlacePosition = new( 0, 0 );
		public int leaderboardPlaceCount = 20;
		public Vector2 leaderboardPlaceSpacing = new( 0, 41 );

		public bool leaderboardOverlayEnabled_Overridden = false;
		public bool leaderboardOverlayPosition_Overridden = false;
		public bool leaderboardFirstPlacePosition_Overridden = false;
		public bool leaderboardPlaceCount_Overridden = false;
		public bool leaderboardPlaceSpacing_Overridden = false;

		public SerializableDictionary<string, SettingsText> textSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsImage> imageSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsTranslation> translationDictionary = new();
	}
}

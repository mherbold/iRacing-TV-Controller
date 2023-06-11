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

		public string[] fontNames = new string[ MaxNumFonts ];

		public bool[] fontNames_Overridden = new bool[ MaxNumFonts ];

		public SerializableDictionary<string, SettingsText> textSettingsDataDictionary = new();
		public SerializableDictionary<string, SettingsImage> imageSettingsDataDictionary = new();
	}
}

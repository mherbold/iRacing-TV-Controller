
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsImage
	{
		public enum ImageType
		{
			None,
			ImageFile,
			SeriesLogo,
			CarNumber,
			Car
		}

		public ImageType imageType = ImageType.None;
		public string filePath = string.Empty;
		public Vector2 position = Vector2.zero;
		public Vector2 size = Vector2.zero;
		public Color tintColor = Color.white;

		public bool imageType_Overridden = false;
		public bool filePath_Overridden = false;
		public bool position_Overridden = false;
		public bool size_Overridden = false;
		public bool tintColor_Overridden = false;
	}
}

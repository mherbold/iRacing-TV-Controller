
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsText
	{
		public enum FontIndex
		{
			None = -1,
			FontA,
			FontB,
			FontC,
			FontD
		};

		public FontIndex fontIndex = FontIndex.FontA;
		public int fontSize = 0;
		public TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
		public Vector2 position = Vector2.zero;
		public Vector2 size = Vector2.zero;
		public Color tintColor = Color.white;

		public bool fontIndex_Overridden = false;
		public bool fontSize_Overridden = false;
		public bool alignment_Overridden = false;
		public bool position_Overridden = false;
		public bool size_Overridden = false;
		public bool tintColor_Overridden = false;
	}
}

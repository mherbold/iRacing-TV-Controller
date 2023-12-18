
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataHud
	{
		public string textLayer1 = string.Empty;
		public Color textLayer1Color = Color.white;

		public string textLayer2 = string.Empty;
		public Color textLayer2Color = Color.white;

		public string textLayer3 = string.Empty;
		public Color textLayer3Color = Color.white;

		public string textLayer4 = string.Empty;
		public Color textLayer4Color = Color.white;

		public string textLayer5 = string.Empty;
		public Color textLayer5Color = Color.white;

		public string textLayer6 = string.Empty;
		public Color textLayer6Color = Color.white;

		public string textLayer7 = string.Empty;
		public Color textLayer7Color = Color.white;

		public string textLayer8 = string.Empty;
		public Color textLayer8Color = Color.white;

		public string speechToText = string.Empty;

		public bool showLeftSpotterIndicator = false;
		public bool showRightSpotterIndicator = false;
	}
}

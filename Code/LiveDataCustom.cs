
using System;
using System.Text.Json.Serialization;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataCustom
	{
		[JsonInclude] public string textLayer1 = string.Empty;
		public Color textLayer1Color = Color.white;

		[JsonInclude] public string textLayer2 = string.Empty;
		public Color textLayer2Color = Color.white;

		[JsonInclude] public string textLayer3 = string.Empty;
		public Color textLayer3Color = Color.white;

		public int carIdx = 0;
	}
}

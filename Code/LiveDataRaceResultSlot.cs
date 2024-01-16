
using System;
using System.Text.Json.Serialization;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataRaceResultSlot
	{
		[JsonInclude] public bool show = false;
		public bool showPreferredCar = false;

		public Vector2 offset = Vector2.zero;

		[JsonInclude] public string textLayer1 = string.Empty;
		public Color textLayer1Color = Color.white;

		[JsonInclude] public string textLayer2 = string.Empty;
		public Color textLayer2Color = Color.white;

		[JsonInclude] public string textLayer3 = string.Empty;
		public Color textLayer3Color = Color.white;

		[JsonInclude] public string textLayer4 = string.Empty;
		public Color textLayer4Color = Color.white;
	}
}

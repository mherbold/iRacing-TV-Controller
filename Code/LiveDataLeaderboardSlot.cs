
using System;
using System.Text.Json.Serialization;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataLeaderboardSlot
	{
		[JsonInclude] public bool show = false;
		public bool showHighlight = false;

		public Vector2 offset = Vector2.zero;

		[JsonInclude] public string positionText = string.Empty;
		public Color positionColor = Color.white;

		[JsonInclude] public string carNumberText = string.Empty;
		public Color carNumberTextColor = Color.white;

		[JsonInclude] public string driverNameText = string.Empty;
		public Color driverNameColor = Color.white;

		[JsonInclude] public string telemetryText = string.Empty;
		public Color telemetryColor = Color.white;

		[JsonInclude] public string speedText = string.Empty;
	}
}

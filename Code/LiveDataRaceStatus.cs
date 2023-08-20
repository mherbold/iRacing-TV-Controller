
using System;
using System.Text.Json.Serialization;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataRaceStatus
	{
		[JsonInclude] public bool showGreenFlag = false;
		[JsonInclude] public bool showYellowFlag = false;
		[JsonInclude] public bool showCheckeredFlag = false;

		[JsonInclude] public string sessionNameText = string.Empty;

		[JsonInclude] public string lapsRemainingText = string.Empty;

		[JsonInclude] public bool showBlackLight = false;
		[JsonInclude] public bool showGreenLight = false;
		[JsonInclude] public bool showWhiteLight = false;
		[JsonInclude] public bool showYellowLight = false;

		[JsonInclude] public string unitsText = string.Empty;

		[JsonInclude] public string currentLapText = string.Empty;
	}
}

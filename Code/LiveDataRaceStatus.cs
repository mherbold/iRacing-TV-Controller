
using System;
using System.Text.Json.Serialization;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataRaceStatus
	{
		[JsonInclude] public bool showBlackLight = false;
		[JsonInclude] public bool showGreenLight = false;
		[JsonInclude] public bool showWhiteLight = false;
		[JsonInclude] public bool showYellowLight = false;

		[JsonInclude] public string textLayer1 = string.Empty;
		[JsonInclude] public string textLayer2 = string.Empty;
		[JsonInclude] public string textLayer3 = string.Empty;
		[JsonInclude] public string textLayer4 = string.Empty;

		[JsonInclude] public bool showGreenFlag = false;
		[JsonInclude] public bool showYellowFlag = false;
		[JsonInclude] public bool showCheckeredFlag = false;
	}
}

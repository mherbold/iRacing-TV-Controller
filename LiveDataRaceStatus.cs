
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataRaceStatus
	{
		public string sessionNameText = string.Empty;

		public string lapsRemainingText = string.Empty;

		public bool showBlackLight = false;
		public bool showGreenLight = false;
		public bool showWhiteLight = false;
		public bool showYellowLight = false;

		public string unitsText = string.Empty;

		public string currentLapText = string.Empty;
	}
}

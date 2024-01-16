
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataControlPanel
	{
		public bool masterOn;
		public bool raceStatusOn;
		public bool leaderboardOn;
		public bool raceResultOn;
		public bool trackMapOn;
		public bool pitLaneOn;
		public bool startLightsOn;
		public bool voiceOfOn;
		public bool chyronOn;
		public bool battleChyronOn;
		public bool subtitlesOn;
		public bool introOn;

		public bool[]? customLayerOn;
	}
}

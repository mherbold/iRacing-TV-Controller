
using System;

namespace iRacingTVController
{
	[Serializable]
	public class SubtitleData
	{
		public int CarIdx = 0;

		public int Index { get; set; } = 0;
		public int SessionNumber { get; set; } = 0;
		public double StartTime { get; set; } = 0;
		public double EndTime { get; set; } = 0;
		public string CarNumber { get; set; } = string.Empty;
		public string Text { get; set; } = string.Empty;
		public bool Ignore { get; set; } = false;

		public string StartTimeFormatted { get { return $"{StartTime:0.00}"; } }
		public string EndTimeFormatted { get { return $"{EndTime:0.00}"; } }
	}
}

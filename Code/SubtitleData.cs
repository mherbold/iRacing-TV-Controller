
using System;

namespace iRacingTVController
{
	[Serializable]
	public class SubtitleData
	{
		public int CarIdx = 0;

		public int SessionNumber { get; set; } = 0;
		public int StartFrame { get; set; } = 0;
		public int EndFrame { get; set; } = 0;
		public string Text { get; set; } = string.Empty;
		public bool Ignore { get; set; } = false;
	}
}

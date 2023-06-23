
using System;

namespace iRacingTVController
{
	[Serializable]
	public class IncidentData
	{
		public int CarIdx = 0;

		public int FrameNumber { get; set; } = 0;
		public string CarNumber { get; set; } = string.Empty;
		public string DriverName { get; set; } = string.Empty;
		public int StartFrame { get; set; } = 0;
		public int EndFrame { get; set; } = 0;
		public bool Ignore { get; set; } = false;
	}
}


using System;

namespace iRacingTVController
{
	[Serializable]
	public class SessionFlagsData
	{
		public int Index { get; set; } = 0;
		public int SessionNumber { get; set; } = 0;
		public double SessionTime { get; set; } = 0;
		public uint SessionFlags { get; set; } = 0;
		public string SessionFlagsAsString { get; set; } = string.Empty;
	}
}

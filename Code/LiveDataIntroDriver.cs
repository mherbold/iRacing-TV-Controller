
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataIntroDriver
	{
		public bool show = false;

		public int carIdx = 0;

		public string positionText = string.Empty;
		public string driverNameText = string.Empty;
		public string qualifyingTimeText = string.Empty;
	}
}

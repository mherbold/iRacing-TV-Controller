
using System;

namespace iRacingTVController
{
	[Serializable]
	public class LiveDataVoiceOf
	{
		public bool show = false;
		public string voiceOfText = string.Empty;
		public string driverNameText = string.Empty;
		public int carIdx = -1;
	}
}

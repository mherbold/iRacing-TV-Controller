using System;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsEditor
	{
		public float positioningSpeedNormal = 0.25f;
		public float positioningSpeedFast = 1.0f;
		public float positioningSpeedSlow = 0.1f;

		public string lastActiveOverlayFilePath = string.Empty;
	}
}

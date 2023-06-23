
using System;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsEditor
	{
		public float positioningSpeedNormal = 0.25f;
		public float positioningSpeedFast = 1.0f;
		public float positioningSpeedSlow = 0.01f;

		public string lastActiveOverlayFilePath = string.Empty;

		public string iracingCustomPaintsDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\iRacing\\paint";
		public float iracingCommandRateLimit = 2.0f;
	}
}

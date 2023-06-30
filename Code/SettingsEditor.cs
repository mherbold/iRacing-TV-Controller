
using System;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsEditor
	{
		public string incidentsScenicCameras = "scenic";
		public string incidentsEditCameras = "far chase, chase, chopper";
		public float incidentsOverlapMergeTime = 10.0f;
		public float incidentsTimeout = 5.0f;

		public string iracingCustomPaintsDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\iRacing\\paint";
		public float iracingCommandRateLimit = 2.0f;

		public float positioningSpeedNormal = 0.25f;
		public float positioningSpeedFast = 1.0f;
		public float positioningSpeedSlow = 0.01f;

		public string lastActiveOverlayFilePath = string.Empty;
		public string lastActiveDirectorFilePath = string.Empty;
	}
}

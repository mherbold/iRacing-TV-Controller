
using System;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsEditor
	{
		public float iracingGeneralCommandRateLimit = 2.0f;

		public string iracingCustomPaintsDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\iRacing\\paint";

		public string iracingDriverNamesSuffixes = "jr, sr, jr., sr.";
		public int iracingDriverNameCapitalizationOption = 0;

		public float editorMousePositioningSpeedNormal = 0.25f;
		public float editorMousePositioningSpeedFast = 1.0f;
		public float editorMousePositioningSpeedSlow = 0.01f;

		public string editorIncidentsScenicCameras = "scenic";
		public string editorIncidentsEditCameras = "far chase, chase, chopper";
		public float editorIncidentsOverlapMergeTime = 10.0f;
		public float editorIncidentsTimeout = 5.0f;

		public string lastActiveOverlayFilePath = string.Empty;
		public string lastActiveDirectorFilePath = string.Empty;
	}
}

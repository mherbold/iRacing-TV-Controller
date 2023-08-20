
using System;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsEditor
	{
		public bool webpageGeneralEnabled = false;
		public string webpageGeneralSourceFolder = Program.documentsFolder + "Assets\\webpage";
		public string webpageGeneralOutputFolder = Program.documentsFolder + "WebPage\\";
		public float webpageGeneralUpdateInterval = 2.0f;

		public string webpageTextTitle = "iRacing-TV Live Telemetry";

		public float iracingGeneralCommandRateLimit = 2.0f;

		public string iracingAccountUsername = string.Empty;
		public string iracingAccountPassword = string.Empty;

		public string iracingCustomPaintsDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\iRacing\\paint";

		public string iracingDriverNamesSuffixes = "jr, sr, jr., sr.";
		public int iracingDriverNameFormatOption = 0;
		public int iracingDriverNameCapitalizationOption = 0;

		public bool editorAlwaysOnTop = false;

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

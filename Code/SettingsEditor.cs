
using System;

using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsEditor
	{
		public bool webpageGeneralEnabled = false;
		public string webpageGeneralSourceFolder = Program.documentsFolder + "Assets\\webpage";
		public string webpageGeneralOutputFolder = Program.documentsFolder + "WebPage\\";

		public string webpageTextTitle = "iRacing-TV Live Telemetry";

		public float iracingGeneralCommandRateLimit = 2.0f;

		public string iracingAccountUsername = string.Empty;
		public string iracingAccountPassword = string.Empty;

		public string iracingCustomPaintsDirectory = Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ) + "\\iRacing\\paint";

		public string iracingDriverNamesSuffixes = "jr, sr, jr., sr., ii, iii";
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

		public bool editorSteamVrEnabled = false;
		public float editorSteamVrWidth = 1.0f;
		public Vector3 editorSteamVrPosition = new( 0.0f, 0.0f, -0.5f );
		public float editorSteamVrCurvature = 0.25f;
		public Guid editorSteamVrControllerGuid = Guid.Empty;

		public bool editorSpeechToTextEnabled = false;
		public string editorSpeechToTextAudioCaptureDeviceId = string.Empty;
		public string editorSpeechToTextCognitiveServiceKey = string.Empty;
		public string editorSpeechToTextCognitiveServiceRegion = string.Empty;
		public string editorSpeechToTextLanguage = "en-US";
		public bool editorSpeechToTextPotatoFilterEnabled = true;

		public bool editorPushToTalkMuteEnabled = false;
		public string editorPushToTalkAudioRenderDeviceId = string.Empty;

		public string editorTriggersSessionChange = string.Empty;

		public bool editorStartupEnableDirector = false;

		public string lastActiveOverlayFilePath = string.Empty;
		public string lastActiveDirectorFilePath = string.Empty;
	}
}

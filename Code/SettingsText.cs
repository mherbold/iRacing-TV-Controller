
using System;
using System.Xml.Serialization;
using static iRacingTVController.Unity;

namespace iRacingTVController
{
	[Serializable]
	public class SettingsText
	{
		public enum FontIndex
		{
			None = -1,
			FontA,
			FontB,
			FontC,
			FontD
		};

		public enum Content // last used = 57
		{
			None = 0,
			Driver_CarNumber = 1,
			Driver_CarBehind_CsvProperty = 44,
			Driver_CarBehind_CarNumber = 40,
			[XmlEnum( Name = "Driver_GapTimeToCarBehind" )]
			Driver_CarBehind_GapTime = 3,
			Driver_CarBehind_Name = 38,
			Driver_CarBehind_Position = 42,
			Driver_CarBehind_UserID = 48,
			Driver_CarBehind_Rating = 57,
			Driver_CarInFront_CarNumber = 41,
			Driver_CarInFront_CsvProperty = 45,
			[XmlEnum( Name = "Driver_GapTimeToCarInFront" )]
			Driver_CarInFront_GapTime = 4,
			Driver_CarInFront_Name = 39,
			Driver_CarInFront_Position = 43,
			Driver_CarInFront_UserID = 47,
			Driver_CarInFront_Rating = 56,
			Driver_CsvProperty = 37,
			Driver_FamilyName = 2,
			Driver_FullName = 51,
			Driver_Gear = 5,
			Driver_GivenName = 6,
			Driver_LapDelta = 7,
			Driver_LapsBehindClassLeader = 8,
			Driver_LapsLed = 32,
			Driver_LapTime_Current = 49,
			Driver_LapTime_LastLap = 50,
			Driver_License = 9,
			Driver_Name = 10,
			Driver_Position = 11,
			Driver_Position_Ordinal = 34,
			Driver_Position_WithP = 33,
			Driver_QualifyLapTime_1 = 52,
			Driver_QualifyLapTime_2 = 53,
			Driver_QualifyLapTime_3 = 54,
			Driver_QualifyLapTime_4 = 55,
			Driver_QualifyPosition = 12,
			Driver_QualifyPosition_Ordinal = 36,
			Driver_QualifyPosition_WithP = 35,
			Driver_QualifyTime = 13,
			Driver_Rating = 14,
			Driver_RPM = 15,
			Driver_Speed = 16,
			Driver_Telemetry = 17,
			Driver_UserID = 46,
			Leaderboard_ClassName = 18,
			Leaderboard_ClassNameShort = 19,
			Player_FuelRemainingInLaps = 20,
			Player_RPM = 21,
			Session_CurrentLap = 22,
			Session_LapsRemaining = 23,
			Session_Name = 24,
			Translation_Gear = 25,
			Translation_License = 26,
			Translation_Rating = 27,
			Translation_RPM = 28,
			Translation_Speed = 29,
			Translation_Units = 30,
			Translation_VoiceOf = 31,
		};

		public FontIndex fontIndex = FontIndex.None;
		public int fontSize = 0;
		public TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
		public Vector2 position = Vector2.zero;
		public Vector2 size = Vector2.zero;
		public Color tintColor = Color.white;
		public bool useClassColors = false;
		public float classColorStrength = 0.5f;
		public bool allowOverflow = true;
		public Content content = Content.None;
		public string csvProperty = string.Empty;

		public bool fontIndex_Overridden = false;
		public bool fontSize_Overridden = false;
		public bool alignment_Overridden = false;
		public bool position_Overridden = false;
		public bool size_Overridden = false;
		public bool tintColor_Overridden = false;
		public bool useClassColors_Overridden = false;
		public bool classColorStrength_Overridden = false;
		public bool allowOverflow_Overridden = false;
		public bool content_Overridden = false;
		public bool csvProperty_Overridden = false;
	}
}

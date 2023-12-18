
using System;

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

		public enum Content
		{
			None,
			Driver_CarNumber,
			Driver_FamilyName,
			Driver_GapTimeToCarBehind,
			Driver_GapTimeToCarInFront,
			Driver_Gear,
			Driver_GivenName,
			Driver_LapDelta,
			Driver_LapsBehindClassLeader,
			Driver_License,
			Driver_Name,
			Driver_Position,
			Driver_QualifyPosition,
			Driver_QualifyTime,
			Driver_Rating,
			Driver_RPM,
			Driver_Speed,
			Driver_Telemetry,
			Leaderboard_ClassName,
			Leaderboard_ClassNameShort,
			Player_FuelRemainingInLaps,
			Player_RPM,
			Session_CurrentLap,
			Session_LapsRemaining,
			Session_Name,
			Translation_Gear,
			Translation_License,
			Translation_Rating,
			Translation_RPM,
			Translation_Speed,
			Translation_Units,
			Translation_VoiceOf,
		};

		public FontIndex fontIndex = FontIndex.FontA;
		public int fontSize = 0;
		public TextAlignmentOptions alignment = TextAlignmentOptions.TopLeft;
		public Vector2 position = Vector2.zero;
		public Vector2 size = Vector2.zero;
		public Color tintColor = Color.white;
		public bool useClassColors = false;
		public float classColorStrength = 0.5f;
		public bool allowOverflow = true;
		public Content content = Content.None;

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
	}
}

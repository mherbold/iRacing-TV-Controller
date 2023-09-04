
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace iRacingTVController
{
	public class NormalizedSession
	{
		public const int MaxNumCheckpoints = 3000;
		public const float CheckpointSpacingInMeters = 5;

		public int sessionID = 0;
		public int subSessionID = 0;

		public int sessionCount = 0;
		public int sessionNumber = -1;
		public string sessionName = string.Empty;
		public string sessionType = string.Empty;

		public bool isReplay = false;
		public bool isDirtTrack = false;
		public bool isInPracticeSession = false;
		public bool isInQualifyingSession = false;
		public bool isInRaceSession = false;

		public int trackID = 0;
		public float trackLengthInMeters = 0;
		public string trackType = string.Empty;

		public string seriesLogoTextureUrl = string.Empty;

		public int numForwardGears = 0;

		public float shiftRpm = 0;
		public float redlineRpm = 0;
		public float blinkRpm = 0;

		public int numCheckpoints = MaxNumCheckpoints;

		public NormalizedSession()
		{
			Reset();
		}

		// called only when director starts up for the first time and when iracing simulation shuts down
		public void Reset()
		{
			sessionID = 0;
			subSessionID = 0;

			sessionCount = 0;
			sessionNumber = -1;
			sessionName = string.Empty;
			sessionType = string.Empty;

			isReplay = false;
			isDirtTrack = false;
			isInPracticeSession = false;
			isInQualifyingSession = false;
			isInRaceSession = false;

			trackID = 0;
			trackLengthInMeters = 0;
			trackType = string.Empty;

			seriesLogoTextureUrl = string.Empty;

			shiftRpm = 0;
			redlineRpm = 0;
			blinkRpm = 0;

			numCheckpoints = MaxNumCheckpoints;
		}

		// called only when header session info version number changes
		public void SessionUpdate()
		{
			if ( IRSDK.session == null )
			{
				return;
			}

			sessionID = IRSDK.session.WeekendInfo.SessionID;
			subSessionID = IRSDK.session.WeekendInfo.SubSessionID;

			sessionCount = IRSDK.session.SessionInfo.Sessions.Count;

			isReplay = IRSDK.session.WeekendInfo.SimMode == "replay";

			trackID = IRSDK.session.WeekendInfo.TrackID;

			var match = Regex.Match( IRSDK.session.WeekendInfo.TrackLength, @"([-+]?[0-9]*\.?[0-9]+)" );

			if ( match.Success )
			{
				var trackLengthInKilometers = float.Parse( match.Groups[ 1 ].Value, CultureInfo.InvariantCulture.NumberFormat );

				trackLengthInMeters = trackLengthInKilometers * 1000;
			}

			trackType = IRSDK.session.WeekendInfo.TrackType;

			isDirtTrack = trackType.Contains( "dirt", StringComparison.OrdinalIgnoreCase );

			seriesLogoTextureUrl = $"https://ir-core-sites.iracing.com/members/member_images/series/seriesid_{IRSDK.session.WeekendInfo.SeriesID}/logo.jpg";

			numForwardGears = IRSDK.session.DriverInfo.DriverCarGearNumForward;

			shiftRpm = IRSDK.session.DriverInfo.DriverCarSLShiftRPM;
			redlineRpm = IRSDK.session.DriverInfo.DriverCarRedLine;
			blinkRpm = IRSDK.session.DriverInfo.DriverCarSLBlinkRPM;

			numCheckpoints = (int) Math.Clamp( Math.Ceiling( trackLengthInMeters / CheckpointSpacingInMeters ), 2, MaxNumCheckpoints );

			LogFile.Write( $"Session ID:{sessionID}, Subsession ID:{subSessionID}, Session count:{sessionCount}, Is replay:{isReplay}, Track ID:{trackID}, Track length:{trackLengthInMeters}m, Track type:{trackType}, Is dirt track:{isDirtTrack}, Num checkpoints:{numCheckpoints}.\r\n" );

			if ( isReplay )
			{
				SessionFlagsPlayback.Load();
				IncidentPlayback.Load();
				SubtitlePlayback.Load();
			}
		}

		// called only when telemetry data session number changes
		public void SessionNumberChange()
		{
			if ( ( IRSDK.session == null ) || ( IRSDK.data == null ) )
			{
				return;
			}

			sessionNumber = IRSDK.data.SessionNum;

			if ( sessionNumber >= 0 )
			{
				var session = IRSDK.session.SessionInfo.Sessions[ sessionNumber ];

				sessionName = session.SessionName;
				sessionType = session.SessionType;

				isInPracticeSession = ( sessionType == "Practice" ) || ( sessionType == "Warmup" );
				isInQualifyingSession = ( sessionType == "Lone Qualify" );
				isInRaceSession = ( sessionType == "Race" );
			}
			else
			{
				sessionName = string.Empty;

				isInPracticeSession = false;
				isInQualifyingSession = false;
				isInRaceSession = false;
			}

			LogFile.Write( $"Session number:{sessionNumber}, Session name:{sessionName}.\r\n" );
		}
	}
}

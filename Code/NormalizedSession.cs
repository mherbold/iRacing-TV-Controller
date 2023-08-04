﻿
using System.Globalization;
using System.Text.RegularExpressions;

namespace iRacingTVController
{
	public class NormalizedSession
	{
		public int sessionID = 0;
		public int subSessionID = 0;

		public int sessionCount = 0;
		public int sessionNumber = -1;
		public string sessionName = string.Empty;
		public string sessionType = string.Empty;

		public bool isReplay = false;
		public bool isInPracticeSession = false;
		public bool isInQualifyingSession = false;
		public bool isInRaceSession = false;

		public int trackID = 0;
		public float trackLengthInMeters = 0;

		public string seriesLogoTextureUrl = string.Empty;

		public NormalizedSession()
		{
			Reset();
		}

		public void Reset()
		{
			sessionID = 0;
			subSessionID = 0;

			sessionCount = 0;
			sessionNumber = -1;
			sessionName = string.Empty;
			sessionType = string.Empty;

			isReplay = false;
			isInPracticeSession = false;
			isInQualifyingSession = false;
			isInRaceSession = false;

			trackID = 0;
			trackLengthInMeters = 0;

			seriesLogoTextureUrl = string.Empty;
		}

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

			LogFile.Write( $"Session ID:{sessionID}, Subsession ID:{subSessionID}, Session count:{sessionCount}, Is replay?{isReplay}, Track ID:{trackID}, Track length:{trackLengthInMeters}.\r\n" );

			if ( isReplay )
			{
				SessionFlagsPlayback.Load();
				IncidentPlayback.Load();
				SubtitlePlayback.Load();
			}

			seriesLogoTextureUrl = $"https://ir-core-sites.iracing.com/members/member_images/series/seriesid_{IRSDK.session.WeekendInfo.SeriesID}/logo.jpg";
		}
	}
}

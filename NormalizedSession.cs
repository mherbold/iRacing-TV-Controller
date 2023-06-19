
using System.Globalization;
using System.Text.RegularExpressions;

namespace iRacingTVController
{
	public class NormalizedSession
	{
		public int sessionId;
		public int subSessionId;

		public int sessionNumber;
		public string sessionName;

		public bool isReplay;
		public bool isInRaceSession;
		public bool isInQualifyingSession;

		public float trackLengthInMeters;

		public int seriesId;
		public Texture2D seriesTexture;

		public NormalizedSession()
		{
			Reset();
		}

		public void Reset()
		{
			sessionId = 0;
			subSessionId = 0;

			sessionNumber = 0;
			sessionName = string.Empty;

			isReplay = false;
			isInRaceSession = false;
			isInQualifyingSession = false;

			trackLengthInMeters = 0;

			seriesId = 0;
			seriesTexture = null;

			SessionFlagsPlayback.Close();
			ChatLogPlayback.Close();
		}

		public void SessionChange()
		{
			sessionNumber = IRSDK.data.SessionNum;

			if ( sessionNumber >= 0 )
			{
				sessionName = IRSDK.session.SessionInfo.Sessions[ sessionNumber ].SessionName;

				isInRaceSession = sessionName == "RACE";
				isInQualifyingSession = sessionName == "QUALIFY";
			}
			else
			{
				sessionName = string.Empty;

				isInRaceSession = false;
				isInQualifyingSession = false;
			}
		}

		public async void SessionUpdate()
		{
			sessionId = IRSDK.session.WeekendInfo.SessionID;
			subSessionId = IRSDK.session.WeekendInfo.SubSessionID;

			isReplay = IRSDK.session.WeekendInfo.SimMode == "replay";

			var match = Regex.Match( IRSDK.session.WeekendInfo.TrackLength, "([-+]?[0-9]*\\.?[0-9]+)" );

			if ( match.Success )
			{
				var trackLengthInKilometers = float.Parse( match.Groups[ 1 ].Value, CultureInfo.InvariantCulture.NumberFormat );

				trackLengthInMeters = trackLengthInKilometers * 1000;
			}

			seriesId = IRSDK.session.WeekendInfo.SeriesID;

			if ( isReplay )
			{
				SessionFlagsPlayback.LoadRecording();
				ChatLogPlayback.LoadRecording();
			}
			else
			{
				SessionFlagsPlayback.OpenForRecording();
			}

			if ( seriesTexture == null )
			{
				var url = $"https://ir-core-sites.iracing.com/members/member_images/series/seriesid_{seriesId}/logo.jpg";

				seriesTexture = await RemoteTexture.Get( url );
			}
		}
	}
}

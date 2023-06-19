
namespace iRacingTVController
{
	public class ChatLogData
	{
		public int sessionNumber;
		public double startSessionTime;
		public double endSessionTime;
		public string text;

		public ChatLogData( int sessionNumber, double startSessionTime, double endSessionTime, string text )
		{
			this.sessionNumber = sessionNumber;
			this.startSessionTime = startSessionTime;
			this.endSessionTime = endSessionTime;
			this.text = text;
		}
	}
}

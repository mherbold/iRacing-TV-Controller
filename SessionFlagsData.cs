
namespace iRacingTVController
{
	public class SessionFlagsData
	{
		public int sessionNumber;
		public double sessionTime;
		public uint sessionFlags;

		public SessionFlagsData( int sessionNumber, double sessionTime, uint sessionFlags )
		{
			this.sessionNumber = sessionNumber;
			this.sessionTime = sessionTime;
			this.sessionFlags = sessionFlags;
		}
	}
}

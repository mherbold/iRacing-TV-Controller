namespace iRacingTVController
{
	public class SimTime
	{
		public int sessionNumber;
		public double sessionTime;

		public SimTime( int sessionNumber, double sessionTime )
		{
			this.sessionNumber = sessionNumber;
			this.sessionTime = sessionTime;
		}

		public static SimTime zero
		{
			get
			{
				return new SimTime( 0, 0 );
			}
		}
	}
}

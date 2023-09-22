
using System.Threading;

namespace iRacingTVController
{
	public class AsyncMutex
	{
		private int counter = 0;

		public void Acquire()
		{
			while ( true )
			{
				if ( Interlocked.Increment( ref counter ) == 1 )
				{
					break;
				}

				Interlocked.Decrement( ref counter );

				Thread.Sleep( 0 );
			}
		}

		public void Release()
		{
			Interlocked.Decrement( ref counter );
		}
	}
}


namespace iRacingTVController
{
	internal class LiveData
	{
		public static LiveData Instance { get; private set; }

		public SerializableDictionary<string, LiveDataCar> liveDataCar = new();

		static LiveData()
		{
			Instance = new LiveData();
		}

		private LiveData()
		{
			Instance = this;
		}
	}
}

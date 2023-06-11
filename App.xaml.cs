
using System.Windows;

namespace iRacingTVController
{
	public partial class App : Application
	{
		public App()
		{
			Startup += AppStartup;
		}

		void AppStartup( object sender, StartupEventArgs e )
		{
			iRacingTVController.MainWindow.Instance.Show();
		}
	}
}

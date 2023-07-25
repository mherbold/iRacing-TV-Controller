
using irsdkSharp.Serialization.Enums.Fastest;
using System.Windows;

namespace iRacingTVController.Windows
{
	public partial class EditSessionFlagsData : Window
	{
		public SessionFlagsData sessionFlagsData;

		public EditSessionFlagsData( SessionFlagsData sessionFlagsData )
		{
			this.sessionFlagsData = sessionFlagsData;

			InitializeComponent();

			Checkered.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Checkered ) != 0;
			White.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.White ) != 0;
			Green.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Green ) != 0;
			Yellow.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Yellow ) != 0;
			Red.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Red ) != 0;
			Blue.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Blue ) != 0;
			Debris.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Debris ) != 0;
			Crossed.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Crossed ) != 0;
			YellowWaving.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.YellowWaving ) != 0;
			OneLapToGreen.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.OneLapToGreen ) != 0;
			GreenHeld.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.GreenHeld ) != 0;
			TenToGo.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.TenToGo ) != 0;
			FiveToGo.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.FiveToGo ) != 0;
			RandomWaving.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.RandomWaving ) != 0;
			Caution.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Caution ) != 0;
			CautionWaving.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.CautionWaving ) != 0;
			Black.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Black ) != 0;
			Disqualify.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Disqualify ) != 0;
			Servicable.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Servicible ) != 0;
			Furled.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Furled ) != 0;
			Repair.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.Repair ) != 0;
			StartHidden.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.StartHidden ) != 0;
			StartReady.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.StartReady ) != 0;
			StartSet.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.StartSet ) != 0;
			StartGo.IsChecked = ( sessionFlagsData.SessionFlags & (uint) SessionFlags.StartGo ) != 0;
		}

		private void Update_Button_Clicked( object sender, RoutedEventArgs e )
		{
			sessionFlagsData.SessionFlags = 0;

			sessionFlagsData.SessionFlags |= ( Checkered.IsChecked ?? false ) ? (uint) SessionFlags.Checkered : 0;
			sessionFlagsData.SessionFlags |= ( White.IsChecked ?? false ) ? (uint) SessionFlags.White : 0;
			sessionFlagsData.SessionFlags |= ( Green.IsChecked ?? false ) ? (uint) SessionFlags.Green : 0;
			sessionFlagsData.SessionFlags |= ( Yellow.IsChecked ?? false ) ? (uint) SessionFlags.Yellow : 0;
			sessionFlagsData.SessionFlags |= ( Red.IsChecked ?? false ) ? (uint) SessionFlags.Red : 0;
			sessionFlagsData.SessionFlags |= ( Blue.IsChecked ?? false ) ? (uint) SessionFlags.Blue : 0;
			sessionFlagsData.SessionFlags |= ( Debris.IsChecked ?? false ) ? (uint) SessionFlags.Debris : 0;
			sessionFlagsData.SessionFlags |= ( Crossed.IsChecked ?? false ) ? (uint) SessionFlags.Crossed : 0;
			sessionFlagsData.SessionFlags |= ( YellowWaving.IsChecked ?? false ) ? (uint) SessionFlags.YellowWaving : 0;
			sessionFlagsData.SessionFlags |= ( OneLapToGreen.IsChecked ?? false ) ? (uint) SessionFlags.OneLapToGreen : 0;
			sessionFlagsData.SessionFlags |= ( GreenHeld.IsChecked ?? false ) ? (uint) SessionFlags.GreenHeld : 0;
			sessionFlagsData.SessionFlags |= ( TenToGo.IsChecked ?? false ) ? (uint) SessionFlags.TenToGo : 0;
			sessionFlagsData.SessionFlags |= ( FiveToGo.IsChecked ?? false ) ? (uint) SessionFlags.FiveToGo : 0;
			sessionFlagsData.SessionFlags |= ( RandomWaving.IsChecked ?? false ) ? (uint) SessionFlags.RandomWaving : 0;
			sessionFlagsData.SessionFlags |= ( Caution.IsChecked ?? false ) ? (uint) SessionFlags.Caution : 0;
			sessionFlagsData.SessionFlags |= ( CautionWaving.IsChecked ?? false ) ? (uint) SessionFlags.CautionWaving : 0;
			sessionFlagsData.SessionFlags |= ( Black.IsChecked ?? false ) ? (uint) SessionFlags.Black : 0;
			sessionFlagsData.SessionFlags |= ( Disqualify.IsChecked ?? false ) ? (uint) SessionFlags.Disqualify : 0;
			sessionFlagsData.SessionFlags |= ( Servicable.IsChecked ?? false ) ? (uint) SessionFlags.Servicible : 0;
			sessionFlagsData.SessionFlags |= ( Furled.IsChecked ?? false ) ? (uint) SessionFlags.Furled : 0;
			sessionFlagsData.SessionFlags |= ( Repair.IsChecked ?? false ) ? (uint) SessionFlags.Repair : 0;
			sessionFlagsData.SessionFlags |= ( StartHidden.IsChecked ?? false ) ? (uint) SessionFlags.StartHidden : 0;
			sessionFlagsData.SessionFlags |= ( StartReady.IsChecked ?? false ) ? (uint) SessionFlags.StartReady : 0;
			sessionFlagsData.SessionFlags |= ( StartSet.IsChecked ?? false ) ? (uint) SessionFlags.StartSet : 0;
			sessionFlagsData.SessionFlags |= ( StartGo.IsChecked ?? false ) ? (uint) SessionFlags.StartGo : 0;

			sessionFlagsData.SessionFlagsAsString = ( (SessionFlags) sessionFlagsData.SessionFlags ).ToString();

			if ( sessionFlagsData.SessionFlagsAsString == "0" )
			{
				sessionFlagsData.SessionFlagsAsString = string.Empty;
			}

			SessionFlagsPlayback.Refresh();

			SessionFlagsPlayback.saveToFileQueued = true;

			Close();
		}

		private void Delete_Button_Clicked( object sender, RoutedEventArgs e )
		{
			if ( MessageBox.Show( this, "Are you sure you want to delete this session flags data entry?", "Are You Sure?", MessageBoxButton.OKCancel, MessageBoxImage.Question ) == MessageBoxResult.Cancel )
			{
				return;
			}

			SessionFlagsPlayback.sessionFlagsDataList.Remove( sessionFlagsData );

			SessionFlagsPlayback.Refresh();

			SessionFlagsPlayback.saveToFileQueued = true;

			Close();
		}
	}
}

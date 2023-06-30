
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace iRacingTVController
{
	/// <summary>
	/// Interaction logic for CameraSelector.xaml
	/// </summary>
	public partial class CameraSelector : Window
	{
		public TextBox textBox;

		public List<string> cameraGroupNames = new();

		public CameraSelector( TextBox textBox )
		{
			this.textBox = textBox;

			InitializeComponent();

			if ( IRSDK.session != null )
			{
				foreach ( var cameraGroup in IRSDK.session.CameraInfo.Groups )
				{
					cameraGroupNames.Add( cameraGroup.GroupName );
				}

				cameraGroupNames.Sort();

				foreach ( var cameraGroupName in cameraGroupNames )
				{
					Cameras.Items.Add( cameraGroupName );
				}
			}
		}

		private void Add_Click( object sender, RoutedEventArgs e )
		{
			var existingCameraGroupNames = textBox.Text.Split( "," ).ToList();

			existingCameraGroupNames = existingCameraGroupNames.Select( s => s.Trim().ToLower() ).ToList().Distinct().ToList();
			
			existingCameraGroupNames.RemoveAll( s => s == string.Empty );

			var textBoxText = string.Join( ", ", existingCameraGroupNames );

			foreach ( var selectedItem in Cameras.SelectedItems )
			{
				var selectedCameraGroupName = ( (string) selectedItem ).ToLower();

				if ( !existingCameraGroupNames.Contains( selectedCameraGroupName ) )
				{
					if ( textBoxText != string.Empty )
					{
						textBoxText += ", ";
					}

					textBoxText += selectedCameraGroupName;
				}
			}

			textBox.Text = textBoxText;

			Close();
		}

		private void Cancel_Click( object sender, RoutedEventArgs e )
		{
			Close();
		}
	}
}

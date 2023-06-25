
using System.IO;
using System.Windows;

namespace iRacingTVController
{
	public partial class RenameDirector : Window
	{
		public string directorName;

		public RenameDirector( string directorName )
		{
			InitializeComponent();

			this.directorName = directorName;

			DirectorName.Text = this.directorName;
		}

		private void Rename_Click( object sender, RoutedEventArgs e )
		{
			var directorName = DirectorName.Text.Trim();

			var lowercaseDirectorName = directorName.ToLower();
			var lowercaseGlobalDirectorName = Settings.directorGlobal.ToString().ToLower();

			if ( directorName == string.Empty )
			{
				MessageBox.Show( this, "You need to give the director a name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else if ( lowercaseDirectorName == lowercaseGlobalDirectorName )
			{
				MessageBox.Show( this, "You can't use that name, it is reserved.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
			}
			else
			{
				var directorFilePath = Settings.directorSettingsFolder + directorName + ".xml";

				if ( Settings.directorLocal.filePath == directorFilePath )
				{
					Close();
				}
				else if ( File.Exists( directorFilePath ) )
				{
					MessageBox.Show( this, "There is already an existing director with that name.", "Error", MessageBoxButton.OK, MessageBoxImage.Error );
				}
				else
				{
					File.Delete( Settings.directorLocal.filePath );

					Settings.directorLocal.filePath = directorFilePath;

					Settings.SaveDirector();

					Settings.editor.lastActiveDirectorFilePath = Settings.directorLocal.filePath;

					Settings.SaveEditor();

					MainWindow.Instance.Initialize();

					Close();
				}
			}
		}

		private void Cancel_Click( object sender, RoutedEventArgs e )
		{
			Close();
		}
	}
}

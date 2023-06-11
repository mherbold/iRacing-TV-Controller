
using System;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace iRacingTVController
{
	public partial class Number : UserControl
	{
		public event EventHandler? ValueChanged;

		private Point? startingPosition;
		private int startingNumber;

		private static readonly Regex regex = new( @"^[-\d]*$" );

		public Number()
		{
			InitializeComponent();
		}

		public int GetValue()
		{
			if ( !int.TryParse( NumberTextBox.Text, out var value ) )
			{
				return 0;
			}

			return value;
		}

		public void SetValue( int value )
		{
			NumberTextBox.Text = value.ToString();
		}

		private void TextBox_PreviewTextInput( object sender, TextCompositionEventArgs e )
		{
			e.Handled = !IsTextAllowed( e.Text );
		}

		private void Pasting( object sender, DataObjectPastingEventArgs e )
		{
			if ( e.DataObject.GetDataPresent( typeof( string ) ) )
			{
				var text = (string) e.DataObject.GetData( typeof( string ) );

				if ( IsTextAllowed( text ) )
				{
					return;
				}
			}

			e.CancelCommand();
		}

		private static bool IsTextAllowed( string text )
		{
			return regex.IsMatch( text );
		}

		private void Button_PreviewMouseDown( object sender, MouseButtonEventArgs e )
		{
			startingPosition = e.GetPosition( this );

			if ( !int.TryParse( NumberTextBox.Text, out startingNumber ) )
			{
				startingNumber = 0;
			}
		}

		private void Button_PreviewMouseUp( object sender, MouseButtonEventArgs e )
		{
			startingPosition = null;
		}

		private void Button_PreviewMouseMove( object sender, MouseEventArgs e )
		{
			if ( startingPosition != null )
			{
				var newPosition = e.GetPosition( this );

				var deltaPosition = newPosition - startingPosition;

				if ( ( deltaPosition.Value.X != 0 ) || ( deltaPosition.Value.Y != 0 ) )
				{
					var scale = Settings.editor.positioningSpeedNormal;

					if ( Keyboard.IsKeyDown( Key.LeftShift ) || Keyboard.IsKeyDown( Key.RightShift ) )
					{
						scale = Settings.editor.positioningSpeedFast;
					}
					else if ( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) )
					{
						scale = Settings.editor.positioningSpeedSlow;
					}

					var deltaNumber = (int) Math.Round( scale * ( deltaPosition.Value.X + deltaPosition.Value.Y ) );

					SetValue( startingNumber + deltaNumber );
				}
			}
		}

		private void NumberTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			ValueChanged?.Invoke( this, EventArgs.Empty );
		}
	}
}

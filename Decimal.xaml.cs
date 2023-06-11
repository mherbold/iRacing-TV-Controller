
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace iRacingTVController
{
	public partial class Decimal : UserControl
	{
		public event EventHandler? ValueChanged;

		private bool mouseIsDown = false;

		private Point startingPosition;
		private float startingDecimal;

		private static readonly Regex regex = new( @"^[-\d.]*$" );

		public Decimal()
		{
			InitializeComponent();
		}

		public float GetValue()
		{
			if ( !float.TryParse( DecimalTextBox.Text, out var value ) )
			{
				return 0;
			}

			return value;
		}

		public void SetValue( float value )
		{
			DecimalTextBox.Text = $"{value:0.000}";
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

			if ( !float.TryParse( DecimalTextBox.Text, out startingDecimal ) )
			{
				startingDecimal = 0;
			}

			mouseIsDown = true;
		}

		private void Button_PreviewMouseUp( object sender, MouseButtonEventArgs e )
		{
			mouseIsDown = false;
		}

		private void Button_PreviewMouseMove( object sender, MouseEventArgs e )
		{
			if ( mouseIsDown )
			{
				var newPosition = e.GetPosition( this );

				var deltaPosition = newPosition - startingPosition;

				if ( ( deltaPosition.X != 0 ) || ( deltaPosition.Y != 0 ) )
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

					var deltaDecimal = scale * (float) ( deltaPosition.X + deltaPosition.Y );

					SetValue( startingDecimal + deltaDecimal );
				}
			}
		}

		private void DecimalTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			ValueChanged?.Invoke( this, EventArgs.Empty );
		}
	}
}

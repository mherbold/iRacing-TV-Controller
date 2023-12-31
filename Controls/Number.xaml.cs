﻿
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace iRacingTVController
{
	public partial class Number : UserControl
	{
		public event EventHandler? ValueChanged;

		private float currentScale;
		private Point? startingPosition;
		private int startingNumber;

		private static readonly Regex regex = new( @"^[-\d]*$" );

		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register( "Value", typeof( int ), typeof( Number ), new PropertyMetadata( 0, OnValueChanged ) );

		public int Value
		{
			get
			{
				if ( !int.TryParse( NumberTextBox.Text, out var value ) )
				{
					return 0;
				}

				return value;
			}

			set
			{
				NumberTextBox.Text = value.ToString();
			}
		}

		public Number()
		{
			InitializeComponent();
		}

		private static void OnValueChanged( DependencyObject obj, DependencyPropertyChangedEventArgs e )
		{
			var number = (Number) obj;

			number.Value = (int) number.GetValue( ValueProperty );
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
			currentScale = Settings.editor.editorMousePositioningSpeedNormal;
			startingPosition = e.GetPosition( this );
			startingNumber = Value;
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
					var scale = Settings.editor.editorMousePositioningSpeedNormal;

					if ( Keyboard.IsKeyDown( Key.LeftShift ) || Keyboard.IsKeyDown( Key.RightShift ) )
					{
						scale = Settings.editor.editorMousePositioningSpeedFast;
					}
					else if ( Keyboard.IsKeyDown( Key.LeftCtrl ) || Keyboard.IsKeyDown( Key.RightCtrl ) )
					{
						scale = Settings.editor.editorMousePositioningSpeedSlow;
					}

					if ( currentScale != scale )
					{
						currentScale = scale;
						startingNumber = Value;
						startingPosition = newPosition;
					}
					else
					{
						var deltaNumber = (int) Math.Round( scale * ( deltaPosition.Value.X + deltaPosition.Value.Y ) );

						Value = startingNumber + deltaNumber;
					}
				}
			}
		}

		private void NumberTextBox_TextChanged( object sender, TextChangedEventArgs e )
		{
			ValueChanged?.Invoke( this, EventArgs.Empty );
		}
	}
}

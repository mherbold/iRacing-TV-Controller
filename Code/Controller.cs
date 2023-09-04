
using System;
using System.Collections.Generic;

using Vortice.DirectInput;

namespace iRacingTVController
{
	public static class Controller
	{
		public enum Mode
		{
			None,
			Width,
			PositionXY,
			PositionZ,
			Curvature,
			Count
		}

		public static Guid currentControllerGuid = Guid.Empty;
		public static IDirectInputDevice8? directInputDevice = null;
		public static Mode currentMode = Mode.None;
		public static bool buttonWasDown = false;

		public static SortedDictionary<string, Guid> FindAll()
		{
			LogFile.Write( "Finding all connected controllers...\r\n" );

			SortedDictionary<string, Guid> controllers = new();

			var directInput = DInput.DirectInput8Create();

			var deviceList = directInput.GetDevices( DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices );

			foreach ( var device in deviceList )
			{
				if ( !controllers.ContainsKey( device.ProductName ) )
				{
					controllers.Add( device.ProductName, device.ProductGuid );

					LogFile.Write( $"...found {device.ProductName}\r\n" );
				}
			}

			return controllers;
		}

		public static void Initialize()
		{
			if ( currentControllerGuid == Settings.editor.editorSteamVrControllerGuid )
			{
				return;
			}

			if ( directInputDevice != null )
			{
				LogFile.Write( "Disposing of previously initialized controller.\r\n" );

				directInputDevice.Dispose();

				directInputDevice = null;
			}

			if ( Settings.editor.editorSteamVrControllerGuid != Guid.Empty )
			{
				DeviceInstance? selectedDevice = null;

				var directInput = DInput.DirectInput8Create();

				var deviceList = directInput.GetDevices( DeviceClass.GameControl, DeviceEnumerationFlags.AllDevices );

				foreach ( var device in deviceList )
				{
					if ( ( device.ProductGuid == Settings.editor.editorSteamVrControllerGuid ) )
					{
						selectedDevice = device;
						break;
					}
				}

				if ( selectedDevice == null )
				{
					LogFile.Write( $"Selected controller device {Settings.editor.editorSteamVrControllerGuid} was not found!\r\n" );
				}
				else
				{
					directInputDevice = directInput.CreateDevice( selectedDevice.InstanceGuid );

					directInputDevice.SetCooperativeLevel( IntPtr.Zero, CooperativeLevel.NonExclusive | CooperativeLevel.Foreground );

					directInputDevice.SetDataFormat<RawJoystickState>();

					LogFile.Write( $"Controller device {selectedDevice.ProductName} was initialized.\r\n" );

					currentControllerGuid = Settings.editor.editorSteamVrControllerGuid;
				}
			}
		}

		public static void Update()
		{
			if ( directInputDevice == null )
			{
				return;
			}

			var result = directInputDevice.Poll();

			if ( result.Failure )
			{
				result = directInputDevice.Acquire();
			}

			if ( result.Success )
			{
				var joystickState = directInputDevice.GetCurrentJoystickState();

				if ( buttonWasDown != joystickState.Buttons[ 0 ] )
				{
					buttonWasDown = joystickState.Buttons[ 0 ];

					if ( buttonWasDown )
					{
						currentMode++;

						if ( currentMode == Mode.Count )
						{
							currentMode = Mode.None;
						}

						LogFile.Write( $"Current controller mode is now {currentMode}.\r\n" );
					}
				}

				if ( currentMode != Mode.None )
				{
					var x = joystickState.X / 32768.0f - 1.0f;
					var y = joystickState.Y / 32768.0f - 1.0f;

					if ( x > 0 )
					{
						x = Math.Max( 0, x - 0.05f );
					}
					else
					{
						x = Math.Min( 0, x + 0.05f );
					}

					if ( y > 0 )
					{
						y = Math.Max( 0, y - 0.05f );
					}
					else
					{
						y = Math.Min( 0, y + 0.05f );
					}

					if ( ( x != 0 ) || ( y != 0 ) )
					{
						switch ( currentMode )
						{
							case Mode.Width:

								Settings.editor.editorSteamVrWidth += x * Program.deltaTime * 0.1f;

								Settings.saveEditorToFileQueued = true;

								MainWindow.Instance.Update();

								break;

							case Mode.PositionXY:

								Settings.editor.editorSteamVrPosition.x += x * Program.deltaTime * 0.1f;
								Settings.editor.editorSteamVrPosition.y -= y * Program.deltaTime * 0.1f;

								Settings.saveEditorToFileQueued = true;

								MainWindow.Instance.Update();

								break;

							case Mode.PositionZ:

								Settings.editor.editorSteamVrPosition.z += y * Program.deltaTime * 0.1f;

								Settings.saveEditorToFileQueued = true;

								MainWindow.Instance.Update();

								break;

							case Mode.Curvature:

								Settings.editor.editorSteamVrCurvature += y * Program.deltaTime * 0.025f;

								Settings.saveEditorToFileQueued = true;

								MainWindow.Instance.Update();

								break;
						}
					}
				}
			}
		}
	}
}

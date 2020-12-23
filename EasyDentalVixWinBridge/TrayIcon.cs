using System;
using System.Drawing;
using System.Windows.Forms;

namespace EasyDentalVixWinBridge
{
	/// <summary>
	/// The tray icon class.
	/// </summary>
	internal class TrayIcon
	{
		private readonly NotifyIcon notifyIcon;

		/// <summary>
		/// Initializes a new instance of the <see cref="TrayIcon"/> class.
		/// </summary>
		internal TrayIcon( )
		{
			notifyIcon = new NotifyIcon( );
			notifyIcon.Icon = new Icon( AppDomain.CurrentDomain.BaseDirectory + "tray_icon.ico" );
			notifyIcon.Text = "Easy Dental VixWin Bridge";
			notifyIcon.ContextMenuStrip = new ContextMenuStrip( );
			notifyIcon.Visible = true;
			PopulateTrayIconMenu( );
		}

		/// <summary>
		/// Populates the tray icon menu.
		/// </summary>
		private void PopulateTrayIconMenu( )
		{
			ToolStripItemCollection trayItems = notifyIcon.ContextMenuStrip.Items;
			trayItems.Add( new ToolStripLabel( "Easy Dental VixWin Bridge" ) );
			trayItems.Add( new ToolStripSeparator( ) );
			trayItems.Add( new ToolStripLabel( "" ) );
			trayItems.Add( new ToolStripLabel( "" ) );
			trayItems.Add( new ToolStripSeparator( ) );
			trayItems.Add( "Exit", null, Exit );

			notifyIcon.MouseClick += ( object sender, MouseEventArgs eventArgs ) =>
			{
				if ( eventArgs.Button == MouseButtons.Left )
				{
					OpenVixWinWithPatientFromUI( );
				} else if ( eventArgs.Button == MouseButtons.Right )
				{
					UpdateCurrentPatientInUI( );
				}
			};
		}

		/// <summary>
		/// Opens VixWin with a patient ID from the UI.
		/// </summary>
		private void OpenVixWinWithPatientFromUI( )
		{
			int patientId = EasyDentalVixWinHelper.GetEasyDentalSelectedPatientId( );

			if ( patientId != 0 )
			{
				EasyDentalVixWinHelper.OpenVixWinWithEasyDentalPatient( patientId );
			} else
			{
				MessageBox.Show( "There is no patient currently selected in Easy Dental.", "No Patient Selected", MessageBoxButtons.OK, MessageBoxIcon.Error );
			}
		}

		/// <summary>
		/// Updates the current patient in the tray icon menu.
		/// </summary>
		private void UpdateCurrentPatientInUI( )
		{
			string selectedPatientName = "None";
			string selectedPatientId = "None";

			try
			{
				int patientId = EasyDentalVixWinHelper.GetEasyDentalSelectedPatientId( );

				if ( patientId != 0 )
				{
					selectedPatientName = EasyDentalVixWinHelper.GetEasyDentalPatientNameFromId( patientId );
					selectedPatientId = patientId.ToString( );
				}
			} catch ( InvalidOperationException e )
			{
				MessageBox.Show( e.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error ); // Easy Dental is not installed or an incompatible version is installed.
			}

			notifyIcon.ContextMenuStrip.Items[ 2 ].Text = "Selected Patient Name: " + selectedPatientName;
			notifyIcon.ContextMenuStrip.Items[ 3 ].Text = "Selected Patient ID: " + selectedPatientId;
		}

		/// <summary>
		/// Exits the application from the tray exit button.
		/// </summary>
		/// <param name="sender">The event sender.</param>
		/// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
		private void Exit( object sender, EventArgs e )
		{
			DialogResult toExit = MessageBox.Show( "Are you sure you want to exit?", "Exit Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question );

			if ( toExit == DialogResult.Yes )
			{
				notifyIcon.Dispose( );
				Application.Exit( );
			}
		}
	}
}

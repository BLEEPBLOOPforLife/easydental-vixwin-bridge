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
			throw new NotImplementedException( );
		}
	}
}

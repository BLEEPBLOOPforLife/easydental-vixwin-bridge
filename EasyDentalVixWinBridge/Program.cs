using System;
using System.Threading;
using System.Windows.Forms;

namespace EasyDentalVixWinBridge
{
	/// <summary>
	/// The program class.
	/// </summary>
	internal static class Program
	{
		/// <summary>
		///  The main entry point for the application.
		/// </summary>
		[STAThread]
		internal static void Main( )
		{
			using ( Mutex appMutex = new Mutex( false, "Global\\{bce94a11-cb8f-41b5-a0b4-3cd8e10628cd}" + Environment.UserName ) )
			{
				if ( !appMutex.WaitOne( 0, false ) )
				{
					MessageBox.Show( "Application is already running.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error );

					return;
				}

				Application.EnableVisualStyles( );
				Application.SetCompatibleTextRenderingDefault( false );
				Application.Run( new App( ) );
			}
		}
	}
}

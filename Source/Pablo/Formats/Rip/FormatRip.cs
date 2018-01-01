using System;
using System.IO;
using System.Text;
using Eto.Drawing;
using Pablo.BGI;
using Eto;
using System.Collections.Generic;
using Eto.Forms;

namespace Pablo.Formats.Rip
{
	public class FormatRip : Animated.AnimatedFormat
	{
		public static Encoding Encoding = Encoding.GetEncoding (437);
		
		public FormatRip (DocumentInfo info) : base(info, "rip", "RIPscrip", "rip")
		{

		}

		public override bool DetectAnimation (Stream stream)
		{
			// rip always should be shown with animation
			return true;
		}
		
		public override bool CanSave { get { return true; } }
		
		
		public void Save (Stream stream, RipDocument document)
		{
			var writer = new RipWriter (stream);
			foreach (var command in document.Commands) {
				command.Write (writer);
			}
			writer.WriteNewCommand ("#|#|#");
			writer.WriteNewLine();
		}
		
		public void Load (Stream stream, RipDocument document, RipHandler handler)
		{
			var reader = new BinaryReader (stream);
			var commands = document.Commands;
			commands.Clear ();
			bool lastEnableZoom = true;
			/*
			 */
			if (document.AnimateView && Application.Instance != null) {
				document.BGI.DelayDraw = true; // for faster animation
				Application.Instance.Invoke (delegate {
					//lastEnableZoom = handler.EnableZoom;
#if DESKTOP
					//handler.EnableZoom = false;
#endif
				});
			}			
		  
			try {
				var args = new WaitEventArgs ();
				while (true) {
					if (document.AnimateView) {
						document.OnWait (args);
						if (args.Exit)
							break;
					}
					byte b = reader.ReadRipByte ();
					/*
					 */
					if (b == (byte)'|') {
					
						string op = ((char)reader.ReadRipByte ()).ToString ();
						if (op == "1")
							op += ((char)reader.ReadRipByte ());
						else if (op == "#")
							break; // done reading rip!
						
						var command = RipCommands.Create (op, document);
						if (command != null) {
							command.Read (reader);
							command.Apply ();
							if (command.Store)
								commands.Add (command);
						}
					}
					/*
					 */
				}
			} catch (EndOfStreamException) {
			} finally {
				if (document.AnimateView && Application.Instance != null) {
					Application.Instance.Invoke (delegate {
						if (document.AnimateView) {
							//handler.EnableZoom = lastEnableZoom;
						}
					});
				}
			}
		}

	}
}

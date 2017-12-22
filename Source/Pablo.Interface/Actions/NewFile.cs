using System;
using System.Text;
using System.Reflection;
using System.Collections;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using Pablo.Interface.Dialogs;
using Pablo.Network;
using Eto;

namespace Pablo.Interface.Actions
{
	public class NewFile : PabloCommand
	{
		Main main;
		public const string ActionID = "newFile";
		
		public NewFile (Main main)
			: base(main)
		{
			this.main = main;
			base.ID = ActionID;
			this.MenuText = "&New";
			this.ToolBarText = "New";
			this.ToolTip = "Create a new file";
			this.Image = ImageCache.IconFromResource("Pablo.Interface.Icons.new.ico");
			this.Shortcut = PabloCommand.CommonModifier | Keys.N;
		}
		
		protected override void Execute (CommandExecuteArgs args)
		{
			NewFileDialog nfd = new NewFileDialog (main.Settings);
			DialogResult dr = nfd.ShowModal(main);
			if (dr == DialogResult.Ok) {
				if (FileModifiedDialog.Show (main) == DialogResult.Ok) {
					// create a new document then edit it
					main.FileList.SelectedIndex = -1;

					var doc = nfd.SelectedDocumentType.Create (main.Platform);
					doc.IsNew = true;
					doc.EditMode = true;

					main.SetDocument (doc, true);
				}	
			}
		}
		
		public override int CommandID {
			get {
				return (int)Network.NetCommands.NewFile;
			}
		}
		
		public override void Receive (ReceiveCommandArgs args)
		{
			base.Receive (args);
			var infoId = args.Message.ReadString ();
			
			DocumentInfo info;
			if (main.Settings.Infos.TryGetValue (infoId, out info)) {
				var doc = info.Create (main.Platform);
				doc.EditMode = true;
				args.Invoke (delegate {
					main.SetDocument (doc, true);
				});
			}
		}
		
		public override bool Send (SendCommandArgs args)
		{
			base.Send (args);
			
			NewFileDialog nfd = new NewFileDialog (main.Settings);
			DialogResult dr = nfd.ShowModal (main);
			if (dr == DialogResult.Ok) {
				if (FileModifiedDialog.Show (main) == DialogResult.Ok) {
					args.Message.Write (nfd.SelectedDocumentType.ID);
					return true;
				}
			}
			
			return false;
		}
	}
}

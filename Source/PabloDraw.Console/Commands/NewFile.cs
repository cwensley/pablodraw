using System;
using System.Text;
using System.Reflection;
using System.Collections;
using Eto.Drawing;
using Eto.Forms;
using Pablo;
using Pablo.Network;
using Eto;
using Lidgren.Network;

namespace PabloDraw.Console.Commands
{
	public class NewFile : ICommand
	{
		IClientDelegate clientDelegate;

		public NewFile (IClientDelegate clientDelegate)
		{
			this.clientDelegate = clientDelegate;
		}

		public UserLevel Level { get { return UserLevel.Operator; } }

		public int CommandID { get { return (int)NetCommands.NewFile; } }

		public NetDeliveryMethod DeliveryMethod { get { return NetDeliveryMethod.ReliableOrdered; } }

		public void Receive (ReceiveCommandArgs args)
		{
			var infoId = args.Message.ReadString ();

			DocumentInfo info;
			if (clientDelegate.DocumentInfos.TryGetValue (infoId, out info)) {
				var doc = info.Create ();
				doc.EditMode = true;
				clientDelegate.SetDocument(doc);
			}
		}

		public bool Send (SendCommandArgs args)
		{
			/*NewFileDialog nfd = new NewFileDialog (main.Settings);
			DialogResult dr = nfd.ShowDialog (main);
			if (dr == DialogResult.Ok) {
				if (FileModifiedDialog.Show (main) == DialogResult.Ok) {
					args.Message.Write (nfd.SelectedDocumentType.ID);
					return true;
				}
			}
			*/
			return false;
		}
	}
}

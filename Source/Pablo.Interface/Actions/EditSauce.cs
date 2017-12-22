using System;
using Eto.Forms;
using Eto.Drawing;
using Pablo.Interface.Dialogs;
using Eto.IO;
using Pablo.Network;
using Pablo.Sauce;

namespace Pablo.Interface.Actions
{
	public class EditSauce : Command
	{
		readonly Main main;
		public const string ActionID = "editSauce";

		public EditSauce(Main main)
			: base(main.ViewHandler)
		{
			this.main = main;
			ID = ActionID;
			Text = "Edit &Sauce Info|Sauce|View/Edit sauce infomation for the current file";
			Image = Icon.FromResource("Pablo.Interface.Icons.sauce.ico");
			Enabled = main.Document != null;
			if (main.Generator.IsMac)
				Accelerator = Command.CommonModifier | Key.I;
			else
				Accelerator = Command.CommonModifier | Key.F11;
			main.DocumentChanged += document_Changed;
		}

		public override int CommandID
		{
			get { return (int)NetCommands.EditSauce; }
		}

		public override UserLevel Level
		{
			get { return UserLevel.Viewer; }
		}

		protected override void OnRemoved(EventArgs e)
		{
			base.OnRemoved(e);
			main.DocumentChanged -= document_Changed;
		}

		void document_Changed(object sender, EventArgs e)
		{
			Enabled = main.Document != null;
		}

		protected override void Execute(CommandExecuteArgs args)
		{
			if (main.Document != null)
			{
				EtoFileInfo file = main.FileList.SelectedFile;
				Document doc = (main.EditMode) ? main.Document : null;
				if (file != null || doc != null)
				{
					var si = new SauceInfoDialog(file, doc);
					if (si.ShowDialog(main) == DialogResult.Ok && !main.EditMode)
						main.ReloadFile(false, false, false);
				}
			}
		}

		public override bool Send(SendCommandArgs args)
		{
			base.Send(args);
			if (main.Document != null)
			{
				var si = new SauceInfoDialog(null, main.Document, args.CurrentUser.Level < UserLevel.Editor, false);

				var result = si.ShowDialog(main);
				if (result == DialogResult.Ok)
				{
					if (si.Sauce == null)
						args.Message.Write(false);
					else
					{
						args.Message.Write(true);
						si.Sauce.Send(args);
					}

					return true;
				}
			}
			return false;
		}

		public override void Receive(ReceiveCommandArgs args)
		{
			base.Receive(args);
			bool hasSauce = args.Message.ReadBoolean();
			if (hasSauce)
			{
				var sauce = new SauceInfo();
				sauce.Receive(args);
				main.Document.Sauce = sauce;
			}
			else
			{
				main.Document.Sauce = null;
			}

			if (!args.IsMe)
			{
				var client = args.Network as Client;
				if (client != null)
				{
					client.OnMessage(new ClientMessageArgs(string.Format("{0} edited sauce metadata", args.User)));
				}

			}
		}
	}
}


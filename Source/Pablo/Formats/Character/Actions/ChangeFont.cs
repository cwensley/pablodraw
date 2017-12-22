using System;
using Eto.Forms;
using Pablo.Network;
using System.Linq;

namespace Pablo.Formats.Character.Actions
{
	public class ChangeFont : RadioCommand, ICommand
	{
		public BitFont Font { get; private set; }

		CharacterHandler Handler { get; set; }

		CharacterDocumentInfo Info { get { return Handler.Info; } }

		public static string ActionID = "character_changeFont_";

		public ChangeFont(CharacterHandler handler)
		: base(null)
		{
			this.Handler = handler;
			if (Handler != null && Handler.Client != null)
				this.Enabled = Handler.Client.CurrentUser.Level >= this.Level;
		}

		public ChangeFont(RadioCommand mainRadio, CharacterHandler handler, BitFont font)
		{
			this.Controller = mainRadio;
			this.Handler = handler;
			this.Font = font;
			ID = string.Format("{0}{1}", ActionID, font.ID);
			MenuText = font.Name; //string.Format ("{0} {1}x{2}", font.Name, font.Width, font.Height);
			//Text = string.Format ("CP {0} {1}x{2}", font.Encoding.CodePage, font.Width, font.Height);
			//Name = "Change Font";
			if (Handler != null)
			{
				if (Handler.Client != null)
				{
					this.Enabled = Handler.Client.CurrentUser.Level >= this.Level;
					this.Checked = font.Equals(Handler.CurrentPage.Font);
				}
				else if (Handler.ClientDelegate.EditMode)
				{
					this.Checked = font.Equals(Handler.CurrentPage.Font);
				}
				else
					this.Checked = font.Equals(Info.SelectedFont);
			}
		}

		protected override void OnExecuted(EventArgs e)
		{
			base.OnExecuted(e);
			if (Handler != null)
			{
				if (Handler.Client != null)
				{
					Handler.Client.SendCommand(this);
				}
				else if (Handler.ClientDelegate.EditMode)
				{
					Handler.CurrentPage.Font = Font;
				}
				else
				{
					Info.SelectedFont = Font;
				}
			}
		}

		public int CommandID { get { return (int)NetCommands.ChangeFont; } }

		public UserLevel Level { get { return UserLevel.Operator; } }

		public Lidgren.Network.NetDeliveryMethod DeliveryMethod
		{
			get { return Lidgren.Network.NetDeliveryMethod.ReliableOrdered; }
		}

		public bool Send(SendCommandArgs args)
		{
			args.Message.Write(Font.ID);
			return true;
		}

		public void Receive(ReceiveCommandArgs args)
		{
			var fontID = args.Message.ReadString();
			var font = Info.GetFonts().SelectMany(r => r.Fonts).FirstOrDefault(s => s.ID == fontID);
			args.Invoke(delegate
			{ 
				if (font != null)
				{
					Handler.CharacterDocument.SetFont(font);
					var client = args.Network as Client;
					if (client != null && !args.IsMe)
						client.OnMessage(new ClientMessageArgs(string.Format("{0} changed font to {1}", args.User, font.DisplayName)));
				}
			});
		}
	}
}


using System;
using Eto.Forms;
using Eto.Drawing;

namespace Pablo.Formats.Character.Actions
{
	public class StartDrawing : ButtonAction
	{
		CharacterHandler handler;
		static UITimer timer;
		
		public const string ActionID = "Character.StartDrawing";
		
		public StartDrawing (CharacterHandler handler)
		{
			this.handler = handler;
			this.ID = ActionID;
			this.Text = "Start Random Drawing|Start Random Drawing|Random Draw!";
		}
		
		protected override void OnActivated (EventArgs e)
		{
			var rand = new Random();
			if (timer == null) {
				timer = new UITimer();
				timer.Interval = 0.01;
				timer.Elapsed += delegate {
					if (handler.CursorPosition.X == handler.CurrentPage.Canvas.Width-1)
						handler.CursorPosition = new Point(0, handler.CursorPosition.Y+1);
					handler.InsertCharacter (rand.Next (255));
				};
				timer.Start ();
			}
			else {
				timer.Stop ();
				timer = null;
			}
		}
	}
}


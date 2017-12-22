using System;
using Eto.Forms;

namespace Pablo.Interface.Actions
{
	public class Homepage : ButtonAction
	{
		public const string ActionID = "homepage";
		
		public Homepage ()
		{
			this.Text = "PabloDraw Website|PabloDraw Website|PabloDraw Website";
			base.ID = ActionID;
		}
		
		protected override void OnActivated (EventArgs e)
		{
			base.OnActivated (e);
			Application.Instance.Open("http://picoe.ca/products/pablodraw");
		}
	}
}


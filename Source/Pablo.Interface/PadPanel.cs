using System;
using System.Collections.Generic;
using System.Linq;
using Eto.Drawing;
using Eto.Forms;
using System.Diagnostics;

namespace Pablo.Interface
{

	public class PadPanel : Panel
	{

		Panel rightPads = new Panel();
		Panel leftPads = new Panel();
		Panel bottomPads = new Panel();
		Panel topPads = new Panel();
		Panel content;

		public new Control Content
		{
			get { return content.Content; }
			set { content.Content = value; }
		}

		public void SetPads(GeneratePadArgs padArgs)
		{
			SuspendLayout();
			GenerateHorizontal(leftPads, padArgs.LeftPads);
			GenerateVertical(topPads, padArgs.TopPads);
			GenerateVertical(bottomPads, padArgs.BottomPads);
			GenerateHorizontal(rightPads, padArgs.RightPads);

			ResumeLayout();
		}

		public void SetTopPads(IEnumerable<Control> pads)
		{
			GenerateVertical(topPads, pads);
			//topPads.ParentLayout.Update ();
		}

		public void SetLeftPads(IEnumerable<Control> pads)
		{
			GenerateHorizontal(leftPads, pads);
			//leftPads.ParentLayout.Update ();
		}

		public void SetBottomPads(IEnumerable<Control> pads)
		{
			GenerateVertical(bottomPads, pads);
			//bottomPads.ParentLayout.Update ();
		}

		public void SetRightPads(IEnumerable<Control> pads)
		{
			GenerateHorizontal(rightPads, pads);
			//rightPads.ParentLayout.Update ();
		}

		Control GenerateHorizontal(Panel container, IEnumerable<Control> pads)
		{
			var count = pads != null ? pads.Count() : 0;
			if (count > 0)
			{
				var layout = new TableLayout(count, 1);
				layout.Padding = Padding.Empty;
				layout.Spacing = Size.Empty;

				int i = 0;
				foreach (var pad in pads)
				{
					layout.Add(pad, i, 0);
					i++;
				}
				container.Content = layout;
			}
			else
				container.Content = null;

			return container.Content;
		}

		Control GenerateVertical(Panel container, IEnumerable<Control> pads)
		{
			var count = pads != null ? pads.Count() : 0;
			if (count > 0)
			{
				var layout = new TableLayout(1, count);
				layout.Padding = Padding.Empty;
				layout.Spacing = Size.Empty;

				int i = 0;
				foreach (var pad in pads)
				{
					layout.Add(pad, 0, i);
					i++;
				}
				container.Content = layout;
			}
			else
				container.Content = null;
			return container.Content;
		}

		public PadPanel()
		{
			var layout = new TableLayout(1, 3);
			layout.Padding = Padding.Empty;
			layout.Spacing = Size.Empty;
			content = new Panel();

			layout.Add(topPads, 0, 0);
			layout.Add(Middle(), 0, 1, true, true);
			layout.Add(bottomPads, 0, 2);

			base.Content = layout;
		}

		Control Middle()
		{
			var layout = new TableLayout(3, 1);
			layout.Padding = Padding.Empty;
			layout.Spacing = Size.Empty;
			layout.Add(leftPads, 0, 0);
			layout.Add(content, 1, 0, true, true);
			layout.Add(rightPads, 2, 0);
			return layout;
		}
	}
}


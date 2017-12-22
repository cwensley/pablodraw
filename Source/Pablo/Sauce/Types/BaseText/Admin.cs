using System;
using Eto.Forms;
using Pablo.Network;
using Eto.Drawing;

namespace Pablo.Sauce.Types.BaseText
{
	public class Admin<T> : BaseFileType.Admin<T>
		where T: DataTypeInfo
	{
		public Admin(T dataType, bool includeFileType)
			: base(dataType, includeFileType)
		{
		}

		protected override void CreateControls()
		{
			base.CreateControls();
			if (DataType.HasFontName)
			{
				Layout.AddRow(new Label { Text = "Font Name", VerticalAlign = VerticalAlign.Middle }, FontName());
			}
		}

		Control FontName()
		{
			var control = new TextBox
			{
				MaxLength = 22,
				Text = DataType.FontName
			};
			control.TextChanged += (sender, e) => DataType.FontName = control.Text;

			var del = (IClientDelegate)Application.Instance.MainForm;
			var selection = new Button { Text = "Select Font" };
			selection.Click += (sender, e) =>
			{
				var doc = del.DocumentInfos[Formats.Character.CharacterDocumentInfo.DocumentID] as Formats.Character.CharacterDocumentInfo;
				if (doc != null)
				{
					var menu = doc.GetFontMenu(null, 
						           selectFont: font => control.Text = font.SauceID, 
						           fontSelected: f => string.Equals(f.SauceID, control.Text, StringComparison.InvariantCultureIgnoreCase)
					           );
					var subMenu = new ContextMenu();
					menu.Actions.Generate(subMenu);
					subMenu.Show(selection);
				}
			};
			var layout = new DynamicLayout(Padding.Empty) { DefaultPadding = Padding.Empty };
			layout.BeginHorizontal();
			layout.AddCentered(control, xscale: true, horizontalCenter: false);
			layout.Add(selection);
			layout.EndHorizontal();
			return layout;
		}

		protected override void OnFileTypeChanged(EventArgs e)
		{
			base.OnFileTypeChanged(e);
			DataType.ICEColors &= DataType.HasICEColors;
			if (!DataType.HasLetterSpacing)
				DataType.LetterSpacing = null;
			if (!DataType.HasAspectRatio)
				DataType.AspectRatio = null;
			if (!DataType.HasFontName)
				DataType.FontName = null;
			RecreateLayout();
		}
	}
}


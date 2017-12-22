using System;
using System.Collections;
using Eto;
using Eto.Forms;
using Eto.Drawing;
using Pablo;
using System.Linq;
using System.Collections.Generic;

namespace Pablo.Interface
{
	public class NewFileDialog : Dialog<DialogResult>
	{
		ListBox fileTypeListBox;
		Settings settings;
		List<DocumentInfo> infos;
		DocumentInfo selected;
		Button okButton;
		
		public DocumentInfo SelectedDocumentType {
			get { return selected; }
		}
		
		/*
		protected override void OnKeyDown(KeyPressEventArgs e)
		{
			if (e.KeyData == Key.Enter)
			{
				this.Result = DialogResult.Ok;
				this.Close();
				e.Handled = true;
			}
			else if (e.KeyData == Key.Escape)
			{
				this.Result = DialogResult.Cancel;
				this.Close();
				e.Handled = true;
			}

			
			base.OnKeyDown(e);
		}*/
		
		public NewFileDialog (Settings settings)
		{
			this.Title = "Select file type";
			//this.Size = new Size (350, 250);
			this.settings = settings;
			
			CreateLayout ();
		}
		
		Control FileTypeListBox()
		{
			fileTypeListBox = new ListBox {
				Size = new Size (150, 100)
			};
			fileTypeListBox.Activated += delegate {
				this.Result = DialogResult.Ok;
				this.Close ();
			};
			fileTypeListBox.SelectedIndexChanged += fileTypeListBox_SelectedIndexChanged;
			fileTypeListBox.Focus ();
			return fileTypeListBox;
		}
		
		private void CreateLayout ()
		{
			var layout = new DynamicLayout ();
		
			layout.Add (new Label { Text = "Please select the type of file to create" });
			
			layout.Add (FileTypeListBox (), true, true);
			
			layout.BeginVertical(Padding.Empty);
			layout.BeginHorizontal();
			
			layout.Add (null, true);
			
			Button cancelButton = new Button ();
			cancelButton.Text = "C&ancel";
			cancelButton.Click += cancelButton_Click;
			this.AbortButton = cancelButton;
			layout.Add (cancelButton);
			
			this.DefaultButton = okButton = new Button ();
			okButton.Text = "O&k";
			okButton.Click += okButton_Click;
			layout.Add (okButton);
			
			
			layout.EndHorizontal ();
			layout.EndVertical ();

			Content = layout;

			PopulateList ();
		}
		
		private void fileTypeListBox_SelectedIndexChanged (Object sender, EventArgs e)
		{
			selected = fileTypeListBox.SelectedIndex >= 0 ? infos [fileTypeListBox.SelectedIndex] : null;
			okButton.Enabled = selected != null;
		}
		
		private void PopulateList ()
		{
			infos = new List<DocumentInfo> ();
			foreach (DocumentInfo info in settings.Infos.Values) {
				if (info.CanEdit)
					infos.Add (info);
			}
			foreach (DocumentInfo info in infos) {
				fileTypeListBox.Items.Add (info.Description);
			}
			fileTypeListBox.SelectedIndex = 0;
			selected = infos [fileTypeListBox.SelectedIndex];
		}
		
		private void cancelButton_Click (Object sender, EventArgs e)
		{
			this.Result = DialogResult.Cancel;
			this.Close ();
		}
		
		private void okButton_Click (Object sender, EventArgs e)
		{
			this.Result = DialogResult.Ok;
			this.Close ();
		}
	}
}

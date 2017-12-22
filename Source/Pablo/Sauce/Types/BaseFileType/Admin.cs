using System;
using Eto.Forms;
using System.Linq;
using System.Collections.Generic;

namespace Pablo.Sauce.Types.BaseFileType
{
	public class Admin<T> : Panel
		where T: DataTypeInfo
	{
		bool includeFileType;
		readonly List<SauceFileTypeInfo> fileTypes;

		protected new DynamicLayout Layout { get; private set; }

		public T DataType { get; private set; }

		public SauceInfo Sauce { get { return DataType.Sauce; } }

		public event EventHandler<EventArgs> FileTypeChanged;

		protected virtual void OnFileTypeChanged(EventArgs e)
		{
			if (FileTypeChanged != null)
				FileTypeChanged(this, e);
		}

		public SauceFileTypeInfo FileType
		{
			get;
			private set;
			
		}

		Control FileTypeComboBox()
		{
			var combo = new ComboBox();
			
			fileTypes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.CurrentCulture));
			combo.Items.AddRange(fileTypes.Cast<IListItem>());
			
			combo.SelectedKey = Convert.ToString(Sauce.ByteFileType);
			combo.SelectedIndexChanged += delegate
			{
				var fileType = (SauceFileTypeInfo)combo.SelectedValue;
				Sauce.ByteFileType = fileType.Type;
				FileType = fileType;
				OnFileTypeChanged(EventArgs.Empty);
			};
		
			return combo;
		}

		public Admin(T dataType, bool includeFileType = true)
		{
			DataType = dataType;
			fileTypes = DataType.FileTypes.ToList();
			FileType = fileTypes.FirstOrDefault(r => r.Type == Sauce.ByteFileType);
			this.includeFileType = includeFileType;
		}

		public override void OnPreLoad(EventArgs e)
		{
			base.OnPreLoad(e);
			RecreateLayout();
		}

		protected void RecreateLayout()
		{
			CreateControls();
			Layout.Add(null);
			Content = Layout;
		}

		protected virtual void CreateControls()
		{
			Layout = new DynamicLayout();
			if (includeFileType)
			{
				Layout.BeginHorizontal();
				Layout.Add(new Label { Text = "File Type", VerticalAlign = VerticalAlign.Middle });
				Layout.AddAutoSized(FileTypeComboBox());
				Layout.EndHorizontal();
			}
			foreach (var flag in DataType.Flags)
			{
				Layout.BeginHorizontal();
				Layout.Add(null, xscale: false);
				Layout.Add(flag.CreateControl());
				Layout.EndHorizontal();
			}
		}
	}
}


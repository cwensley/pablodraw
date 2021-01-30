using Eto.Drawing;
using Eto.Forms;
using Eto.IO;
using Pablo.Sauce;
using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;

namespace Pablo.Interface.Dialogs
{
	public class SauceInfoDialog : Dialog<DialogResult>
	{
		Panel fileTypeHolder;
		SauceInfo sauce;
		readonly EtoFileInfo file;
		readonly Document document;
		readonly bool allowRemove;
		readonly bool allowSave;
		readonly bool directSave;

		public SauceInfo Sauce
		{
			get { return sauce; }
		}

		public SauceInfoDialog(EtoFileInfo file, Document document, bool readOnly = false, bool directSave = true)
		{
			this.directSave = directSave;
			this.file = file;
			this.document = document;
			//DisplayMode = DialogDisplayMode.Attached;
			MinimumSize = new Size(400, 500);
			if (file == null && document == null)
				throw new ArgumentException("Must specify either file or document argument");
			Resizable = true;
			Title = "Sauce Metadata";
			
			allowRemove = !readOnly;
			allowSave = !readOnly;
			if (document != null)
			{
				if (document.Sauce != null)
					sauce = new SauceInfo(document.Sauce);
			}
			else if (file != null)
			{
				using (var stream = file.OpenRead())
				{
					sauce = SauceInfo.GetSauce(stream);
				}
				if (file.ReadOnly)
				{
					allowRemove = false;
					allowSave = false;
				}
			}
			
			if (sauce == null)
			{
				sauce = new SauceInfo();
				if (document != null)
					document.FillSauce(sauce, document.LoadedFormat);
				allowRemove = false;
			}
			
			CreateControls();
			UpdateDataType();
		}

		void RemoveSauce()
		{
			sauce = null;
			if (!directSave)
				return;
			if (document != null)
			{
				document.Sauce = null;
				document.IsModified = true;
			}
			else
			{
				bool hasSauce;
				using (var ms = new MemoryStream())
				{
					using (var stream = file.Open(FileMode.Open))
					{
						var ss = new SauceStream(stream);
						hasSauce = ss.Sauce != null;
						if (hasSauce)
							ss.WriteTo(ms);
					}
					if (hasSauce)
					{
						file.Delete();
						ms.Flush();
						ms.Seek(0, SeekOrigin.Begin);
						using (var newfile = file.Open(FileMode.CreateNew))
						{
							ms.WriteTo(newfile);
						}
					}
				}
			}
			
		}

		void Save()
		{
			if (!directSave)
				return;
			
			if (document != null)
			{
				document.Sauce = sauce;
				document.IsModified = true;
			}
			else
			{
				using (var ms = new MemoryStream())
				{
					using (var stream = file.Open(FileMode.Open))
					{
						var ss = new SauceStream(stream);
						ss.WriteTo(ms);
						sauce.SaveSauce(ms, false);
					}
					file.Delete();
					ms.Flush();
					ms.Seek(0, SeekOrigin.Begin);
					using (var newfile = file.Open(FileMode.CreateNew))
					{
						ms.WriteTo(newfile);
					}
				}
			}
		}

		void CreateControls()
		{
			fileTypeHolder = new Panel();

			var layout = new TableLayout(1, 3);
			layout.Padding = new Padding(10);
			
			layout.Add(EditorControls(), 0, 0, true, true);
			layout.Add(fileTypeHolder, 0, 1);
			layout.Add(Buttons(), 0, 2);

			Content = layout;
		}

		Control TypeComboBox()
		{
			var combo = new EnumDropDown<SauceDataType>();
			
			combo.SelectedValue = sauce.DataType;
			
			combo.SelectedIndexChanged += delegate
			{
				sauce.DataType = combo.SelectedValue;
				UpdateDataType();
			};
		
			return combo;
		}

		void UpdateDataType()
		{
			var ui = sauce.TypeInfo.GenerateUI();
			if (ui != null)
			{
				var gb = new GroupBox();
				gb.Text = "Type Options";
				gb.Content = ui;
				gb.Padding = new Padding(10);
				fileTypeHolder.Content = gb;
			}
			else
			{
				fileTypeHolder.Content = null;
			}
		}

		class MyLabel : Label
		{
			public MyLabel()
			{
				VerticalAlignment = VerticalAlignment.Center;
			}
		}

		Control EditorControls()
		{
			var layout = new DynamicLayout();
			layout.DefaultSpacing = new Size(10, 10);
			layout.DefaultPadding = Padding.Empty;
			
			layout.AddRow(new MyLabel { Text = "Title" }, TitleTextBox());
			layout.AddRow(new MyLabel { Text = "Group" }, GroupTextBox());
			layout.AddRow(new MyLabel { Text = "Author" }, AuthorTextBox());
			layout.AddRow(new MyLabel { Text = "Date Created" }, DateBox());
			layout.AddRow(new MyLabel { Text = "Data Type" }, TableLayout.AutoSized(TypeComboBox()));
			layout.AddRow(new MyLabel { Text = "Notes" }, CommentTextArea());
			
			return layout;
		}

		Control DateBox()
		{
			var control = new DateTimePicker
			{
				Mode = DateTimePickerMode.Date,
				Value = sauce.Date
			};
			control.ValueChanged += delegate
			{
				sauce.Date = control.Value;
			};
			return control;
		}

		Control CommentTextArea()
		{
			var control = new TextArea
			{
				Text = sauce.Comments != null ? string.Join(Environment.NewLine, sauce.Comments) : string.Empty,
				Size = new Size(80, 80),
				Wrap = false
			};
			var changing = false;
			control.TextChanged += delegate
			{

				if (!changing)
				{
					changing = true;
					sauce.Comments.Clear();
					if (!string.IsNullOrEmpty(control.Text))
					{
						var comments = control.Text.Split(new [] { Environment.NewLine }, StringSplitOptions.None).ToList();
						if (comments.Any(r => r.Length > SauceComment.CommentSize))
						{
							var pos = control.CaretIndex;
							for (int i = 0; i < comments.Count; i++)
							{
								if (comments[i].Length > SauceComment.CommentSize)
								{
									var ending = comments[i].Substring(SauceComment.CommentSize);
									comments.Insert(i+1, ending);
									comments[i] = comments[i].Substring(0, SauceComment.CommentSize);
									var totalLengthUpToLine = comments.Take(i+1).Sum(r => r.Length + Environment.NewLine.Length);
									if (pos > totalLengthUpToLine - Environment.NewLine.Length)
										pos += Environment.NewLine.Length;
								}
							}
							control.Text = string.Join(Environment.NewLine, comments);
							control.CaretIndex = pos;
						}
						foreach (var comment in comments)
							sauce.Comments.Add(comment);
					}
					changing = false;
				}
			};
			return control;
		}

		Control TitleTextBox()
		{
			var control = new TextBox
			{
				Text = sauce.Title,
				MaxLength = 35
			};
			control.TextChanged += delegate
			{
				sauce.Title = control.Text;
			};
			return control;
		}

		Control GroupTextBox()
		{
			var control = new TextBox
			{
				Text = sauce.Group,
				MaxLength = 20
			};
			control.TextChanged += delegate
			{
				sauce.Group = control.Text;
			};
			return control;
		}

		Control AuthorTextBox()
		{
			var control = new TextBox
			{
				Text = sauce.Author,
				MaxLength = 20
			};
			control.TextChanged += delegate
			{
				sauce.Author = control.Text;
			};
			return control;
		}

		Control Buttons()
		{
			var layout = new TableLayout(4, 1);
			layout.Padding = Padding.Empty;
			layout.SetColumnScale(1);

			layout.Add(RemoveSauceButton(), 0, 0);
			layout.Add(CancelButton(), 2, 0);
			layout.Add(SaveButton(), 3, 0);
			
			return layout;
		}

		Control RemoveSauceButton()
		{
			var control = new Button
			{
				Text = "Remove Sauce",
				Enabled = allowRemove
			};
			control.Click += delegate
			{
				RemoveSauce();
				Result = DialogResult.Ok;
				Close();
			};
			return control;
		}

		Control CancelButton()
		{
			var control = new Button
			{
				Text = "C&ancel"
			};
			control.Click += delegate
			{
				Close();
			};
			AbortButton = control;
			
			return control;
		}

		Control SaveButton()
		{
			var control = new Button
			{
				Text = "&Save",
				Enabled = allowSave
			};
			control.Click += delegate
			{
				Save();
				Result = DialogResult.Ok;
				Close();
			};
			DefaultButton = control;
			
			return control;
		}

		static T LabelControl<T>(TableLayout layout, T control, string label, int y)
			where T: Control
		{
			layout.Add(new Label{ Text = label, VerticalAlignment = VerticalAlignment.Center }, 0, y);
			layout.Add(control, 1, y);
			return control;
		}
	}
}


using System;
using Eto.Forms;

namespace Pablo.Sauce.Types.Character
{
	public class Admin<T> : BaseText.Admin<T>
		where T: DataTypeInfo
	{
		public Admin(T dataType)
			: base(dataType, true)
		{
		}

		protected override void CreateControls()
		{
			base.CreateControls();
			if (DataType.HasDimensions)
			{
				Layout.BeginHorizontal();
				Layout.Add(new Label { Text = "Width", VerticalAlign = VerticalAlign.Middle });
				Layout.BeginVertical(Eto.Drawing.Padding.Empty);
				Layout.BeginHorizontal();
				Layout.Add(WidthControl(), xscale: true);
				Layout.Add(new Label { Text = "Height", VerticalAlign = VerticalAlign.Middle }); 
				Layout.Add(HeightControl(), xscale: true);
				Layout.EndHorizontal();
				Layout.EndVertical();
				Layout.EndHorizontal();
			}
			if (DataType.HasNumberOfColors)
			{
				Layout.AddRow(new Label { Text = "Number of Colors", VerticalAlign = VerticalAlign.Middle }, NumberOfColors());
			}
		}

		Control WidthControl()
		{
			var control = new NumericUpDown
			{
				MinValue = 0,
				MaxValue = ushort.MaxValue,
				Value = DataType.Width
			};
			control.ValueChanged += delegate
			{
				//ushort val;
				DataType.Width = (ushort)control.Value; //ushort.TryParse(control.Text, out val) ? val : (ushort)0;
			};
			return control;
		}

		Control HeightControl()
		{
			var control = new NumericUpDown
			{
				//Text = DataType.Height > 0 ? DataType.Height.ToString() : string.Empty
				MinValue = 0,
				MaxValue = ushort.MaxValue,
				Value = DataType.Height
			};
			control.ValueChanged += delegate
			{
				DataType.Height = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, control.Value));
			};
			return control;
		}

		Control NumberOfColors()
		{
			var control = new NumericUpDown
			{
				MinValue = 0,
				MaxValue = ushort.MaxValue,
				Value = DataType.NumberOfColors,
			};
			control.ValueChanged += delegate
			{
				DataType.NumberOfColors = (ushort)Math.Max(0, Math.Min(ushort.MaxValue, control.Value));
			};
			return control;
		}

		protected override void OnFileTypeChanged(EventArgs e)
		{
			base.OnFileTypeChanged(e);
			if (!DataType.HasDimensions)
			{
				DataType.Width = 0;
				DataType.Height = 0;
			}
			if (!DataType.HasNumberOfColors)
			{
				DataType.NumberOfColors = 0;
			}
			RecreateLayout();
		}
	}
}


using System;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Formats.Animated
{
	/// <summary>
	/// Summary description for AnimatedDocumentInfo.
	/// </summary>
	public abstract class AnimatedDocumentInfo : DocumentInfo
	{
		private long baudRate = 115200;
		bool autoDetectAnimation = true;
		bool animationEnabled = true;
		private BaudRateMapCollection baudRateMap = null;
		
		public event EventHandler<EventArgs> BaudRateChanged;

		protected AnimatedDocumentInfo(string id, string description) : base(id, description)
		{
		}

		public long Baud
		{
			get { return baudRate; }
			set { baudRate = value; }
		}

		public bool AnimationEnabled
		{
			get { return animationEnabled; }
			set { animationEnabled = value; }
		}

		public bool AutoDetectAnimation
		{
			get { return autoDetectAnimation; }
			set { autoDetectAnimation = value; }
		}

		public override IEnumerable<DocumentInfoOption> Options {
			get { return base.Options.Concat (GetOptions()); }
		}

		IEnumerable<DocumentInfoOption> GetOptions()
		{
			//yield return new DocumentInfoOption { ID = "animation", Comment = "Turns on ansimation" };
			yield break;
		}

		public override bool SetOption (string option, string value)
		{
			switch (option.ToLowerInvariant())
			{
			case "animation":
				bool animation;
				if (bool.TryParse(value, out animation))
				{
					AnimationEnabled = animation;
					return true;
				}
				break;
			}
			return base.SetOption (option, value);
		}

		public override void GenerateCommands(GenerateCommandArgs args)
		{
			base.GenerateCommands(args);

			bool editMode = args.EditMode;
			string area = args.Area;
			if (area == "main")
			{
				if (!editMode)
				{
					baudRateMap = new BaudRateMapCollection();
#if DEBUG
#endif
					baudRateMap.Add("Fastest", 0);
					baudRateMap.Add(460800);
					baudRateMap.Add(230400);
					baudRateMap.Add(115200);
					baudRateMap.Add(57600);
					baudRateMap.Add(38400);
					baudRateMap.Add(33600);
					baudRateMap.Add(28800);
					baudRateMap.Add(19200);
					baudRateMap.Add(14400);
					baudRateMap.Add(9600);
					baudRateMap.Add(2400);
					baudRateMap.Add(1200);
					baudRateMap.Add(300);
		
					
					var animAuto = new CheckCommand { ID = "animAuto", MenuText = "&Auto Detect", Checked = autoDetectAnimation };
					animAuto.CheckedChanged += AutoDetect_CheckedChanged;
		
					var animEnabled = new CheckCommand { ID = "animEnabled", MenuText = "&Enabled", Checked = animationEnabled };
					animEnabled.CheckedChanged += AnimEnabled_CheckedChanged;


					var aiView = args.Menu.Items.GetSubmenu("&View");

					var aiAnim = aiView.Items.GetSubmenu("&Animate", 600);
		
					aiAnim.Items.Add(animEnabled, 500);
					aiAnim.Items.Add(animAuto, 500);
					aiAnim.Items.AddSeparator(500);
		
					foreach (BaudRateMap brm in baudRateMap)
					{
						brm.Command.Checked = (baudRate == brm.Baud);
						aiAnim.Items.Add(brm.Command, 500);
					}
					baudRateMap.BaudChanged += baudRateMap_BaudChanged;
				}
			}

		}

		private void AutoDetect_CheckedChanged(object sender, EventArgs e)
		{
			var action = (CheckCommand)sender;
			this.autoDetectAnimation = action.Checked;
		}

		private void baudRateMap_BaudChanged(object sender, EventArgs e)
		{
			var action = (Command)sender;
			BaudRateMap brm = (BaudRateMap)action.Tag;
			this.baudRate = brm.Baud;
			//Console.WriteLine("baud set: {0}", this.baudRate);
			if (BaudRateChanged != null) BaudRateChanged(this, e);
			// changed baud rate, get value!
		}

		private void AnimEnabled_CheckedChanged(object sender, EventArgs e)
		{
			var action = (CheckCommand)sender;
			this.animationEnabled = action.Checked;
		}

		public override void ReadXml(System.Xml.XmlElement element)
		{
			base.ReadXml (element);

			if (element.HasAttribute("baudRate")) baudRate = Convert.ToInt32(element.GetAttribute("baudRate"));
			if (element.HasAttribute("autoDetectAnimation")) autoDetectAnimation = Convert.ToBoolean(element.GetAttribute("autoDetectAnimation"));
			if (element.HasAttribute("animationEnabled")) animationEnabled = Convert.ToBoolean(element.GetAttribute("animationEnabled"));
		}

		public override void WriteXml(System.Xml.XmlElement element)
		{
			base.WriteXml (element);
			element.SetAttribute("baudRate", baudRate.ToString());
			element.SetAttribute("autoDetectAnimation", autoDetectAnimation.ToString());
			element.SetAttribute("animationEnabled", animationEnabled.ToString());
		}
	}
}

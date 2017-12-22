using Eto.Drawing;
using System;
using System.IO;
using Pablo.Formats.Image;
using Pablo.Network;
using System.Runtime.InteropServices;
using Pablo.BGI;
using System.Collections.Generic;
using Eto;

namespace Pablo.Formats.Rip
{
	public class RipDocument : Animated.AnimatedDocument
	{
		IndexedBitmap image;
		string text;
		List<RipCommand> commands = new List<RipCommand>();
		HashSet<string> optionalApplied = new HashSet<string>();

		public IList<RipCommand> Commands
		{
			get { return commands; }
		}

		public BGICanvas.BGIImage RipImage { get; set; }

		public Palette Palette
		{
			get { return BGI != null ? BGI.Palette : null; }
		}

		public HashSet<string> OptionalApplied
		{
			get { return optionalApplied; }
		}

		public override void PreLoad(Stream stream, Format format, Handler handler)
		{
			base.PreLoad(stream, format, handler);
			SetBGI(handler as RipHandler);
		}

		public T Create<T>()
			where T: RipCommand
		{
			return RipCommands.Create<T>(this);
		}

		public T Create<T, CT>()
			where T: RipCommand, new()
			where CT: RipCommandType
		{
			return RipCommands.Create<T, CT>(this);
		}

		public BGICanvas BGI { get; set; }

		public RipDocument(DocumentInfo info) : base(info)
		{
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);
			if (this.image == null)
			{
				var image = new IndexedBitmap(640, 350, 8);
				var pal = Palette.GetEgaPalette();
				while (pal.Count < image.Palette.Count)
					pal.Add(Colors.Black);
				image.Palette = pal;
				this.image = image;
			}
		}

		public override Handler CreateHandler()
		{
			return new RipHandler(this);
		}

		public new RipDocumentInfo Info
		{
			get { return (RipDocumentInfo)base.Info; }
		}

		public WaitEventHandler Wait;

		public override void SetWaitHandler(WaitEventHandler waitHandler)
		{
			Wait += waitHandler;

		}

		public void OnWait(WaitEventArgs args)
		{
			if (Wait != null)
				Wait(this, args);
		}

		protected override void Dispose(bool disposing)
		{
			if (image != null)
				image.Dispose();
			base.Dispose(disposing);
		}

		public override Size Size
		{
			get { return image.Size; }
		}

		public IndexedBitmap Image
		{
			get { return image; }
			set
			{
				lock (this)
				{
					if (image != null)
						image.Dispose();
					image = value;
				}
			}
		}

		public string Text
		{
			get { return text; }
			set { text = value; }
		}

		public override void PostLoad(Handler handler)
		{
			base.PostLoad(handler);
			if (this.EditMode && this.IsNew)
			{
				Commands.Add(Create<Commands.KillMouseFields>());
				Commands.Add(Create<Commands.ResetWindows>());
			}
		}

		protected override void LoadingAnimated(Stream stream, Format format, Handler handler)
		{
			base.LoadingAnimated(stream, format, handler);
			
			SetBGI(handler as RipHandler);
		}

		void SetBGI(RipHandler handler)
		{
			var pane = handler != null && handler.HasViewerControl ? handler.ViewerControl as ViewerPane : null;
			var viewer = pane != null ? pane.Viewer : null;
			
			if (BGI != null)
				BGI.Control = viewer;
			else
				BGI = new BGICanvas(viewer);
		}

		protected override void LoadStream(Stream stream, Format format, Handler handler)
		{
			SetBGI(handler as RipHandler);

			
			var formatRip = (FormatRip)format;
			formatRip.Load(stream, this, handler as RipHandler);
			
		}

		protected override void SaveStream(Stream stream, Format format, Handler handler)
		{
			var formatRip = (FormatRip)format;
			formatRip.Save(stream, this);
		}

		protected override void EnsureSauce(Format format, Handler handler)
		{
			base.EnsureSauce(format, handler);
			if (Sauce != null)
			{
				Sauce.DataType = Pablo.Sauce.SauceDataType.Character;
				var info = Sauce.TypeInfo as Sauce.Types.Character.DataTypeInfo;
				if (info != null)
				{
					info.Type = Pablo.Sauce.Types.Character.CharacterFileType.Rip;
					info.Width = (ushort)BGI.WindowSize.Width;
					info.Height = (ushort)BGI.WindowSize.Height;
					info.NumberOfColors = (ushort)BGI.Palette.Count;
				}
			}
		}

		public override Document ConvertDocument(DocumentInfo targetDocumentInfo, Handler handler)
		{
			if (targetDocumentInfo is ImageDocumentInfo)
			{
				var doc = (ImageDocument)targetDocumentInfo.Create(this.Generator);
				doc.Image = handler.Generate();
				return doc;
			}
			return base.ConvertDocument(targetDocumentInfo, handler);
		}

		public override bool IsModified { get; set; }

		public override bool Send(SendCommandArgs args)
		{
			base.Send(args);
			args.Message.Write(this.Commands);
			return true;
		}

		public override void Receive(ReceiveCommandArgs args)
		{
			base.Receive(args);
			SetBGI(null);
			var updates = new List<Rectangle>();
			BGI.GraphDefaults(updates);
			commands.Clear();
			args.Message.ReadCommands(this, commands);
			foreach (var command in commands)
			{
				updates.Clear();
				command.Apply(updates);
			}
		}
	}
}

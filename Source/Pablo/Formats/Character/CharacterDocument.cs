using Eto.Drawing;
using System;
using System.IO;
using Pablo.Formats.Image;
using Pablo.Network;

namespace Pablo.Formats.Character
{
	public class CharacterDocument : Animated.AnimatedDocument
	{
		Page[] pages;
		readonly Size defaultSize;
		BitFont defaultFont;
		readonly Palette defaultPalette;
		bool resizeCanvas = true;
		public const int EditSize = 10000;
		bool? use9x;
		bool? dosAspect;
		bool? iceColours;

		public event EventHandler<EventArgs> ICEColoursChanged;
		public event EventHandler<EventArgs> Use9xChanged;
		public event EventHandler<EventArgs> DosAspectChanged;

		public bool Use9x
		{
			get { return use9x ?? Info.Use9x; }
			set
			{
				if (value != use9x)
				{
					use9x = value;
					SetFont(false);
					if (Use9xChanged != null)
						Use9xChanged(this, EventArgs.Empty);
				}
			}
		}

		public override bool EditMode
		{
			get
			{
				return base.EditMode;
			}
			set
			{
				if (value != base.EditMode)
				{
					base.EditMode = value;
					UpdateCanvasSize(Pages[0], true);
				}
			}
		}

		public bool DosAspect
		{
			get { return dosAspect ?? Info.DosAspect; }
			set
			{
				if (value != dosAspect)
				{
					dosAspect = value;
					OnSizeChanged(EventArgs.Empty);
					if (DosAspectChanged != null)
						DosAspectChanged(this, EventArgs.Empty);
				}
			}
		}

		public bool ICEColours
		{
			get { return iceColours ?? Info.iCEColours; }
			set
			{
				if (iceColours != value)
				{
					iceColours = value;
					if (ICEColoursChanged != null)
						ICEColoursChanged(this, EventArgs.Empty);
				}
			}
		}

		public bool IsUsingStandard8x16Font
		{
			get
			{
				var font = Pages[0].Font;
				return font.IsStandardFont && font.Height == 16 && font.Width == 8 && font.CodePage == BitFont.StandardCodePage;
			}
		}

		public event EventHandler<EventArgs> SizeChanged;

		protected virtual void OnSizeChanged(EventArgs e)
		{
			if (SizeChanged != null)
				SizeChanged(this, e);
		}

		public CharacterDocument(DocumentInfo info) : base(info)
		{
			defaultFont = BitFont.GetStandard8x16();
			defaultSize = new Size(80, EditSize);
			defaultPalette = Palette.GetDosPalette();
			Initialize(1, defaultSize);
			
			Info.Use9xChanged += Info_Use9xChanged;
			Info.SelectedFontChanged += Info_SelectedFontChanged;
			SetFont(true);
		}

		protected override void Dispose(bool disposing)
		{
			Info.Use9xChanged -= Info_Use9xChanged;
			Info.SelectedFontChanged -= Info_SelectedFontChanged;
			base.Dispose(disposing);
		}

		public override Handler CreateHandler()
		{
			return new CharacterHandler(this);
		}

		public new CharacterDocumentInfo Info
		{
			get { return (CharacterDocumentInfo)base.Info; }
		}

		public override Document ConvertDocument(DocumentInfo targetDocumentInfo, Handler handler)
		{
			if (targetDocumentInfo is ImageDocumentInfo)
			{
				int endy = (Pages[0].Canvas.FindEndY(CanvasElement.Default) + 1) * Pages[0].Font.Height;
				var rect = new Rectangle(0, 0, Size.Width * Pages[0].Font.Width, endy);
				var chandler = (CharacterHandler)handler;
				var oldallow = chandler.AllowEditing;
				chandler.AllowEditing = false;
				var doc = (ImageDocument)targetDocumentInfo.Create(Generator);
				doc.Image = handler.Generate(rect);
				chandler.AllowEditing = oldallow;
				return doc;
			}
			return base.ConvertDocument(targetDocumentInfo, handler);
		}
		/*
		public CharacterDocument(Size defaultSize, BitFont defaultFont, Palette defaultPalette)
		{
			this.Handler = new CharacterHandler(this);
			this.defaultFont = defaultFont;
			this.defaultSize = defaultSize;
			this.defaultPalette = defaultPalette;
			Initialize(1, defaultSize);
		}
		*/
		void Initialize(int numPages, Size canvasSize)
		{
			pages = new Page[numPages];
			for (int i = 0; i < numPages; i++)
			{
				var page = new Page(this, canvasSize, defaultFont, defaultPalette);
				pages[i] = page;
			}
		}

		public bool ResizeCanvas
		{
			get { return resizeCanvas; }
			
		}

		public override Size Size
		{
			get { return pages[0].Canvas.Size; }
		}

		public Page[] Pages
		{
			get { return pages; }
		}

		protected override void LoadingAnimated(Stream stream, Format format, Handler handler)
		{
			base.LoadingAnimated(stream, format, handler);
			
			var canAnimate = true;
			var charFormat = format as CharacterFormat;
			if (charFormat != null)
			{
				canAnimate = charFormat.CanAnimate;
			}

			if (canAnimate && AnimateView && !EditMode)
			{
				// when animating, limit to size of text screen
				Pages[0].Canvas.ResizeCanvas(new Size(80, 25), true);
				resizeCanvas = false;
			}
			else
			{
				Pages[0].Canvas.ResizeCanvas(new Size(80, EditSize), false);
			}
		}

		protected override void SaveStream(Stream stream, Format format, Handler handler)
		{
			var charFormat = (CharacterFormat)format;
			charFormat.Save(stream, this);
		}

		protected override void EnsureSauce(Format format, Handler handler)
		{
			base.EnsureSauce(format, handler);
			var charFormat = (CharacterFormat)format;
			charFormat.EnsureSauce(this);
		}

		protected override void LoadStream(Stream stream, Format format, Handler handler)
		{
			resizeCanvas = true;
			var charFormat = (CharacterFormat)format;
			use9x = charFormat.Use9pxFont;
			
			
			IsModified = false;
			if (Sauce != null)
			{
				var info = Sauce.TypeInfo as Sauce.Types.BaseText.DataTypeInfo;
				if (info != null)
				{
					var font = info.GetFont(Info.GetFonts());
					if (font != null)
						Pages[0].Font = font;
					if (info.HasAspectRatio && info.AspectRatio != null)
						DosAspect = info.AspectRatio == true;
					if (info.HasICEColors)
						ICEColours = info.ICEColors;
					if (info.HasLetterSpacing && info.LetterSpacing != null)
						Use9x = info.LetterSpacing == true;
				}

			}
			SetFont(false, false);
			Pages[0].Palette = defaultPalette;
			Pages[0].Load(stream, charFormat, (CharacterHandler)handler, resizeCanvas);
			
			UpdateCanvasSize(Pages[0], Info.AutoResize);
			Pages[0].Canvas.Update += delegate
			{
				IsModified = true;
			};
		}

		void UpdateCanvasSize(Page page, bool resizeNonEdit)
		{
			if (EditMode)
			{
				Size size = page.Canvas.Size;
				if (size.Height < EditSize)
				{
					size.Height = EditSize;
					page.Canvas.ResizeCanvas(size, true);
				}
			}
			else if (resizeNonEdit)
			{
				Size size = page.Canvas.Size;
				size.Height = page.Canvas.FindEndY(CanvasElement.Default) + 1;
				page.Canvas.ResizeCanvas(size, true);
			}
		}

		public override void SetWaitHandler(WaitEventHandler waitHandler)
		{
			Wait += waitHandler;
		}

		public WaitEventHandler Wait;

		public void OnWait(WaitEventArgs args)
		{
			if (Wait != null)
				Wait(this, args);
		}

		void Info_Use9xChanged(object sender, EventArgs e)
		{
			SetFont(false);
		}

		void Info_SelectedFontChanged(object sender, EventArgs e)
		{
			SetFont(true);
		}

		public void SetFont(BitFont font)
		{
			Pages[0].Font = font;
			SetFont(false);
		}

		internal void SetFont(bool reload, bool sendEvent = true)
		{
			if (Pages[0] == null)
				return;
			if (reload)
			{
				defaultFont = Info.SelectedFont;
				if (!EditMode && Sauce != null)
				{
					var info = Sauce.TypeInfo as Sauce.Types.BaseText.DataTypeInfo;
					if (info != null)
					{
						defaultFont = info.GetFont(Info.GetFonts()) ?? defaultFont;
					}
				}
			}
			else
				defaultFont = Pages[0].Font;
			if (defaultFont.Width >= 8 && defaultFont.Width <= 9)
			{
				int newWidth = Use9x ? 9 : 8;
				if (newWidth != defaultFont.Width)
				{
					defaultFont = new BitFont(defaultFont);
					defaultFont.Resize(newWidth, defaultFont.Height, false, Use9x);
				}
				Pages[0].Font = defaultFont;
			}
			if (sendEvent)
				OnSizeChanged(EventArgs.Empty);
		}

		public override bool IsModified
		{
			get;
			set;
		}

		public override bool Send(SendCommandArgs args)
		{
			base.Send(args);
			args.Message.Write(ICEColours);
			args.Message.Write(Use9x);
			args.Message.Write(DosAspect);
			var page = Pages[0];
			args.Message.Write(page.Canvas.Size);
			args.Message.WritePadBits();
			using (var stream = new MemoryStream())
			{
			
				var type = new Types.Pablo(Info);
				type.Save(stream, this);
				stream.Flush();
				stream.Seek(0, SeekOrigin.Begin);
				
				args.Message.WriteStream(stream);
			}
			return true;
		}

		public override void Receive(ReceiveCommandArgs args)
		{
			base.Receive(args);
			ICEColours = args.Message.ReadBoolean();
			Use9x = args.Message.ReadBoolean();
			DosAspect = args.Message.ReadBoolean();
			var page = Pages[0];
			var canvasSize = args.Message.ReadSize();
			args.Message.ReadPadBits();
			page.Canvas.ResizeCanvas(canvasSize, false);
			var stream = args.Message.ReadStream();
			var type = new Types.Pablo(Info);
			resizeCanvas = !EditMode;
			type.Load(stream, this, null);
			if (Use9x)
			{
				SetFont(false);
			}
		}
	}
}

using System.IO;
using System.Text;
using Eto.Drawing;
using System.Collections.Generic;
using Pablo.Sauce;
using System;

namespace Pablo.Formats.Character.Types
{
	public class XBin : CharacterFormat
	{
		[Flags]
		enum CompressionType
		{
			NoCompression = 0x00,
			Character = 0x40,
			Attribute = 0x80,
			Both = 0xc0
		}

		readonly FormatParameter fpEnableCompression;

		public override IEnumerable<FormatParameter> GetParameters(SauceInfo sauce)
		{
			yield return fpEnableCompression;
		}

		public XBin(DocumentInfo info) : base(info, "xbin", "XBIN", "xb", "xbin")
		{
			fpEnableCompression = new FormatParameter("compress", "Enable Compression", typeof(bool), true);
		}

		public override void FillSauce(Sauce.SauceInfo sauce, CharacterDocument document)
		{
			base.FillSauce(sauce, document);
			sauce.ByteFileType = 0;
			FillFlags(sauce, document);
		}

		public override SauceDataType GetSauceDataType(CharacterDocument document)
		{
			return SauceDataType.XBIN;
		}

		public override bool CanSave
		{
			get { return true; }
		}

		public override bool CanAnimate
		{
			get { return false; }
		}

		public bool EnableCompression
		{
			get { return Convert.ToBoolean(fpEnableCompression.Value); }
			set { fpEnableCompression.Value = value; }
		}

		class XBinHeader
		{
			public const string XBIN_ID = "XBIN";
			byte[] xbinID;

			public byte EofChar { get; set; }

			public ushort Width { get; set; }

			public ushort Height { get; set; }

			public byte FontSize { get; set; }

			public byte Flags { get; set; }

			public XBinHeader()
			{
				XBinID = XBIN_ID;
				Flags = 0;
				FontSize = 16;
				Width = 80;
				Height = 25;
				EofChar = (byte)26;
			}

			public XBinHeader(BinaryReader br)
			{
				xbinID = br.ReadBytes(4);
				EofChar = br.ReadByte();
				Width = br.ReadUInt16();
				Height = br.ReadUInt16();
				FontSize = br.ReadByte();
				Flags = br.ReadByte();
			}

			public string XBinID
			{
				get { return Encoding.ASCII.GetString(xbinID); }
				set { xbinID = Encoding.ASCII.GetBytes(value); }
			}

			public bool Palette
			{
				get { return (Flags & 0x01) != 0; }
				set
				{
					if (value)
						Flags |= (byte)0x01;
					else
						Flags &= unchecked((byte)~0x01);
				}
			}

			public bool Font
			{
				get { return (Flags & 0x02) != 0; }
				set
				{
					if (value)
						Flags |= (byte)0x02;
					else
						Flags &= unchecked((byte)~0x02);
				}
			}

			public bool Compress
			{
				get { return (Flags & 0x04) != 0; }
				set
				{
					if (value)
						Flags |= (byte)0x04;
					else
						Flags &= unchecked((byte)~0x04);
				}
			}

			public bool NonBlink
			{
				get { return (Flags & 0x08) != 0; }
				set
				{
					if (value)
						Flags |= (byte)0x08;
					else
						Flags &= unchecked((byte)~0x08);
				}
			}

			public bool Font512
			{
				get { return (Flags & 0x10) != 0; }
				set
				{
					if (value)
						Flags |= (byte)0x10;
					else
						Flags &= unchecked((byte)~0x10);
				}
			}

			public void Write(BinaryWriter bw)
			{
				bw.Write(xbinID);
				bw.Write(EofChar);
				bw.Write(Width);
				bw.Write(Height);
				bw.Write(FontSize);
				bw.Write(Flags);
			}
		}

		static void WriteCompressedRow(Canvas canvas, int y, BinaryWriter bw, Func<CanvasElement, CanvasElement> translate)
		{
			var runMode = CompressionType.NoCompression;
			int runCount = 0;
			var runce = CanvasElement.Default;
			var buf = new byte[2 * 64];
			int bufIndex = 0;
			
			for (int x=0; x<canvas.Width; x++)
			{
				
				var ce = translate(canvas[x, y]);
				if (runCount > 0)
				{
					bool endRun = false;
					if (runCount == 64)
						endRun = true;
					else if (runCount > 0)
					{
						switch (runMode)
						{
							case CompressionType.NoCompression:
								if (x < canvas.Width - 1 && ce == translate(canvas[x + 1, y]))
								{
									endRun = true;
								}
								else if (x < canvas.Width - 2)
								{
									if (ce.Character == translate(canvas[x + 1, y]).Character && ce.Character == translate(canvas[x + 2, y]).Character)
										endRun = true;
									else if (ce.Attribute == translate(canvas[x + 1, y]).Attribute && ce.Attribute == translate(canvas[x + 2, y]).Attribute)
										endRun = true;
								}
								break;
							case CompressionType.Character:
								if (ce.Character != runce.Character)
									endRun = true;
								else if (x < canvas.Width - 2)
								{
									if (ce == translate(canvas[x + 1, y]) && ce == translate(canvas[x + 2, y]))
										endRun = true;
								}
								break;
							case CompressionType.Attribute:
								if (ce.Attribute != runce.Attribute)
									endRun = true;
								else if (x < canvas.Width - 2)
								{
									endRun |= ce == translate(canvas[x + 1, y]) && ce == translate(canvas[x + 2, y]);
								}
								break;
							case CompressionType.Both:
								endRun |= ce != runce;
								break;
						}
					}
					
					if (endRun)
					{
						bw.Write((byte)((int)runMode | (runCount - 1)));
						bw.Write(buf, 0, bufIndex);
						runCount = 0;
					}
				}

				if (runCount > 0)
				{
					switch (runMode)
					{
						case CompressionType.NoCompression:
							buf[bufIndex++] = ce.Character;
							buf[bufIndex++] = (byte)ce.Attribute;
							break;
						case CompressionType.Character:
							buf[bufIndex++] = (byte)ce.Attribute;
							break;
						case CompressionType.Attribute:
							buf[bufIndex++] = ce.Character;
							break;
						case CompressionType.Both:
						// nothing
							break;
					}	
				}
				else
				{
					bufIndex = 0;
					if (x < canvas.Width - 1)
					{
						var nextce = translate(canvas[x + 1, y]);
						if (ce == nextce)
						{
							runMode = CompressionType.Both;
						}
						else if (ce.Character == nextce.Character)
						{
							runMode = CompressionType.Character;
						}
						else if (ce.Attribute == nextce.Attribute)
						{
							runMode = CompressionType.Attribute;
						}
						else
							runMode = CompressionType.NoCompression;
					}
					else
						runMode = CompressionType.NoCompression;
					if (runMode == CompressionType.Attribute)
					{
						buf[bufIndex++] = (byte)ce.Attribute;
						buf[bufIndex++] = ce.Character;
					}
					else
					{
						buf[bufIndex++] = ce.Character;
						buf[bufIndex++] = (byte)ce.Attribute;
					}
					
					runce = ce;
				}
				runCount++;
			}
			if (runCount > 0)
			{
				bw.Write((byte)((int)runMode | (runCount - 1)));
				bw.Write(buf, 0, bufIndex);
				runCount = 0;
			}
		}

		public override void Save(Stream stream, CharacterDocument document)
		{
			var bw = new BinaryWriter(stream);
			
			var page = document.Pages[0];
			var canvas = page.Canvas;
			int lasty = canvas.FindEndY(CanvasElement.Default);
			
			var xbh = new XBinHeader
			{
				Width = (ushort)canvas.Size.Width,
				Height = (ushort)(lasty + 1),
				Compress = EnableCompression,
				NonBlink = document.ICEColours
			};
			if (page.Palette.Count == 16 && page.Palette != Palette.GetDosPalette())
				xbh.Palette = true;
			xbh.Font = true;
			if (xbh.Font)
			{
				xbh.FontSize = (byte)page.Font.Size.Height;
				xbh.Font512 |= page.Font.NumChars > 256;
			}
			xbh.Write(bw);
			
			if (xbh.Palette)
				page.Palette.Save(bw, 2);
			
			if (xbh.Font)
				page.Font.Save(bw);

			CanvasElement Translate(CanvasElement element)
			{
				if (xbh.Font512 && element.Character >= 256)
				{
					element.Character -= 256;
					element.Attribute.Bold = true;
				}
				return element;
			}
			
			if (xbh.Compress)
			{
				for (int y=0; y <= lasty; y++)
				{
					WriteCompressedRow(canvas, y, bw, Translate);
				}
			}
			else
			{
				for (int y=0; y <= lasty; y++)
					for (int x=0; x < canvas.Width; x++)
					{
						var ce = Translate(canvas[x, y]);
						bw.Write((byte)ce.Character);
						bw.Write((byte)ce.Attribute);
					}
			}
			
			bw.Flush();
		}

		protected override int? GetWidth(Stream stream, CharacterDocument document, object state = null)
		{
			var header = state as XBinHeader;
			return header.Width;
		}

		public override void Load(Stream fs, CharacterDocument document, CharacterHandler handler)
		{
			var page = document.Pages[0];

			var br = new BinaryReader(fs);
			try
			{
				var header = new XBinHeader(br);
				if (header.XBinID != XBinHeader.XBIN_ID)
					throw new FileLoadException("not a valid XBIN file");
				var loadSize = new Size(0, 0);
				var canvas = page.Canvas;
				document.ICEColours = header.NonBlink;
				ResizeCanvasWidth(fs, document, canvas, header);

				loadSize.Width = (document.EditMode && header.Width > canvas.Size.Width) ? canvas.Size.Width : header.Width;
				loadSize.Height = (document.EditMode && header.Height > canvas.Size.Height) ? canvas.Size.Height : header.Height;

				CanvasElement Translate(CanvasElement element)
				{
					if (header.Font512 && element.Attribute.Bold)
					{
						element.Attribute.Bold = false;
						element.Character += 256;
					}
					return element;
				}

				if (header.Palette)
				{
					page.Palette.Load(br, 16, 2);
				}

				if (header.Font)
				{
					var f = new BitFont(header.Font512 ? 512 : 256, 8, header.FontSize, BitFont.StandardCodePage);
					f.Load(br);
#if DEBUG
					/* Used to save an XBIN font for use in PabloDraw
					 * 
					Stream s = new FileStream("c:\\font.fnt", FileMode.Create);
					BinaryWriter bw = new BinaryWriter(s);
					f.Save(bw);
					bw.Close();
					s.Close();
					/*
					 */
#endif
					page.Font = f;
				}
				
				ResizeCanvasHeight(document, canvas, loadSize.Height);

				var ce = new CanvasElement(32, 7);
				var args = new WaitEventArgs();
				if (header.Compress)
				{
					for (int y=0; y<loadSize.Height; y++)
					{
						int x = 0;
						while (x < header.Width)
						{
							document.OnWait(args);
							if (args.Exit)
								break;
							int countbyte = br.ReadByte();
							if (countbyte == -1)
								break;
							int runlength = (countbyte & 0x3F) + 1;
							switch ((CompressionType)(countbyte & 0xc0))
							{
								case CompressionType.NoCompression:
									while (runlength > 0)
									{
										ce.Character = br.ReadByte();
										ce.Attribute = br.ReadByte();
										if (x < canvas.Size.Width)
											canvas[x, y] = Translate(ce);
										x++;
										runlength--;
									}
									break;
								case CompressionType.Character:
									ce.Character = br.ReadByte();
									while (runlength > 0)
									{
										ce.Attribute = br.ReadByte();
										if (x < canvas.Size.Width)
											canvas[x, y] = Translate(ce);
										x++;
										runlength--;
									}
									break;
								case CompressionType.Attribute:
									ce.Attribute = br.ReadByte();
									while (runlength > 0)
									{
										ce.Character = br.ReadByte();
										if (x < canvas.Size.Width)
											canvas[x, y] = Translate(ce);
										x++;
										runlength--;
									}
									break;
								case CompressionType.Both:
									ce.Character = br.ReadByte();
									ce.Attribute = br.ReadByte();
									while (runlength > 0)
									{
										if (x < canvas.Size.Width)
											canvas[x, y] = Translate(ce);
										x++;
										runlength--;
									}
									break;
							}
						}
					}
				}
				else
				{
					for (int y=0; y<loadSize.Height; y++)
					{
						for (int x=0; x<header.Width; x++)
						{
							document.OnWait(args);
							if (args.Exit)
								break;
							ce.Character = br.ReadByte();
							ce.Attribute = br.ReadByte();
							if (x < canvas.Size.Width)
								canvas[x, y] = Translate(ce);
						}
					}
				}
			}
			catch (EndOfStreamException)
			{
				// reached end of file, so we're okay
			}
		}
	}
}

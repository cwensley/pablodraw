using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using Pablo.Network;
using Eto.Forms;

namespace Pablo.Sauce
{
	public abstract class SauceFlag
	{
		protected SauceInfo Sauce { get; private set; }

		protected SauceFlag(SauceInfo sauce, string description)
		{
			Description = description;
			Sauce = sauce;
		}

		public string Description { get; set; }

		public abstract object Value { get; set; }

		public abstract Control CreateControl();
	}

	public class SauceBitFlag : SauceFlag
	{
		readonly int flag;

		public SauceBitFlag(SauceInfo sauce, string description, int flag)
			: base(sauce, description)
		{
			this.flag = flag;
		}

		byte Mask
		{
			get { return (byte)(0x01 << flag); }
		}

		public override object Value
		{
			get { return BoolValue; }
			set { BoolValue = (bool)value; }
		}

		public bool BoolValue
		{
			get { return (Sauce.ByteFlags & Mask) != 0; }
			set
			{
				if (value)
					Sauce.ByteFlags |= Mask;
				else
					Sauce.ByteFlags &= (byte)~Mask;
			}
		}

		public override Control CreateControl()
		{
			var control = new CheckBox
			{
				Text = Description,
				Checked = BoolValue
			};
			control.CheckedChanged += delegate
			{
				BoolValue = control.Checked ?? false;
			};
			return control;
		}
	}

	public class SauceTwoBitFlag : SauceFlag
	{
		readonly int start;
		readonly int nullValue;
		readonly int trueValue;
		readonly int falseValue;

		public SauceTwoBitFlag(SauceInfo sauce, string description, int start, int trueValue = 0x01, int falseValue = 0x02, int nullValue = 0x0)
			: base(sauce, description)
		{
			this.start = start;
			this.nullValue = nullValue;
			this.trueValue = trueValue;
			this.falseValue = falseValue;
		}

		byte Mask
		{
			get { return (byte)(0x03 << start); }
		}

		public override object Value
		{
			get { return BoolValue; }
			set { BoolValue = (bool?)value; }
		}

		public bool? BoolValue
		{
			get
			{ 
				var val = (Sauce.ByteFlags & Mask) >> start;
				return val == nullValue ? null : (bool?)(val == trueValue);
			}
			set
			{
				var val = value == null ? nullValue : value == true ? trueValue : falseValue;
				var flags = Sauce.ByteFlags;
				flags &= (byte)~Mask;
				flags |= (byte)(val << start);
				Sauce.ByteFlags = flags;
			}
		}

		public override Control CreateControl()
		{
			var control = new CheckBox
			{
				Text = Description,
				ThreeState = true,
				Checked = BoolValue
			};
			control.CheckedChanged += delegate
			{
				BoolValue = control.Checked;
			};
			return control;
		}
	}

	public enum SauceDataType
	{
		None = 0,
		Character = 1,
		Bitmap = 2,
		Vector = 3,
		Audio = 4,
		BinaryText = 5,
		XBIN = 6,
		Archive = 7,
		Executable = 8
	}

	/// <summary>
	/// Summary description for Sauce.
	/// </summary>
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public class SauceInfo : INetworkReadWrite
	{
		const string SauceID = "SAUCE";
		const long SauceSize = 128;
		byte[] title;
		byte[] author;
		byte[] group;
		byte[] date;
		byte dataType;
		string tinfoS;
		SauceComment comments;
		SauceDataTypeInfo typeInfo;

		public event EventHandler<EventArgs> DataTypeChanged;

		protected virtual void OnDataTypeChanged(EventArgs e)
		{
			TInfo1 = TInfo2 = TInfo3 = TInfo4 = 0;
			TInfoS = null;
			ByteFlags = 0;
			ByteFileType = 0;

			if (DataTypeChanged != null)
				DataTypeChanged(this, e);
		}

		public static bool HasSauce(Stream s)
		{
			if (s.Length <= SauceSize + 1) // +1 for eof
				return false;
			long pos = s.Position;
			s.Seek(s.Length - SauceSize, SeekOrigin.Begin);
			bool exists;

			var br = new BinaryReader(s);
			byte[] id = br.ReadBytes(5);
			exists = (Encoding.ASCII.GetString(id) == SauceID);
			
			// restore original position
			s.Seek(pos, SeekOrigin.Begin);
			return exists;
		}

		public SauceInfo(SauceInfo sauce)
		{
			Title = sauce.Title;
			Author = sauce.Author;
			Group = sauce.Group;
			Date = sauce.Date;
			FileSize = sauce.FileSize;
			dataType = sauce.dataType;
			ByteFileType = sauce.ByteFileType;
			TInfo1 = sauce.TInfo1;
			TInfo2 = sauce.TInfo2;
			TInfo3 = sauce.TInfo3;
			TInfo4 = sauce.TInfo4;
			TInfoS = sauce.TInfoS;
			ByteFlags = sauce.ByteFlags;
			comments = new SauceComment(sauce.Comments);
			SetTypeInfo();
		}

		public SauceInfo(string fileName)
		{
			using (var fs = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				LoadSauce(fs, true);
			}
		}

		public SauceInfo(Stream stream)
		{
			LoadSauce(stream, true);
		}

		public SauceInfo()
		{
			Author = string.Empty;
			Title = string.Empty;
			Group = string.Empty;
			Date = DateTime.Now;
			DataType = SauceDataType.Character;
			
			ByteFileType = (byte)Types.Character.CharacterFileType.Ansi;
			
			SetTypeInfo();
			
			comments = new SauceComment();
		}

		public static SauceInfo GetSauce(Stream stream)
		{
			SauceInfo sauce = null;
			if (HasSauce(stream))
				sauce = new SauceInfo(stream);
			return sauce;
		}

		public static SauceInfo GetSauce(string filename)
		{
			SauceInfo sauce = null;
			using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				if (HasSauce(fs))
				{
					sauce = new SauceInfo(fs);
				}
			}
			return sauce;
		}

		void LoadSauce(Stream stream, bool seekPosition)
		{
			long origin = stream.Position;
			try
			{
				var br = new BinaryReader(stream);
				if (seekPosition)
				{
					var saucePos = stream.Length - SauceSize - 1;
					if (saucePos < 0)
						throw new Exception("Sauce does not exist! File is too small.");
					br.BaseStream.Seek(saucePos, SeekOrigin.Begin);
				}
				var eof = br.ReadByte();
				var hasEof = eof == 26;
				long start = br.BaseStream.Position;
				var id = br.ReadBytes(5);
				var idstring = Encoding.ASCII.GetString(id);
				if (idstring != SauceID)
					throw new Exception("Sauce does not exist!");
				br.ReadBytes(2); // version
				title = br.ReadBytes(35);
				author = br.ReadBytes(20);
				@group = br.ReadBytes(20);
				date = br.ReadBytes(8);
				FileSize = br.ReadInt32();
				dataType = br.ReadByte();
				SetTypeInfo();
				ByteFileType = br.ReadByte();
				TInfo1 = br.ReadUInt16();
				TInfo2 = br.ReadUInt16();
				TInfo3 = br.ReadUInt16();
				TInfo4 = br.ReadUInt16();
				var numComments = br.ReadByte();
				ByteFlags = br.ReadByte();
				// InfoS is a zero-terminated string
				var infoSBytes = br.ReadBytes(22);
				var zeroIdx = Array.FindIndex(infoSBytes, r => r == 0);
				if (zeroIdx == -1)
					zeroIdx = infoSBytes.Length;
				TInfoS = Encoding.ASCII.GetString(infoSBytes, 0, zeroIdx);

				if (numComments > 0)
				{
					var commentPos = start - (numComments * SauceComment.CommentSize) - 5;
					if (commentPos >= 0)
						br.BaseStream.Seek(commentPos, SeekOrigin.Begin);
					else
						numComments = 0;
				}

				comments = new SauceComment(br, numComments);
				FileSize = (int)(stream.Length - SauceSize);
				if (numComments > 0)
					FileSize -= SauceComment.CommentID.Length + numComments * SauceComment.CommentSize;
				if (hasEof)
					FileSize--;
			}
			finally
			{
				stream.Seek(origin, SeekOrigin.Begin);
			}
		}

		public void SaveSauce(Stream stream, bool seekPosition = true)
		{
			if (seekPosition && stream.CanSeek)
				stream.Seek(0, SeekOrigin.End);
			FileSize = (int)stream.Position;
			var bw = new BinaryWriter(stream);


			bw.Write((byte)26); // EOF

			byte numComments = comments.Save(bw);

			bw.Write(Encoding.ASCII.GetBytes(SauceID));
			bw.Write(Encoding.ASCII.GetBytes("00"));
			bw.Write(title);
			bw.Write(author);
			bw.Write(@group);
			bw.Write(date);
			bw.Write(FileSize);
			bw.Write(dataType);
			bw.Write(ByteFileType);
			bw.Write(TInfo1);
			bw.Write(TInfo2);
			bw.Write(TInfo3);
			bw.Write(TInfo4);
			bw.Write(numComments);
			bw.Write(ByteFlags);
			if (TInfoS != null)
			{
				bw.Write(Encoding.ASCII.GetBytes(TInfoS));
				bw.WriteBytes(0, 22 - TInfoS.Length);
			}
			else
				bw.WriteBytes(0, 22);

			//FileSize = (int)(stream.Length - SAUCE_SIZE - (numComments * SauceComment.COMMENT_SIZE));
		}

		void SetTypeInfo()
		{
			switch (DataType)
			{
				default:
					typeInfo = new SauceDataTypeInfo();
					ByteFileType = 0;
					TInfo1 = TInfo2 = TInfo3 = TInfo4 = 0;
					break;
				case SauceDataType.Archive:
					typeInfo = new Types.Archive.DataTypeInfo();
					break;
				case SauceDataType.BinaryText:
					typeInfo = new Types.Binary.DataTypeInfo();
					break;
				case SauceDataType.Character:
					typeInfo = new Types.Character.DataTypeInfo();
					break;
				case SauceDataType.Bitmap:
					typeInfo = new Types.Bitmap.DataTypeInfo();
					break;
				case SauceDataType.Audio:
					typeInfo = new Types.Sound.DataTypeInfo();
					break;
				case SauceDataType.Vector:
					typeInfo = new Types.Vector.DataTypeInfo();
					break;
				case SauceDataType.XBIN:
					typeInfo = new Types.XBin.DataTypeInfo();
					break;
			}
			typeInfo.Sauce = this;
		}
		/*
		public string FileType
		{
			get { return typeInfo.FileType; }
			set { typeInfo.FileType = value; }
		}*/
		public string Title
		{
			get { return Encoding.ASCII.GetString(title).TrimEnd(); }
			set
			{
				string s = value.PadRight(35);
				if (s.Length > 35)
					throw new Exception("title too long");
				title = new byte[35];
				Encoding.ASCII.GetBytes(s, 0, 35, title, 0);
			}
		}

		public string Author
		{
			get { return Encoding.ASCII.GetString(author).TrimEnd(); }
			set
			{
				string s = value.PadRight(20);
				if (s.Length > 20)
					throw new Exception("author too long");
				author = Encoding.ASCII.GetBytes(s);
			}
		}

		public string Group
		{
			get { return Encoding.ASCII.GetString(group).TrimEnd(); }
			set
			{
				string s = value.PadRight(20);
				if (s.Length > 20)
					throw new Exception("group too long");
				group = Encoding.ASCII.GetBytes(s);
			}
		}

		public string DateString
		{
			get { return Encoding.ASCII.GetString(date); }
			set
			{
				string s = value.PadRight(8);
				if (s.Length > 8)
					throw new Exception("date too long");
				date = Encoding.ASCII.GetBytes(s);
			}
		}

		public DateTime? Date
		{
			get
			{
				string s = Encoding.ASCII.GetString(date);
				if (s.Length != 8)
					return null;
				int year = Convert.ToInt32(s.Substring(0, 4));
				int month = Convert.ToInt32(s.Substring(4, 2));
				int day = Convert.ToInt32(s.Substring(6, 2));

				if (year > 1900 && year < 3000)
				{
					if (month >= 1 && month <= 12)
					{
						if (day > 1 && day <= DateTime.DaysInMonth(year, month))
						{
							return new DateTime(year, month, day);
						}
					}
				}

				return null;
			}
			set
			{
				if (value == null)
					value = DateTime.Today;
				string s = value.Value.ToString("yyyyMMdd");
				date = Encoding.ASCII.GetBytes(s);
			}
		}

		public bool IsValid
		{
			get {

				return TypeInfo != null && TypeInfo.IsValid;
			}
		}

		public int FileSize { get; set; }

		public byte ByteFileType { get; set; }

		public byte ByteFlags { get; set; }

		public IEnumerable<SauceFlag> Flags
		{
			get { return typeInfo.Flags; }
		}

		public IList<string> Comments
		{
			get { return comments.Comments; }
		}

		public SauceDataType DataType
		{
			get { return (SauceDataType)dataType; }
			set
			{
				if (dataType != (byte)value)
				{
					dataType = (byte)value;
					ByteFileType = 0;
					SetTypeInfo();
					OnDataTypeChanged(EventArgs.Empty);
				}
			}
		}

		public SauceDataTypeInfo TypeInfo
		{
			get { return typeInfo; }
		}

		public UInt16 TInfo1 { get; set; }

		public UInt16 TInfo2 { get; set; }

		public UInt16 TInfo3 { get; set; }

		public UInt16 TInfo4 { get; set; }

		public string TInfoS
		{
			get { return tinfoS; }
			set
			{
				if (value != null && value.Length > 22)
					throw new ArgumentOutOfRangeException("value", "Value string must be 22 characters or less");
				tinfoS = value;
			}
		}

		#region INetworkReadWrite implementation

		public bool Send(SendCommandArgs args)
		{
			args.Message.Write(Title);
			args.Message.Write(Author);
			args.Message.Write(Group);
			args.Message.WriteDate(Date);
			args.Message.WriteVariableInt32(FileSize);
			args.Message.Write(dataType);
			args.Message.Write(ByteFileType);
			args.Message.Write(TInfo1);
			args.Message.Write(TInfo2);
			args.Message.Write(TInfo3);
			args.Message.Write(TInfo4);
			args.Message.Write(TInfoS);
			var numComments = (byte)(Comments != null ? Comments.Count : 0);
			args.Message.Write(numComments);
			args.Message.Write(ByteFlags);
			for (int i = 0; i < numComments; i++)
			{
				args.Message.Write(Comments[i]);
			}
			return true;
			
		}

		public void Receive(ReceiveCommandArgs args)
		{
			Title = args.Message.ReadString();
			Author = args.Message.ReadString();
			Group = args.Message.ReadString();
			Date = args.Message.ReadNullableDate();
			FileSize = args.Message.ReadVariableInt32();
			dataType = args.Message.ReadByte();
			ByteFileType = args.Message.ReadByte();
			TInfo1 = args.Message.ReadUInt16();
			TInfo2 = args.Message.ReadUInt16();
			TInfo3 = args.Message.ReadUInt16();
			TInfo4 = args.Message.ReadUInt16();
			TInfoS = args.Message.ReadString();
			var numComments = args.Message.ReadByte();
			ByteFlags = args.Message.ReadByte();
			Comments.Clear();
			for (int i = 0; i < numComments; i++)
			{
				Comments.Add(args.Message.ReadString());
			}
		}

		#endregion

	}

	public class SauceComment
	{
		public const string CommentID = "COMNT";
		public const int CommentSize = 64;

		public SauceComment()
		{
			Comments = new List<string>();
		}

		public SauceComment(IEnumerable<string> comments)
		{
			Comments = new List<string>(comments);
		}

		public SauceComment(BinaryReader br, int numComments)
		{
			var curid = br.ReadBytes(5);
			var curidstring = Encoding.ASCII.GetString(curid);
			Comments = new List<string>();
			if (curidstring == CommentID)
			{
				for (int i = 0; i < numComments; i++)
				{
					var bytes = br.ReadBytes(CommentSize);
					Comments.Add(Encoding.ASCII.GetString(bytes).TrimEnd());
				}
			}
		}

		public byte Save(BinaryWriter bw)
		{
			byte length = 0;
			if (Comments != null && Comments.Count > 0)
			{
				bw.Write(Encoding.ASCII.GetBytes(CommentID));
				
				for (int i = 0; i < Comments.Count; i++)
				{
					var comment = Comments[i];
					if (string.IsNullOrEmpty(comment))
					{
						bw.WriteBytes(32, CommentSize);	
						length++;
					}
					else
					{
						var chunks = comment.Chunks(CommentSize);
						foreach (string chunk in chunks)
						{
							var comment_str = chunk.PadRight(CommentSize);
							length++;
							bw.Write(Encoding.ASCII.GetBytes(comment_str));
						}
					}
				}
			}
			return length;
		}

		public IList<string> Comments
		{
			get;
			private set;
		}
	}
}

using System;
using Eto.Forms;
using System.Collections.Generic;
using System.Linq;

namespace Pablo.Sauce.Types.Sound
{
	public enum SoundFileType
	{
		/// <summary>
		/// (4, 6 or 8 channel MOD/NST file)
		/// </summary>
		MOD = 0,
		/// <summary>
		/// (Renaissance 8 channel 669 format)
		/// </summary>
		R669,
		/// <summary>
		/// (Future Crew 4 channel ScreamTracker format)
		/// </summary>
		STM,
		/// <summary>
		/// (Future Crew variable channel ScreamTracker3 format)
		/// </summary>
		S3M,
		/// <summary>
		/// (Renaissance variable channel MultiTracker Module)
		/// </summary>
		MTM,
		/// <summary>
		/// (Farandole composer module)
		/// </summary>
		FAR,
		/// <summary>
		/// (UltraTracker module)
		/// </summary>
		ULT,
		/// <summary>
		/// (DMP/DSMI Advanced Module Format)
		/// </summary>
		AMF,
		/// <summary>
		/// (Delusion Digital Music Format (XTracker))
		/// </summary>
		DMF,
		/// <summary>
		/// (Oktalyser module)
		/// </summary>
		OKT,
		/// <summary>
		/// (AdLib ROL file (FM))
		/// </summary>
		ROL,
		/// <summary>
		/// (Creative Labs FM)
		/// </summary>
		CMF,
		/// <summary>
		/// (MIDI file)
		/// </summary>
		MIDI,
		/// <summary>
		/// (SAdT composer FM Module)
		/// </summary>
		SADT,
		/// <summary>
		/// (Creative Labs Sample)
		/// </summary>
		VOC,
		/// <summary>
		/// (Windows Wave file)
		/// </summary>
		WAV,
		/// <summary>
		/// (8 Bit Sample, TInfo1 holds sampling rate)
		/// </summary>
		SMP8,
		/// <summary>
		/// (8 Bit sample stereo, TInfo1 holds sampling rate)
		/// </summary>
		SMP8S,
		/// <summary>
		/// (16 Bit sample, TInfo1 holds sampling rate)
		/// </summary>
		SMP16,
		/// <summary>
		/// (16 Bit sample stereo, TInfo1 holds sampling rate)
		/// </summary>
		SMP16S,
		/// <summary>
		/// (8 Bit patch-file)
		/// </summary>
		PATCH8,
		/// <summary>
		/// (16 Bit Patch-file)
		/// </summary>
		PATCH16,
		/// <summary>
		/// (FastTracker ][ Module)
		/// </summary>
		XM,
		/// <summary>
		/// (HSC Module)
		/// </summary>
		HSC,
		/// <summary>
		/// (Impulse Tracker)
		/// </summary>
		IT,
	}

	public class DataTypeInfo : BaseFileType.DataTypeInfo
	{
		public override IEnumerable<SauceFileTypeInfo> FileTypes
		{
			get
			{
				yield return new SauceFileTypeInfo{ Type = 0, Name = "MOD" };
				yield return new SauceFileTypeInfo{ Type = 1, Name = "669" };
				yield return new SauceFileTypeInfo{ Type = 2, Name = "STM" };
				yield return new SauceFileTypeInfo{ Type = 3, Name = "S3M" };
				yield return new SauceFileTypeInfo{ Type = 4, Name = "MTM" };
				yield return new SauceFileTypeInfo{ Type = 5, Name = "FAR" };
				yield return new SauceFileTypeInfo{ Type = 6, Name = "ULT" };
				yield return new SauceFileTypeInfo{ Type = 7, Name = "AMF" };
				yield return new SauceFileTypeInfo{ Type = 8, Name = "DMF" };
				yield return new SauceFileTypeInfo{ Type = 9, Name = "OKT" };
				yield return new SauceFileTypeInfo{ Type = 10, Name = "ROL" };
				yield return new SauceFileTypeInfo{ Type = 11, Name = "CMF" };
				yield return new SauceFileTypeInfo{ Type = 12, Name = "MIDI" };
				yield return new SauceFileTypeInfo{ Type = 13, Name = "SADT" };
				yield return new SauceFileTypeInfo{ Type = 14, Name = "VOC" };
				yield return new SauceFileTypeInfo{ Type = 15, Name = "WAV" };
				yield return new SauceFileTypeInfo{ Type = 16, Name = "SMP8" };
				yield return new SauceFileTypeInfo{ Type = 17, Name = "SMP8S" };
				yield return new SauceFileTypeInfo{ Type = 18, Name = "SMP16" };
				yield return new SauceFileTypeInfo{ Type = 19, Name = "SMP16S" };
				yield return new SauceFileTypeInfo{ Type = 20, Name = "PATCH8" };
				yield return new SauceFileTypeInfo{ Type = 21, Name = "PATCH16" };
				yield return new SauceFileTypeInfo{ Type = 22, Name = "XM" };
				yield return new SauceFileTypeInfo{ Type = 23, Name = "HSC" };
				yield return new SauceFileTypeInfo{ Type = 24, Name = "IT" };
			}
		}

		public SoundFileType Type
		{
			get { return (SoundFileType)Sauce.ByteFileType; }
			set { Sauce.ByteFileType = (byte)value; }
		}
	}
}

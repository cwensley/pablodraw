using Eto;
using Eto.Drawing;
using Eto.Forms;
using System;
using System.IO;
using Pablo.Sauce;
using Pablo.Network;

namespace Pablo
{
	public delegate void UpdateEventHandler(object sender,Rectangle rect);
	public class WaitEventArgs : EventArgs
	{

		public bool Exit { get; set; }
	}
	public delegate void WaitEventHandler(object sender,WaitEventArgs args);
	public abstract class Document : IDisposable, INetworkReadWrite
	{
		readonly DocumentInfo info;
		bool converted;

		public event EventHandler<EventArgs> StartLoad;
		public event EventHandler<EventArgs> Initialized;
		public event EventHandler<EventArgs> Loaded;
		public event EventHandler<EventArgs> Saved;

		public bool IsNew { get; set; }

		public abstract bool IsModified { get; set; }

		public bool HasSavePermission { get; set; }

		protected void OnStartLoad(EventArgs e)
		{
			if (StartLoad != null)
				StartLoad(this, e);
		}

		protected void OnLoaded(EventArgs e)
		{
			if (Loaded != null)
				Loaded(this, e);
		}

		protected void OnSaved(EventArgs e)
		{
			if (Saved != null)
				Saved(this, e);
		}

		protected virtual void OnInitialized(EventArgs e)
		{
			if (Initialized != null)
				Initialized(this, e);
		}

		public Platform Generator { get; set; }

		public Format LoadedFormat { get; set; }

		public string FileName { get; set; }

		public abstract Size Size { get; }

		public virtual bool EditMode { get; set; }

		protected Document(DocumentInfo info)
		{
			this.info = info;
		}

		public abstract Handler CreateHandler();

		public SauceInfo Sauce { get; set; }

		public DocumentInfo Info
		{
			get { return info; }
		}

		~Document()
		{
			//Console.WriteLine ("GC: {0}", this.GetType ().Name);
			Dispose(false);
		}

		public virtual void PostLoad(Handler handler)
		{
			// no initialization needed for base
			OnInitialized(EventArgs.Empty);
		}

		public virtual void PreLoad(Stream stream, Format format, Handler handler)
		{
		}

		public virtual void Load(string fileName, Format format, Handler handler)
		{
			this.FileName = fileName;
			var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read); // can't dispose here if we load in background!
			Load(stream, format, handler);
		}

		public virtual void Load(Stream stream, Format format, Handler handler)
		{
			LoadedFormat = format;
			OnStartLoad(EventArgs.Empty);
			SauceStream ss;
			ss = Sauce != null ? new SauceStream(Sauce, stream) : new SauceStream(stream);
			Sauce = ss.Sauce;

			try
			{
				//if (handler != null)
				//	handler.PreLoad(ss, format);
				LoadStream(ss, format, handler);
				OnLoaded(EventArgs.Empty);
				if (handler != null)
					handler.Loaded();
			}
			finally
			{
				//ss.Close();
				//stream.Close();
			}
		}

		protected abstract void LoadStream(Stream stream, Format format, Handler handler);

		public virtual Document ConvertDocument(DocumentInfo targetDocumentInfo, Handler handler)
		{
			if (targetDocumentInfo.ID != Info.ID)
				throw new Exception("Cannot convert to target document type");
			return null;
		}

		public virtual void Save(string fileName, Format format, Handler handler)
		{
			using (var stream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
			{
				Save(stream, format, handler);
			}
		}

		public virtual void Save(Stream stream, Format format, Handler handler)
		{
			var doc = converted ? null : ConvertDocument(format.Info, handler);
			if (doc != null)
			{
				doc.converted = true;
				doc.Save(stream, format, handler);
				doc.Dispose();
			}
			else
			{
				SaveStream(stream, format, handler);
				EnsureSauce(format, handler);
				if (Sauce != null)
				{
					var c = Sauce.Comments;
					Sauce.SaveSauce(stream, false);
				}
				if (LoadedFormat == null)
					LoadedFormat = format;
			}
			OnSaved(EventArgs.Empty);
		}

		protected virtual void EnsureSauce(Format format, Handler handler)
		{
		}

		protected abstract void SaveStream(Stream stream, Format format, Handler handler);

		public virtual void Close()
		{
			
		}

		#region IDisposable Members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			//Console.WriteLine ("{0}: {1}", disposing ? "Dispose" : "GC", this.GetType ().Name);
		}

		#endregion

		public virtual void GenerateActions(GenerateCommandArgs args)
		{
			Info.GenerateCommands(args);
		}

		#region INetworkReadWrite implementation

		public virtual bool Send(SendCommandArgs args)
		{
			args.Message.Write(Path.GetFileName(FileName));
			args.Message.Write(EditMode);
			args.Message.Write(Sauce);
			args.Message.WritePadBits();
			return true;
		}

		public virtual void Receive(ReceiveCommandArgs args)
		{
			FileName = args.Message.ReadString();
			EditMode = args.Message.ReadBoolean();
			Sauce = args.Message.Read<SauceInfo>();
			args.Message.ReadPadBits();
		}

		#endregion

	}
}


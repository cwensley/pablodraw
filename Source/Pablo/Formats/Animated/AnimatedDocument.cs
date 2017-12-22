using System;
using System.IO;
using System.Threading;
using Eto;
using Eto.Forms;
using System.Diagnostics;

namespace Pablo.Formats.Animated
{
	public abstract class AnimatedDocument : Document
	{
		DocumentThreadLoader loader;
		bool animationDetected;
		bool canAnimate;

		public AnimatedDocument(DocumentInfo info) : base(info)
		{
		}

		public new AnimatedDocumentInfo Info
		{
			get { return (AnimatedDocumentInfo)base.Info; }
		}

		public override bool EditMode
		{
			get
			{
				return base.EditMode;
			}
			set
			{
				if (base.EditMode != value && value)
				{
					
					// reload when editing
					var temploader = loader;
					if (temploader != null)
					{
						loader = null;
						temploader.Abort();
						var stream = temploader.Stream;
						if (stream != null)
						{
							stream.Seek(0, SeekOrigin.Begin);
							canAnimate = false;
							//LoadBase (stream, temploader.Format, temploader.Handler);
						}
					}
				}
				base.EditMode = value;
			}
		}

		public bool AnimateView
		{
			get	{ return (Info.AnimationEnabled && (!Info.AutoDetectAnimation || animationDetected)) && canAnimate; }
		}

		internal class DocumentThreadLoader : IDisposable
		{
			AnimatedDocument document;
			Handler handler;
			Stream stream;
			BaudStream bs;
			Format format;
			bool aborting;
			System.Threading.ManualResetEvent threadKilledEvent;
			System.Threading.ManualResetEvent quitThreadEvent;
			Thread loadingThread;
			bool running;
			//bool stop = false;
			public Handler Handler
			{
				get { return handler; }
			}

			public Format Format
			{
				get { return format; }
			}

			public Stream Stream
			{
				get { return stream; }
			}

			~DocumentThreadLoader ()
			{
				Dispose(false);
			}

			public void Dispose()
			{
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			protected virtual void Dispose(bool disposing)
			{
				Stop();
				document.Info.BaudRateChanged -= BaudRateChanged;
			}

			public DocumentThreadLoader(AnimatedDocument document, Handler handler, Stream stream, Format format)
			{
				threadKilledEvent = new System.Threading.ManualResetEvent(false);
				quitThreadEvent = new System.Threading.ManualResetEvent(false);
				this.handler = handler;
				this.document = document;
				this.stream = stream;
				this.format = format;
				loadingThread = new Thread(Load);
				bs = new BaudStream(stream);
				bs.Baud = document.Info.Baud;
				document.SetWaitHandler(OnWait);
				
				document.Info.BaudRateChanged += BaudRateChanged;
			}

			public AnimatedDocument Document
			{
				get { return document; }
			}

			public void Start()
			{
				if (!loadingThread.IsAlive)
					loadingThread.Start();
			}

			private void Load()
			{
				try
				{
					using (var context = Generator.Current.ThreadStart())
					{
						// if switched quickly, the thread may abort here!!
						try
						{
							running = true;
							// only tell other threads to wait when we know we'll be getting into the 'finally' section below
							document.LoadBase(bs, format, handler);
						}
						finally
						{
							//running = false;
							//Console.WriteLine("Killed!");
							if (!aborting)
							{
								if (bs != null)
									bs.Close();
								if (stream != null)
									stream.Close();
								bs = null;
								stream = null;
							}
						}
					}
				}
				finally
				{
					threadKilledEvent.Set();
					Debug.Print("Killed!");
				}
			}

			private void BaudRateChanged(object sender, EventArgs e)
			{
				if (bs != null)
				{
					bs.Baud = document.Info.Baud;
				}
			}

			public void OnWait(object sender, WaitEventArgs args)
			{
				if (quitThreadEvent.WaitOne(0, false))
				{
					args.Exit = true;
					//Console.WriteLine ("Quitting!");
					//Thread.CurrentThread.Abort ();
				}
				
				//lock (this) if (stop) args.Exit = true;
				if (bs != null)
					bs.Wait();
			}

			public void Abort()
			{
				aborting = true;
				Stop();
			}

			public void Stop()
			{
				if (running)
				{
					running = false;
					quitThreadEvent.Set();

					/*
					 *
					if (handler != null && handler.Viewer != null)
					{
						while (!threadKilledEvent.WaitOne(1000, false))
						{
							handler.Viewer.RunIteration();
						}
					}
					else
					/*
					 */
					if (loadingThread.IsAlive)
					{
						//Console.WriteLine ("Aborting thread..");
						loadingThread.Abort();
					}
					//threadKilledEvent.WaitOne ();
					//threadKilledEvent.Set ();
					if (!handler.Generator.IsMac && !handler.Generator.IsIos && !handler.Generator.IsWpf)
					{
#if DESKTOP
						while (!threadKilledEvent.WaitOne(100, true))
						{
							Application.Instance.RunIteration();
						}
#else
						threadKilledEvent.WaitOne();
#endif
					}
					Console.Write("Stopped!");
				}
			}
		}

		public void LoadBase(System.IO.Stream stream, Format format, Handler handler)
		{
			base.Load(stream, format, handler);
		}

		protected virtual void LoadingAnimated(System.IO.Stream stream, Format format, Handler handler)
		{
			
		}

		public override void PreLoad(Stream stream, Format format, Handler handler)
		{
			base.PreLoad(stream, format, handler);
			animationDetected = false;
			canAnimate = false;
			var animFormat = format as AnimatedFormat;
			if (stream != null && handler != null && !EditMode && Info.AnimationEnabled && animFormat != null)
			{
				if (Info.AutoDetectAnimation)
					animationDetected = animFormat.DetectAnimation(stream);
				canAnimate = animFormat.CanAnimate;
			}
		}

		public override void Load(System.IO.Stream stream, Format format, Handler handler)
		{
			if (loader != null)
			{
				loader.Dispose();
				loader = null;
			}
			
			bool loaded = false;
			var animFormat = format as AnimatedFormat;
			if (handler != null && !EditMode && Info.AnimationEnabled && animFormat != null)
			{
				if (AnimateView)
				{
					// use thread to load
					LoadingAnimated(stream, format, handler);
					
					loader = new DocumentThreadLoader(this, handler, stream, format);
					loaded = true;
					if (handler != null)
						handler.BackgroundLoaded();
				}
			}
			if (!loaded)
			{
				base.Load(stream, format, handler);
				loaded = true;
			}
		}

		protected override void OnInitialized(EventArgs e)
		{
			base.OnInitialized(e);

			if (loader != null)
				loader.Start();
		}

		public override void Close()
		{
			if (loader != null)
			{
				loader.Dispose();
				loader = null;
			}
		}
		/*
		public override void Receive (Lidgren.Network.NetIncomingMessage message)
		{
			base.Receive (message);
			forceAnimation = message.ReadBoolean ();
		}
		
		public override bool Send (Lidgren.Network.NetOutgoingMessage message)
		{
			base.Send (message);
			message.Write(!EditMode && Info.AnimationEnabled && AnimateView);
			return true;
		}*/

		#region IDisposable Members

		public abstract void SetWaitHandler(WaitEventHandler waitHandler);

		protected override void Dispose(bool disposing)
		{
			if (loader != null)
			{ 
				loader.Dispose();
				//Console.WriteLine ("STOPPED!");
				loader = null; 
			}
			
			base.Dispose(disposing);
		}

		#endregion

	}
}

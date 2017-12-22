using System;
using System.IO;

namespace Pablo.Formats.Animated
{
	public class BaudStream : Stream
	{
		readonly Stream stream;
		long tickWait;
		long tickStart;

		public BaudStream(Stream stream)
		{
			this.stream = stream;
			tickStart = DateTime.Now.Ticks; //Environment.TickCount; //timer.Command(Misc.TimerCommand.GetAbsoluteTime);
		}
		
		public long Baud
		{
			get
			{
				return (long)(((TimeSpan.TicksPerSecond * 1.2) / TickWait) * 8);
			}
			set
			{
				TickWait = (value > 0) ? (long)(((TimeSpan.TicksPerSecond * 1.2) / (value / 8))) : 0;
			}
		}
		
		public override bool CanRead
		{
			get { return true; }
		}

		public override bool CanSeek
		{
			get { return false; }
		}

		public override bool CanWrite
		{
			get { return false; }
		}
		
		public long TickWait
		{
			get { return tickWait; }
			set { 
				tickWait = value;
				tickStart = DateTime.Now.Ticks;
			}
		}

		public override void Flush() { stream.Flush(); }

		public override long Length { get { return stream.Length; } }
		public override long Position
		{
			get { return stream.Position; }
			set { stream.Position = value; }
		}

		public override int ReadByte()
		{
			tickStart += tickWait;
			return stream.ReadByte();
		}

		public override int Read(byte[] buffer, int offset, int count)
		{
			tickStart += tickWait * count;
			return stream.Read(buffer, offset, count);
		}

		//static int blah = 0;
		public void Wait()
		{
			if (tickWait == 0) return;
			long ticks = DateTime.Now.Ticks;
			//if (((blah++ % 100) == 0))
			//Console.WriteLine("interval: {0} out: {1}", tickWait, ticks-tickStart);
			//while (ticks < tickStart)
			if (ticks < tickStart)
			{
				int sleepInterval = (int)(((tickStart - ticks) * 3/4) / TimeSpan.TicksPerMillisecond);
				//Console.WriteLine(string.Format("Waiting for {0} ms", sleepInterval));
				if (sleepInterval > 0) {
				
					System.Threading.Thread.Sleep(sleepInterval);
				} 
				//ticks = DateTime.Now.Ticks;
			}
			
		}

		public override long Seek(long offset, SeekOrigin origin)
		{
			return stream.Seek(offset, origin);
		}

		public override void SetLength(long value)
		{
			stream.SetLength(value);
		}

		public override void Write(byte[] buffer, int offset, int count)
		{
			stream.Write(buffer, offset, count);
		}
	}
}

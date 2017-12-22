using System.IO;

namespace Pablo.Formats.Animated
{
	/// <summary>
	/// Summary description for AnimatedFormat.
	/// </summary>
	public abstract class AnimatedFormat : Format
	{
		public AnimatedFormat(DocumentInfo info, string id, string name, params string[] extensions)
			: base(info, id, name, extensions)
		{
		}

		public virtual bool CanAnimate
		{
			get { return true; }
		}

		public bool DetectAnimation(string filename)
		{
			using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return DetectAnimation(fs);
			}
		}

		public virtual bool DetectAnimation(Stream stream)
		{
			return false;
		}

	}
}

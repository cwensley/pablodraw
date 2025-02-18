using System;
using Eto.IO;
using System.Collections.Generic;
using System.Net;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pablo.Gallery
{
	public abstract class GenericDirectory<T> : GenericDirectory<T, EtoFileInfo>
		where T: EtoDirectoryInfo
	{
		protected GenericDirectory(EtoDirectoryInfo parent)
			: base(parent)
		{
		}
	}

	public abstract class GenericDirectory<T, F> : EtoDirectoryInfo
		where T: EtoDirectoryInfo
		where F: EtoFileInfo
	{
		List<T> entries;
		List<F> files;
		readonly EtoDirectoryInfo parent;

		public abstract string ApiPath
		{
			get;
		}

		protected GenericDirectory(EtoDirectoryInfo parent)
		{
			this.parent = parent;
		}

		protected override IEnumerable<EtoDirectoryInfo> GetPathDirectories()
		{
			if (entries == null)
				entries = new List<T>(GetEntries());
			return entries.Cast<EtoDirectoryInfo>();
		}

		protected string LoadJsonString()
		{
			Console.WriteLine(ApiPath);
			using var client = new HttpClient();
			using var response = Task.Run(async () => await client.GetStreamAsync(ApiPath)).GetAwaiter().GetResult();
			using var reader = new StreamReader(response);
			
			var s = reader.ReadToEnd();	
			//Console.WriteLine(s);
			return s;
		}

		protected abstract IEnumerable<T> GetEntries();

		public override IEnumerable<EtoFileInfo> GetFiles()
		{
			if (files == null)
				files = new List<F>(GetFileEntries());
			return files.Cast<EtoFileInfo>();
		}

		public virtual IEnumerable<F> GetFileEntries()
		{
			yield break;
		}

		public override EtoDirectoryInfo Parent
		{
			get
			{
				return parent;
			}
		}
	}
}


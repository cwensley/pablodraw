using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Pablo
{
	public class CommandLine
	{
		const string commandLineRegEx =
			@"(?<=^|\s)(
(((--)|-|/)(?<name>[^\s:=]+)((\s*:\s*)|(\s*=\s*)|(\s+(?=[^-])))
(([""""](?<value>(("""")|[^""])*)[""])|(?<value>[^\s]*))
|
([""""]((--)|-|/)(?<name>[^\s:=]+)((\s*:\s*)|(\s*=\s*)|(\s+(?=[^-])))
(?<value>(("""")|[^""])*)[""]))
|
(((--)|-|/)(?<name>[^\s]+))
|
([""]((--)|-|/)(?<name>[^""]+)[""])
)";

		const string topLevel = @"
^(([""][^""]+[""])|([^\s]+))\s*(([""](?<generic>[^""]*)[""])|(?<generic>.*))$
";

        const string genericCommand = @"
(""(?<value>(("""")|[^""])+)"")|(?<value>[^\s]+)
";

		readonly IDictionary<string, string> vals;

		public string GenericCommand { get; private set; }

		public bool IsEmpty
		{
			get { return vals.Count == 0 && string.IsNullOrEmpty(GenericCommand); }
		}

		Dictionary<string, string> ParseCommand (string command)
		{
			var result = new Dictionary<string, string> (StringComparer.InvariantCultureIgnoreCase);
			const RegexOptions options = RegexOptions.Compiled | RegexOptions.ExplicitCapture | RegexOptions.IgnorePatternWhitespace;
			var topMatch = Regex.Match (command, topLevel, options);
			if (topMatch.Success) {
				var generic = topMatch.Groups["generic"].Value;
                var matches = Regex.Matches(generic, commandLineRegEx, options).OfType<Match>().ToArray();
				if (matches.Any ()) {
					foreach (var match in matches) {
						result[match.Groups["name"].Value] = match.Groups["value"].Value;
					}
					var max = matches.Max (r => r.Index + r.Length);
					GenericCommand = max < generic.Length ? generic.Substring(max).Trim(' ', '"') : null;
				} else {
					GenericCommand = generic;
				}
			}
            if (!string.IsNullOrEmpty(GenericCommand))
            {
                var matches = Regex.Matches(GenericCommand, genericCommand, options).OfType<Match>();
                var count = 0;
                foreach (var match in matches)
                {
                    result["generic:" + count++] = match.Value;
                }
            }

			return result;
		}

		public CommandLine (params string[] args)
		{
			var sb = new StringBuilder ();
			foreach (var arg in args) {
				if (sb.Length > 0)
					sb.Append (' ');
				if (arg.IndexOf (' ') > 0) {
					sb.Append ('"');
					sb.Append (arg);
					sb.Append ('"');
				} else
					sb.Append (arg);
			}
			vals = ParseCommand (sb.ToString ());
		}

		public CommandLine (string command)
		{
			vals = ParseCommand (command);
		}

		public bool TryGetValue (string[] keys, out string val)
		{
			foreach (var key in keys) {
				if (vals.TryGetValue (key, out val))
					return true;
			}
			val = null;
			return false;

		}

		public string GetValue (params string[] keys)
		{
			string val;
			return TryGetValue(keys, out val) ? val : null;
		}

		public bool? GetBool (params string[] keys)
		{
			string str;
			bool val;
			if (TryGetValue (keys, out str))
			{
				if (string.IsNullOrEmpty(str))
					return true;
				if (bool.TryParse(str, out val))
					return val;
				switch (str.ToLowerInvariant())
				{
					case "1":
					case "yes":
					case "y":
						return true;
					case "0":
					case "no":
					case "n":
						return false;
				}
			}
			return null;
		}

		public int? GetInt (params string[] keys)
		{
			string str;
			int val;
			if (TryGetValue (keys, out str) && int.TryParse (str, out val))
				return val;
			return null;
		}

		public float? GetFloat (params string[] keys)
		{
			string str;
			float val;
			if (TryGetValue (keys, out str) && float.TryParse (str, out val))
				return val;
			return null;
		}

		public double? GetDouble (params string[] keys)
		{
			string str;
			double val;
			if (TryGetValue (keys, out str) && double.TryParse (str, out val))
				return val;
			return null;
		}

		public decimal? GetDecimal(params string[] keys)
		{
			string str;
			decimal val;
			if (TryGetValue(keys, out str) && decimal.TryParse(str, out val))
				return val;
			return null;
		}

		public T? GetEnum<T>(params string[] keys)
			where T : struct
		{
			string str;
			T val;
			if (TryGetValue (keys, out str) && Enum.TryParse<T> (str, true, out val))
				return val;
			return null;

		}
	}
}

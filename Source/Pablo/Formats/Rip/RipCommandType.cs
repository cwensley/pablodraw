using System;
using System.Collections.Generic;
using Eto.Drawing;
using System.Reflection;
using System.Linq;

namespace Pablo.Formats.Rip
{
	public class RipCommandDictionary : Dictionary<string, RipCommandType>
	{
		public void Add (RipCommandType type)
		{
			this.Add (type.OpCode, type);
		}
	}
	
	public static class RipCommands
	{
		public static readonly RipCommandDictionary Types = new RipCommandDictionary () {
			new Commands.Arc.Type (),
			new Commands.Bar.Type (),
			new Commands.BeginText.Type (),
			new Commands.Bezier.Type (),
			new Commands.Button.Type (),
			new Commands.ButtonStyle.Type (),
			new Commands.Circle.Type (),
			new Commands.Color.Type (),
			new Commands.DrawRectangle.Type (),
			new Commands.EndText.Type (),
			new Commands.EraseEOL.Type (),
			new Commands.EraseView.Type (),
			new Commands.EraseWindow.Type (),
			new Commands.Fill.Type (),
			new Commands.FilledOval.Type (),
			new Commands.FilledPolygon.Type (),
			new Commands.FillPattern.Type (),
			new Commands.FillStyle.Type (),
			new Commands.FontStyle.Type (),
			new Commands.GetImage.Type (),
			new Commands.GotoXY.Type (),
			new Commands.Home.Type (),
			new Commands.KillMouseFields.Type (),
			new Commands.Line.Type (),
			new Commands.LineStyle.Type (),
			new Commands.LoadIcon.Type (),
			new Commands.Mouse.Type (),
			new Commands.Move.Type (),
			new Commands.OnePalette.Type (),
			new Commands.OutText.Type (),
			new Commands.OutTextXY.Type (),
			new Commands.Oval.Type (),
			new Commands.OvalArc.Type (),
			new Commands.OvalPieSlice.Type (),
			new Commands.PieSlice.Type (),
			new Commands.Pixel.Type (),
			new Commands.Polygon.Type (),
			new Commands.PolyLine.Type (),
			new Commands.PutImage.Type (),
			new Commands.RegionText.Type (),
			new Commands.ResetWindows.Type (),
			new Commands.SetPalette.Type (),
			new Commands.TextWindow.Type (),
			new Commands.ViewPort.Type (),
			new Commands.WriteIcon.Type (),
			new Commands.WriteMode.Type ()
		};
		
		public static RipCommand Create (string id, RipDocument document)
		{
			RipCommandType type;
			if (Types.TryGetValue (id, out type)) {
				return type.Create (document);
			}
			return null;
		}
		
		public static T Create<T> (RipDocument document)
			where T: RipCommand
		{
			var type = Types.Values.FirstOrDefault(r => r.CommandType == typeof(T));
			return (T)type.Create (document);
		}
		
		public static T Create<T, CT> (RipDocument document)
			where T: RipCommand, new()
			where CT: RipCommandType
		{
			var type = Types.Values.FirstOrDefault(r => r.GetType () == typeof(CT));
			
			return (T)type.Create<T>(document);
		}
		
	}

	public abstract class RipCommandType
	{
		public abstract string OpCode { get; }
		
		public abstract Type CommandType { get; }
		
		public abstract RipCommand Create (RipDocument document);

		public abstract RipCommand Create<R> (RipDocument document)
			where R: RipCommand, new();
	}
	
	public abstract class RipCommandType<T> : RipCommandType
		where T: RipCommand, new()
	{
		public override Type CommandType {
			get {
				return typeof(T);
			}
		}
		
		public override RipCommand Create<R> (RipDocument document) 
		{
			if (!typeof(T).IsAssignableFrom(typeof(R)))
				throw new ArgumentOutOfRangeException("command type R is not derived from type T");
			
			var command = new R ();
			command.CommandType = this;
			command.Document = document;
			return command;
		}
		
		public override RipCommand Create (RipDocument document)
		{
			return Create<T>(document);
		}

	}
}


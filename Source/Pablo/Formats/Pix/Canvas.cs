using System;
using System.Collections;

namespace PabloDraw.Handlers.Pix
{
	/// <summary>
	/// Summary description for Canvas.
	/// </summary>
	public class Canvas
	{
		CanvasRows rows;
		public Canvas(int width)
		{
			rows = new CanvasRows(width);
			
		}

		public CanvasRows Rows
		{
			get { return rows; }
		}

		
	}

	
	public class CanvasRow
	{
		char[][] chars;
		public CanvasRow(int width)
		{
			chars = new char[width][];
		}
		
		public char[] this[int col]
		{
			get 
			{
				if (chars[col] == null) return new char[0];
				return (char[])chars[col];
			}
		}

		public void AddChar(int col, char value)
		{
			if (chars[col] == null) 
			{
				chars[col] = new char[1];
				chars[col][0] = value;
			}						  
			else
			{
				char[] oldchars = chars[col];
				chars[col] = new char[oldchars.Length+1];
				for (int i=0; i<oldchars.Length; i++)
				{
					chars[col][i] = oldchars[i];
				}
				chars[col][oldchars.Length] = value;
			}
		}

	}


	public class CanvasRows : CollectionBase
	{
		int width;

		public CanvasRows(int width)
		{
			this.width = width;
		}

		public CanvasRow this[int row]
		{
			get 
			{
				while (List.Count <= row) List.Add(new CanvasRow(width));
				return (CanvasRow)List[row];
			}
		}
	}
}

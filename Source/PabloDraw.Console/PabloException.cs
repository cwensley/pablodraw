using System;
using System.IO;
using Pablo;
using System.Linq;
using Eto;
using System.Collections.Generic;

namespace PabloDraw
{
	[Serializable]
	public class PabloException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:PabloException"/> class
		/// </summary>
		public PabloException()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:PabloException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		public PabloException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:PabloException"/> class
		/// </summary>
		/// <param name="message">A <see cref="T:System.String"/> that describes the exception. </param>
		/// <param name="inner">The exception that is the cause of the current exception. </param>
		public PabloException(string message, Exception inner) : base(message, inner)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:PabloException"/> class
		/// </summary>
		/// <param name="context">The contextual information about the source or destination.</param>
		/// <param name="info">The object that holds the serialized object data.</param>
		protected PabloException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context) : base(info, context)
		{
		}
	}
}

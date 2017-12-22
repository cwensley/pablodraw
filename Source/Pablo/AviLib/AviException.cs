// AviLib.AviException
//
// Gabriel Boyer
// 08/21/2003

using System;

namespace AviLib
{
	public class AviException : Exception
	{
        int _errorCode;

		public AviException() : base()
		{            
		}

        public AviException(string message) : base(message)
        {
        }

        public AviException(string message, int errorCode) : base(message)
        {
            _errorCode = errorCode;
        }

        public int ErrorCode
        {
            get
            {
                return _errorCode;
            }
        }
	}
}

// AviLib.AviCompression -- Utility class for getting encoder/decoder compression codes.
//   (Note:  Using the pre-defines (DivX, Indeo5, etc.) will cause an exception if that
//    codec is not found on the local machine.)
//
// Gabriel Boyer
// 08/21/2003

using System;

namespace AviLib
{
	public class AviCompression
	{
        public static UInt32 GetFourCC(char ch0, char ch1, char ch2, char ch3)
        {
            return Convert.ToUInt32(ch0 | ch1 << 8 | ch2 << 16 | ch3 << 24);
        }

        public static UInt32 GetFourCC(string fcc)
        {
            if(fcc.Length != 4)
            {
                throw new AviException("Invalid FourCC string.");
            }
            else
            {
                char[] ch = (fcc.ToLower()).ToCharArray();

                return GetFourCC(ch[0], ch[1], ch[2], ch[3]);
            }
        }

        public static UInt32 DivX
        {
            get
            {
                return GetFourCC("divx");
            }
        }

        public static UInt32 Indeo5
        {
            get
            {
                return GetFourCC("iv50");
            }
        }

        public static UInt32 MicrosoftMPEG4
        {
            get
            {
                return GetFourCC("mpg4");
            }
        }

        public static UInt32 XviD
        {
            get
            {
                return GetFourCC("xvid");
            }
        }

        public static UInt32 None
        {
            get
            {
                return 0;
            }
        }
	}
}

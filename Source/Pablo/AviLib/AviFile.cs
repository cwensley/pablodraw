// AviLib.AviFile -- Class for interop with AVIFile API functions.
//
// Gabriel Boyer
// 08/21/2003

using System;
using System.Runtime.InteropServices;

namespace AviLib
{
    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct AVISTREAMINFOW 
    {
        public UInt32 fccType;
        public UInt32 fccHandler;
        public UInt32 dwFlags;
        public UInt32 dwCaps;
        public UInt16 wPriority;
        public UInt16 wLanguage;
        public UInt32 dwScale;
        public UInt32 dwRate;
        public UInt32 dwStart;
        public UInt32 dwLength;
        public UInt32 dwInitialFrames;
        public UInt32 dwSuggestedBufferSize;
        public UInt32 dwQuality;
        public UInt32 dwSampleSize;
        public UInt32 rect_left;
        public UInt32 rect_top;
        public UInt32 rect_right;
        public UInt32 rect_bottom;
        public UInt32 dwEditCount;
        public UInt32 dwFormatChangeCount;
        public UInt16 szName0;
        public UInt16 szName1;
        public UInt16 szName2;
        public UInt16 szName3;
        public UInt16 szName4;
        public UInt16 szName5;
        public UInt16 szName6;
        public UInt16 szName7;
        public UInt16 szName8;
        public UInt16 szName9;
        public UInt16 szName10;
        public UInt16 szName11;
        public UInt16 szName12;
        public UInt16 szName13;
        public UInt16 szName14;
        public UInt16 szName15;
        public UInt16 szName16;
        public UInt16 szName17;
        public UInt16 szName18;
        public UInt16 szName19;
        public UInt16 szName20;
        public UInt16 szName21;
        public UInt16 szName22;
        public UInt16 szName23;
        public UInt16 szName24;
        public UInt16 szName25;
        public UInt16 szName26;
        public UInt16 szName27;
        public UInt16 szName28;
        public UInt16 szName29;
        public UInt16 szName30;
        public UInt16 szName31;
        public UInt16 szName32;
        public UInt16 szName33;
        public UInt16 szName34;
        public UInt16 szName35;
        public UInt16 szName36;
        public UInt16 szName37;
        public UInt16 szName38;
        public UInt16 szName39;
        public UInt16 szName40;
        public UInt16 szName41;
        public UInt16 szName42;
        public UInt16 szName43;
        public UInt16 szName44;
        public UInt16 szName45;
        public UInt16 szName46;
        public UInt16 szName47;
        public UInt16 szName48;
        public UInt16 szName49;
        public UInt16 szName50;
        public UInt16 szName51;
        public UInt16 szName52;
        public UInt16 szName53;
        public UInt16 szName54;
        public UInt16 szName55;
        public UInt16 szName56;
        public UInt16 szName57;
        public UInt16 szName58;
        public UInt16 szName59;
        public UInt16 szName60;
        public UInt16 szName61;
        public UInt16 szName62;
        public UInt16 szName63;
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct AVICOMPRESSOPTIONS 
    {
        public UInt32 fccType;
        public UInt32 fccHandler;
        public UInt32 dwKeyFrameEvery;
        public UInt32 dwQuality;
        public UInt32 dwBytesPerSecond;
        public UInt32 dwFlags;
        public IntPtr lpFormat;
        public UInt32 cbFormat;
        public IntPtr lpParms;
        public UInt32 cbParms;
        public UInt32 dwInterleaveEvery;
    }

    [StructLayout(LayoutKind.Sequential, Pack=1)]
    internal struct BITMAPINFOHEADER 
    {
        public UInt32 biSize;
        public  Int32 biWidth;
        public  Int32 biHeight;
        public  Int16 biPlanes;
        public  Int16 biBitCount;
        public UInt32 biCompression;
        public UInt32 biSizeImage;
        public  Int32 biXPelsPerMeter;
        public  Int32 biYPelsPerMeter;
        public UInt32 biClrUsed;
        public UInt32 biClrImportant;
    }

    internal enum AviOpenMode
    {
        Read        = 0x00000000,
        Write       = 0x00000001,
        Create      = 0x00001000,
        CreateWrite = Create | Write
    }

    internal class AviFile
    {
        [DllImport("avifil32.dll")]
        public static extern void AVIFileInit();

        [DllImport("avifil32.dll")]
        public static extern int AVIFileOpenW(ref int ppfile, [MarshalAs(UnmanagedType.LPWStr)]string szFile, int uMode, int lpHandler);

        [DllImport("avifil32.dll")]
        public static extern int AVIFileCreateStream(int pfile, out IntPtr ppavi, ref AVISTREAMINFOW psi); 

        [DllImport("avifil32.dll")]
        public static extern int AVIMakeCompressedStream(out IntPtr ppsCompressed, IntPtr ppsSource, ref AVICOMPRESSOPTIONS lpOptions, int pclsidHandler);

        [DllImport("avifil32.dll")]
        public static extern int AVIStreamSetFormat(IntPtr pavi, Int32 lPos, ref BITMAPINFOHEADER lpFormat, Int32 cbFormat);

        [DllImport("avifil32.dll")]
        unsafe public static extern int AVISaveOptions(int hwnd, UInt32 uiFlags, int nStreams, IntPtr* ppavi, AVICOMPRESSOPTIONS** plpOptions);

        [DllImport("avifil32.dll")]
        public static extern int AVIStreamWrite(IntPtr pavi, Int32 lStart, Int32 lSamples, IntPtr lpBuffer, Int32 cbBuffer, Int32 dwFlags, Int32 plSampWritten, Int32 plBytesWritten);

        [DllImport("avifil32.dll")]
        public static extern int AVIStreamRelease(IntPtr pavi);

        [DllImport("avifil32.dll")]
        public static extern int AVIFileRelease(int pfile);

        [DllImport("avifil32.dll")]
        public static extern void AVIFileExit();
    }
}

// AviLib.AviWriter -- Class for creating and writing to AVI video files.
//
// Gabriel Boyer
// 08/21/2003

using System;
#if AVIWRITER
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace AviLib
{
    public class AviWriter
	{
        private int    _aviFile;
        private IntPtr _aviStream;
        private IntPtr _aviStreamCompressed;
        private UInt32 _frameRate;
        private int    _frameCount;
        private UInt32 _width;
        private UInt32 _height;
        private UInt32 _stride;
        private UInt32 _fccType;
        private UInt32 _fccHandler;
        private int    _result;

        public AviWriter(string path, UInt32 compression, UInt32 frameRate, int width, int height)
        {
            _fccType    = AviCompression.GetFourCC("vids");
            _fccHandler = compression;
            _frameRate  = frameRate;
            _width      = (UInt32)width;
            _height     = (UInt32)height;

            Bitmap bmp = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            _stride = (UInt32)bmpData.Stride;
            bmp.UnlockBits(bmpData);

            AviFile.AVIFileInit();

            _result = AviFile.AVIFileOpenW(ref _aviFile, path, (int)AviOpenMode.CreateWrite, 0);

            if(_result != 0)
            {
                throw new AviException("Failed to open AVI file.", _result);
            }
        }

        public void WriteFrame(Bitmap frame)
        {
            if(_frameCount == 0)
            {
                CreateStream();
                SetOptions();
            }

            frame.RotateFlip(RotateFlipType.RotateNoneFlipY);

            BitmapData frameData = frame.LockBits(new Rectangle(0, 0, (int)_width, (int)_height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

            _result = AviFile.AVIStreamWrite(_aviStreamCompressed, _frameCount, 1, frameData.Scan0, (Int32)(_stride * _height), 0, 0, 0);

            if(_result != 0)
            {
                throw new AviException("Failed to write frame to AVI.", _result);
            }

            frame.UnlockBits(frameData);

            _frameCount++;
        }

        public void Close()
        {
            if(_aviStream.ToInt32() != 0)
            {
                AviFile.AVIStreamRelease(_aviStream);
            }

            if(_aviStreamCompressed.ToInt32() != 0)
            {
                AviFile.AVIStreamRelease(_aviStreamCompressed);
            }

            if(_aviFile != 0)
            {
                AviFile.AVIFileRelease(_aviFile);
            }

            AviFile.AVIFileExit();
        }

        private void CreateStream()
        {
            AVISTREAMINFOW aviStreamInfo = new AVISTREAMINFOW();

            aviStreamInfo.fccType               = _fccType;
            aviStreamInfo.fccHandler            = _fccHandler;
            aviStreamInfo.dwScale               = 1;
            aviStreamInfo.dwRate                = _frameRate;
            aviStreamInfo.dwSuggestedBufferSize = _height * _stride;
            aviStreamInfo.dwQuality             = 0xffffffff;
            aviStreamInfo.rect_bottom           = _height;
            aviStreamInfo.rect_right            = _width;

            _result = AviFile.AVIFileCreateStream(_aviFile, out _aviStream, ref aviStreamInfo);

            if(_result != 0)
            {
                throw new AviException("Failed to create AVI stream.", _result);
            }
        }

        private void SetOptions()
        {
            AVICOMPRESSOPTIONS compressionOptions = new AVICOMPRESSOPTIONS();

            compressionOptions.fccType    = _fccType;
            compressionOptions.fccHandler = _fccHandler;
            compressionOptions.lpFormat   = new IntPtr(0);
            compressionOptions.lpParms    = new IntPtr(0);

            /*
            AVICOMPRESSOPTIONS*  p  = &compressionOptions;
            AVICOMPRESSOPTIONS** pp = &p;

            IntPtr  i  = _aviStream;
            IntPtr* ii = &i;

            _result = AviFile.AVISaveOptions(0, 0, 1, ii, pp);

            if(_result != 0)
            {
                throw new AviException("Failed to save AVI encoding options.", _result);
            }
            */

            _result = AviFile.AVIMakeCompressedStream(out _aviStreamCompressed, _aviStream, ref compressionOptions, 0);

            if(_result != 0)
            {
                throw new AviException("Failed to create compressed AVI stream.", _result);
            }

            BITMAPINFOHEADER streamFormat = new BITMAPINFOHEADER();

            streamFormat.biSize      = 40;
            streamFormat.biWidth     = (Int32)_width;
            streamFormat.biHeight    = (Int32)_height;
            streamFormat.biPlanes    = 1;
            streamFormat.biBitCount  = 24;
            streamFormat.biSizeImage = _stride * _height;

            _result = AviFile.AVIStreamSetFormat(_aviStreamCompressed, 0, ref streamFormat, 40);

            if(_result != 0)
            {
                throw new AviException("Failed to set AVI stream format.", _result);
            }
        }
	}
}
#endif

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Palettes
{
    [Serializable]
    public class Colour
    {
        public byte R = 0;
        public byte G = 0;
        public byte B = 0;
        public byte A = 0;

        public Colour(byte R, byte G, byte B, byte A)
        {
            this.R = R;
            this.G = G;
            this.B = B;
            this.A = A;
        }
        public Colour(byte R, byte G, byte B)
        {
            this.R = R;
            this.G = G;
            this.B = B;
        }
        public Colour()
        {

        }

        public long DecCode
        {
            set
            {
                A = (byte)(value % 256);
                value /= 256;
                B = (byte)(value % 256);
                value /= 256;
                G = (byte)(value % 256);
                value /= 256;
                R = (byte)(value % 256);
            }
            get
            {
                return 16777216 * R + 65536 * G + 256 * B + A;
            }
        }
    }

    [Serializable]
    public class Palette
    {
        public List<Colour> Colours = new List<Colour>();
        public int size { get { return Colours.Count; } }
    }

    [Serializable]
    public class AnimPalette
    {
        public List<Palette> Palettes = new List<Palette>();
        public int size
        {
            get
            {
                int temp = Palettes[0].size;
                foreach (Palette P in Palettes)
                    if (P.size != temp)
                        throw new Exception("Incompatible palettes!");
                return temp;
            }
        }
    }

    //Image with undefined colours
    [Serializable]
    public class PalMap
    {
        public int[][] Pixels; //Contains indexes in a palette
        public int Width { get { return Pixels[0].Length; } }
        public int Height { get { return Pixels.Length; } }
    }

    [Serializable]
    public class AnimPalMap
    {
        public List<PalMap> PalMaps = new List<PalMap>();
        public int Width { get { return PalMaps[0].Pixels[0].Length; } }
        public int Height { get { return PalMaps[0].Pixels.Length; } }
    }

    public static class Converter
    {
        public static int CurW = 0;

        static int W(int i, int Width)
        {
            return i % Width;
        }

        static int H(int i, int Width)
        {
            return i / Width;
        }

        public static byte[] Convert(PalMap PM, Palette Pal)
        {
            byte[] res = new byte[PM.Width * PM.Height * 4];

            for (int i = 0; i < PM.Width * PM.Height; i++)
            {
                int w = W(i, PM.Width);
                int h = H(i, PM.Width);

                res[4 * i] = Pal.Colours[PM.Pixels[h][w]].B;
                res[4 * i + 1] = Pal.Colours[PM.Pixels[h][w]].G;
                res[4 * i + 2] = Pal.Colours[PM.Pixels[h][w]].R;
                res[4 * i + 3] = Pal.Colours[PM.Pixels[h][w]].A;
            }

            return res;
        }

        public static Tuple<List<PalMap>, Palette> Convert(byte[][] Imgs, int W)
        {
            Palette Pal = new Palette();
            List<PalMap> PMs = new List<PalMap>();

            foreach (byte[] Img in Imgs)
            {
                PalMap PM = new PalMap();
                PMs.Add(PM);

                PM.Pixels = new int[Img.Length / W][];
                for (int i = 0; i < PM.Pixels.Length; i++)
                    PM.Pixels[i] = new int[W];

                for (int i = 0; i < Img.Length / 4; i++)
                {
                    byte B = Img[4 * i];
                    byte G = Img[4 * i + 1];
                    byte R = Img[4 * i + 2];
                    byte A = Img[4 * i + 3];

                    Colour C = new Colour(R, G, B, A);

                    if (Pal.Colours.Count == 0)
                    {
                        Pal.Colours.Add(C);
                        PM.Pixels[i / W][i % W] = 0;
                    }
                    else
                        for (int j = 0; j < Pal.Colours.Count; j++)
                            if (Pal.Colours[j].DecCode == C.DecCode)
                            {
                                PM.Pixels[i / W][i % W] = j;
                                break;
                            }
                            else if (j == Pal.Colours.Count - 1)
                            {
                                Pal.Colours.Add(C);
                                PM.Pixels[i / W][i % W] = j + 1;
                            }
                }


            }

            return new Tuple<List<PalMap>, Palette>(PMs, Pal);
        }

        public static byte[] Convert(Palette Pal)
        {
            byte[] res = new byte[Pal.Colours.Count * 4];
            for (int i = 0; i < Pal.Colours.Count; i++)
            {
                res[4 * i] = Pal.Colours[i].B;
                res[4 * i + 1] = Pal.Colours[i].G;
                res[4 * i + 2] = Pal.Colours[i].R;
                res[4 * i + 3] = Pal.Colours[i].A;
            }
            return res;
        }

        public static AnimPalette ConvertAP(params byte[][] imgs)
        {
            AnimPalette APal = new AnimPalette();
            APal.Palettes.Add(Convert(imgs, imgs[0].Length).Item2);
            return APal;
        }

        public static Palette LoadAP(byte[] img)
        {
            Palette Pal = new Palette();
            for (int i = 0; i < img.Length / 4; i++)
            {
                byte B = img[4 * i];
                byte G = img[4 * i + 1];
                byte R = img[4 * i + 2];
                byte A = img[4 * i + 3];

                Colour C = new Colour(R, G, B, A);

                Pal.Colours.Add(C);
            }
            return Pal;
        }

        public static AnimPalMap ConvertAPM(params byte[][] imgs)
        {
            AnimPalMap APM = new AnimPalMap();
            APM.PalMaps = Convert(imgs, CurW).Item1;
            return APM;
        }

        // converts filename to BitmapImage
        public static BitmapImage FilenameToImage(string filename)
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(filename, UriKind.RelativeOrAbsolute);
            image.EndInit();
            return image;
        }

        // converts only PNG image to byte[]
        public static byte[] ImageToByteArray(string filename)
        {
            PngBitmapDecoder myImage = new PngBitmapDecoder(new Uri(filename, UriKind.RelativeOrAbsolute), BitmapCreateOptions.DelayCreation, BitmapCacheOption.OnLoad);
            byte[] myImageBytes = new byte[myImage.Frames[0].PixelWidth * 4 * myImage.Frames[0].PixelHeight];
            myImage.Frames[0].CopyPixels(myImageBytes, myImage.Frames[0].PixelWidth * 4, 0);
            CurW = myImage.Frames[0].PixelWidth;
            return myImageBytes;
        }

        // converts byte[] to BitmapSource
        public static BitmapSource ByteArrayToImage(byte[] data, int w, int h, int ch)
        {
            PixelFormat format = PixelFormats.Default;

            if (ch == 1) format = PixelFormats.Gray8; //grey scale image 0-255
            if (ch == 3) format = PixelFormats.Bgr24; //RGB
            if (ch == 4) format = PixelFormats.Bgr32; //RGB + alpha

            WriteableBitmap wbm = new WriteableBitmap(w, h, 96, 96, format, null);
            wbm.WritePixels(new Int32Rect(0, 0, w, h), data, ch * w, 0);

            return wbm;
        }
    }
}

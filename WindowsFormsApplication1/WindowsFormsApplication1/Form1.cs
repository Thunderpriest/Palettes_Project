using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Palettes;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public AnimPalMap APM;
        public AnimPalette AP;

        public int cPM; //current
        public int cP; //current

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            APM = new AnimPalMap();
            AP = new AnimPalette();

            List<byte[]> imgs = new List<byte[]>();

            int i = 1;
            for (i = 1; i < 1024; i++)
                try
                {
                    byte[] bts = Converter.ImageToByteArray(@"SpriteImgs\" + i + @".png");
                    imgs.Add(bts);
                }
                catch
                {
                    break;
                }

            byte[][] Imgs = imgs.ToArray();

            APM = Converter.ConvertAPM(Imgs);
            AP = Converter.ConvertAP(Imgs);
        }

        private void button2_Click(object sender, EventArgs e)
        {

            for (int i = 0; i < AP.Palettes.Count; i++)
            {
                BitmapSource img = Converter.ByteArrayToImage(Converter.Convert(AP.Palettes[i]), AP.Palettes[i].Colours.Count, 1, 4);

                using (var fileStream = new FileStream(@"PaletteImgs\" + (i + 1) + ".png", FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(img));
                    encoder.Save(fileStream);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (APM.PalMaps.Count == 1 && AP.Palettes.Count == 1)
            {
                BitmapSource img = Converter.ByteArrayToImage(Converter.Convert(APM.PalMaps[0], AP.Palettes[0]), APM.PalMaps[0].Width, APM.PalMaps[0].Height / 4, 4); //I have no idea why am I dividing by 4 =D

                using (var fileStream = new FileStream(@"FinalImage\1.png", FileMode.Create))
                {
                    BitmapEncoder encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(img));
                    encoder.Save(fileStream);
                }
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(@"Palettes\1.plt", FileMode.Create);
            bf.Serialize(fs, AP);
            fs.Flush();
            fs.Close();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream fs = new FileStream(@"PalMaps\1.plm", FileMode.Create);
            bf.Serialize(fs, APM);
            fs.Flush();
            fs.Close();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            cP = 0;
            cPM = 0;
            Timer t = new Timer();
            t.Interval = 50;
            t.Tick += new EventHandler(DrawImage);
            t.Start();
        }

        private void DrawImage(object sender, EventArgs e)
        {
            cP = (cP + 1) % AP.Palettes.Count;
            cPM = (cPM + 1) % APM.PalMaps.Count;
            pictureBox1.Image = BitmapFromSource(Converter.ByteArrayToImage(Converter.Convert(APM.PalMaps[cPM], AP.Palettes[cP]), APM.PalMaps[cPM].Width, APM.PalMaps[cPM].Height / 4, 4));
            pictureBox1.Refresh();
        }

        private System.Drawing.Bitmap BitmapFromSource(BitmapSource bitmapsource)
        {
            System.Drawing.Bitmap bitmap;
            using (MemoryStream outStream = new MemoryStream())
            {
                BitmapEncoder enc = new BmpBitmapEncoder();

                enc.Frames.Add(BitmapFrame.Create(bitmapsource));
                enc.Save(outStream);
                bitmap = new System.Drawing.Bitmap(outStream);
            }
            return bitmap;
        }

        private void button8_Click(object sender, EventArgs e)
        {
            AP = new AnimPalette();

            int i = 1;
            for (i = 1; i < 1024; i++)
                try
                {
                    byte[] bts = Converter.ImageToByteArray(@"InPalettes\" + i + @".png");
                    AP.Palettes.Add(Converter.LoadAP(bts));
                }
                catch
                {
                    break;
                }
        }
    }
}

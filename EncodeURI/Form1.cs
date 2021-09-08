using MessagingToolkit.QRCode.Codec;
using Newtonsoft.Json.Linq;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Windows.Forms;

namespace EncodeURI
{
    public partial class Form1 : Form
    {
        string DIR_SAIDA = string.Empty;
        float escala = 1.8f;

        public Form1()
        {
            InitializeComponent();
            DIR_SAIDA = Application.StartupPath + "\\Output\\";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string texto = textBox2.Text;
            string telefone = textBox1.Text;

            VerificaDir(DIR_SAIDA);
            string linkWhats = GeraWhatsappLink(texto, telefone);

            string label = string.Empty;

            try
            {
                JObject objJson = JObject.Parse(texto);

                if (objJson.HasValues)
                {
                    QR item = Newtonsoft.Json.JsonConvert.DeserializeObject<QR>(objJson.ToString());
                    label = item.Chassi;
                }
            }
            catch { }

            GeraQRCode(linkWhats, label);
            MessageBox.Show("Finalizado!", "Gerador QR Code", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
        private void VerificaDir(string dirSaida)
        {
            if (!System.IO.Directory.Exists(dirSaida))
            {
                System.IO.Directory.CreateDirectory(dirSaida);
            }
        }
        private string GeraWhatsappLink(string texto, string telefone)
        {
            string textEncoded = "https://api.whatsapp.com/send?phone=" + telefone + "&text=";
            textEncoded += Uri.EscapeDataString(texto);

            return textEncoded;
        }
        private void GeraQRCode(string textoQR, string textoLabel)
        {
            try
            {
                string nomeArquivo = "\\QR Code_" + textoLabel.Replace("Chassi: ", "") + ".png";
                QRCodeEncoder qrCodecEncoder = new QRCodeEncoder();
                qrCodecEncoder.QRCodeBackgroundColor = Color.White;
                qrCodecEncoder.QRCodeForegroundColor = Color.Black;
                qrCodecEncoder.CharacterSet = "UTF-8";
                qrCodecEncoder.QRCodeEncodeMode = QRCodeEncoder.ENCODE_MODE.BYTE;
                qrCodecEncoder.QRCodeScale = 6;
                qrCodecEncoder.QRCodeVersion = 0;
                qrCodecEncoder.QRCodeErrorCorrect = QRCodeEncoder.ERROR_CORRECTION.Q;

                Image img = qrCodecEncoder.Encode(textoQR);
                Bitmap bmp = new Bitmap(img);

                bmp.SetResolution(1920, 1080);
                Image labelChassi = DrawText("Chassi: " + textoLabel.ToUpper(), new Font("Arial Black", 18f, FontStyle.Bold), Color.White, Color.Black, bmp.Width,50);
                Bitmap bmp2 = new Bitmap(labelChassi);
                bmp2.SetResolution(1920, 1080);
                

                labelChassi = Scale(bmp2, escala, escala);
                Image imgY = Scale(bmp, escala, escala);
                Image imgX = CombineImages(imgY, labelChassi);

                pictureBox1.Image = Scale((Bitmap)imgX, escala, escala);
                pictureBox1.Image.Save(DIR_SAIDA + nomeArquivo);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro : " + ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private Image DrawText(string text, Font font, Color textColor, Color backColor, int largura, int altura)
        {
            Image img = new Bitmap(largura, altura);
            Graphics drawing = Graphics.FromImage(img);
            drawing.Clear(backColor);
            Brush textBrush = new SolidBrush(textColor);
            drawing.SmoothingMode = SmoothingMode.AntiAlias;
            drawing.InterpolationMode = InterpolationMode.HighQualityBicubic;
            drawing.TextRenderingHint = TextRenderingHint.AntiAlias;
            drawing.DrawString(text, font, textBrush, 5, 10);
            drawing.Save();

            textBrush.Dispose();
            drawing.Dispose();

            return img;
        }
        private Image CombineImages(Image QRCode, Image label)
        {
            int width = QRCode.Width + 30;
            int height = QRCode.Height + label.Height + 40;
            Bitmap imageMerged = new Bitmap(width, height);

            Graphics g = Graphics.FromImage(imageMerged);
            g.Clear(Color.White);
            g.InterpolationMode = InterpolationMode.NearestNeighbor;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
            g.InterpolationMode = InterpolationMode.High;

            g.DrawImage(QRCode, new Point(15, 15));
            g.DrawImage(label, new Point(15, QRCode.Height + 30));
            g.Dispose();
            imageMerged.SetResolution(1920, 1080);

            return imageMerged;
        }
        private static float Lerp(float s, float e, float t)
        {
            return s + (e - s) * t;
        }
        private static float Blerp(float c00, float c10, float c01, float c11, float tx, float ty)
        {
            return Lerp(Lerp(c00, c10, tx), Lerp(c01, c11, tx), ty);
        }
        private static Image Scale(Bitmap self, float scaleX, float scaleY)
        {
            int newWidth = (int)(self.Width * scaleX);
            int newHeight = (int)(self.Height * scaleY);
            Bitmap newImage = new Bitmap(newWidth, newHeight, self.PixelFormat);

            for (int x = 0; x < newWidth; x++)
            {
                for (int y = 0; y < newHeight; y++)
                {
                    float gx = ((float)x) / newWidth * (self.Width - 1);
                    float gy = ((float)y) / newHeight * (self.Height - 1);
                    int gxi = (int)gx;
                    int gyi = (int)gy;
                    Color c00 = self.GetPixel(gxi, gyi);
                    Color c10 = self.GetPixel(gxi + 1, gyi);
                    Color c01 = self.GetPixel(gxi, gyi + 1);
                    Color c11 = self.GetPixel(gxi + 1, gyi + 1);

                    int red = (int)Blerp(c00.R, c10.R, c01.R, c11.R, gx - gxi, gy - gyi);
                    int green = (int)Blerp(c00.G, c10.G, c01.G, c11.G, gx - gxi, gy - gyi);
                    int blue = (int)Blerp(c00.B, c10.B, c01.B, c11.B, gx - gxi, gy - gyi);
                    Color rgb = Color.FromArgb(red, green, blue);
                    newImage.SetPixel(x, y, rgb);
                }
            }

            return newImage;
        }
        private Bitmap ConvertToBitonal(Bitmap original)
        {
            Bitmap source = null;

            // If original bitmap is not already in 32 BPP, ARGB format, then convert
            if (original.PixelFormat != PixelFormat.Format32bppArgb)
            {
                source = new Bitmap(original.Width, original.Height, PixelFormat.Format32bppArgb);
                source.SetResolution(original.HorizontalResolution, original.VerticalResolution);
                using (Graphics g = Graphics.FromImage(source))
                {
                    g.DrawImageUnscaled(original, 0, 0);
                }
            }
            else
            {
                source = original;
            }
            // some stuff here

            // Create destination bitmap
            Bitmap destination = new Bitmap(source.Width, source.Height, PixelFormat.Format1bppIndexed);


            return destination;
            // other stuff
        }
        public void GeraQRCode2(string texto)
        {
            //QRCoder.QRCodeGenerator qr = new QRCoder.QRCodeGenerator();
            //var myData = qr.CreateQrCode(texto, QRCoder.QRCodeGenerator.ECCLevel.M);
            //var code = new QRCoder.QRCode(myData);

            //Image newImage = code.GetGraphic(10);
            //pictureBox2.Image = code.GetGraphic(10);
            //pictureBox2.Image.Save("D:\\qrcode2.png");
            //if (newImage is Bitmap oi)
            //{
            //    oi.SetResolution(1920, 1080);
            //    pictureBox1.Image = Scale(oi, 1.1f, 1.1f);
            //    pictureBox1.Image.Save("D:\\qrcode.png");

            //    //pictureBox1.Image = code.GetGraphic(10);
            //    //pictureBox1.Image = code.GetGraphic(10);
            //}

            //Bitmap bmp = new Bitmap(code.GetGraphic(10));
            //Graphics g = Graphics.FromImage(bmp);
            //g.DrawImage(bmp, new Point(pictureBox3.Size.Width, pictureBox3.Size.Height));
            //g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            //g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;

            //bmp.SetResolution(640, 480);

            //pictureBox3.Image = bmp;
            //pictureBox3.Image.Save("D:\\qrcode3.png");
        }
        public void MergeImage()
        {
            Image playbutton;
            Image frame;

            playbutton = Image.FromFile(@"D:\Imagens teste\01.jpg");
            frame = Image.FromFile(@"D:\Imagens teste\01.jpg");

            using (frame)
            {
                using (var bitmap = new Bitmap(640, 480))
                {
                    using (var canvas = Graphics.FromImage(bitmap))
                    {
                        canvas.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        canvas.DrawImage(frame, new Rectangle(0, 0, 320, 240), new Rectangle(0, 0, frame.Width, frame.Height), GraphicsUnit.Pixel);
                        canvas.DrawImage(playbutton, (bitmap.Width / 2) - (playbutton.Width / 2), (bitmap.Height / 2) - (playbutton.Height / 2));
                        canvas.Save();
                    }
                    try
                    {
                        bitmap.Save(@"D:\Imagens teste\ImageMerged.png", ImageFormat.Jpeg);
                    }
                    catch (Exception ex) { }
                }
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            string telefone = textBox1.Text;
            DIR_SAIDA = DIR_SAIDA + @"Massa_" + DateTime.Now.ToShortDateString().Replace(":", "_").Replace("/", "_");

            VerificaDir(DIR_SAIDA);
            string[] linhas = richTextBox1.Text.Split('\n');

            for (int i = 0; i < linhas.Length; i++)
            {
                string linkWhats = GeraWhatsappLink(linhas[i], telefone);
                string label = string.Empty;

                try
                {
                    label = linhas[i].ToUpper().Split(new string[] { "\"CHASSI\":" }, StringSplitOptions.None)[1].Replace("\"", "").Replace("}", "");
                }
                catch { }

                GeraQRCode(linkWhats, label);
            }

            MessageBox.Show("Finalizado!", "Gerador QR Code", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }
}

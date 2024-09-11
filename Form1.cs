using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Emgu.CV;
using Emgu.CV.Structure;
using System.IO;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Filtracja
{
    public partial class Form1 : Form
    {
        //Zmienna typu Size moze byc przez nas dowolnie 
        //powołana i wykorzystana w kodzie. Dzieki temu
        //mozna zdefiniowac zadany rozmiar obrazku z wewnatrz kodu.
        //Właściwość StrechImage będzie odpowiedzialna za dopasowanie
        //rozmiarow
        private Size desired_image_size;
        Image<Bgr, byte> image_PB1, image_PB2, image_PB3, image_PB4;
        Image<Bgr, byte>[] image_buffers;
        double[] filter_coeff;
        VideoCapture camera;

        //Konstruktor klasy Form1. Odpowiada za inicjalizację wszystkich
        //komponentów (kontrolki i ich rozmieszczenie i właściwości)
        //na oknie aplikacji
        public Form1()
        {
            InitializeComponent();
            desired_image_size = new Size(320, 240);
            image_PB1 = new Image<Bgr, byte>(desired_image_size);
            image_PB2 = new Image<Bgr, byte>(desired_image_size);

            // dodane poniezej jedno
            image_PB3 = new Image<Bgr, byte>(pictureBox3.Size);
            image_PB4 = new Image<Bgr, byte>(pictureBox4.Size);

            filter_coeff = new double[9];

            image_buffers = new Image<Bgr, byte>[3];
            for (int i = 0; i < image_buffers.Length; i++)
            {
                image_buffers[i] = new Image<Bgr, byte>(desired_image_size);
            }

            //Blok try catch aby przechwycić ewentualne niepowodzenie
            //tworzenia nowej instancji obiektu VideoCapture. Potrzebny, gdyz
            //w chwili tworzenia następuje próba połączenia się z kamerą która
            //może zakończyć się niepowodzeniem.
            try
            {
                camera = new VideoCapture();
                camera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameWidth, desired_image_size.Width);
                camera.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.FrameHeight, desired_image_size.Height);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        private void button_From_CvInvoke_PB1_Click(object sender, EventArgs e)
        {
            //imagePB1.SetValue(new MCvScalar(100, 100, 100)); tło 
            CvInvoke.Circle(image_PB1, new Point(80, 60), 80, new MCvScalar(255, 0, 0), -2);// - minus bo wypelniona 

            Rectangle rect = new Rectangle(10, 5, 280, 200);
            MCvScalar rCol = new MCvScalar(180, 10, 10);
            CvInvoke.Rectangle(image_PB1, rect, rCol, 10);

            Point p1 = new Point(0, 0);
            Point p2 = new Point(320, 240);
            CvInvoke.Line(image_PB1, p1, p2, new MCvScalar(0, 10, 0xFF), 10);

            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_From_CvInvoke_PB2_Click(object sender, EventArgs e)
        {
            //imagePB1.SetValue(new MCvScalar(100, 100, 100)); tło 
            CvInvoke.Circle(image_PB2, new Point(80, 60), 80, new MCvScalar(255, 0, 0), -2);// - minus bo wypelniona 

            Rectangle rect = new Rectangle(15, 15, 220, 180);
            MCvScalar rCol = new MCvScalar(170, 10, 15);
            CvInvoke.Rectangle(image_PB2, rect, rCol, 10);

            Point p1 = new Point(0, 0);
            Point p2 = new Point(320, 240);
            CvInvoke.Line(image_PB2, p1, p2, new MCvScalar(0, 10, 0xFF), 10);

            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void button_Clr_PB1_Click(object sender, EventArgs e)
        {
            //Możliwe jest przekazanie obiektów jakie chcemy zmodyfikować
            //jako argumenty metody.
            //Obiekty w C# są domyślnie przekazywane jako referencje. Są to tzw. typy referencyjne
            //Oznacza to, że zmiany dokonane na tak przekazanych obiektach "przeniosą się"
            //poza metodę, w której te modyfikacje były dokonane
            //PS: zmienne typów int, double itd to tzw typy wartościowe, a nie referencyjne.
            //Oznacza to, że te typy są kopiowane do metody.
            // clear_image(pictureBox1, image_PB1);
            wyczyszczenie_ekranow();
            listView1.Items.Clear();
            
        }

        private void button_Clr_PB2_Click(object sender, EventArgs e)
        {
            clear_image(pictureBox2, image_PB2);
        }

        private void clear_image(PictureBox PB, Image<Bgr, byte> Image)
        {
            //Zmienne typu PictureBox i Image<Bgr, byte> to instancje klas.
            //Zostały zatem "pod maską" przekazane do metody jako referencje.
            Image.SetZero();
            PB.Image = Image.Bitmap;
        }

        private void button_Browse_Files_PB1_Click(object sender, EventArgs e)
        {
            textBox_Image_Path_PB1.Text = get_image_path();
        }


        private string get_image_path()
        {
            string ret = "";
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
            openFileDialog1.Filter = "Obrazy|*.jpg;*.jpeg;*.png;*.bmp";
            openFileDialog1.Title = "Wybierz obrazek.";
            //Jeśli wszystko przebiegło ok to pobiera nazwę pliku
            if (openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            { 
                ret = openFileDialog1.FileName;
            }

            return ret;
        }

        private void button_From_File_PB1_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = get_image_bitmap_from_file(textBox_Image_Path_PB1.Text, ref image_PB1);
        }


        private Bitmap get_image_bitmap_from_file(string path, ref Image<Bgr, byte> Data)
        {
            Mat temp = CvInvoke.Imread(path);
            CvInvoke.Resize(temp, temp, desired_image_size);
            Data = temp.ToImage<Bgr, byte>();
            return Data.Bitmap;
        }

        private void button_From_Camera_PB1_Click(object sender, EventArgs e)
        {
            Mat temp = new Mat();
            camera.Read(temp);
            CvInvoke.Resize(temp, temp, pictureBox1.Size);
            image_PB1 = temp.ToImage<Bgr, byte>();
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_From_Camera_PB2_Click(object sender, EventArgs e)
        {
            Mat temp = new Mat();
            camera.Read(temp);
            CvInvoke.Resize(temp, temp, pictureBox2.Size);
            image_PB2 = temp.ToImage<Bgr, byte>();
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        //Bufory
        private void button_Buf1_To_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[0], image_PB1);
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_Buf1_From_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB1, image_buffers[0]);
            pictureBox_BUF1.Image = image_buffers[0].Bitmap;


        }

        private void button_Buf2_To_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[1], image_PB1);
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_Buf2_From_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB1, image_buffers[1]);
            pictureBox_BUF2.Image = image_buffers[1].Bitmap;
        }

        private void button_Buf3_To_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[2], image_PB1);
            pictureBox1.Image = image_PB1.Bitmap;
        }

        private void button_Buf3_From_PB1_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB1, image_buffers[2]);
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void button_Buf1_To_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[0], image_PB2);
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void button_Buf1_From_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB2, image_buffers[0]);
            pictureBox_BUF1.Image = image_buffers[0].Bitmap;
        }

        private void button_Buf2_To_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[1], image_PB2);
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void button_Buf2_From_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB2, image_buffers[1]);
            pictureBox_BUF2.Image = image_buffers[1].Bitmap;
        }

        private void button_Buf3_To_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_buffers[2], image_PB2);
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private void button_Buf3_From_PB2_Click(object sender, EventArgs e)
        {
            copy_image_data(image_PB2, image_buffers[2]);
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void copy_image_data(Image<Bgr, byte> src, Image<Bgr, byte> dest)
        {
            src.CopyTo(dest);
        }

        private void button_Logical_Operation_Click(object sender, EventArgs e)
        {
            byte[, ,] b1, b2, b3; // pobranie jako referencja 
            b1 = image_buffers[0].Data; // uproszczenie dostepu do danych
            b2 = image_buffers[1].Data;
            b3 = image_buffers[2].Data;
            for (int x = 0; x < desired_image_size.Width; x++)
            {
                for (int y = 0; y < desired_image_size.Height; y++)
                {
                    if (radioButton_buf_AND.Checked)
                    {
                        b3[y, x, 0] = (byte)(b1[y, x, 0] & b2[y, x, 0]); // & iloczyn bitowy pixel w pixel
                        b3[y, x, 1] = (byte)(b1[y, x, 1] & b2[y, x, 1]);
                        b3[y, x, 2] = (byte)(b1[y, x, 2] & b2[y, x, 2]);
                    }
                    else if (radioButton_buf_OR.Checked)
                    {
                        b3[y, x, 0] = (byte)(b1[y, x, 0] | b2[y, x, 0]); // 
                        b3[y, x, 1] = (byte)(b1[y, x, 1] | b2[y, x, 1]);
                        b3[y, x, 2] = (byte)(b1[y, x, 2] | b2[y, x, 2]);
                    }
                    else if (radioButton_buf_XOR.Checked)
                    {
                        b3[y, x, 0] = (byte)(b1[y, x, 0] ^ b2[y, x, 0]); // 
                        b3[y, x, 1] = (byte)(b1[y, x, 1] ^ b2[y, x, 1]);
                        b3[y, x, 2] = (byte)(b1[y, x, 2] ^ b2[y, x, 2]);
                    }
                }
            }
        
            image_buffers[2].Data = b3;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        //Filtry

        private void button_Low_Pass_Coeff_Click(object sender, EventArgs e)
        {
            numericUpDown_Filtr_Param11.Value = 1;
            numericUpDown_Filtr_Param12.Value = 1;
            numericUpDown_Filtr_Param13.Value = 1;

            numericUpDown_Filtr_Param21.Value = 1;
            numericUpDown_Filtr_Param22.Value = 0;
            numericUpDown_Filtr_Param23.Value = 1;

            numericUpDown_Filtr_Param31.Value = 1;
            numericUpDown_Filtr_Param32.Value = 1;
            numericUpDown_Filtr_Param33.Value = 1;
        }

        private void button_High_Pass_Coeff_Click(object sender, EventArgs e)
        {
            numericUpDown_Filtr_Param11.Value = 1;
            numericUpDown_Filtr_Param12.Value = 1;
            numericUpDown_Filtr_Param13.Value = 0;

            numericUpDown_Filtr_Param21.Value = 1;
            numericUpDown_Filtr_Param22.Value = 0;
            numericUpDown_Filtr_Param23.Value = -1;

            numericUpDown_Filtr_Param31.Value = 0;
            numericUpDown_Filtr_Param32.Value = -1;
            numericUpDown_Filtr_Param33.Value = -1;
        }

        private void button_Apply_Filter_Click(object sender, EventArgs e)
        {
            filter();
        }

        private void filter()
        {
            //Dodać kod g(x, y) = { - sumy wzor str 1

            double[] wsp = new double[9];
            double suma_wsp = 0;
            wsp[0] = (double)numericUpDown_Filtr_Param11.Value;
            wsp[1] = (double)numericUpDown_Filtr_Param12.Value;
            wsp[2] = (double)numericUpDown_Filtr_Param13.Value;
            wsp[3] = (double)numericUpDown_Filtr_Param21.Value;
            wsp[4] = (double)numericUpDown_Filtr_Param22.Value;
            wsp[5] = (double)numericUpDown_Filtr_Param23.Value;
            wsp[6] = (double)numericUpDown_Filtr_Param31.Value;
            wsp[7] = (double)numericUpDown_Filtr_Param32.Value;
            wsp[8] = (double)numericUpDown_Filtr_Param33.Value;

            for (int i = 0; i < 9; i++)
                suma_wsp += wsp[i];

            byte[,,] temp1 = image_buffers[1].Data;
            byte[,,] temp2 = image_buffers[2].Data;

            for (int x = 1; x < desired_image_size.Width - 1; x++) // od 1 i do -1 bo pomijamy krawedzie jak na stronie wytlumaczone bylo strom.io
            {
                for (int y = 1; y < desired_image_size.Height - 1; y++)
                { //Zerowanie zmiennych pomocniczych
                  //Dodać kod
                    double R = 0, G = 0, B = 0;
                    B += wsp[0] * temp1[y - 1, x - 1, 0];
                    B += wsp[1] * temp1[y - 1, x , 0];
                    B += wsp[2] * temp1[y - 1, x + 1, 0];

                    B += wsp[3] * temp1[y , x - 1, 0];
                    B += wsp[4] * temp1[y, x, 0];
                    B += wsp[5] * temp1[y, x + 1, 0];

                    B += wsp[6] * temp1[y + 1, x - 1, 0];
                    B += wsp[7] * temp1[y + 1, x, 0];
                    B += wsp[8] * temp1[y + 1, x + 1, 0];

                    G += wsp[0] * temp1[y - 1, x - 1, 1];
                    G += wsp[1] * temp1[y - 1, x, 1];
                    G += wsp[2] * temp1[y - 1, x + 1, 1];

                    G += wsp[3] * temp1[y, x - 1, 1];
                    G += wsp[4] * temp1[y, x, 1];
                    G += wsp[5] * temp1[y, x + 1, 1];

                    G += wsp[6] * temp1[y + 1, x - 1, 1];
                    G += wsp[7] * temp1[y + 1, x, 1];
                    G += wsp[8] * temp1[y + 1, x + 1, 1];

                    R += wsp[0] * temp1[y - 1, x - 1, 2];
                    R += wsp[1] * temp1[y - 1, x, 2];
                    R += wsp[2] * temp1[y - 1, x + 1, 2];

                    R += wsp[3] * temp1[y, x - 1, 2];
                    R += wsp[4] * temp1[y, x, 2];
                    R += wsp[5] * temp1[y, x + 1, 2];

                    R += wsp[6] * temp1[y + 1, x - 1, 2];
                    R += wsp[7] * temp1[y + 1, x, 2];
                    R += wsp[8] * temp1[y + 1, x + 1, 2];

                    if((int)suma_wsp != 0)
                    {
                        B /= suma_wsp;
                        G /= suma_wsp;
                        R /= suma_wsp;
                    }
                    if (checkBox_Add_Half.Checked) // dodanie szarego tla 
                    {
                        B /= 2;
                        G /= 2;
                        R /= 2;
                        B += 128;
                        G += 128;
                        R += 128;
                    }
                    if (B < 0) B = 0;
                    if (G < 0) G = 0;
                    if (R < 0) R = 0;
                    if (B > 255) B = 255;
                    if (G > 255) G = 255;
                    if (R > 255) R = 255;

                    temp2[y, x, 0] = (byte)B;
                    temp2[y, x, 1] = (byte)G;
                    temp2[y, x, 2] = (byte)R;

                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void button_Thresh_Click(object sender, EventArgs e)
        {
            Threshold();
        }

        private void Threshold()
        {

        }

        private void button_Dilate_Click(object sender, EventArgs e)
        {
            Dilate();
        }

        private void button_Erode_Click(object sender, EventArgs e)
        {
            Erode();
        }

        private void Dilate()
        {
            double R, G, B;

            byte[, ,] temp1 = image_buffers[1].Data;
            byte[, ,] temp2 = image_buffers[2].Data;

            for (int x = 1; x < desired_image_size.Width - 1; x++)
            {
                for (int y = 1; y < desired_image_size.Height - 1; y++)
                {
                    R = G = B = 0;
                    B = temp1[y - 1, x - 1, 0];
                    B = Math.Max(temp1[y - 1, x, 0], B);
                    B = Math.Max(temp1[y - 1, x+1, 0], B);
                    B = Math.Max(temp1[y , x-1, 0], B);
                    B = Math.Max(temp1[y , x, 0], B);
                    B = Math.Max(temp1[y , x+1, 0], B);
                    B = Math.Max(temp1[y + 1, x - 1, 0], B);
                    B = Math.Max(temp1[y + 1, x, 0], B);
                    B = Math.Max(temp1[y + 1, x+1, 0], B);

                    G = temp1[y - 1, x - 1, 1];
                    G = Math.Max(temp1[y - 1, x, 1], G);
                    G = Math.Max(temp1[y - 1, x + 1, 1], G);
                    G = Math.Max(temp1[y, x - 1, 1], G);
                    G = Math.Max(temp1[y, x, 1], G);
                    G = Math.Max(temp1[y, x + 1, 1], G);
                    G = Math.Max(temp1[y + 1, x - 1, 1], G);
                    G = Math.Max(temp1[y + 1, x, 1], G);
                    G = Math.Max(temp1[y + 1, x + 1, 1], G);

                    R = temp1[y - 1, x - 1, 2];
                    R = Math.Max(temp1[y - 1, x, 2], R);
                    R = Math.Max(temp1[y - 1, x + 1, 2], R);
                    R = Math.Max(temp1[y, x - 1, 2], R);
                    R = Math.Max(temp1[y, x, 2], R);
                    R = Math.Max(temp1[y, x + 1, 2], R);
                    R = Math.Max(temp1[y + 1, x - 1, 2], R);
                    R = Math.Max(temp1[y + 1, x, 2], R);
                    R = Math.Max(temp1[y + 1, x + 1, 2], R);

                    temp2[y, x, 0] = (byte)B;
                    temp2[y, x, 1] = (byte)G;
                    temp2[y, x, 2] = (byte)R;
                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private void Progow_Click(object sender, EventArgs e)
        {
            ThresholdColor();
        }

        private void ThresholdColor()
        {
            byte[,,] temp1 = image_buffers[1].Data;
            byte[,,] temp2 = image_buffers[2].Data;
            double BL, BH, GL, GH, RL, RH;
            BL = (double)UpDownBL.Value;
            BH = (double)UpDownBH.Value;
            GL = (double)UpDownGL.Value;
            GH = (double)UpDownGH.Value;
            RL = (double)UpDownRL.Value;
            RH = (double)UpDownRH.Value;
            for (int x = 0; x < desired_image_size.Width; x++)
            {
                for (int y = 0; y < desired_image_size.Height; y++) 
                {
                    if (temp1[y, x, 0]>= BL && temp1[y,x,0]<=BH&&
                       temp1[y, x, 1] >= GL && temp1[y, x, 1] <= GH &&
                       temp1[y, x, 2] >= RL && temp1[y, x, 2] <= RH)
                    {
                        temp2[y, x, 0] = 255;
                        temp2[y, x, 1] = 255;
                        temp2[y, x, 2] = 255;
                    }
                    else
                    {
                        temp2[y, x, 0] = 0;
                        temp2[y, x, 1] = 0;
                        temp2[y, x, 2] = 0;
                    }
                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
            button_Buf3_To_PB2.PerformClick();

        }

        private void UpDownBL_ValueChanged(object sender, EventArgs e)
        {
            ThresholdColor();
        }

        private void UpDownBH_ValueChanged(object sender, EventArgs e)
        {
            ThresholdColor();
        }

        private void UpDownGL_ValueChanged(object sender, EventArgs e)
        {
            ThresholdColor();
        }

        private void UpDownGH_ValueChanged(object sender, EventArgs e)
        {
            ThresholdColor();
        }

        private void UpDownRL_ValueChanged(object sender, EventArgs e)
        {
            ThresholdColor();
        }

        private void UpDownRH_ValueChanged(object sender, EventArgs e)
        {
            ThresholdColor();
        }

      
        private void Erode()
        {
            double R, G, B;

            byte[, ,] temp1 = image_buffers[1].Data;
            byte[, ,] temp2 = image_buffers[2].Data;

            for (int x = 1; x < desired_image_size.Width - 1; x++)
            {
                for (int y = 1; y < desired_image_size.Height - 1; y++)
                {
                    R = G = B = 0;
                    B = temp1[y - 1, x - 1, 0];
                    B = Math.Min(temp1[y - 1, x, 0], B);
                    B = Math.Min(temp1[y - 1, x + 1, 0], B);
                    B = Math.Min(temp1[y, x - 1, 0], B);
                    B = Math.Min(temp1[y, x, 0], B);
                    B = Math.Min(temp1[y, x + 1, 0], B);
                    B = Math.Min(temp1[y + 1, x - 1, 0], B);
                    B = Math.Min(temp1[y + 1, x, 0], B);
                    B = Math.Min(temp1[y + 1, x + 1, 0], B);

                    G = temp1[y - 1, x - 1, 1];
                    G = Math.Min(temp1[y - 1, x, 1], G);
                    G = Math.Min(temp1[y - 1, x + 1, 1], G);
                    G = Math.Min(temp1[y, x - 1, 1], G);
                    G = Math.Min(temp1[y, x, 1], G);
                    G = Math.Min(temp1[y, x + 1, 1], G);
                    G = Math.Min(temp1[y + 1, x - 1, 1], G);
                    G = Math.Min(temp1[y + 1, x, 1], G);
                    G = Math.Min(temp1[y + 1, x + 1, 1], G);

                    R = temp1[y - 1, x - 1, 2];
                    R = Math.Min(temp1[y - 1, x, 2], R);
                    R = Math.Min(temp1[y - 1, x + 1, 2], R);
                    R = Math.Min(temp1[y, x - 1, 2], R);
                    R = Math.Min(temp1[y, x, 2], R);
                    R = Math.Min(temp1[y, x + 1, 2], R);
                    R = Math.Min(temp1[y + 1, x - 1, 2], R);
                    R = Math.Min(temp1[y + 1, x, 2], R);
                    R = Math.Min(temp1[y + 1, x + 1, 2], R);

                    temp2[y, x, 0] = (byte)B;
                    temp2[y, x, 1] = (byte)G;
                    temp2[y, x, 2] = (byte)R;
                }
            }
            image_buffers[2].Data = temp2;
            pictureBox_BUF3.Image = image_buffers[2].Bitmap;
        }

        private MCvScalar cecha_palnosci = new MCvScalar(0xFF, 0xFF, 0xFF);
        Queue<Point> pix_tlace = new Queue<Point>();
        private MCvScalar kolor_tlenia = new MCvScalar(51, 153, 255);

        private void Rozpocznij_pozar(/*object sender, EventArgs e*/)
        {
            int X = 0;
            int Y = 0;
            byte[,,] temp = image_PB1.Data;
            //X = Convert.ToInt32(textBox_X.Text);
            //Y = Convert.ToInt32(textBox_Y.Text);

            if (Sprawdz_czy_cecha_palnosci(temp[Y, X, 0], temp[Y, X, 1], temp[Y, X, 2]))
            {
                pix_tlace.Enqueue(new Point(X, Y));
                temp[Y, X, 0] = (byte)kolor_tlenia.V0;
                temp[Y, X, 1] = (byte)kolor_tlenia.V1;
                temp[Y, X, 2] = (byte)kolor_tlenia.V2;
            }

            // Wyswietl_dane_pozaru();
            image_PB1.Data = temp;
            pictureBox1.Image = image_PB1.Bitmap;
        }

        

        private bool Sprawdz_czy_cecha_palnosci(byte B, byte G, byte R)
        {
            if (B == cecha_palnosci.V0 && G == cecha_palnosci.V1 && R == cecha_palnosci.V2)
                return true;
            else
                return false;
        }

        private int nr_pozaru = 0;
        private MCvScalar aktualny_kolor_wypalenia = new MCvScalar(100, 100, 100);
        private MCvScalar kolor_wypalenia = new MCvScalar(100, 100, 100);


        private void Pozar_Calosci()
        {
            byte[,,] temp = image_PB1.Data;
            for (int y = 1; y < desired_image_size.Height - 2; y++)
            {
                for (int x = 1; x < desired_image_size.Width - 2; x++)
                {
                    if (Sprawdz_czy_cecha_palnosci(temp[y, x, 0], temp[y, x, 1], temp[y, x, 2]))
                    {
                        nr_pozaru++;
                        aktualny_kolor_wypalenia.V0 = kolor_wypalenia.V0 + nr_pozaru;
                        aktualny_kolor_wypalenia.V1 = kolor_wypalenia.V1 + nr_pozaru;
                        aktualny_kolor_wypalenia.V2 = kolor_wypalenia.V2 + nr_pozaru;
                        pix_tlace.Enqueue(new Point(x, y));
                        Cykl_Pozaru();
                        temp = image_PB1.Data;
                    }
                }

            }
            // Wyswietl_dane_pozaru();
            image_PB1.Data = temp;
            pictureBox1.Image = image_PB1.Bitmap;

            // Dokończyć podpowiedz w button_Rozpocznij_pozar_Click
        }

        private void Cykl_Pozaru()
        {
            while (pix_tlace.Count > 0)
            {
                Krok_Pozaru();
            }
        }

        Queue<Point> pix_palace = new Queue<Point>();

        private void Krok_Pozaru()
        {
            //W języku C# wszystkie tablice są tzw typami referencyjnymi. Oznacza to, że w tym przypadku
            //do metody zostanie przekazana referencja, a nie skopiowana wartość czyli zmiany dokonane w metodzie
            //będą widoczne poza nią, a wydajność nie zostanie pogorszona nadmiarowymi operacjami kopiowania.
            byte[,,] temp = image_PB1.Data;

            Tlace_do_palacych(temp);

            foreach (Point pix in pix_palace)
            {
                Tlenie_od_palacego(temp, pix);
            }

            foreach (Point pix in pix_palace)
            {
                Nadpalenie_palacego(temp, pix);
            }

            Wypalenie_palacego(temp);

            image_PB1.Data = temp;
            pictureBox1.Image = image_PB1.Bitmap;
            // Wyswietl_dane_pozaru();
            //Dokańcza kolejkę oczekujących zdarzeń interfejsu graficznego. Dodatkowy opis w "button_Krok_pozaru_Click"
            Application.DoEvents();
        }

        private MCvScalar kolor_palenia = new MCvScalar(0, 0, 204);


        private void Tlace_do_palacych(byte[,,] temp)
        {
            while (pix_tlace.Count > 0)
            {
                Point p = pix_tlace.Dequeue();
                pix_palace.Enqueue(p);
                temp[p.Y, p.X, 0] = (byte)kolor_palenia.V0;
                temp[p.Y, p.X, 1] = (byte)kolor_palenia.V1;
                temp[p.Y, p.X, 2] = (byte)kolor_palenia.V2;
            }
        }

        private void Tlenie_od_palacego(byte[,,] temp, Point pix_in)
        {
            if (Czy_piksel_w_zakresie(pix_in))
            {
                Point[] sasiedzi = Wylicz_wspolrzedne_sasiednich_pikseli(pix_in);
                foreach (Point p in sasiedzi)
                {
                    if (Sprawdz_czy_cecha_palnosci(temp[p.Y, p.X, 0], temp[p.Y, p.X, 1], temp[p.Y, p.X, 2]))
                    {
                        pix_tlace.Enqueue(new Point(p.X, p.Y));
                        temp[p.Y, p.X, 0] = (byte)kolor_tlenia.V0;
                        temp[p.Y, p.X, 1] = (byte)kolor_tlenia.V1;
                        temp[p.Y, p.X, 2] = (byte)kolor_tlenia.V2;
                    }
                }
            }
        }

        private bool Czy_piksel_w_zakresie(Point pix_in)
        {
            int max_W, max_H;
            max_W = desired_image_size.Width - 1;
            max_H = desired_image_size.Height - 1;
            if (pix_in.X > 0 && pix_in.X < max_W && pix_in.Y > 0 && pix_in.Y < max_H)
                return true;
            else
                return false;
        }

        private bool skos = false;

        private Point[] Wylicz_wspolrzedne_sasiednich_pikseli(Point pix_in)
        {
            List<Point> sasiedzi = new List<Point>();
            sasiedzi.Add(new Point(pix_in.X - 1, pix_in.Y));
            sasiedzi.Add(new Point(pix_in.X + 1, pix_in.Y));
            sasiedzi.Add(new Point(pix_in.X, pix_in.Y - 1));
            sasiedzi.Add(new Point(pix_in.X, pix_in.Y + 1));

            if (skos)
            {
                sasiedzi.Add(new Point(pix_in.X - 1, pix_in.Y - 1));
                sasiedzi.Add(new Point(pix_in.X + 1, pix_in.Y + 1));
                sasiedzi.Add(new Point(pix_in.X - 1, pix_in.Y + 1));
                sasiedzi.Add(new Point(pix_in.X + 1, pix_in.Y - 1));
            }

            return sasiedzi.ToArray();
        }

        private bool cecha_dowolna = false;
        Queue<Point> pix_nadpalone = new Queue<Point>();
        private MCvScalar kolor_nadpalenia = new MCvScalar(51, 204, 51);
        private void Nadpalenie_palacego(byte[,,] temp, Point pix_in)
        {
            //Należy zobaczyć co się stanie z rysunkiem innym niż *.bmp i/lub takim na którym została wywołana metoda
            //resize zarówno dla cechy dowolnej (jakiejkolwiek) jak i konkretnej
            //Należy zwrócic uwagę na nieoczekiwane zmiany kolorów na modyfikowanych lub kompresowanych obrazach
            if (Czy_piksel_w_zakresie(pix_in))
            {
                Point[] sasiedzi = Wylicz_wspolrzedne_sasiednich_pikseli(pix_in);
                bool nalezy_nadpalic = false;
                foreach (Point p in sasiedzi)
                {
                    if (cecha_dowolna)
                        nalezy_nadpalic = Sprawdz_czy_jakiekolwiek_nadpalenie(temp[p.Y, p.X, 0], temp[p.Y, p.X, 1], temp[p.Y, p.X, 2]);
                    else
                        nalezy_nadpalic = Sprawdz_czy_cecha_nadpalenia(temp[p.Y, p.X, 0], temp[p.Y, p.X, 1], temp[p.Y, p.X, 2]);
                    if (nalezy_nadpalic)
                    {
                        pix_nadpalone.Enqueue(new Point(p.X, p.Y));
                        temp[p.Y, p.X, 0] = (byte)kolor_nadpalenia.V0;
                        temp[p.Y, p.X, 1] = (byte)kolor_nadpalenia.V1;
                        temp[p.Y, p.X, 2] = (byte)kolor_nadpalenia.V2;
                    }
                }
            }
        }

        private MCvScalar cecha_nadpalenia = new MCvScalar(0, 0, 0);

        private bool Sprawdz_czy_cecha_nadpalenia(byte B, byte G, byte R)
        {
            if (B == cecha_nadpalenia.V0 && G == cecha_nadpalenia.V1 && R == cecha_nadpalenia.V2)
                return true;
            else
                return false;
        }

        private bool Sprawdz_czy_jakiekolwiek_nadpalenie(byte B, byte G, byte R)
        {
            if (B == cecha_palnosci.V0 && G == cecha_palnosci.V1 && R == cecha_palnosci.V2)
                return false;
            else if (B == cecha_nadpalenia.V0 && G == cecha_nadpalenia.V1 && R == cecha_nadpalenia.V2)
                return true;
            else if (B == kolor_tlenia.V0 && G == kolor_tlenia.V1 && R == kolor_tlenia.V2)
                return false;
            else if (B == kolor_nadpalenia.V0 && G == kolor_nadpalenia.V1 && R == kolor_nadpalenia.V2)
                return false;
            else if (B == kolor_palenia.V0 && G == kolor_palenia.V1 && R == kolor_palenia.V2)
                return false;
            else if (B == aktualny_kolor_wypalenia.V0 && G == aktualny_kolor_wypalenia.V1 && R == aktualny_kolor_wypalenia.V2)
                return false;
            else
                return true;
        }

        Queue<Point> pix_wypalone = new Queue<Point>();

        private void Wypalenie_palacego(byte[,,] temp)
        {
            while (pix_palace.Count > 0)
            {
                Point p = pix_palace.Dequeue();
                pix_wypalone.Enqueue(p);
                temp[p.Y, p.X, 0] = (byte)(aktualny_kolor_wypalenia.V0);
                temp[p.Y, p.X, 1] = (byte)(aktualny_kolor_wypalenia.V1);
                temp[p.Y, p.X, 2] = (byte)(aktualny_kolor_wypalenia.V2);
            }
        }

        private void Narysuj_wybrany_obiekt(int nr)
        {
            image_PB2.SetZero();
            byte[,,] temp1 = image_PB1.Data;
            byte[,,] temp2 = image_PB2.Data;

            MCvScalar kolor = new MCvScalar();
            kolor.V0 = kolor_wypalenia.V0 + nr;
            kolor.V1 = kolor_wypalenia.V1 + nr;
            kolor.V2 = kolor_wypalenia.V2 + nr;

            for (int y = 1; y < desired_image_size.Height - 2; y++)
            {
                for (int x = 1; x < desired_image_size.Width - 2; x++)
                {

                    if (kolor.V0 == temp1[y, x, 0] && kolor.V1 == temp1[y, x, 1] && kolor.V2 == temp1[y, x, 2])
                    {
                        temp2[y, x, 0] = (byte)255;
                        temp2[y, x, 1] = (byte)255;
                        temp2[y, x, 2] = (byte)255;

                    }
                    else
                    {
                        temp2[y, x, 0] = (byte)0;
                        temp2[y, x, 1] = (byte)0;
                        temp2[y, x, 2] = (byte)0;
                    }
                }
            }

            image_PB2.Data = temp2;
            pictureBox2.Image = image_PB2.Bitmap;
        }

        private Point obliczenie_srodka_ciezkosci()
        {
            // listView_Dane_Mechanika.Items.Clear();
            // image_PB3.Data = image_PB2.Data;
            // u mnie na PB4
            image_PB4.Data = image_PB2.Data;

            //Reczne liczenie
            double F, Sx, Sy, x0, y0;
            double Jx0, Jy0, Jx0y0, Jx, Jy, Jxy, Je_0, Jt_0;
            double alfa_e, alfa_t, alfa_e_deg, alfa_t_deg;
            F = Sx = Sy = Jx0 = Jy0 = Jx0y0 = Jx = Jy = Jxy = Je_0 = Jt_0 = alfa_e = alfa_t = alfa_e_deg = alfa_t_deg = x0 = y0 = 0;

            //Odciecie ewentualnego stykania sie z krawedzia obrazu
            CvInvoke.Rectangle(image_PB2, new Rectangle(0, 0, desired_image_size.Width, desired_image_size.Height), new MCvScalar(0, 0, 0), 2);
            pictureBox2.Image = image_PB2.Bitmap;
            Application.DoEvents();

            //Wyliczenie momentow 1 i 2 stopnia
            byte[,,] temp = image_PB2.Data;
            for (int X = 0; X < desired_image_size.Width; X++)
            {
                for (int Y = 0; Y < desired_image_size.Height; Y++)
                {
                    if (temp[Y, X, 0] == 0xFF && temp[Y, X, 1] == 0xFF && temp[Y, X, 2] == 0xFF)
                    {
                        F = F + 1;
                        Sx = Sx + Y;
                        Sy = Sy + X;
                        Jx = Jx + Math.Pow(Y, 2);
                        Jy = Jy + Math.Pow(X, 2);
                        Jxy = Jxy + X * Y;
                    }
                }
            }
            //Obliczenie środka cieżkości
            if (F > 0)
            {
                x0 = Sy / F; // changed to int from double 
                y0 = Sx / F;
            }

            
            Point srodek_ciezkosci = new Point((int)x0, (int)y0);
            return srodek_ciezkosci;
        }

        private double[] tabela_promieni;
        private double[] tabela_wartosci_srednich;

        private string wyznaczenie_figury()
        {
            namaluj_dane_z_tabeli(tabela_promieni, null, new MCvScalar(255, 0, 0), TrybRysowania.TYLKO_DANE);
            usrednianie_wykresu();

            int liczba_wierzcholkow = 0;
            liczba_wierzcholkow = licz_wierzcholki(tabela_promieni, tabela_wartosci_srednich);

            switch (liczba_wierzcholkow)
            {
                case 3:
                    return "trojkat";
                case 2: // dla prostokatow pokrywaja sie dwa wierzcholki?
                    return "prostokat";
                case 5:
                    return "pieciokat";
                case 6:
                    return "szesciokat";
                default:
                    return "nieznany";
            }
        }

        private void czysc_obraz(Image<Bgr, byte> im, PictureBox PB)
        {
            im.SetZero();
            PB.Image = im.Bitmap;
        }

        private void usrednianie_wykresu()
        {
            MCvScalar kolor_nad_srednia = new MCvScalar(0, 255, 255);

            tabela_wartosci_srednich = wylicz_srednia_z_sygnatury(tabela_promieni);

            /*
            if (radioButton_Average_constant.Checked)
                kolor_nad_srednia = new MCvScalar(0, 255, 255);
            else if (radioButton_Average_minmax.Checked)
                kolor_nad_srednia = new MCvScalar(255, 0, 255);
            else if (radioButton_Average_moving.Checked)
                kolor_nad_srednia = new MCvScalar(0, 100, 255);
            

            if ((radioButton_Average_constant.Checked || radioButton_Average_moving.Checked) &&
                checkBox_Mix_averages.Checked)
            {
                kolor_nad_srednia = new MCvScalar(0, 255, 0);
            }
            */

            // wybrany trzeci warunek z if
            kolor_nad_srednia = new MCvScalar(0, 255, 0);

            namaluj_dane_z_tabeli(tabela_promieni, tabela_wartosci_srednich, kolor_nad_srednia, TrybRysowania.NAD_KRZYWA);
            namaluj_dane_z_tabeli(tabela_promieni, tabela_wartosci_srednich, kolor_nad_srednia, TrybRysowania.TYLKO_KRZYWA);
        }

        private double[] wylicz_srednia_z_sygnatury(double[] data)
        {
            double[] srednia = new double[data.Length];
            //bool mix_averages = checkBox_Mix_averages.Checked;
            /*
            if (radioButton_Average_minmax.Checked)
            {
                double avg = (data.Max() + data.Min()) / 2.0;
                for (int i = 0; i < data.Length; i++)
                {
                    srednia[i] = avg;
                }
            }
            else if (mix_averages == false)
            {
                if (radioButton_Average_moving.Checked)
                {
                    int avg_width = (int)numericUpDown_Moving_Average.Value;
                    int maxID = data.Length - 1;
                    for (int i = 0; i < data.Length; i++)
                    {
                        double avg = 0;
                        int nr = 0;
                        int id = 0;
                        for (int f = -avg_width; f <= avg_width; f++)
                        {
                            nr++;
                            id = i + f;
                            avg += data[modulo(id, maxID)];
                        }
                        avg /= nr;
                        srednia[i] = avg;
                    }
                }
                else if (radioButton_Average_constant.Checked)
                {
                    double avg = 0;
                    for (int i = 0; i < data.Length; i++)
                    {
                        avg += data[i];
                    }
                    avg /= (data.Length);
                    for (int i = 0; i < data.Length; i++)
                    {
                        srednia[i] = avg;
                    }
                }
            }
            else if (mix_averages == true)
            {
                double avg_C = 0;
                double ratio = (double)numericUpDown_Average_C2M_weight.Value;
                for (int i = 0; i < data.Length; i++)
                {
                    avg_C += data[i];
                }
                avg_C /= (data.Length);

                int avg_width = (int)numericUpDown_Moving_Average.Value;
                int maxID = data.Length - 1;
                for (int i = 0; i < data.Length; i++)
                {
                    double avg_M = 0;
                    int nr = 0;
                    int id = 0;
                    for (int f = -avg_width; f <= avg_width; f++)
                    {
                        nr++;
                        id = i + f;
                        avg_M += data[modulo(id, maxID)];
                    }
                    avg_M /= nr;
                    srednia[i] = ((avg_C * ratio) + (avg_M * (1 - ratio)));
                }

            }
            */

            // wybrany pierwszy warunek z if

            double avg = (data.Max() + data.Min()) / 2.0;
            for (int i = 0; i < data.Length; i++)
            {
                srednia[i] = avg;
            }

            return srednia;
        }

        private int licz_wierzcholki(double[] dane, double[] krzywa)
        {
            double sX;
            int przeskok = (liczba_promieni / 15);
            int wierzcholki = 0;
            sX = ((double)pictureBox3.Width / (double)liczba_promieni);//Dopasowanie szerokości

            for (int i = 0; i < liczba_promieni - 1; i++)
            {
                if (dane[i] < krzywa[i] && dane[i + 1] >= krzywa[i + 1])
                {
                    wierzcholki++;
                    CvInvoke.Line(image_PB3, new Point((int)(i * sX), pictureBox3.Height), new Point((int)(i * sX), 40), new MCvScalar(0, 255, 0), 1);
                    i += przeskok;
                    CvInvoke.Line(image_PB3, new Point((int)(i * sX), pictureBox3.Height), new Point((int)(i * sX), 50), new MCvScalar(255, 255, 0), 1);
                }
            }
            // textBox_LW.Text = wierzcholki.ToString();
            pictureBox3.Image = image_PB3.Bitmap;
            return wierzcholki;
        }

        private int liczba_promieni = 720;
        private double margines_na_tekst = 15;
        private enum TrybRysowania { NAD_KRZYWA, TYLKO_DANE, TYLKO_KRZYWA };

        private void namaluj_dane_z_tabeli(double[] dane, double[] krzywa, MCvScalar kolor, TrybRysowania tryb)
        {
            int w, h;
            int rX, rY, rW, rH;
            double sX, sY;
            w = pictureBox3.Width;
            h = pictureBox3.Height;
            // w = pictureBox4.Width; 
            // h = pictureBox4.Height;
            sX = ((double)w / (double)liczba_promieni);//Dopasowanie szerokości
            sY = (((double)h - margines_na_tekst) / Math.Max(dane.Max(), 10));//Dopasowanie wysokości
            rX = rY = rW = rH = 0;

            if (tryb != TrybRysowania.TYLKO_KRZYWA)
            {
                for (int p = 0; p < liczba_promieni; p++)
                {
                    Rectangle r;
                    rX = (int)(sX * (double)p);
                    rW = ((int)(sX * (double)(p + 1))) - rX;

                    //Wybor rysowania
                    if (tryb == TrybRysowania.NAD_KRZYWA)
                    {
                        rY = (int)(h - sY * dane[p]);
                        rH = (int)((dane[p] - krzywa[p]) * sY) + 1;
                        if (rH < 0)
                            continue;
                    }
                    else if (tryb == TrybRysowania.TYLKO_DANE)
                    {
                        rY = (int)(h - sY * dane[p]);
                        rH = (int)(sY * dane[p]);
                    }

                    r = new Rectangle(rX, rY, rW, rH);
                    CvInvoke.Rectangle(image_PB3, r, kolor, -1);
                    // CvInvoke.Rectangle(image_PB4, r, kolor, -1);
                }
            }
            else
            {
                for (int p = 0; p < liczba_promieni - 1; p++)
                {
                    Point P1, P2;
                    int curr_x, next_x;
                    curr_x = (int)(sX * (double)p);
                    next_x = (int)(sX * (double)(p + 1));
                    P1 = new Point(curr_x, (int)(h - (int)(sY * krzywa[p])));
                    P2 = new Point(next_x, (int)(h - (int)(sY * krzywa[p + 1])));
                    CvInvoke.Line(image_PB3, P1, P2, kolor, 1);
                    // CvInvoke.Line(image_PB4, P1, P2, kolor, 1);
                }
            }


            pictureBox3.Image = image_PB3.Bitmap;
            // pictureBox4.Image = image_PB4.Bitmap;
        }


        int liczba_kolorow = 3;
        string kolor = "unknown";
        int ilosc_obiektow = 0;
        string figura = "unknown";
        int numer_obiektu = 0;

        private void button1_Click(object sender, EventArgs e)
        {
            numer_obiektu = 0;
            listView1.Items.Clear();
            
            button_From_File_PB1.PerformClick();
            button_Buf1_From_PB1.PerformClick();
            button_Buf2_From_PB1.PerformClick();
            
            for (int i = 1; i < liczba_kolorow + 1; i++)
            {
                nr_pozaru = 0;
                wyczyszczenie_ekranow();
                button_From_File_PB1.PerformClick();
                button_Buf1_From_PB1.PerformClick();
                button_Buf2_From_PB1.PerformClick();
                
                switch (i)
                {
                    case 1:
                        kolor = "czerwony";
                        ustawienie_wyszukiwania_koloru_czerwonego();
                        break;
                    case 2:
                        kolor = "zielony";
                        ustawienie_wyszukiwania_koloru_zielonego();
                        break;
                    case 3:
                        kolor = "zolty";
                        ustawienie_wyszukiwania_koloru_zoltego();
                        break;
                }

                operacje_wyluskania_obiektow();

                button_Buf2_To_PB1.PerformClick();

                Rozpocznij_pozar(); // tego co jest na PB1
                Pozar_Calosci(); // stad biore nr pozaru
                ilosc_obiektow = nr_pozaru + 1;

                clear_image(pictureBox2, image_PB2);

                for (int j = 1; j < ilosc_obiektow; j++)
                {
                    numer_obiektu += 1;
                    Narysuj_wybrany_obiekt(j); // na PB2 
                    Point srodek = new Point();
                    srodek = obliczenie_srodka_ciezkosci(); // tego co na PB2 ale jakby kopiuje dane do PB4 tez
                    wyznaczenie_sygnatury(srodek);
                    figura = wyznaczenie_figury(); 
                    listView1.Items.Add(numer_obiektu + ". "  + kolor + " " + figura);
                }
            }
            button_Buf1_To_PB1.PerformClick();
        }
        
        private void operacje_wyluskania_obiektow()
        {
            Progow.PerformClick();
            button_Buf2_From_PB2.PerformClick();

            for (int i = 0; i < 2; i++)
            {
                button_Dilate.PerformClick();
                button_Buf3_To_PB2.PerformClick();
                button_Buf2_From_PB2.PerformClick();
            }

            for (int i = 0; i < 5; i++)
            {
                button_Erode.PerformClick();
                button_Buf3_To_PB2.PerformClick();
                button_Buf2_From_PB2.PerformClick();
            }
            
            for (int i = 0; i < 1; i++)
            {
                button_Dilate.PerformClick();
                button_Buf3_To_PB2.PerformClick();
                button_Buf2_From_PB2.PerformClick();
            }

            radioButton_buf_AND.PerformClick();
            button_Logical_Operation.PerformClick();
        }

        private void wyczyszczenie_ekranow()
        {
            clear_image(pictureBox1, image_PB1);
            clear_image(pictureBox2, image_PB2);
            clear_image(pictureBox3, image_PB3);
            clear_image(pictureBox4, image_PB4);
            clear_image(pictureBox_BUF1, image_buffers[0]);
            clear_image(pictureBox_BUF2, image_buffers[1]);
            clear_image(pictureBox_BUF3, image_buffers[2]);
        }

        private void ustawienie_wyszukiwania_koloru_czerwonego()
        {
            // paint
            /*
            UpDownBL.Value = 33;
            UpDownBH.Value = 52;
            UpDownGL.Value = 27;
            UpDownGH.Value = 41;
            UpDownRL.Value = 200;
            UpDownRH.Value = 255;
            */
            // zdjecia
            UpDownBL.Value = 30;
            UpDownBH.Value = 210;

            UpDownGL.Value = 50;
            UpDownGH.Value = 80;

            UpDownRL.Value = 120;
            UpDownRH.Value = 175;
        }

        private void ustawienie_wyszukiwania_koloru_zielonego()
        {
            // paint
            /*
            UpDownBL.Value = 50;
            UpDownBH.Value = 100;
            UpDownGL.Value = 150;
            UpDownGH.Value = 200;
            UpDownRL.Value = 10;
            UpDownRH.Value = 60;
            */
            // zdjecia
            UpDownBL.Value = 30;
            UpDownBH.Value = 70;

            UpDownGL.Value = 40;
            UpDownGH.Value = 125;

            UpDownRL.Value = 30;
            UpDownRH.Value = 75;
        }

        private void ustawienie_wyszukiwania_koloru_zoltego()
        {
            // paint
            /*
            UpDownBL.Value = 0;
            UpDownBH.Value = 25;
            UpDownGL.Value = 215;
            UpDownGH.Value = 255;
            UpDownRL.Value = 230;
            UpDownRH.Value = 255;
            */
            // zdjecia
            UpDownBL.Value = 40;
            UpDownBH.Value = 80;

            UpDownGL.Value = 150;
            UpDownGH.Value = 220;

            UpDownRL.Value = 140;
            UpDownRH.Value = 250;
        }

        private void wyznaczenie_sygnatury(Point m_srodek)
        {
            //Rysuje sygnature w miejscu klikniecia
            // MouseEventArgs me = e as MouseEventArgs;
            tabela_promieni = sygnatura_radialna(m_srodek);
            namaluj_dane_z_tabeli(tabela_promieni, null, new MCvScalar(255, 0, 0), TrybRysowania.TYLKO_DANE);
            // private void namaluj_dane_z_tabeli(double[] dane, double[] krzywa, MCvScalar kolor, TrybRysowania tryb)
            usrednianie_wykresu();
        }
        private int kat_poczatkowy;

        private double[] sygnatura_radialna(Point start)
        {
            MCvScalar kolor_promienia = new MCvScalar();
            double[,] katy_kolejnych_promieni = new double[liczba_promieni, 2];
            double[] promienie = new double[liczba_promieni];
            double krok_katowy, aktualny_kat;

            generuj_losowy_kolor(ref kolor_promienia);
            aktualny_kat = kat_poczatkowy * (Math.PI / 180);
            /*
            if (radioButton_Draw_clockwise.Checked)
                krok_katowy = (2 * Math.PI / liczba_promieni);
            else
                krok_katowy = -(2 * Math.PI / liczba_promieni);
            */
            // chose from above
            krok_katowy = (2 * Math.PI / liczba_promieni);

            for (int i = 0; i < liczba_promieni; i++)
            {
                katy_kolejnych_promieni[i, 0] = Math.Cos(aktualny_kat);
                katy_kolejnych_promieni[i, 1] = Math.Sin(aktualny_kat);
                aktualny_kat += krok_katowy;
            }

            // image_PB2.SetZero(); // czysci?
            image_PB3.SetZero();
            // byte[,,] temp1 = image_PB1.Data; // z PB1 bierze i na PB2 rysuje tymi promieniamu?
            byte[,,] temp1 = image_PB2.Data;

            int zakres = (int)Math.Sqrt(Math.Pow(desired_image_size.Width, 2) + Math.Pow(desired_image_size.Height, 2));
            for (int p = 0; p < liczba_promieni; p++)
            {
                for (int d = 0; d < zakres; d++)
                {
                    Point cp = new Point();
                    int dx, dy;
                    dx = (int)(d * katy_kolejnych_promieni[p, 0]);
                    dy = (int)(d * katy_kolejnych_promieni[p, 1]);
                    if (Math.Abs(dx) < zakres && Math.Abs(dy) < zakres)
                    {
                        cp.X = start.X + dx;
                        cp.Y = start.Y + dy;
                        if (temp1[cp.Y, cp.X, 0] == 0x00)
                        {
                            // CvInvoke.Line(image_PB2, start, cp, kolor_promienia, 1);
                            CvInvoke.Line(image_PB3, start, cp, kolor_promienia, 1);
                            promienie[p] = Math.Sqrt(Math.Pow(dx, 2) + Math.Pow(dy, 2));
                            break;
                        }
                    }
                }
            }

            // pictureBox2.Image = image_PB2.Bitmap;
            pictureBox3.Image = image_PB3.Bitmap;

            return promienie;
        }
        private Random rnd = new Random();
        private void generuj_losowy_kolor(ref MCvScalar kolor)
        {
            kolor.V0 = rnd.Next(0, 255);
            kolor.V1 = rnd.Next(0, 255);
            kolor.V2 = rnd.Next(0, 255);
        }
    }
}


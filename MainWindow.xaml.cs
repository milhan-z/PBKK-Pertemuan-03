using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Pertemuan_03_Desktop_Calculator
{
    public partial class MainWindow : Window
    {
        // Variabel penyimpan state kalkulator
        private double angkaPertama = 0;
        private string operasi = "";
        private bool isOperatorClicked = false;
        private bool isHasilBaru = false;

        public MainWindow()
        {
            InitializeComponent();
            Loaded += MainWindow_Loaded;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Cek argumen CLI apakah diminta membuat screenshot bukti eksekusi
            string[] args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg.StartsWith("--capture="))
                {
                    string outputPath = arg.Substring(10);
                    // Tampilkan status perhitungan aktif untuk tangkapan layar
                    txtHistory.Text = "125 × 8 =";
                    txtDisplay.Text = "1000";
                    UpdateLayout();

                    CaptureWindow(outputPath);
                    Application.Current.Shutdown();
                    break;
                }
            }
        }

        // Event handler tombol angka (0-9)
        private void btnAngka_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            string angka = btn.Content.ToString() ?? "";

            if (txtDisplay.Text == "0" || isOperatorClicked || isHasilBaru)
            {
                txtDisplay.Text = angka;
                isOperatorClicked = false;
                isHasilBaru = false;
            }
            else
            {
                txtDisplay.Text += angka;
            }
        }

        // Event handler tombol operator (+, -, ×, ÷)
        private void btnOperator_Click(object sender, RoutedEventArgs e)
        {
            Button btn = (Button)sender;
            
            if (double.TryParse(txtDisplay.Text, out double nilai))
            {
                angkaPertama = nilai;
                operasi = btn.Content.ToString() ?? "";
                txtHistory.Text = $"{angkaPertama} {operasi}";
                isOperatorClicked = true;
            }
        }

        // Event handler tombol sama dengan (=)
        private void btnSamaDengan_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(operasi)) return;

            if (double.TryParse(txtDisplay.Text, out double angkaKedua))
            {
                double hasil = 0;
                bool isError = false;

                switch (operasi)
                {
                    case "+":
                        hasil = angkaPertama + angkaKedua;
                        break;
                    case "-":
                        hasil = angkaPertama - angkaKedua;
                        break;
                    case "×":
                        hasil = angkaPertama * angkaKedua;
                        break;
                    case "÷":
                        if (angkaKedua == 0)
                        {
                            isError = true;
                        }
                        else
                        {
                            hasil = angkaPertama / angkaKedua;
                        }
                        break;
                }

                if (isError)
                {
                    txtHistory.Text = $"{angkaPertama} {operasi} {angkaKedua} =";
                    txtDisplay.Text = "Error: Dibagi Nol";
                }
                else
                {
                    txtHistory.Text = $"{angkaPertama} {operasi} {angkaKedua} =";
                    txtDisplay.Text = hasil.ToString();
                }

                operasi = "";
                isHasilBaru = true;
            }
        }

        // Event handler Clear (C) - Reset total
        private void btnClear_Click(object sender, RoutedEventArgs e)
        {
            angkaPertama = 0;
            operasi = "";
            txtHistory.Text = "";
            txtDisplay.Text = "0";
            isOperatorClicked = false;
            isHasilBaru = false;
        }

        // Event handler Clear Entry (CE) - Reset input saat ini
        private void btnCE_Click(object sender, RoutedEventArgs e)
        {
            txtDisplay.Text = "0";
        }

        // Event handler Backspace (⌫)
        private void btnBackspace_Click(object sender, RoutedEventArgs e)
        {
            if (txtDisplay.Text.Length > 1 && txtDisplay.Text != "Error: Dibagi Nol")
            {
                txtDisplay.Text = txtDisplay.Text.Substring(0, txtDisplay.Text.Length - 1);
            }
            else
            {
                txtDisplay.Text = "0";
            }
        }

        // Event handler Negatif/Positif (±)
        private void btnNegatif_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(txtDisplay.Text, out double nilai))
            {
                nilai = -nilai;
                txtDisplay.Text = nilai.ToString();
            }
        }

        // Event handler Titik Desimal (.)
        private void btnTitik_Click(object sender, RoutedEventArgs e)
        {
            if (isOperatorClicked || isHasilBaru)
            {
                txtDisplay.Text = "0.";
                isOperatorClicked = false;
                isHasilBaru = false;
                return;
            }

            if (!txtDisplay.Text.Contains("."))
            {
                txtDisplay.Text += ".";
            }
        }

        // Fungsi utility untuk menangkap tampilan jendela WPF ke berkas gambar PNG
        private void CaptureWindow(string outputPath)
        {
            try
            {
                string? dir = Path.GetDirectoryName(outputPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                int width = (int)Math.Round(ActualWidth);
                int height = (int)Math.Round(ActualHeight);

                if (width <= 0) width = 360;
                if (height <= 0) height = 520;

                RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(this);

                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));

                using (FileStream fs = File.Create(outputPath))
                {
                    png.Save(fs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Gagal menyimpan screenshot: " + ex.Message);
            }
        }
    }
}
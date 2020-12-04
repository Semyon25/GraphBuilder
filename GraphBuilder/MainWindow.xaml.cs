using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO.Ports;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace GraphBuilder
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;

            timer.Tick += new EventHandler(timerTick);
            timer.Interval = new TimeSpan(0, 0, 2);
        }

        private void timerTick(object sender, EventArgs e)
        {
            timer.Stop();
            PlotGraph();
        }

        System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

        SerialPort port = new SerialPort();
        List<double> values = new List<double>();
        private double maxValue;

        public int PortName { get; set; } = 2;

        public string Command { get; set; } = "start_ADC_100";
        public double MaxValue { get => maxValue; set { maxValue = value; OnPropertyChanged(); } }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (PortName <= 0) return;

                port.PortName = "COM" + PortName.ToString();
                port.BaudRate = 9600;
                port.DataBits = 8;
                port.Parity = Parity.None;
                port.StopBits = StopBits.One;
                port.ReadTimeout = 1000;
                port.WriteTimeout = 1000;
                port.Open();
                port.DataReceived += Port_DataReceived;
                //MessageBox.Show("Порт открыт");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Port_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            Thread.Sleep(1000);
            string s = port.ReadExisting();
            var strs = s.Split(new char[] { '\r' });
            foreach (string str in strs)
            {
                if (str.Contains("start"))
                {
                    values.Clear();
                }
                else if (str.Contains("stop"))
                {
                    timer.Start();
                }
                else
                {
                    double x = 0.0;
                    if (double.TryParse(str, out x))
                    {
                        values.Add(x);
                    }
                }
            }
        }

        private void PlotGraph()
        {
            const int or = 10;
            const int ot = 10;
            const int width = 650;
            const int heigth = 400;

            int stepX = width / values.Count;
            double stepY = (double)(heigth / values.Max());

            for (int i = 0; i < canvas.Children.Count; i++)
            {
                if (canvas.Children[i] is Ellipse)
                {
                    canvas.Children.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < values.Count; i++)
            {
                CreatePoint(or + i * stepX, ot + heigth - values[i] * stepY);
            }
            MaxValue = values.Max();
        }

        private void CreatePoint(int x, double y)
        {
            Ellipse ellipse = new Ellipse();
            ellipse.Fill = Brushes.Black;
            ellipse.Width = ellipse.Height = 6;
            ellipse.Margin = new Thickness(x, y, 0, 0);
            canvas.Children.Add(ellipse);
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                port.WriteLine(Command);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged([CallerMemberName]string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}

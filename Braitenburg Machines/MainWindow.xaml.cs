using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
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
using System.Timers;
using System.Windows.Threading;
using System.IO;

namespace Braitenburg_Machines
{
    public class Robot
    {
        //-----------------------
        // Data Members
        //-----------------------
        public Point position, ICC; // (x, y) of Robot center, (x,y) of Instantaneous Center of Curvature
        double Theta, Rho;      // Pose Angle relative to x axis, Rate of Rotation relative to axle axis
        double Vr, Vl, R;       // Velocities for Left and Right wheels, dist from midpoint on axle to ICC
        double[,] KMatrix;

        //-----------------------
        // Sprite Variables
        //-----------------------
        // Sensor Positions on sprite, relative to top left as 0,0
        private Tuple<int, int> S1Pos = Tuple.Create<int, int>(1, 1);
        private Tuple<int, int> S2Pos = Tuple.Create<int, int>(1, 7);
        const double Len = 7;   // Dist Between wheels (determined by sprite)

        //-----------------------
        // Constructors 
        //-----------------------
        public Robot()
        {
            KMatrix = new double[,]
            {
                {0.6, 0.4},
                {0.4, 0.6},
            };
            position.X = 0.0;
            position.Y = 0.0;
            ICC.X = 0.0;
            ICC.Y = 0.0;
            Theta = (Math.PI / 2); // Starting pose is sensors facing up. (aka 90 degrees)
        }
        public Robot(double Xpos, double Ypos, double Angle, double[,] matrix = null)
        {
            if (!ReferenceEquals(null, matrix))
                KMatrix = matrix;
            else {
                Console.WriteLine("Null Matrix in Constructor!");
                KMatrix = new double[,]
                {
                    {0.6, 0.4},
                    {0.4, 0.6},
                };
            }
            position.X = Xpos;
            position.Y = Ypos;
            ICC.X = 0.0;
            ICC.Y = 0.0;
            Theta = Angle * (Math.PI / 180); // Starting pose is sensors facing up. (aka 90 degrees)
        }

        //-----------------------
        // Main Functions
        //-----------------------

        public double[] FillerSensorRead()
        {
            // Should Poll Positions of Sensors for light values.
            // Currently Filler
            double[] darray = { 1.0, 1.0 };
            return darray;
        }

        // Takes in new Sensor Data, then Calculates the change in x, y, and Theta
        public Tuple<double, double, double> Step()
        {
            double[] S = FillerSensorRead();
            double s1 = S[0], s2 = S[1];
            Vl = KMatrix[0, 0] * s1 + KMatrix[0, 1] * s2;
            Vr = KMatrix[1, 0] * s1 + KMatrix[1, 1] * s2;
            R = (Len / 2) * ((Vl + Vr) / (Vr - Vl));
            Rho = (Vr - Vl) / Len;
            ICC.X = position.X - R * Math.Sin(Theta);
            ICC.Y = position.Y - R * Math.Cos(Theta);

            double xPrime = Math.Cos(Rho) * (position.X - ICC.X) + -Math.Sin(Rho) * (position.Y - ICC.Y);
            double yPrime = Math.Sin(Rho) * (position.X - ICC.X) + Math.Cos(Rho) * (position.Y - ICC.Y);
            double ThetaPrime = Theta + Rho;
            double DX = position.X - xPrime;
            double DY = position.Y - yPrime;
            return new Tuple<double, double, double>(DX, DY, Rho);
        }

        //-----------------------
        // Member Updating 
        //-----------------------
        public void setKMatrix(double[,] matrix)
        {
            KMatrix = matrix;
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer; //timer to handle updating the GUI
        private bool running = false;
        private uint numRobots = 1;
        private ArrayList<>

        public MainWindow()
        {
            InitializeComponent();
            TransformGroup tg = new TransformGroup();
            RotateTransform rt = new RotateTransform(90);
            tg.Children.Add(rt);
            TranslateTransform tt = new TranslateTransform();
            tt.X = 0;
            tt.Y = 0;
            tg.Children.Add(tt);
            RobotSprite.RenderTransformOrigin = new Point(0.5, 0.5);
            RobotSprite.RenderTransform = tg;

            //create and start the timer to handle the GUI updates
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(500);
            timer.Tick += OnTimedEvent;
            timer.Start();
        }

        void OnTimedEvent(object sender, EventArgs e)
        {
            Console.WriteLine("Hello World!");
            if (running)
            {
                RobotSprite.RenderTransformOrigin = new Point(0.5, 0.5);
                TransformGroup tg = RobotSprite.RenderTransform as TransformGroup;
                RotateTransform rt = tg.Children[0] as RotateTransform;
                rt.Angle += 30;
            }
        }

        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            running = true;
            Console.WriteLine("Checkbox Checked!");
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            running = false;
        }

        private void LayoutRoot_MouseDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}

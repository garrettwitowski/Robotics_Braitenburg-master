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
        private Point position, ICC; // (x, y) of Robot center, (x,y) of Instantaneous Center of Curvature
        private double Theta, Omega;      // Pose Angle relative to x axis, Rate of Rotation relative to axle axis
        private double Vr, Vl, R;       // Velocities for Left and Right wheels, dist from midpoint on axle to ICC
        private double[,] KMatrix;
        private double s1Intensity, s2Intensity; //intensity values for each sensor

        //-----------------------
        // Sprite Variables
        //-----------------------
        // Sensor Positions on sprite, relative to top left as 0,0
        //******* MIGHT WANT TO CHANGE THESE TO JUST BE POINTS, TUPLE SHOULDN'T REALLY BE USED HERE BUT IF IT DOESN'T CAUSE PROBLEMS NO NEED TO CHANGE
        private Point s1Pos = new Point(1, 1);//Tuple.Create<int, int>(1, 1);
        private Point s2Pos = new Point(7, 1);//Tuple.Create<int, int>(1, 7);
        private const double Len = 7;   // Dist Between wheels (determined by sprite)
        private int canvasIndex; //Index of this robot's sprite on the canvas

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
            canvasIndex = -1;
        }
        public Robot(double Xpos, double Ypos, double Angle, double[,] matrix = null)
        {
            if (!ReferenceEquals(null, matrix))
            {
                KMatrix = new double[2, 2];
                //this is not good style but trying to get this done, should use a given function to make a deep copy...
                KMatrix[0, 0] = matrix[0, 0];
                KMatrix[0, 1] = matrix[0, 1];
                KMatrix[1, 0] = matrix[1, 0];
                KMatrix[1, 1] = matrix[1, 1];
            }
            else {
                Console.WriteLine("Null Matrix in Constructor!");
                KMatrix = new double[,]
                {
                    {1.2, 0.8},
                    {0.8, 1.2},
                };
            }
            position.X = Xpos;
            position.Y = Ypos;
            ICC.X = 0.0;
            ICC.Y = 0.0;
            Theta = Angle * (Math.PI / 180); // Starting pose is sensors facing up. (i.e., 90 degrees)
            canvasIndex = -1;
        }

        //-----------------------
        // Main Functions
        //-----------------------

        public double[] FillerSensorRead()
        {
            // Should Poll Positions of Sensors for light values.
            // Currently Filler
            double[] darray = { s1Intensity, s2Intensity};
            return darray;
        }

        // Takes in new Sensor Data, then Calculates the change in x, y, and Theta
        public Tuple<double, double, double> CalculateStep()
        {
            double[] S = FillerSensorRead();
            double s1 = S[0], s2 = S[1];
            Vl = KMatrix[0, 0] * s1 + KMatrix[0, 1] * s2;
            Vr = KMatrix[1, 0] * s1 + KMatrix[1, 1] * s2;
            ICC.X = 0.0;
            ICC.Y = 0.0;
            // Turning
            if (Vr != Vl)
            {
                R = (Len / 2) * ((Vl + Vr) / (Vr - Vl));
                Omega = ((Vr - Vl) / Len);
                ICC.X = position.X - R * Math.Sin(Theta);
                ICC.Y = position.Y - R * Math.Cos(Theta);

                double xPrime = Math.Cos(Omega) * (position.X - ICC.X) + -Math.Sin(Omega) * (position.Y - ICC.Y) + ICC.X;
                double yPrime = Math.Sin(Omega) * (position.X - ICC.X) + Math.Cos(Omega) * (position.Y - ICC.Y) + ICC.Y;
                double ThetaPrime = Theta + Omega; // Note that we dont actually do anything with this?
                Console.WriteLine("x': {0} \t y': {1}", xPrime, yPrime);
                double DX = xPrime - position.X;
                double DY = yPrime - position.Y;
                return new Tuple<double, double, double>(DX, DY, Omega);
            }
            // Forward Motion
            else
            {
                Omega = 0;
                double xPrime = position.X + Vl * Math.Cos(Theta);
                double yPrime = position.Y + Vl * Math.Sin(Theta);
                Console.WriteLine("Sx': {0} \t Sy': {1}", xPrime, yPrime);
                double DX = xPrime - position.X;
                double DY = yPrime - position.Y;
                return new Tuple<double, double, double>(DX, DY, Omega);
            }
        }

        public void PerformStep(Tuple<double, double, double>  deltas)
        {
            position.X += deltas.Item1;
            position.Y += deltas.Item2;
            Theta = (Theta + deltas.Item3) % (2 * Math.PI);
        }

        //-----------------------
        // Member Updating 
        //-----------------------
        public void setKMatrix(double[,] matrix)
        {
            matrix.CopyTo(KMatrix, 0);
        }

        public override string ToString()
        {
            var deltas = CalculateStep();
            return "{" + position.X + ", " + position.Y + ", " + Theta + "} + {" + deltas.Item1 + ", " + deltas.Item2 + ", " + deltas.Item3 + "}";
        }

        public int CanvasIndex
        {
            get
            {
                return canvasIndex;
            }
            set
            {
                canvasIndex = value;
            }
        }

        public Point Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }

        public Point S1Pos
        {
            get
            {
                return s1Pos;
            }
        }

        public Point S2Pos
        {
            get
            {
                return s2Pos;
            }
        }

        public double S1Intensity
        {
            get
            {
                return s1Intensity;
            }

            set
            {
                s1Intensity = value;
            }
        }

        public double S2Intensity
        {
            get
            {
                return s2Intensity;
            }

            set
            {
                s2Intensity = value;
            }
        }
    }


    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private DispatcherTimer timer; //timer to handle updating the GUI
        private bool running = false;
        // Robots
        private uint numRobots = 0;
        private List<Robot> robots;
        //private List<Image> sprites;
        private string filename = "../../Resources/robot_data.txt";
        private const uint SPRITE_WIDTH = 9;
        private const uint SPRITE_HEIGHT = 9;
        // Lights
        private bool LightSwitch = false;
        BitmapImage LightIMG = new BitmapImage(new Uri("../../Resources/LightSource.bmp", UriKind.Relative));
        BitmapImage robotIMG = new BitmapImage(new Uri("../../Resources/RobotSprite.bmp", UriKind.Relative));
        //List<Image> LightList;
        List<Point> LightLocs; // The Positions of the lights
        private int LightCt = 0;
        private const int MAX_NUM_LIGHTS = 300;
        private const int MAX_LIGHT_INTENSITY = 100;
        private Point P1Abs;
        private Point P2Abs;

        public MainWindow()
        {
            InitializeComponent();
            robots = new List<Robot>();
            //LightList = new List<Image>(MAX_NUM_LIGHTS);
            LightLocs = new List<Point>(MAX_NUM_LIGHTS);
            //sprites = new List<Image>();

            /*Render the first robot*/
            TransformGroup tg = new TransformGroup();
            RotateTransform rt = new RotateTransform(0);
            tg.Children.Add(rt);
            //TranslateTransform tt = new TranslateTransform();
            //tt.X = 0;
            //tt.Y = 0;
            //tg.Children.Add(tt);
            RobotSprite.RenderTransformOrigin = new Point(0.5, 0.5);
            RobotSprite.RenderTransform = tg; //also might have a problem here...
            //let's find out what happens by actually running our program... probably just overthinking this...

            /*Open the robot_data file and parse it*/
            try
            {   // Open the text file using a stream reader.
                StreamReader myIFS = new StreamReader(filename.ToString());
                string line;
                uint robotCount = 0;
                if((line = myIFS.ReadLine()) != null)
                    numRobots = Convert.ToUInt32(line);
                else
                {
                    Console.WriteLine("Error: Invalid file format!");
                    return;
                }

                while((line = myIFS.ReadLine()) != null)
                {
                    // Read the stream to a string, and write the string to the console
                    Console.WriteLine(line);
                    robotCount++;
                    char[] delims = { ' ' };
                    var splitLine = line.Split(delims);
                    double[,] kMatrix = new double[2, 2];
                    double xPos, yPos, theta;
                    tg = new TransformGroup();
                    rt = new RotateTransform(0);
                    //tt = new TranslateTransform();
                    //tt.X = 0;
                    //tt.Y = 0;

                    if (splitLine.Length >= 3)
                    {
                        xPos = Convert.ToDouble(splitLine[0]);
                        yPos = Convert.ToDouble(splitLine[1]);
                        theta = Convert.ToDouble(splitLine[2]);
                        rt.Angle = theta;
                        tg.Children.Add(rt);
                        //tg.Children.Add(tt);

                        Image img = new Image();
                        img.Source = robotIMG;
                        img.Width = SPRITE_WIDTH;
                        img.Height = SPRITE_HEIGHT;
                        img.Name = "RobotSprite" + robotCount;
                        Canvas.SetTop(img, yPos - (img.Height + 1) / 2);
                        Canvas.SetLeft(img, xPos - (img.Width + 1) / 2);
                        img.RenderTransformOrigin = new Point(0.5, 0.5);
                        img.RenderTransform = tg;
                        //sprites.Add(img); //probably don't need this, just need to put them on the canvas...
                        LayoutRoot.Children.Add(img); //this might be a problem because img gets GC'ed at the end of this section
                        //might instead need to add a new Image to the canvas and update its properties... we shall see...
                        int spriteLoc = LayoutRoot.Children.Count - 1;

                        if (splitLine.Length >= 7)
                        {
                            Console.WriteLine("kMatrix provided");
                            kMatrix[0, 0] = Convert.ToDouble(splitLine[3]);
                            kMatrix[0, 1] = Convert.ToDouble(splitLine[4]);
                            kMatrix[1, 0] = Convert.ToDouble(splitLine[5]);
                            kMatrix[1, 1] = Convert.ToDouble(splitLine[6]);
                            robots.Add(new Robot(xPos, yPos, 90 - theta, kMatrix));
                            robots[robots.Count - 1].CanvasIndex = spriteLoc; //track which robot sprite this robot corresponds to
                        }
                        else
                        {
                            Console.WriteLine("No kMatrix provided");
                            robots.Add(new Robot(xPos, yPos, 90 - theta));
                            robots[robots.Count - 1].CanvasIndex = spriteLoc; //track which robot sprite this robot corresponds to
                        }
                    }
                    else Console.WriteLine("Line number {0} has an invalid format!", robotCount + 1);
                    
                }

                if (robotCount != numRobots)
                {
                    Console.WriteLine("Error: Robot Count does not match number of robots specified!");
                    return;
                }
                myIFS.Close(); //Close the file!
            }
            catch (Exception e)
            {
                Console.WriteLine("The file could not be read:");
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            
            //create and start the timer to handle the GUI updates
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(10);
            timer.Tick += OnTimedEvent;
            timer.Start();
        }

        private void OnTimedEvent(object sender, EventArgs e)
        {
            bool printState = false;
            //Console.WriteLine("Hello World!");
            if (running)
            {
                RobotSprite.RenderTransformOrigin = new Point(0.5, 0.5);
                TransformGroup tg = RobotSprite.RenderTransform as TransformGroup;
                RotateTransform rt = tg.Children[0] as RotateTransform;
                TranslateTransform tt;
                rt.Angle += 3;
                for(int i = 0; i < robots.Count; i++)
                {
                    Robot robot = robots[i]; //this might not be well defined... not sure...
                    int idx = robot.CanvasIndex;
                    var sprite = LayoutRoot.Children[idx];
                    sprite.RenderTransformOrigin = new Point(0.5, 0.5);
                    //compute the absolute coordinates of each sensor on the canvas
                    UIElement container = VisualTreeHelper.GetParent(sprite) as UIElement;
                    Point s1Absolute = sprite.TranslatePoint(new Point(-3, 3), container);
                    Point s2Absolute = sprite.TranslatePoint(new Point(3, 3), container);
                    //Console.WriteLine("s1Abs: {0} \ts2Abs: {1}", s1Absolute, s2Absolute);
                    //now compute the intensity of the light perceived by each sensor
                    double s1Int = intensityAt(s1Absolute);
                    double s2Int = intensityAt(s2Absolute);
                    //next update the intensity values for each sensor
                    robot.S1Intensity = s1Int;
                    robot.S2Intensity = s2Int;
                    //then calculate the deltas for the step
                    var deltas = robot.CalculateStep();
                    //finally, update the pose of both the robot and its sprite
                    tg = sprite.RenderTransform as TransformGroup;
                    rt = tg.Children[0] as RotateTransform;
                    //tt = tg.Children[1] as TranslateTransform;
                    Point ttt = new Point(0,0);
                    rt.Angle += deltas.Item3;
                    ttt.X = 0;
                    ttt.X += deltas.Item1;
                    //Console.WriteLine("X");
                    //Console.WriteLine(tt.X);
                    if ((robot.Position.X + ttt.X) > 842)
                    {
                        Console.WriteLine("-X");
                        Console.WriteLine("Pos: {0}", robot.Position.X);
                        Console.WriteLine("DX: {0}", ttt.X);
                        Console.WriteLine("Sum: {0}", robot.Position.X + ttt.X);
                        ttt.X -= 842;
                        Console.WriteLine("NewSum: {0}", robot.Position.X + ttt.X);
                        printState = true;
                    }
                    else if ((robot.Position.X + ttt.X) < 0)
                    {
                        Console.WriteLine("+X");
                        Console.WriteLine("Pos: {0}", robot.Position.X);
                        Console.WriteLine("DX: {0}", ttt.X);
                        Console.WriteLine("Sum: {0}", robot.Position.X + ttt.X);
                        ttt.X += 842;
                        Console.WriteLine("NewSum: {0}", robot.Position.X + ttt.X);
                        printState = true;
                    }
                    ttt.Y = 0;
                    ttt.Y += deltas.Item2;
                    //Console.WriteLine("Y");
                    //Console.WriteLine(tt.Y);
                    // Currently doesnt stay within our range. Also note that for some reason, it treats its starting position as 0,0
                    if ((robot.Position.Y + ttt.Y) > 600)
                    {
                        Console.WriteLine("+Y");
                        Console.WriteLine("Pos: {0}", robot.Position.Y);
                        Console.WriteLine("DY: {0}", ttt.Y);
                        Console.WriteLine("Sum: {0}", robot.Position.Y + ttt.Y);
                        ttt.Y -= 600;
                        Console.WriteLine("NewSum: {0}", robot.Position.Y + ttt.Y);
                        printState = true;
                    }
                    else if ((robot.Position.Y + ttt.Y) < 0)
                    {
                        Console.WriteLine("-Y");
                        Console.WriteLine("Pos: {0}", robot.Position.Y);
                        Console.WriteLine("DY: {0}", ttt.Y);
                        Console.WriteLine("Sum: {0}", robot.Position.Y + ttt.Y);
                        ttt.Y += 600;
                        Console.WriteLine("NewSum: {0}", robot.Position.Y + ttt.Y);
                        printState = true;
                    }
                    Tuple<double, double, double> output = Tuple.Create<double, double, double>(ttt.X, ttt.Y, deltas.Item3);
                    robot.PerformStep(output);
                    sprite.RenderTransform = tg;
                    Canvas.SetLeft(sprite, robot.Position.X);
                    Canvas.SetTop(sprite, robot.Position.Y);
                    if (printState)
                    {
                        Console.WriteLine("X: {0}", robot.Position.X);
                        Console.WriteLine("Y: {0}", robot.Position.Y);
                        printState = false;
                    }
                }
            }
        }

        private double distance(Point p1, Point p2)
        {
            double dxSquared = Math.Pow((p1.X - p2.X), 2);
            double dySquared = Math.Pow((p1.Y - p2.Y), 2);
            return Math.Sqrt(dxSquared + dySquared);
        }

        private double intensityAt(Point sensorLoc)
        {
            double max_intensity = 0;
            double dist;
            double intensity;
            for(int i = 0; i < LightLocs.Count; i++)
            {
                dist = distance(sensorLoc, LightLocs[i]);
                if(dist < 1)
                    dist = 1;
                intensity = 100 / dist;
                if (intensity > max_intensity)
                    max_intensity = intensity;
            }
            return max_intensity;
        }
        
        private void checkBox_Checked(object sender, RoutedEventArgs e)
        {
            running = true;
            Console.WriteLine("Checkbox Checked!");
            if (LightSwitch == true)
            {
                lights.IsChecked = false;
                Lights_Unchecked(sender, e);
            }
        }

        private void checkBox_Unchecked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Checkbox Unchecked.");
            running = false;
        }

        private void Lights_Checked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Lights on.");
            LightSwitch = true;
            if (running == true)
            {
                checkBox.IsChecked = false;
                checkBox_Unchecked(sender, e);
            }
        }

        private void Lights_Unchecked(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Lights off.");
            LightSwitch = false;
        }

        /************ FFV: Add a reset button ***********/

        private void LayoutRoot_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (LightSwitch && LightLocs.Count < MAX_NUM_LIGHTS)
            {
                //Console.WriteLine("mouseLeft is clicked");
                Point p = e.MouseDevice.GetPosition(this);
                //Console.WriteLine(x.X);
                //Console.WriteLine(x.Y);
                // Add Image for Light Source

                Image img = new Image();
                img.Source = LightIMG;
                img.Width = LightIMG.Width;
                img.Height = LightIMG.Height;
                Canvas.SetLeft(img, p.X);
                Canvas.SetTop(img, p.Y - 50);
                //LightList.Add(img);
                // Add to Canvas
                LayoutRoot.Children.Add(img);
                // Update Light Locs
                LightLocs.Add(new Point(p.X, p.Y));
                //LightCt++;
                Console.WriteLine("Light Placed.");
            }
            else Console.WriteLine("Unable to place more lights.");
        }
    } //end of class MainWindow
}

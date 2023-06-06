using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AimLabsFromWish
{
    public partial class MainWindow : Window
    {
        private readonly List<Terč> terče = new List<Terč>();
        private readonly DispatcherTimer timer = new DispatcherTimer();
        private readonly Random random = new Random();
        private int score = 0;

        public MainWindow()
        {
            InitializeComponent();

            timer.Tick += Timer_Tick;
            timer.Interval = TimeSpan.FromSeconds(2);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            timer.Start();

            // Přidáme červenou tečku do všech existujících terčů na canvasu
            foreach (var terč in terče)
            {
                double dotLeft = terč.Left + (terč.Diameter - terč.Dot.Width) / 2;
                double dotTop = terč.Top + (terč.Diameter - terč.Dot.Height) / 2;
                Canvas.SetLeft(terč.Dot, dotLeft);
                Canvas.SetTop(terč.Dot, dotTop);
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Generovat nový terč
            var terč = new Terč(canvas);
            terč.Zasažen += Terč_Zasažen;
            terč.Minut += Terč_Minut;
            terče.Add(terč);
        }

        private void Terč_Zasažen(object sender, EventArgs e)
        {
            // Terč byl zasažen, zvýšit skóre
            score++;
            UpdateScore();
            // Odebrat terč z kolekce a z canvasu
            var terč = (Terč)sender;
            terče.Remove(terč);
            canvas.Children.Remove(terč.OuterCircle);
            canvas.Children.Remove(terč.Dot);
            foreach (var innerCircle in terč.InnerCircles)
            {
                canvas.Children.Remove(innerCircle);
            }
        }

        private void Terč_Minut(object sender, EventArgs e)
        {
            // Terč byl "minut", snížit skóre
            if (score > 0)
            {
                score--;
                UpdateScore();
            }
            // Odebrat terč z kolekce a z canvasu
            var terč = (Terč)sender;
            terče.Remove(terč);
            canvas.Children.Remove(terč.OuterCircle);
            canvas.Children.Remove(terč.Dot);
            foreach (var innerCircle in terč.InnerCircles)
            {
                canvas.Children.Remove(innerCircle);
            }
        }

        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(canvas);
            bool terčZasažen = false;

            // Projít všechny terče a zkontrolovat, jestli byl zasažen
            foreach (var terč in terče)
            {
                if (terč.ContainsPoint(mousePosition))
                {
                    terč.ZasaženTerč();
                    terčZasažen = true;
                    break;
                }
            }

            if (!terčZasažen)
            {
                OdečtiSkóre();
            }
        }

        private void OdečtiSkóre()
        {
            // Odečíst skóre pouze pokud je větší než nula
            if (score > 0)
            {
                score--;
                UpdateScore();
            }
        }

        private void UpdateScore()
        {
            scoreTextBlock.Text = $"Score: {score}";
        }
    }

    public class Terč
    {
        private const int ZásahováZónaPočetKruhů = 3;
        private const double ZásahováZónaKoeficient = 0.8;

        private readonly Random random = new Random();

        public double Left { get; private set; }
        public double Top { get; private set; }
        public double Diameter { get; private set; }
        public double HitZoneDiameter { get; private set; }
        public List<Ellipse> InnerCircles { get; private set; }

        public event EventHandler Zasažen;
        public event EventHandler Minut;

        public Ellipse OuterCircle { get; private set; }
        public Ellipse Dot { get; private set; }

        public Terč(Canvas canvas)
        {
            Diameter = random.Next(50, 150);
            HitZoneDiameter = Diameter * ZásahováZónaKoeficient;

            Left = random.NextDouble() * (canvas.ActualWidth - Diameter);
            Top = random.NextDouble() * (canvas.ActualHeight - Diameter);

            OuterCircle = new Ellipse
            {
                Width = Diameter,
                Height = Diameter,
                Fill = Brushes.White,
                Stroke = Brushes.Black,
                StrokeThickness = 2
            };

            InnerCircles = new List<Ellipse>();

            // Vytvořit soustředné kruhy terče
            for (int i = 0; i < ZásahováZónaPočetKruhů; i++)
            {
                double innerCircleDiameter = Diameter - (i + 1) * (Diameter - HitZoneDiameter) / ZásahováZónaPočetKruhů;

                var innerCircle = new Ellipse
                {
                    Width = innerCircleDiameter,
                    Height = innerCircleDiameter,
                    Fill = Brushes.Transparent,
                    Stroke = Brushes.Black,
                    StrokeThickness = 2
                };

                double innerCircleLeft = Left + (Diameter - innerCircleDiameter) / 2;
                double innerCircleTop = Top + (Diameter - innerCircleDiameter) / 2;

                Canvas.SetLeft(innerCircle, innerCircleLeft);
                Canvas.SetTop(innerCircle, innerCircleTop);

                InnerCircles.Add(innerCircle);
                canvas.Children.Add(innerCircle);
            }

            Dot = new Ellipse
            {
                Width = Diameter * 0.1,
                Height = Diameter * 0.1,
                Fill = Brushes.Red
            };

            double dotLeft = Left + (Diameter - Dot.Width) / 2;
            double dotTop = Top + (Diameter - Dot.Height) / 2;
            Canvas.SetLeft(Dot, dotLeft);
            Canvas.SetTop(Dot, dotTop);

            Canvas.SetLeft(OuterCircle, Left);
            Canvas.SetTop(OuterCircle, Top);

            canvas.Children.Add(OuterCircle);
            canvas.Children.Add(Dot);
        }

        public bool ContainsPoint(Point point)
        {
            double centerX = Left + Diameter / 2;
            double centerY = Top + Diameter / 2;

            double distance = Math.Sqrt(Math.Pow(centerX - point.X, 2) + Math.Pow(centerY - point.Y, 2));

            return distance <= Diameter / 2;
        }

        public void ZasaženTerč()
        {
            Zasažen?.Invoke(this, EventArgs.Empty);
        }

        public void MinutTerč()
        {
            Minut?.Invoke(this, EventArgs.Empty);
        }
    }
}

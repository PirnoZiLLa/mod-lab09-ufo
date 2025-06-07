using ScottPlot;
using ScottPlot.Plottables;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace MotionSim
{
    public partial class SimulationForm : Form
    {
        private enum MoveResult { Completed, OutOfBounds }

        private float _dotSize = 15f;
        private float _trackWidth = 3f;
        private float _traceSize = 5f;
        private float _stepSize = 1.2f;
        private bool _isRunning = false;
        private PointF _startPos = new PointF(120, 120);
        private PointF _endPos = new PointF(620, 1820);
        private NumericUpDown _termsInput;
        private NumericUpDown _zoneSizeInput;
        private System.Windows.Forms.Label _termsLabel;
        private System.Windows.Forms.Label _zoneLabel;
        private Button _launchButton;
        private Button _analyzeButton;

        public SimulationForm()
        {
            SetupInterface();
        }

        private void OnCanvasPaint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.ScaleTransform(0.5f, 0.5f);

            RenderDot(g, Brushes.DarkOliveGreen, _startPos, _dotSize);
            RenderDot(g, Brushes.Turquoise, _endPos, _dotSize);
            RenderZone(g, Pens.DarkOrchid, _endPos, (float)_zoneSizeInput.Value);
            DrawTrack(g);

            if (!_isRunning)
            {
                SimulateMotion(g, (int)_termsInput.Value);
            }
            else
            {
                int terms = 1;
                MoveResult result;
                do
                {
                    _termsInput.Value = terms;
                    result = SimulateMotion(g, terms);
                    terms++;
                } while (result != MoveResult.Completed);
                _isRunning = false;
            }
        }

        private void RenderDot(Graphics g, Brush brush, PointF point, float size)
        {
            g.FillEllipse(brush, point.X - size, point.Y - size, 2 * size, 2 * size);
        }

        private void RenderDot(Graphics g, Brush brush, float x, float y, float size)
        {
            g.FillEllipse(brush, x - size, y - size, 2 * size, 2 * size);
        }

        private void RenderZone(Graphics g, Pen pen, PointF point, float radius)
        {
            g.DrawEllipse(pen, point.X - radius, point.Y - radius, 2 * radius, 2 * radius);
        }

        private void DrawTrack(Graphics g)
        {
            double m = (_endPos.Y - _startPos.Y) / (_endPos.X - _startPos.X);
            double c = _startPos.Y - m * _startPos.X;
            for (float x = _startPos.X; x <= _endPos.X; x += 1)
            {
                RenderDot(g, Brushes.Ivory, x, (float)(m * x + c), _trackWidth);
            }
        }

        private MoveResult SimulateMotion(Graphics g, int terms)
        {
            double theta = ComputeArctan(
                Math.Abs(_endPos.Y - _startPos.Y) / Math.Abs(_endPos.X - _startPos.X), terms);

            float x = _startPos.X;
            float y = _startPos.Y;
            double dist = CalculateDistance(x, y);
            float zone = (float)_zoneSizeInput.Value;

            while (dist > zone)
            {
                x += (float)(ComputeCos(theta, terms) * _stepSize);
                y += (float)(ComputeSin(theta, terms) * _stepSize);
                RenderDot(g, Brushes.DimGray, x, y, _traceSize);
                dist = CalculateDistance(x, y);

                if (x > _endPos.X + 2 * zone)
                {
                    return MoveResult.OutOfBounds;
                }
            }
            return MoveResult.Completed;
        }

        private double ComputeArctan(double val, int terms)
        {
            if (Math.Abs(val) <= 1)
            {
                double sum = 0;
                for (int k = 0; k < terms; k++)
                {
                    sum += Math.Pow(-1, k) * Math.Pow(val, 2 * k + 1) / (2 * k + 1);
                }
                return sum;
            }
            return Math.Sign(val) * Math.PI / 2 - ComputeArctan(1 / val, terms);
        }

        private double CalculateDistance(float x, float y)
        {
            float dx = _endPos.X - x;
            float dy = _endPos.Y - y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        private double ComputeSin(double angle, int terms)
        {
            double sum = 0;
            for (int k = 0; k < terms; k++)
            {
                int power = 2 * k + 1;
                sum += Math.Pow(-1, k) * Math.Pow(angle, power) / Factorial(power);
            }
            return sum;
        }

        private double ComputeCos(double angle, int terms)
        {
            double sum = 0;
            for (int k = 0; k < terms; k++)
            {
                int power = 2 * k;
                sum += Math.Pow(-1, k) * Math.Pow(angle, power) / Factorial(power);
            }
            return sum;
        }

        private double Factorial(int n)
        {
            return n == 0 ? 1 : n * Factorial(n - 1);
        }

        private void LaunchSimulation(object sender, EventArgs e)
        {
            Invalidate();
        }

        private void PerformAnalysis(object sender, EventArgs e)
        {
            string baseDir = AppContext.BaseDirectory;
            string projDir = Path.GetFullPath(Path.Combine(baseDir, "../../.."));
            string plotPath = Path.Combine(projDir, "output/plot.png");
            string dataPath = Path.Combine(projDir, "output/data.txt");

            int[] zoneSizes = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 25, 30, 35, 40 };

            File.WriteAllText(dataPath, "Радиус зоны\tЧлены ряда\n");
            foreach (int size in zoneSizes)
            {
                _zoneSizeInput.Value = size;
                int terms = 1;
                MoveResult result;
                do
                {
                    _termsInput.Value = terms;
                    _isRunning = true;
                    Invalidate();
                    Update();
                    result = SimulateMotion(CreateGraphics(), terms);
                    terms++;
                } while (result != MoveResult.Completed);
                File.AppendAllText(dataPath, $"{size}\t{terms - 1}\n");
            }

            var data = File.ReadAllLines(dataPath).Skip(1)
                .Select(line => int.Parse(line.Split('\t')[1])).ToArray();

            var plt = new Plot();
            plt.Title("Зависимость числа членов ряда от радиуса зоны", size: 16);
            plt.XLabel("Радиус зоны");
            plt.YLabel("Число членов ряда");
            var scatter = plt.Add.Scatter(zoneSizes, data);
            scatter.LineWidth = 3;
            scatter.MarkerSize = 8;
            scatter.Color = ScottPlot.Colors.Navy;
            plt.SavePng(plotPath, 1200, 800);
        }
    }
}
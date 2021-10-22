using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace polygon_intersection
{

    /// <summary>
    /// https://ru.stackoverflow.com/questions/672647/Как-найти-угол-между-векторами
    /// </summary>
    struct Vector
    {
        public double X { get; }
        public double Y { get; }

        public Vector(double x, double y)
        {
            X = x; Y = y;
        }

        public static readonly Vector Reference = new Vector(1, 0);

        public static double AngleOfReference(Vector v)
            => NormalizeAngle(Math.Atan2(v.Y, v.X) / Math.PI * 180);

        public static double AngleOfVectors(Vector first, Vector second)
            => NormalizeAngle(AngleOfReference(first) - AngleOfReference(second));

        private static double NormalizeAngle(double angle)
        {
            bool CheckBottom(double a) => a >= 0;
            bool CheckTop(double a) => a < 360;

            double turn = CheckBottom(angle) ? -360 : 360;
            while (!(CheckBottom(angle) && CheckTop(angle))) angle += turn;
            return angle;
        }
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            g = canvas.CreateGraphics();
        }

        bool isDrawingMode = false;
        bool isClearingMode = false;
        bool isFirstDrawn = false;
        List<Point> polygon1Points = new List<Point>();
        List<Point> polygon2Points = new List<Point>();
        Pen blackPen = new Pen(Color.Black, 3);
        SolidBrush blackBrush = new SolidBrush(Color.Black);
        Graphics g;

        private void btnDraw_Click(object sender, EventArgs e)
        {
            if (isClearingMode)
            {
                isDrawingMode = false;
                isClearingMode = false;
                canvas.Image = new Bitmap(1300, 900);
                polygon1Points.Clear();
                polygon2Points.Clear();
                btnDraw.Text = "Нарисовать полигоны";
                textView.Visible = false;
            }
            else
            {
                isDrawingMode = true;
                isClearingMode = true;
                btnDraw.Text = "Очистить";
                textView.Text = "Нарисуйте 1 полигон";
                textView.Visible = true;
            }
        }
        
        private void canvas_MouseClick(object sender, MouseEventArgs e)
        {
            if (isDrawingMode)
            {
                if (isFirstDrawn)
                {
                    drawPolygon(e, ref polygon2Points);
                }
                else
                {
                    drawPolygon(e, ref polygon1Points);
                }
            }
        }

        void drawPolygon(MouseEventArgs e, ref List<Point> polygonPoints)
        {
            if (polygonPoints.Count > 0)
            {
                if (polygonPoints.Count == 1)
                {
                    g.Clear(Color.White);
                    if (isFirstDrawn)
                    {
                        for(int i = 0; i < polygon1Points.Count; i++)
                        {
                            g.DrawLine(blackPen, polygon1Points[i], polygon1Points[(i+1) % polygon1Points.Count]);
                        }
                    }
                }
                if (Math.Abs(polygonPoints[0].X - e.X) * Math.Abs(polygonPoints[0].Y - e.Y) < 25)
                {
                    g.DrawLine(blackPen, polygonPoints.Last(), polygonPoints[0]);
                    if (isFirstDrawn)
                    {
                        isFirstDrawn = false;
                        textView.Text = "Нарисуйте 1 полигон";
                        textView.Visible = false;
                        isDrawingMode = false;
                        intersectPolygons();
                    }
                    else
                    {
                        isFirstDrawn = true;
                        textView.Text = "Нарисуйте 2 полигон";
                    }
                    return;
                }
                g.DrawLine(blackPen, polygonPoints.Last(), e.Location);
                polygonPoints.Add(e.Location);
            }
            else
            {
                polygonPoints.Add(e.Location);
                g.FillEllipse(blackBrush, e.X - 2, e.Y - 2, 5, 5);
            }
        }

        bool isPointInPolygon(Point p, List<Point> polygonPoints)
        {
            // определение принадлежности точки многоугольнику
            bool inPolygon = false;
            int counter = 0;
            double xinters;
            Point p1, p2;
            int pointCount = polygonPoints.Count;
            p1 = polygonPoints[0];
            for (int i = 1; i <= pointCount; i++)
            {
                p2 = polygonPoints[i % pointCount];
                if (p.Y > Math.Min(p1.Y, p2.Y)// Y контрольной точки больше минимума Y конца отрезка
                && p.Y <= Math.Max(p1.Y, p2.Y))// Y контрольной точки меньше максимального Y конца отрезка
                {
                    if (p.X <= Math.Max(p1.X, p2.X))// X контрольной точки меньше максимального X конечной точки сегмента изолинии (для оценки используйте левый луч контрольной точки).
                    {
                        if (p1.Y != p2.Y)// Отрезок не параллелен оси X
                        {
                            xinters = (p.Y - p1.Y) * (p2.X - p1.X) / (p2.Y - p1.Y) + p1.X;
                            if (p1.X == p2.X || p.X <= xinters)
                            {
                                counter++;
                            }
                        }
                    }

                }
                p1 = p2;
            }

            if (counter % 2 == 0)

                inPolygon = false;
            else
                inPolygon = true;
            return inPolygon;
        }

        Tuple<int, int, int> GetFunc(Point p1, Point p2)//чтобы получить коэффициенты уравнения, через которое проходит прямая
        {
            int coef1 = p1.Y - p2.Y;
            int coef2 = p2.X - p1.X;
            int coef3 = p1.X * p2.Y - p2.X * p1.Y;
            return Tuple.Create(coef1, coef2, coef3);
        }

        /// <summary>
        /// The following code is stolen from https://www.geeksforgeeks.org/check-if-two-given-line-segments-intersect/
        /// </summary>
        /// <param name="p"></param>
        /// <param name="q"></param>
        /// <param name="r"></param>
        /// <returns></returns>

        // Given three collinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        static Boolean onSegment(Point p, Point q, Point r)
        {
            return q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y);
        }

        // To find orientation of ordered triplet (p, q, r).
        // The function returns following values
        // 0 --> p, q and r are collinear
        // 1 --> Clockwise
        // 2 --> Counterclockwise
        static int orientation(Point p, Point q, Point r)
        {
            // See https://www.geeksforgeeks.org/orientation-3-ordered-points/
            // for details of below formula.
            int val = (q.Y - p.Y) * (r.X - q.X) -
                    (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0; // collinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }

        // The main function that returns true if line segment 'p1q1'
        // and 'p2q2' intersect.
        static Boolean doIntersect(Point p1, Point q1, Point p2, Point q2)
        {
            // Find the four orientations needed for general and
            // special cases
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
                return true;

            // Special Cases
            // p1, q1 and p2 are collinear and p2 lies on segment p1q1
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and q2 are collinear and q2 lies on segment p1q1
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are collinear and p1 lies on segment p2q2
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are collinear and q1 lies on segment p2q2
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases
        }

        Point CrossLines(Point p1, Point p2, Point p3, Point p4)
        {
            (int a1, int b1, int c1) = GetFunc(p1, p2);
            (int a2, int b2, int c2) = GetFunc(p3, p4);
            int divisor = a1 * b2 - a2 * b1;
            if (divisor == 0)
                return new Point(int.MaxValue, int.MaxValue);
            int x = (b1 * c2 - b2 * c1) / divisor;
            int y = (c1 * a2 - c2 * a1) / divisor;
            return new Point(x, y);
        }

        void intersectPolygons()
        {
            List<Point> innerPolygon = new List<Point>();
            innerPolygon.AddRange(polygon1Points.Where(p => isPointInPolygon(p,polygon2Points)));
            innerPolygon.AddRange(polygon2Points.Where(p => isPointInPolygon(p, polygon1Points)));

            for(int i =0; i < polygon1Points.Count; i++)
            {
                for(int j = 0; j < polygon2Points.Count; j++)
                {
                    var p1 = polygon1Points[i];
                    var q1 = polygon1Points[(i + 1) % polygon1Points.Count];
                    var p2 = polygon2Points[j];
                    var q2 = polygon2Points[(j + 1) % polygon2Points.Count];
                    if (doIntersect(p1,q1,p2,q2))
                    {
                        innerPolygon.Add(CrossLines(p1, q1, p2, q2));
                    }
                }
            }
            if(innerPolygon.Count == 0)
            {
                return;
            }
            int x = 0, y = 0;
            foreach(var p in innerPolygon)
            {
                x += p.X;
                y += p.Y;
            }
            var center = new Point(x/innerPolygon.Count, y/innerPolygon.Count);
            innerPolygon = innerPolygon.OrderBy(p => Vector.AngleOfVectors(new Vector(1, 0), new Vector(p.X - center.X, p.Y - center.Y))).ToList();

            foreach(var p in innerPolygon)
            {
                g.FillEllipse(new SolidBrush(Color.Red), p.X - 4, p.Y - 4, 9, 9);
            }
            for(int i = 0; i < innerPolygon.Count; i++)
            {
                g.DrawLine(new Pen(Color.Red, 5), innerPolygon[i], innerPolygon[(i + 1) % innerPolygon.Count]);
            }
        }
    }
}

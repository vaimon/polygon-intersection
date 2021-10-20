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

        void intersectPolygons()
        {

        }
    }
}

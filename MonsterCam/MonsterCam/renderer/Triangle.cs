using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonsterCam.renderer
{
    public class Triangle  
    {
       
        public class Point 
        {
            internal System.Windows.Point p = new System.Windows.Point();
            public double U { get; set; }
            public double V { get; set; }

            public double X { get { return p.X; } set { p.X = value; } }
            public double Y { get { return p.Y; } set { p.Y = value; } }

            public Point(double x = 0.0, double y = 0.0, double u = 0.0, double v = 0.0)
            {
                this.X = x;
                this.Y = y;
                this.U = u;
                this.V = v;
            }


        }

        public Point P1 { get; private set; }
        public Point P2 { get; private set; }
        public Point P3 { get; private set; }

       public Polygon Polygon{get;private set;}
       ImageBrush brush = new ImageBrush();
       TransformGroup trans = new TransformGroup();

       public Triangle(Point p1 = null, Point p2 = null, Point p3 = null)
        {
            Polygon = new Polygon();
            P1 = p1 != null ? p1 : new Point();
            P2 = p2 != null ? p2 : new Point();
            P3 = p3 != null ? p3 : new Point();

            Polygon.Points.Add(P1.p);
            Polygon.Points.Add(P2.p);
            Polygon.Points.Add(P3.p);
            Polygon.Fill = brush;
            Polygon.Stroke = brush;
            brush.RelativeTransform = trans;
        }

        
        public  ImageSource ImageSource
        {
            get { return brush.ImageSource; }
            set { brush.ImageSource = value; }
        }
        public Transform textureTransform
        {
            get;
            set;
        }

        
        public void updatePolygon()
        {
            try
            {
                Polygon.Points.Clear();
                Polygon.Points.Add(P1.p);
                Polygon.Points.Add(P2.p);
                Polygon.Points.Add(P3.p);

                var invertMat = new MatrixTransform();
                invertMat.Matrix = new Matrix(
                   P2.U - P1.U,
                    P2.V - P1.V,
                     P3.U - P1.U,
                    P3.V - P1.V,
                    P1.U,
                    P1.V
                   );

                trans.Children.Clear();
                if (textureTransform != null)
                    trans.Children.Add(textureTransform);

                double xmin = P1.X;
                double xmax = P1.X;
                double ymin = P1.Y;
                double ymax = P1.Y;

                if (P2.X < xmin) xmin = P2.X;
                if (P3.X < xmin) xmin = P3.X;
                if (P2.X > xmax) xmax = P2.X;
                if (P3.X > xmax) xmax = P3.X;

                if (P2.Y < ymin) ymin = P2.Y;
                if (P3.Y < ymin) ymin = P3.Y;
                if (P2.Y > ymax) ymax = P2.Y;
                if (P3.Y > ymax) ymax = P3.Y;

                double w = xmax - xmin;
                double h = ymax - ymin;
                var Mat = new MatrixTransform();
                Mat.Matrix = new Matrix(
                    (P2.X - P1.X) / w,
                    (P2.Y - P1.Y) / h,
                    (P3.X - P1.X) / w,
                    (P3.Y - P1.Y) / h,
                    (P1.X - xmin) / w,
                    (P1.Y - ymin) / h
                );
                trans.Children.Add(invertMat.Inverse as Transform);
                trans.Children.Add(Mat);
            }
            catch (Exception)
            {
                trans.Children.Clear();
                if (textureTransform != null)
                    trans.Children.Add(textureTransform);
            }

          
        }


    }
}

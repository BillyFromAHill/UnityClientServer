using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Shared
{
    [Serializable]
    public struct Point
    {
        public Point(float x, float y)
        {
            X = x;
            Y = y;
        }

        public float X { get; set; }

        public float Y { get; set; }

        public static Point operator-(Point point)
        {
            return new Point(-point.X, -point.Y);
        }

        public static Point operator *(Point point, float coef)
        {
            return new Point(point.X * coef, point.Y * coef);
        }

        public static Point operator +(Point point1, Point point2)
        {
            return new Point(point1.X + point2.X, point1.Y + point2.Y);
        }

        public static Point operator -(Point point1, Point point2)
        {
            return point1 + (-point2);
        }

        public float Length
        {
            get
            {
                return (float)Math.Sqrt(X * X + Y * Y);
            }
        }

        public Point GetNormalized()
        {
            float length = Length;

            return new Point(X / Length, Y / Length);
        }

        public float GetDistanceTo(Point point)
        {
            return (this - point).Length;
        }

        public override bool Equals(object obj)
        {
            return obj is Point && Equals((Point)obj);
        }

        public bool Equals(Point point)
        {
            return Math.Abs(this.X - point.X) < float.Epsilon &&
                   Math.Abs(this.Y - point.Y) < float.Epsilon;
        }
    }
}

﻿using System.Diagnostics;

namespace Research.GraphBasedShapePrior.Util
{
    public struct Circle
    {
        public Vector Center { get; set; }

        public double Radius { get; set; }

        public Circle(Vector center, double radius)
            : this()
        {
            Debug.Assert(radius >= 0);

            this.Center = center;
            this.Radius = radius;
        }

        public Circle(double x, double y, double radius)
            : this(new Vector(x, y), radius)
        {
        }

        public bool Contains(Circle circle)
        {
            return circle.Radius <= this.Radius &&
                   (circle.Center - this.Center).LengthSquared <= MathHelper.Sqr(this.Radius - circle.Radius);
        }

        public bool Contains(Vector point)
        {
            return point.DistanceToPointSquared(this.Center) <= this.Radius * this.Radius;
        }

        public override string ToString()
        {
            return string.Format("C={0} R={1}", this.Center, this.Radius);
        }
    }
}

﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MicrosoftResearch.Infer.Maths;
using Research.GraphBasedShapePrior;
using Vector = Research.GraphBasedShapePrior.Vector;

namespace TestArea
{
    class Program
    {
        static ShapeModel CreateSimpleShapeModel1()
        {
            List<ShapeEdge> edges = new List<ShapeEdge>();
            edges.Add(new ShapeEdge(0, 1));

            List<ShapeVertexParams> vertexParams = new List<ShapeVertexParams>();
            vertexParams.Add(new ShapeVertexParams(0.3, 0.1));
            vertexParams.Add(new ShapeVertexParams(0.3, 0.1));

            Dictionary<Tuple<int, int>, ShapeEdgePairParams> edgePairParams =
                new Dictionary<Tuple<int, int>, ShapeEdgePairParams>();
            
            return ShapeModel.Create(edges, vertexParams, edgePairParams);
        }

        static ShapeModel CreateGiraffeShapeModel()
        {
            List<ShapeEdge> edges = new List<ShapeEdge>();
            edges.Add(new ShapeEdge(0, 1)); // Body
            edges.Add(new ShapeEdge(0, 2)); // Neck
            edges.Add(new ShapeEdge(2, 3)); // Head
            edges.Add(new ShapeEdge(0, 4)); // Front leg (top)
            edges.Add(new ShapeEdge(4, 6)); // Front Leg (bottom)
            edges.Add(new ShapeEdge(1, 5)); // Back leg (top)
            edges.Add(new ShapeEdge(5, 7)); // Back leg (bottom)

            List<ShapeVertexParams> vertexParams = new List<ShapeVertexParams>();
            vertexParams.Add(new ShapeVertexParams(0.6, 0.05));
            vertexParams.Add(new ShapeVertexParams(0.5, 0.05));
            vertexParams.Add(new ShapeVertexParams(0.15, 0.02));
            vertexParams.Add(new ShapeVertexParams(0.1, 0.01));
            vertexParams.Add(new ShapeVertexParams(0.1, 0.01));
            vertexParams.Add(new ShapeVertexParams(0.1, 0.01));
            vertexParams.Add(new ShapeVertexParams(0.1, 0.01));
            vertexParams.Add(new ShapeVertexParams(0.1, 0.01));

            Dictionary<Tuple<int, int>, ShapeEdgePairParams> edgePairParams =
                new Dictionary<Tuple<int, int>, ShapeEdgePairParams>();
            edgePairParams.Add(new Tuple<int, int>(0, 1), new ShapeEdgePairParams(-Math.PI * 0.666, 2, 0.1, 0.1));
            edgePairParams.Add(new Tuple<int, int>(1, 2), new ShapeEdgePairParams(-Math.PI * 0.5, 0.1, 0.1, 0.1));
            edgePairParams.Add(new Tuple<int, int>(0, 3), new ShapeEdgePairParams(Math.PI * 0.5, 0.5, 0.1, 0.1));
            edgePairParams.Add(new Tuple<int, int>(3, 4), new ShapeEdgePairParams(0, 1, 0.1, 0.1));
            edgePairParams.Add(new Tuple<int, int>(0, 5), new ShapeEdgePairParams(Math.PI * 0.5, 0.5, 0.1, 0.1));
            edgePairParams.Add(new Tuple<int, int>(5, 6), new ShapeEdgePairParams(0, 1, 0.1, 0.1));

            return ShapeModel.Create(edges, vertexParams, edgePairParams);
        }
        
        static void MainForUnaryPotentialsCheck()
        {
            ShapeModel model = CreateSimpleShapeModel1();
            
            Image2D<Color> result = new Image2D<Color>(320, 240);
            Circle edgeStart = new Circle(new Vector(100, 50), 70);
            Circle edgeEnd = new Circle(new Vector(150, 100), 20);

            VertexConstraints constraint1 = new VertexConstraints(new Point(80, 100), new Point(130, 140), 40, 41);
            VertexConstraints constraint2 = new VertexConstraints(new Point(260, 180), new Point(300, 210), 20, 21);
            ShapeConstraintsSet constraintsSet = ShapeConstraintsSet.Create(CreateSimpleShapeModel1(), new []{constraint1, constraint2});

            for (int x = 0; x < result.Width; ++x)
                for (int y = 0; y < result.Height; ++y)
                {
                    Tuple<double, double> potentials = BranchAndBoundSegmentator.CalculateShapeTerm(
                        constraintsSet, new Point(x, y));
                    double diff = potentials.Item1 - potentials.Item2;
                    int redColor = diff < 0 ? 0 : (int) Math.Min(diff * 200, 255);
                    int blueColor = diff > 0 ? 0 : (int) Math.Min(-diff * 200, 255);
                    result[x, y] = Color.FromArgb(redColor, 0, blueColor);
                }
            Image2D.SaveToFile(result, "../../potentials.png");
        }

        static void MainForDistanceTransform()
        {
            Image2D<Color> mask = Image2D.LoadFromFile("../../mask.png");
            Func<Point, double> penaltyFunc = p => mask[p.X, p.Y].ToArgb() == Color.Black.ToArgb() ? 0 : 1e+10;
            GeneralizedDistanceTransform2D transform = new GeneralizedDistanceTransform2D(
                Point.Empty, new Point(mask.Width, mask.Height), 1, 1, penaltyFunc);

            Image2D<Color> result = new Image2D<Color>(mask.Width, mask.Height);
            for (int i = 0; i < mask.Width; ++i)
                for (int j = 0; j < mask.Height; ++j)
                {
                    double distance = Math.Sqrt(transform[i, j]);
                    int color = Math.Min((int) Math.Round(distance), 255);
                    result[i, j] = Color.FromArgb(color, color, color);
                }
            Image2D.SaveToFile(result, "../../result.png");
        }

        static void MainForSegmentation()
        {
            BranchAndBoundSegmentator segmentator = new BranchAndBoundSegmentator();
            segmentator.ShapeModel = CreateSimpleShapeModel1();

            DebugConfiguration.VerbosityLevel = VerbosityLevel.Everything;

            const double scale = 0.15;
            Image2D<Color> image = Image2D.LoadFromFile("../../simple_1.png", scale);
            Rectangle bigLocation = new Rectangle(153, 124, 796, 480);
            Rectangle location = new Rectangle(
                (int)(bigLocation.X * scale),
                (int)(bigLocation.Y * scale),
                (int)(bigLocation.Width * scale),
                (int)(bigLocation.Height * scale));

            Image2D<bool> mask = segmentator.SegmentImage(image, location);
            Image2D.SaveToFile(mask, "../../result.png");
        }

        static void MainForConvexHull()
        {
            List<Vector> points = new List<Vector>();
            points.Add(new Vector(0, 0));
            points.Add(new Vector(1, 2));
            points.Add(new Vector(2, 1));
            points.Add(new Vector(3, 2));
            points.Add(new Vector(4, 2));
            points.Add(new Vector(3, -1));

            Polygon p = Polygon.ConvexHull(points);
            Console.WriteLine(p.IsPointInside(new Vector(2, 1)));
            Console.WriteLine(p.IsPointInside(new Vector(2, -1)));
            Console.WriteLine(p.IsPointInside(new Vector(0, -1)));
            Console.WriteLine(p.IsPointInside(new Vector(-1, 0)));
            Console.WriteLine(p.IsPointInside(new Vector(2, 0)));
            Console.WriteLine(p.IsPointInside(new Vector(3, 1)));
        }

        static void Main()
        {
            Rand.Restart(666);
            
            //MainForUnaryPotentialsCheck();
            MainForSegmentation();
            //MainForConvexHull();
        }
    }
}
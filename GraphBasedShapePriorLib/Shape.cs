﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;

namespace Research.GraphBasedShapePrior
{
    public class Shape
    {
        private readonly List<Circle> vertices;

        public Shape(ShapeModel model)
        {
            Debug.Assert(model != null);

            this.vertices = new List<Circle>();
            this.Model = model;
        }

        public Shape(ShapeModel model, IEnumerable<Circle> vertices)
            : this(model)
        {
            int index = 0;
            foreach (Circle shapeVertex in vertices)
            {
                if (index >= this.vertices.Count)
                    throw new ArgumentException("Too many vertices for given model", "vertices");
                this.vertices[index++] = shapeVertex;
            }

            if (index != this.vertices.Count)
                throw new ArgumentException("Too few vertices for given model", "vertices");
        }

        public ShapeModel Model { get; private set; }

        public ReadOnlyCollection<Circle> Vertices
        {
            get { return this.vertices.AsReadOnly(); }
        }

        public ReadOnlyCollection<ShapeEdge> Edges
        {
            get { return this.Model.Edges; }
        }

        public double GetObjectPotential(Point point)
        {
            return GetObjectPotential(new Vector(point.X, point.Y));
        }

        public double GetObjectPotential(Vector point)
        {
            double potential = 0;
            foreach (ShapeEdge edge in this.Edges)
            {
                double edgePotential = this.Model.CalculateObjectPotentialForEdge(
                    point,
                    this.vertices[edge.Index1],
                    this.vertices[edge.Index2]);

                potential = Math.Max(potential, edgePotential);
            }

            return potential;
        }

        public double CalculateEnergy(double bodyLength)
        {
            double result = 0;

            for (int i = 0; i < this.Model.VertexCount; ++i)
                result += this.Model.CalculateVertexEnergyTerm(i, bodyLength, this.vertices[i].Radius);

            foreach (Tuple<int, int> edgePair in this.Model.ConstrainedEdgePairs)
            {
                result += this.Model.CalculateEdgePairEnergyTerm(
                    edgePair.Item1,
                    edgePair.Item2,
                    this.vertices[this.Model.Edges[edgePair.Item1].Index2].Center - this.vertices[this.Model.Edges[edgePair.Item1].Index1].Center,
                    this.vertices[this.Model.Edges[edgePair.Item2].Index2].Center - this.vertices[this.Model.Edges[edgePair.Item2].Index1].Center);
            }

            return result;
        }
    }
}

using System;
using System.Drawing;
using AlgeoSharp;
using AlgeoSharp.Visualization;

namespace Example1
{
	class MainClass
	{
		public static void Main(string[] args)
		{
			var win = new AlgeoWindow();

			// Create some points
			MultiVector p1 = IPNS.CreatePoint(2, 3, 0);
			MultiVector p2 = IPNS.CreatePoint(-3, -4, 3);
			MultiVector p3 = IPNS.CreatePoint(1, -5, 0);
			MultiVector p4 = IPNS.CreatePoint(-1, -2, -3);

			// Calculate sphere with four points
			MultiVector s = (p1 ^ p2 ^ p3 ^ p4).Dual;

			// Calculate plane with three points
			MultiVector p = (p1 ^ p2 ^ p3 ^ Basis.E8).Dual;

			// Add points
			win.Visualizer.Add(p1, Color.Yellow);
			win.Visualizer.Add(p2, Color.Yellow);
			win.Visualizer.Add(p3, Color.Yellow);
			win.Visualizer.Add(p4, Color.Orange);

			// Add sphere and plane
			win.Visualizer.Add(s, Color.Gray);
			win.Visualizer.Add(p, Color.Violet);

			// Calculate circle defined by intersection of the sphere and the plane and add it
			win.Visualizer.Add(s ^ p, Color.Yellow);

			// Calculate line through two points and add it
			win.Visualizer.Add((p1 ^ p2 ^ Basis.E8).Dual, Color.White);

			// Add some vectors to visualize the coordinate system
			win.Visualizer.Add(MultiVector.Vector(5, 0, 0), Color.Red);
			win.Visualizer.Add(MultiVector.Vector(0, 5, 0), Color.Green);
			win.Visualizer.Add(MultiVector.Vector(0, 0, 5), Color.Blue);

			// Run
			win.Run(25);
		}
	}
}

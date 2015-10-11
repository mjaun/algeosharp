using System;
using System.Collections.Generic;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using AlgeoSharp;

namespace AlgeoSharp.Visualization
{
	public class AlgeoVisualizer
	{
		struct Vbo
		{
			public int VboId;
			public int EboId;
			public int NumElements;
			public PrimitiveType Mode;
		}

		Vbo vectorLineVbo;
		Vbo vectorArrowVbo;
		Vbo pointVbo;
		Vbo sphereVbo;
		Vbo planeVbo;
		Vbo lineVbo;
		Vbo circleVbo;

		public AlgeoVisualizer()
		{
			AlgeoObjects = new List<AlgeoObject>();

			Eye = MultiVector.Vector(10, 10, 10);
			Target = MultiVector.Vector(0, 0, 0);
			Up = MultiVector.Vector(0, 1, 0);

			VectorArrowLength = 0.5f;
			VectorArrowRadius = 0.15f;
			VectorArrowQuality = 10;

			PointRadius = 0.15f;
			PointQuality = 10;

			SphereSegments = 20;
			SphereRings = 20;

			PlaneInfinity = 10.0f;
			PlaneLineDensity = 1.0f;

			LineInfinity = 10.0f;

			CircleLines = 40;
		}


		public List<AlgeoObject> AlgeoObjects { get; private set; }


		public MultiVector Eye { get; set; }

		public MultiVector Target { get; set; }

		public MultiVector Up { get; set; }


		public float VectorArrowLength { get; set; }
		public float VectorArrowRadius { get; set; }
		public int VectorArrowQuality { get; set; }

		public float PointRadius { get; set; }
		public int PointQuality { get; set; }

		public int SphereSegments { get; set; }
		public int SphereRings { get; set; }

		public float PlaneInfinity { get; set; }
		public float PlaneLineDensity { get; set; }

		public float LineInfinity { get; set; }

		public int CircleLines { get; set; }


		public void Load()
		{
			GL.ClearColor(Color.Black);
			GL.Enable(EnableCap.DepthTest);

			initModels();
		}

		public void Unload()
		{
			destroyModels();
		}

		public void Resize(int width, int height)
		{
			GL.Viewport(0, 0, width, height);

			float aspect = (float)width / (float)height;
			Matrix4 perpective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver4, aspect, 1, 64);
			GL.MatrixMode(MatrixMode.Projection);
			GL.LoadMatrix(ref perpective);
		}

		public void Render()
		{
			Matrix4 lookat = Matrix4.LookAt((float)Eye.E1, (float)Eye.E2, (float)Eye.E3,
				(float)Target.E1, (float)Target.E2, (float)Target.E3, (float)Up.E1, (float)Up.E2, (float)Up.E3);
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadMatrix(ref lookat);

			GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			GL.LineWidth(1);

			foreach (AlgeoObject obj in AlgeoObjects)
			{
				switch (IPNS.GetGeometricEntity(obj.Value))
				{
				case GeometricEntity.Vector:
					drawVector(obj.Value, obj.Color);
					break;

				case GeometricEntity.Point:
					drawPoint(obj.Value, obj.Color);
					break;

				case GeometricEntity.Sphere:
					drawSphere(obj.Value, obj.Color);
					break;

				case GeometricEntity.Plane:
					drawPlane(obj.Value, obj.Color);
					break;

				case GeometricEntity.Line:
					drawLine(obj.Value, obj.Color);
					break;

				case GeometricEntity.Circle:
					drawCircle(obj.Value, obj.Color);
					break;

				case GeometricEntity.PointPair:
					drawPointPair(obj.Value, obj.Color);
					break;

				default:
					break;
				}
			}
		}


		public void Add(AlgeoObject item)
		{
			AlgeoObjects.Add(item);
		}

		public AlgeoObject Add(MultiVector value, Color color)
		{
			AlgeoObject ret = new AlgeoObject(value, color);
			Add(ret);
			return ret;
		}

		public void Remove(AlgeoObject item)
		{
			AlgeoObjects.Remove(item);
		}


		private void initModels()
		{
			vectorLineVbo = createVectorLineVbo();
			vectorArrowVbo = createVectorArrowVbo(VectorArrowQuality, VectorArrowRadius, VectorArrowLength);

			pointVbo = createSphereVbo(PointQuality, PointQuality, false);
			sphereVbo = createSphereVbo(SphereSegments, SphereRings, true);
			planeVbo = createPlaneVbo(PlaneInfinity, PlaneLineDensity);
			lineVbo = createLineVbo(LineInfinity);
			circleVbo = createCircleVbo(CircleLines);
		}

		private void destroyModels()
		{
			deleteVbo(vectorLineVbo);
			deleteVbo(vectorArrowVbo);

			deleteVbo(pointVbo);
			deleteVbo(sphereVbo);
			deleteVbo(planeVbo);
			deleteVbo(lineVbo);
			deleteVbo(circleVbo);
		}

		private Vbo createVbo(PrimitiveType mode, Vector3[] vertices, ushort[] elements = null)
		{
			Vbo result = new Vbo();
			result.Mode = mode;

			result.VboId = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ArrayBuffer, result.VboId);

			GL.BufferData(BufferTarget.ArrayBuffer,
				(IntPtr)(vertices.Length * BlittableValueType.StrideOf(vertices)),
				vertices,
				BufferUsageHint.StaticDraw);

			if (elements == null)
			{
				elements = new ushort[vertices.Length];

				for (ushort i = 0; i < vertices.Length; i++)
				{
					elements[i] = i;
				}
			}

			result.NumElements = elements.Length;

			result.EboId = GL.GenBuffer();
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, result.EboId);

			GL.BufferData(BufferTarget.ElementArrayBuffer,
				(IntPtr)(elements.Length * sizeof(short)),
				elements,
				BufferUsageHint.StaticDraw);

			return result;
		}

		private void deleteVbo(Vbo vbo)
		{
			GL.DeleteBuffer(vbo.VboId);
			GL.DeleteBuffer(vbo.EboId);
		}


		private Vbo createSphereVbo(int segments, int rings, bool wires)
		{
			Vector3[] vertices = new Vector3[segments * rings];

			int i = 0;

			for (int y = 0; y < rings; y++)
			{
				double phi = ((double)y / (rings - 1)) * MathHelper.Pi;

				for (int x = 0; x < segments; x++)
				{
					double theta = ((double)x / (segments - 1)) * MathHelper.TwoPi;

					vertices[i] = new Vector3()
					{
						X = (float)(Math.Sin(phi) * Math.Sin(theta)),
						Y = (float)(Math.Sin(phi) * Math.Cos(theta)),
						Z = (float)(Math.Cos(phi)),
					};

					i++;
				}
			}

			ushort[] elements;
			i = 0;

			if (wires == false)
			{
				elements = new ushort[(segments - 1) * (rings - 1) * 6];

				for (int y = 0; y < rings - 1; y++)
				{
					for (int x = 0; x < segments - 1; x++)
					{
						elements[i++] = (ushort)((y + 0) * segments + x);
						elements[i++] = (ushort)((y + 1) * segments + x);
						elements[i++] = (ushort)((y + 1) * segments + x + 1);

						elements[i++] = (ushort)((y + 1) * segments + x + 1);
						elements[i++] = (ushort)((y + 0) * segments + x + 1);
						elements[i++] = (ushort)((y + 0) * segments + x);
					}
				}
			}
			else
			{
				elements = new ushort[(segments - 1) * (rings - 1) * 8];

				for (int y = 0; y < rings - 1; y++)
				{
					for (int x = 0; x < segments - 1; x++)
					{
						elements[i++] = (ushort)((y + 0) * segments + x);
						elements[i++] = (ushort)((y + 1) * segments + x);

						elements[i++] = (ushort)((y + 1) * segments + x);
						elements[i++] = (ushort)((y + 1) * segments + x + 1);

						elements[i++] = (ushort)((y + 1) * segments + x + 1);
						elements[i++] = (ushort)((y + 0) * segments + x + 1);

						elements[i++] = (ushort)((y + 0) * segments + x + 1);
						elements[i++] = (ushort)((y + 0) * segments + x);
					}
				}
			}

			PrimitiveType mode = (wires) ? PrimitiveType.Lines : PrimitiveType.Triangles;
			return createVbo(mode, vertices, elements);
		}

		private Vbo createPlaneVbo(float dist, float density)
		{
			int linesPerSide = 2 * (int)(dist / density) + 1;
			Vector3[] vertices = new Vector3[4 * linesPerSide];

			for (int i = 0; i < linesPerSide; i++)
			{
				vertices[i * 4 + 0] = new Vector3()
				{
					X = -dist + i * density,
					Y = -dist,
					Z = 0.0f,
				};

				vertices[i * 4 + 1] = new Vector3()
				{
					X = -dist + i * density,
					Y = +dist,
					Z = 0.0f,
				};

				vertices[i * 4 + 2] = new Vector3()
				{
					X = -dist,
					Y = -dist + i * density,
					Z = 0.0f,
				};

				vertices[i * 4 + 3] = new Vector3()
				{
					X = +dist,
					Y = -dist + i * density,
					Z = 0.0f,
				};
			}

			return createVbo(PrimitiveType.Lines, vertices);
		}

		private Vbo createLineVbo(float infinity)
		{
			Vector3[] vertices = new Vector3[2];

			vertices[0] = new Vector3()
			{
				X = 0.0f,
				Y = 0.0f,
				Z = -infinity,
			};

			vertices[1] = new Vector3()
			{
				X = 0.0f,
				Y = 0.0f,
				Z = +infinity,
			};

			return createVbo(PrimitiveType.Lines, vertices);
		}

		private Vbo createCircleVbo(int lines)
		{
			Vector3[] vertices = new Vector3[lines];

			for (int i = 0; i < lines; i++)
			{
				double angle = (double)i / CircleLines * MathHelper.TwoPi;

				vertices[i] = new Vector3()
				{
					X = (float)Math.Cos(angle),
					Y = (float)Math.Sin(angle),
					Z = 0.0f,
				};
			}

			return createVbo(PrimitiveType.LineLoop, vertices);
		}

		private Vbo createVectorLineVbo()
		{
			Vector3[] vertices = new Vector3[2];

			vertices[0] = new Vector3()
			{
				X = 0.0f,
				Y = 0.0f,
				Z = 0.0f,
			};

			vertices[1] = new Vector3()
			{
				X = 0.0f,
				Y = 0.0f,
				Z = 1.0f,
			};

			return createVbo(PrimitiveType.Lines, vertices);
		}

		private Vbo createVectorArrowVbo(int triangles, float radius, float length)
		{
			Vector3[] vertices = new Vector3[triangles + 2];

			vertices[0] = new Vector3(0.0f, 0.0f, 0.0f);

			for (int i = 1; i < vertices.Length; i++)
			{
				double angle = (double)(i - 1) / triangles * 2 * Math.PI;

				vertices[i] = new Vector3()
				{
					X = radius * (float)Math.Cos(angle),
					Y = radius * (float)Math.Sin(angle),
					Z = -length,
				};
			}

			return createVbo(PrimitiveType.TriangleFan, vertices);
		}


		private void drawModel(Vbo vbo)
		{
			GL.EnableClientState(ArrayCap.VertexArray);

			GL.BindBuffer(BufferTarget.ArrayBuffer, vbo.VboId);
			GL.BindBuffer(BufferTarget.ElementArrayBuffer, (int)vbo.EboId);

			GL.VertexPointer(3, VertexPointerType.Float, BlittableValueType.StrideOf(new Vector3()), 0);
			GL.DrawElements(vbo.Mode, vbo.NumElements, DrawElementsType.UnsignedShort, IntPtr.Zero);

			GL.DisableClientState(ArrayCap.VertexArray);
		}

		private void drawVector(MultiVector vector, Color color)
		{
			GL.Color3(color);

			MultiVector rotAxis = MultiVector.CrossProduct(Basis.E3, vector);
			double rotAngle = Math.Acos(vector.E3 / vector.Length); ;

			if (rotAxis == MultiVector.Zero)
			{
				rotAxis = Basis.E1;
				rotAngle = 0.0;
			}


			double length = vector.Length;

			GL.PushMatrix();

			GL.Rotate(MathHelper.RadiansToDegrees(rotAngle), rotAxis.E1, rotAxis.E2, rotAxis.E3);
			GL.Scale(length, length, length);

			drawModel(vectorLineVbo);

			GL.PopMatrix();

			GL.PushMatrix();

			GL.Translate(vector.E1, vector.E2, vector.E3);
			GL.Rotate(MathHelper.RadiansToDegrees(rotAngle), rotAxis.E1, rotAxis.E2, rotAxis.E3);

			drawModel(vectorArrowVbo);

			GL.PopMatrix();
		}

		private void drawPoint(MultiVector point, Color color)
		{
			MultiVector x;

			IPNS.GetPointParams(point, out x);

			GL.Color3(color);

			GL.PushMatrix();

			GL.Translate(x.E1, x.E2, x.E3);
			GL.Scale(PointRadius, PointRadius, PointRadius);

			drawModel(sphereVbo);

			GL.PopMatrix();
		}

		private void drawSphere(MultiVector sphere, Color color)
		{
			MultiVector c;
			double r;

			IPNS.GetSphereParams(sphere, out c, out r);

			GL.Color3(color);

			GL.PushMatrix();

			GL.Translate(c.E1, c.E2, c.E3);
			GL.Scale(r, r, r);

			drawModel(sphereVbo);

			GL.PopMatrix();
		}

		private void drawPlane(MultiVector plane, Color color)
		{
			MultiVector n;
			double d;

			IPNS.GetPlaneParams(plane, out n, out d);

			MultiVector rotAxis = MultiVector.CrossProduct(Basis.E3, n);
			double rotAngle = Math.Acos(n.E3);

			if (rotAxis == MultiVector.Zero)
			{
				rotAxis = Basis.E1;
				rotAngle = 0.0;
			}

			GL.Color3(color);

			GL.PushMatrix();

			GL.Translate(n.E1 * d, n.E2 * d, n.E3 * d);
			GL.Rotate(MathHelper.RadiansToDegrees(rotAngle), rotAxis.E1, rotAxis.E2, rotAxis.E3);

			drawModel(planeVbo);

			GL.PopMatrix();
		}

		private void drawLine(MultiVector line, Color color)
		{
			MultiVector t, d;

			IPNS.GetLineParams(line, out t, out d);

			MultiVector rotAxis = MultiVector.CrossProduct(Basis.E3, d);
			double rotAngle = Math.Acos(d.E3);

			if (rotAxis == MultiVector.Zero)
			{
				rotAxis = Basis.E1;
				rotAngle = 0.0;
			}

			GL.Color3(color);

			GL.PushMatrix();

			GL.Translate(t.E1, t.E2, t.E3);
			GL.Rotate(MathHelper.RadiansToDegrees(rotAngle), rotAxis.E1, rotAxis.E2, rotAxis.E3);

			drawModel(lineVbo);

			GL.PopMatrix();
		}

		private void drawCircle(MultiVector circle, Color color)
		{
			MultiVector n, c;
			double r;

			IPNS.GetCircleParams(circle, out n, out c, out r);

			MultiVector rotAxis = MultiVector.CrossProduct(Basis.E3, n);
			double rotAngle = Math.Acos(n.E3);

			if (rotAxis == MultiVector.Zero)
			{
				rotAxis = Basis.E1;
				rotAngle = 0.0;
			}

			GL.Color3(color);

			GL.PushMatrix();

			GL.Translate(c.E1, c.E2, c.E3);
			GL.Rotate(MathHelper.RadiansToDegrees(rotAngle), rotAxis.E1, rotAxis.E2, rotAxis.E3);
			GL.Scale(r, r, r);

			drawModel(circleVbo);

			GL.PopMatrix();
		}

		private void drawPointPair(MultiVector pointPair, Color color)
		{
			MultiVector p1, p2;

			IPNS.GetPointPairParams(pointPair, out p1, out p2);

			drawPoint(p1, color);
			drawPoint(p2, color);
		}
	}
}


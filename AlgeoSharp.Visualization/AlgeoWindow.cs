using System;
using OpenTK;
using OpenTK.Input;

namespace AlgeoSharp.Visualization
{
	public class AlgeoWindow : GameWindow
	{
		const float ANGLE_STEP = 0.05f;
		const float DISTANCE_STEP = 0.2f;

		public AlgeoWindow()
		{
			Visualizer = new AlgeoVisualizer();

			alpha = (float)Math.PI / 8;
			beta = (float)Math.PI / 4;
			distance = 20f;
		}

		float distance;
		float alpha;
		float beta;

		public AlgeoVisualizer Visualizer { get; private set; }

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			MakeCurrent();
			Visualizer.Load();
		}

		protected override void OnUnload(EventArgs e)
		{
			base.OnUnload(e);

			MakeCurrent();
			Visualizer.Unload();
		}

		protected override void OnResize(EventArgs e)
		{
			base.OnResize(e);

			MakeCurrent();
			Visualizer.Resize(Width, Height);
		}

		protected override void OnRenderFrame(FrameEventArgs e)
		{
			base.OnRenderFrame(e);

			MakeCurrent();
			Visualizer.Render();
			SwapBuffers();
		}

		protected override void OnUpdateFrame(FrameEventArgs e)
		{
			base.OnUpdateFrame(e);

			var state = OpenTK.Input.Keyboard.GetState();

			if (state[Key.Up]) {
				alpha += ANGLE_STEP;
			}

			if (state[Key.Down]) {
				alpha -= ANGLE_STEP;
			}

			if (state[Key.Left]) {
				beta += ANGLE_STEP;
			}

			if (state[Key.Right]) {
				beta -= ANGLE_STEP;
			}

			if (state[Key.PageUp]) {
				distance -= DISTANCE_STEP;
			}

			if (state[Key.PageDown]) {
				distance += DISTANCE_STEP;
			}

			float y = distance * (float)Math.Sin(alpha);
			float rxz = distance * (float)Math.Cos(alpha);
			float z = rxz * (float)Math.Sin(beta);
			float x = rxz * (float)Math.Cos(beta);

			Visualizer.Eye = MultiVector.Vector(x, y, z);
		}
	}
}


using System;
using OpenTK;

namespace AlgeoSharp.Visualization
{
	public class AlgeoWindow : GameWindow
	{
		public AlgeoWindow()
		{
			Visualizer = new AlgeoVisualizer();
		}

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
	}
}


namespace PokeJuegos.Mobile.Controls;

/// <summary>
/// Roulette background: radial rays plus two jagged "burst" layers, drawn at runtime so they stay sharp and
/// keep the web proportions on any screen. On the web these were three CSS layers (<c>.rayos</c>,
/// <c>.estallido</c>, <c>.centro</c> in <c>RouletteView.vue</c>), each sized on its own; the bundled SVG
/// (<c>bg_roulette_burst.svg</c>) flattened them into one fixed ratio, which doesn't match once scaled to cover a phone.
/// </summary>
/// <remarks>
/// The burst is centered <see cref="CenterOffsetRatio"/> of the height above the middle, like the silhouette
/// (<c>margin-top: -8vh</c>); the page moves the sprite by the same amount so both share the same center.
/// </remarks>
public class BurstBackground : GraphicsView
{
	/// <summary>Vertical offset of the center, as a fraction of the height (negative = up).</summary>
	public const double CenterOffsetRatio = -0.08;

	public BurstBackground()
	{
		Drawable = new BurstDrawable();
		InputTransparent = true;
		SizeChanged += (_, _) => Invalidate();
	}

	private sealed class BurstDrawable : IDrawable
	{
		private const int RayCount = 36;
		private const float RayDegrees = 4;

		private static readonly Color BackgroundColor = Color.FromArgb("#8C3A3A");

		// The web layers the accent rays at 90% opacity over the background.
		private static readonly Color RayColor = Color.FromArgb("#D97757").WithAlpha(0.9f);
		private static readonly Color BurstColor = Color.FromArgb("#6B9B9F");
		private static readonly Color CenterColor = Color.FromArgb("#E8F1EE");

		// clip-path polygons of .estallido and .centro, in % of the layer's side.
		private static readonly float[] BurstPolygon =
		[
			50, 0, 54, 16, 62, 3, 63, 20, 73, 8, 71, 24, 84, 14, 78, 29, 92, 22, 82, 35, 97, 30, 84, 41, 100, 40,
			85, 48, 100, 50, 85, 52, 100, 60, 84, 59, 97, 70, 82, 65, 92, 78, 78, 71, 84, 86, 71, 76, 73, 92,
			63, 80, 62, 97, 54, 84, 50, 100, 46, 84, 38, 97, 37, 80, 27, 92, 29, 76, 16, 86, 22, 71, 8, 78, 18, 65,
			3, 70, 16, 59, 0, 60, 15, 52, 0, 50, 15, 48, 0, 40, 16, 41, 3, 30, 18, 35, 8, 22, 22, 29, 16, 14,
			29, 24, 27, 8, 37, 20, 38, 3, 46, 16,
		];

		private static readonly float[] CenterPolygon =
		[
			50, 4, 55, 18, 65, 8, 64, 22, 76, 14, 71, 27, 86, 20, 78, 33, 94, 27, 82, 38, 96, 35, 85, 44, 98, 44,
			86, 50, 98, 50, 86, 53, 98, 58, 85, 56, 96, 65, 82, 62, 94, 73, 78, 67, 86, 80, 71, 73, 76, 86,
			65, 78, 64, 92, 55, 82, 50, 96, 45, 82, 35, 92, 36, 78, 24, 86, 29, 73, 14, 80, 22, 67, 6, 73, 18, 62,
			4, 65, 15, 56, 0.9f, 54, 15.6f, 48.8f, 16.4f, 50, 2, 44, 15, 44, 4, 35, 18, 38, 6, 27, 22, 33, 14, 20,
			29, 27, 24, 14, 36, 22, 35, 8, 45, 18,
		];

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			float width = dirtyRect.Width;
			float height = dirtyRect.Height;
			if (width <= 0 || height <= 0)
			{
				return;
			}

			PointF center = new(dirtyRect.Center.X, dirtyRect.Center.Y + (float)(CenterOffsetRatio * height));

			canvas.FillColor = BackgroundColor;
			canvas.FillRectangle(dirtyRect);

			DrawRays(canvas, center, MathF.Sqrt((width * width) + (height * height)));

			// .estallido: clamp(320px, min(78vh, 92vw), 620px); .centro: clamp(220px, min(55vh, 65vw), 440px).
			float burstSide = Math.Clamp(Math.Min(0.78f * height, 0.92f * width), 320, 620);
			float centerSide = Math.Clamp(Math.Min(0.55f * height, 0.65f * width), 220, 440);
			FillPolygon(canvas, BurstPolygon, center, burstSide, BurstColor);
			FillPolygon(canvas, CenterPolygon, center, centerSide, CenterColor);
		}

		/// <summary>
		/// repeating-conic-gradient(from 0deg, accent 0deg 4deg, background 4deg 10deg): a 4° ray every 10°,
		/// starting at 12 o'clock and going clockwise. Long enough to reach every corner from the center.
		/// </summary>
		private static void DrawRays(ICanvas canvas, PointF center, float radius)
		{
			PathF rays = new();
			for (int i = 0; i < RayCount; i++)
			{
				float start = i * 360f / RayCount;
				rays.MoveTo(center);
				rays.LineTo(PointOnCircle(center, radius, start));
				rays.LineTo(PointOnCircle(center, radius, start + RayDegrees));
				rays.Close();
			}

			canvas.FillColor = RayColor;
			canvas.FillPath(rays);
		}

		// Degrees measured clockwise from 12 o'clock, like CSS conic gradients.
		private static PointF PointOnCircle(PointF center, float radius, float degrees)
		{
			float radians = degrees * MathF.PI / 180;
			return new PointF(center.X + (radius * MathF.Sin(radians)), center.Y - (radius * MathF.Cos(radians)));
		}

		private static void FillPolygon(ICanvas canvas, float[] percentPoints, PointF center, float side, Color color)
		{
			float left = center.X - (side / 2);
			float top = center.Y - (side / 2);
			PathF polygon = new();
			for (int i = 0; i < percentPoints.Length; i += 2)
			{
				PointF point = new(left + (percentPoints[i] * side / 100), top + (percentPoints[i + 1] * side / 100));
				if (i == 0)
				{
					polygon.MoveTo(point);
				}
				else
				{
					polygon.LineTo(point);
				}
			}

			polygon.Close();
			canvas.FillColor = color;
			canvas.FillPath(polygon);
		}
	}
}

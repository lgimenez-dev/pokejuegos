namespace PokeJuegos.Mobile.Controls;

/// <summary>
/// The round's fuse (port of the <c>.fuseBar</c> in <c>RouletteView.vue</c>, simplified): the striped track,
/// the burnt part growing from the left and a glowing tip where it burns. Drawn on a <see cref="GraphicsView"/>
/// every frame instead of scaling a view, so it renders the same on Android and Windows. Purely visual: the real
/// timeout is in <c>RouletteViewModel</c>, like <c>startFuseTimer</c> on the web.
/// The rolling pokeball and sparks come in a later iteration.
/// </summary>
public class FuseBar : GraphicsView
{
	private const string AnimationName = "FuseBurn";

	private readonly FuseDrawable drawable = new();

	public FuseBar()
	{
		Drawable = drawable;
		HeightRequest = 32;
		InputTransparent = true;
	}

	/// <summary>Burns the fuse from empty to full in <paramref name="durationMs"/>, linearly.</summary>
	public void Start(double durationMs)
	{
		Stop();
		SetProgress(0);
		new Animation(SetProgress, 0, 1)
			.Commit(this, AnimationName, rate: 16, length: (uint)durationMs, easing: Easing.Linear);
	}

	/// <summary>Freezes the fuse where it is.</summary>
	public void Stop() => this.AbortAnimation(AnimationName);

	private void SetProgress(double progress)
	{
		drawable.Progress = (float)progress;
		Invalidate();
	}

	private sealed class FuseDrawable : IDrawable
	{
		// .fuseTrack: 0.8rem tall, 0.4rem in from each side, stripes of 6px.
		private const float TrackHeight = 8;
		private const float TrackInset = 4;
		private const float StripeWidth = 6;
		private const float TipRadius = 7;

		private static readonly Color StripeLight = Color.FromArgb("#8A5A3C");
		private static readonly Color StripeDark = Color.FromArgb("#6E4429");
		private static readonly Color TipGlow = Color.FromArgb("#FF8A3D");
		private static readonly Color TipCore = Color.FromArgb("#FFF2C8");

		// .fuseBurnt gradient.
		private static readonly PaintGradientStop[] BurntStops =
		[
			new(0, Color.FromArgb("#1A1A1A")),
			new(0.85f, Color.FromArgb("#3A3A3A")),
			new(0.95f, TipGlow),
			new(1, TipCore),
		];

		/// <summary>0 = untouched, 1 = burnt out.</summary>
		public float Progress { get; set; }

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			float trackWidth = dirtyRect.Width - (2 * TrackInset);
			if (trackWidth <= 0)
			{
				return;
			}

			RectF track = new(dirtyRect.X + TrackInset, dirtyRect.Center.Y - (TrackHeight / 2), trackWidth, TrackHeight);

			canvas.SaveState();
			PathF clip = new();
			clip.AppendRoundedRectangle(track, TrackHeight / 2);
			canvas.ClipPath(clip);

			canvas.FillColor = StripeLight;
			canvas.FillRectangle(track);
			canvas.FillColor = StripeDark;
			for (float x = track.Left + StripeWidth; x < track.Right; x += 2 * StripeWidth)
			{
				canvas.FillRectangle(x, track.Top, StripeWidth, TrackHeight);
			}

			float burntWidth = trackWidth * Math.Clamp(Progress, 0, 1);
			if (burntWidth > 0)
			{
				RectF burnt = new(track.Left, track.Top, burntWidth, TrackHeight);
				canvas.SetFillPaint(new LinearGradientPaint(BurntStops, new Point(0, 0.5), new Point(1, 0.5)), burnt);
				canvas.FillRectangle(burnt);
			}

			canvas.RestoreState();

			// Glowing tip where the fuse is burning (stand-in for the web's .fuseFire).
			PointF tip = new(track.Left + burntWidth, track.Center.Y);
			canvas.SaveState();
			canvas.SetShadow(SizeF.Zero, 10, TipGlow);
			canvas.FillColor = TipGlow;
			canvas.FillCircle(tip, TipRadius);
			canvas.RestoreState();
			canvas.FillColor = TipCore;
			canvas.FillCircle(tip, TipRadius / 2);
		}
	}
}

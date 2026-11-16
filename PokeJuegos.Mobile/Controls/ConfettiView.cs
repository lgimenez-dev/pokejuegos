using System.Diagnostics;

namespace PokeJuegos.Mobile.Controls;

/// <summary>
/// Confetti burst for a correct guess: port of the <c>canvas-confetti</c> call in <c>useGuessFeedback.ts</c>
/// (140 particles, spread 360, startVelocity 45, ticks 200, the burst palette) with the same per-tick physics
/// as that library. One <see cref="GraphicsView"/> redrawn while the burst lasts (~3.3 s), instead of a view
/// per particle. Place it over the page; it never takes touches.
/// </summary>
public class ConfettiView : GraphicsView
{
	private const string AnimationName = "ConfettiBurst";

	// canvas-confetti advances its particles once per animation frame (60 per second).
	private const double TicksPerSecond = 60;
	private const int TotalTicks = 200;

	private readonly ConfettiDrawable drawable = new();
	private readonly Stopwatch clock = new();
	private int ticksDone;

	public ConfettiView()
	{
		Drawable = drawable;
		InputTransparent = true;
	}

	/// <summary>Fires a new burst from <paramref name="origin"/> (in this view's coordinates), replacing any running one.</summary>
	public void Burst(Point origin)
	{
		Stop();
		drawable.Spawn((float)origin.X, (float)origin.Y);
		ticksDone = 0;
		clock.Restart();

		// The Animation is only the frame clock: each frame catches the particles up to the elapsed time,
		// so the burst lasts the same on a slow device.
		double durationMs = TotalTicks / TicksPerSecond * 1000;
		new Animation(_ => OnFrame(), 0, 1)
			.Commit(this, AnimationName, rate: 16, length: (uint)durationMs, finished: (_, _) => Clear());
	}

	/// <summary>Removes the confetti at once (reset, leaving the page).</summary>
	public void Stop()
	{
		this.AbortAnimation(AnimationName);
		Clear();
	}

	private void OnFrame()
	{
		int targetTick = Math.Min(TotalTicks, (int)(clock.Elapsed.TotalSeconds * TicksPerSecond));
		for (; ticksDone < targetTick; ticksDone++)
		{
			drawable.Step(ticksDone, TotalTicks);
		}

		Invalidate();
	}

	private void Clear()
	{
		clock.Stop();
		drawable.Particles = [];
		Invalidate();
	}

	private sealed class ConfettiDrawable : IDrawable
	{
		private const int ParticleCount = 140;
		private const float StartVelocity = 45;
		private const float Decay = 0.9f;
		private const float Gravity = 3;

		// CONFETTI_COLORS in useGuessFeedback.ts.
		private static readonly Color[] Colors =
		[
			Color.FromArgb("#D97757"),
			Color.FromArgb("#6B9B9F"),
			Color.FromArgb("#E8F1EE"),
			Color.FromArgb("#8C3A3A"),
			Color.FromArgb("#FFFFFF"),
		];

		public Particle[] Particles { get; set; } = [];

		public void Spawn(float x, float y)
		{
			Particles = new Particle[ParticleCount];
			for (int i = 0; i < ParticleCount; i++)
			{
				Particles[i] = new Particle
				{
					X = x,
					Y = y,
					Color = Colors[Random.Shared.Next(Colors.Length)],

					// spread 360: any direction.
					Angle = Random.Shared.NextSingle() * 2 * MathF.PI,
					Velocity = (StartVelocity * 0.5f) + (Random.Shared.NextSingle() * StartVelocity),
					Wobble = Random.Shared.NextSingle() * 10,
					WobbleSpeed = MathF.Min(0.11f, (Random.Shared.NextSingle() * 0.1f) + 0.05f),
					TiltAngle = ((Random.Shared.NextSingle() * 0.5f) + 0.25f) * MathF.PI,
				};
			}
		}

		/// <summary>One canvas-confetti tick (<c>updateFetti</c>): move, slow down, fall, flutter, fade.</summary>
		public void Step(int tick, int totalTicks)
		{
			foreach (Particle p in Particles)
			{
				p.X += MathF.Cos(p.Angle) * p.Velocity;
				p.Y += (MathF.Sin(p.Angle) * p.Velocity) + Gravity;
				p.Velocity *= Decay;

				// The wobble point and the tilt make the square twist like a falling paper.
				p.Wobble += p.WobbleSpeed;
				p.WobbleX = p.X + (10 * MathF.Cos(p.Wobble));
				p.WobbleY = p.Y + (10 * MathF.Sin(p.Wobble));
				p.TiltAngle += 0.1f;
				p.TiltSin = MathF.Sin(p.TiltAngle);
				p.TiltCos = MathF.Cos(p.TiltAngle);
				p.Jitter = Random.Shared.NextSingle() + 2;
				p.Opacity = 1 - ((float)(tick + 1) / totalTicks);
			}
		}

		public void Draw(ICanvas canvas, RectF dirtyRect)
		{
			foreach (Particle p in Particles)
			{
				if (p.Opacity <= 0)
				{
					continue;
				}

				// Same quad as canvas-confetti's "square" shape.
				float x1 = p.X + (p.Jitter * p.TiltCos);
				float y1 = p.Y + (p.Jitter * p.TiltSin);
				float x2 = p.WobbleX + (p.Jitter * p.TiltCos);
				float y2 = p.WobbleY + (p.Jitter * p.TiltSin);

				PathF quad = new();
				quad.MoveTo(p.X, p.Y);
				quad.LineTo(p.WobbleX, y1);
				quad.LineTo(x2, y2);
				quad.LineTo(x1, p.WobbleY);
				quad.Close();

				canvas.FillColor = p.Color.WithAlpha(p.Opacity);
				canvas.FillPath(quad);
			}
		}
	}

	private sealed class Particle
	{
		public float X;
		public float Y;
		public float Angle;
		public float Velocity;
		public float Wobble;
		public float WobbleSpeed;
		public float WobbleX;
		public float WobbleY;
		public float TiltAngle;
		public float TiltSin;
		public float TiltCos;
		public float Jitter;
		public float Opacity;
		public Color Color = Colors.Transparent;
	}
}

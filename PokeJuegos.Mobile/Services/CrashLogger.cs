namespace PokeJuegos.Mobile.Services;

/// <summary>
/// Last-resort logging for exceptions nobody caught, so a crash in the Release APK leaves a trace:
/// it goes to <c>adb logcat</c> (tag <c>PokeJuegos</c>) and to <c>last-crash.txt</c> in the app data folder.
/// </summary>
public static class CrashLogger
{
	private const string LogTag = "PokeJuegos";
	private const string CrashFileName = "last-crash.txt";

	/// <summary>
	/// Subscribes to the process-wide unhandled exception events. Call once, as early as possible.
	/// </summary>
	public static void Register()
	{
		AppDomain.CurrentDomain.UnhandledException += (_, e) =>
			Log("AppDomain.UnhandledException", e.ExceptionObject as Exception);

		TaskScheduler.UnobservedTaskException += (_, e) =>
		{
			Log("TaskScheduler.UnobservedTaskException", e.Exception);
			e.SetObserved();
		};

#if ANDROID
		// Exceptions thrown inside Java callbacks (UI events, lifecycle) surface here before the process dies.
		Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
			Log("AndroidEnvironment.UnhandledExceptionRaiser", e.Exception);
#endif
	}

	/// <summary>
	/// Writes an exception to the platform log and overwrites the crash file with it.
	/// </summary>
	/// <param name="source">Which handler caught the exception.</param>
	/// <param name="exception">The exception, if any was provided.</param>
	public static void Log(string source, Exception? exception)
	{
		string message = $"[{DateTime.UtcNow:O}] {source}{Environment.NewLine}{exception}";

#if ANDROID
		Android.Util.Log.Error(LogTag, message);
#else
		Console.Error.WriteLine($"{LogTag}: {message}");
#endif

		try
		{
			File.WriteAllText(Path.Combine(FileSystem.AppDataDirectory, CrashFileName), message);
		}
		catch (Exception)
		{
			// Logging must never throw on top of the original crash.
		}
	}
}

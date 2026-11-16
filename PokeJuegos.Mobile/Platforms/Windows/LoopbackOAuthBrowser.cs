using System.Net;
using System.Net.Sockets;
using System.Text;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile;

/// <summary>
/// <see cref="IOAuthBrowser"/> on Windows, where MAUI's <see cref="WebAuthenticator"/> doesn't work: listens on
/// <c>http://127.0.0.1:&lt;free port&gt;/callback</c>, opens the system browser with that as the return URL and
/// reads the backend's redirect (the loopback flow of RFC 8252 §7.3).
/// A plain <see cref="TcpListener"/> instead of <see cref="HttpListener"/>, which needs a URL reservation (admin) on Windows.
/// </summary>
public class LoopbackOAuthBrowser : IOAuthBrowser
{
	private const string CallbackPath = "/callback";

	// The user may close the tab without finishing: give up eventually instead of listening forever.
	private static readonly TimeSpan Timeout = TimeSpan.FromMinutes(5);

	private static readonly TimeSpan RequestReadTimeout = TimeSpan.FromSeconds(2);

	private const string DonePage =
		"""
		<!doctype html>
		<html><head><meta charset="utf-8"><title>PokeJuegos</title></head>
		<body style="font-family:sans-serif;text-align:center;margin-top:20vh">
		<h2>You can close this tab and return to PokeJuegos.</h2>
		</body></html>
		""";

	/// <inheritdoc />
	public async Task<IReadOnlyDictionary<string, string>?> AuthenticateAsync(Func<string, Uri> buildStartUrl, CancellationToken ct)
	{
		using CancellationTokenSource timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
		timeout.CancelAfter(Timeout);

		TcpListener listener = new(IPAddress.Loopback, 0);
		listener.Start();
		try
		{
			int port = ((IPEndPoint)listener.LocalEndpoint).Port;
			await Launcher.Default.OpenAsync(buildStartUrl($"http://127.0.0.1:{port}{CallbackPath}"));

			while (true)
			{
				using TcpClient client = await listener.AcceptTcpClientAsync(timeout.Token);
				using NetworkStream stream = client.GetStream();

				string? target;
				try
				{
					// Chrome opens speculative connections that never send a request: don't let one block the real callback.
					using CancellationTokenSource readTimeout = CancellationTokenSource.CreateLinkedTokenSource(timeout.Token);
					readTimeout.CancelAfter(RequestReadTimeout);
					target = await ReadRequestTargetAsync(stream, readTimeout.Token);
				}
				catch (Exception ex) when (ex is IOException || (ex is OperationCanceledException && !timeout.IsCancellationRequested))
				{
					continue;
				}

				string path = target?.Split('?', 2)[0] ?? string.Empty;
				if (path != CallbackPath)
				{
					// Anything else the browser asks for (e.g. /favicon.ico).
					await TryWriteResponseAsync(stream, "404 Not Found", string.Empty, timeout.Token);
					continue;
				}

				await TryWriteResponseAsync(stream, "200 OK", DonePage, timeout.Token);
				BringAppToFront();
				return ParseQuery(target!);
			}
		}
		catch (OperationCanceledException)
		{
			return null;
		}
		finally
		{
			listener.Stop();
		}
	}

	/// <summary>
	/// Reads the request line (<c>GET /callback?a=b HTTP/1.1</c>) and skips the headers, so the browser
	/// gets a clean response instead of a reset connection.
	/// </summary>
	private static async Task<string?> ReadRequestTargetAsync(NetworkStream stream, CancellationToken ct)
	{
		using StreamReader reader = new(stream, Encoding.ASCII, leaveOpen: true);
		string? requestLine = await reader.ReadLineAsync(ct);

		string? header;
		do
		{
			header = await reader.ReadLineAsync(ct);
		}
		while (!string.IsNullOrEmpty(header));

		string[]? parts = requestLine?.Split(' ');
		return parts is { Length: >= 2 } && parts[0] == "GET" ? parts[1] : null;
	}

	// The browser may already have dropped the connection; the request was read, so that's not an error.
	private static async Task TryWriteResponseAsync(NetworkStream stream, string status, string html, CancellationToken ct)
	{
		byte[] body = Encoding.UTF8.GetBytes(html);
		string head =
			$"HTTP/1.1 {status}\r\n" +
			"Content-Type: text/html; charset=utf-8\r\n" +
			$"Content-Length: {body.Length}\r\n" +
			"Connection: close\r\n\r\n";
		try
		{
			await stream.WriteAsync(Encoding.ASCII.GetBytes(head), ct);
			await stream.WriteAsync(body, ct);
		}
		catch (IOException)
		{
		}
	}

	private static Dictionary<string, string> ParseQuery(string target)
	{
		Dictionary<string, string> values = [];
		string[] parts = target.Split('?', 2);
		if (parts.Length < 2)
		{
			return values;
		}

		foreach (string pair in parts[1].Split('&', StringSplitOptions.RemoveEmptyEntries))
		{
			string[] keyValue = pair.Split('=', 2);
			string key = Uri.UnescapeDataString(keyValue[0].Replace('+', ' '));
			string value = keyValue.Length > 1 ? Uri.UnescapeDataString(keyValue[1].Replace('+', ' ')) : string.Empty;
			values.TryAdd(key, value);
		}

		return values;
	}

	// The browser has the focus after the sign-in; Windows may only flash the taskbar button instead.
	private static void BringAppToFront() =>
		MainThread.BeginInvokeOnMainThread(() =>
			(Application.Current?.Windows.FirstOrDefault()?.Handler?.PlatformView as Microsoft.UI.Xaml.Window)?.Activate());
}

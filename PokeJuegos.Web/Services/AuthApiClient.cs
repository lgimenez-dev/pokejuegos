using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using PokeJuegos.DTO;
using PokeJuegos.Web.Models;

namespace PokeJuegos.Web.Services;

/// <summary>
/// Calls the token-based endpoints of <c>/api/auth</c> in <c>PokeJuegos.Api</c> (server to server).
/// </summary>
/// <param name="http">The client configured with <c>Api:BaseUrl</c> in <c>Program.cs</c>.</param>
public class AuthApiClient(HttpClient http)
{
    private const string ServerUnreachable = "Can't reach the server. Please try again in a few minutes.";
    private const string UnexpectedError = "Something went wrong. Please try again.";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Activates a pending account (<c>POST api/auth/activate</c>).
    /// </summary>
    /// <param name="token">The activation token from the email link.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The outcome of the call.</returns>
    public Task<ApiResult> ActivateAsync(string token, CancellationToken ct = default) =>
        PostAsync("api/auth/activate", new ActivateAccountRequest(token), ct);

    /// <summary>
    /// Sets a new password with a recovery token (<c>POST api/auth/change-password</c>).
    /// </summary>
    /// <param name="token">The recovery token from the email link.</param>
    /// <param name="password">The new password.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The outcome of the call.</returns>
    public Task<ApiResult> ChangePasswordAsync(string token, string password, CancellationToken ct = default) =>
        PostAsync("api/auth/change-password", new ChangePasswordRequest(token, password), ct);

    private async Task<ApiResult> PostAsync<T>(string path, T body, CancellationToken ct)
    {
        try
        {
            using HttpResponseMessage response = await http.PostAsJsonAsync(path, body, JsonOptions, ct);
            return response.IsSuccessStatusCode
                ? ApiResult.Success
                : await ReadFailureAsync(response, ct);
        }
        catch (Exception ex) when ((ex is HttpRequestException or TaskCanceledException) && !ct.IsCancellationRequested)
        {
            return ApiResult.Failure(ServerUnreachable);
        }
    }

    /// <summary>
    /// Maps an error response: <c>{ errors: [{ field, message }] }</c> (validation) or <c>{ message }</c> (business rule).
    /// </summary>
    private static async Task<ApiResult> ReadFailureAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.StatusCode == HttpStatusCode.TooManyRequests)
        {
            return ApiResult.Failure("Too many attempts. Try again in a minute.");
        }

        try
        {
            string content = await response.Content.ReadAsStringAsync(ct);
            using JsonDocument document = JsonDocument.Parse(content);
            JsonElement root = document.RootElement;

            // ASP.NET's own ProblemDetails also has "errors", but as an object: only the API's array shape is read.
            if (root.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Array)
            {
                ValidationErrorResponse? validation = root.Deserialize<ValidationErrorResponse>(JsonOptions);
                return ToInvalid(validation?.Errors ?? []);
            }

            if (root.TryGetProperty("message", out _))
            {
                BusinessErrorResponse? business = root.Deserialize<BusinessErrorResponse>(JsonOptions);
                if (!string.IsNullOrEmpty(business?.Message))
                {
                    return ApiResult.Failure(business.Message);
                }
            }
        }
        catch (JsonException)
        {
            // Not one of the API's error shapes (empty body, HTML from a proxy…).
        }

        return ApiResult.Failure(UnexpectedError);
    }

    private static ApiResult ToInvalid(IReadOnlyList<FieldError> errors)
    {
        Dictionary<string, string> fieldErrors = [];
        string? general = null;
        foreach (FieldError error in errors)
        {
            if (string.IsNullOrEmpty(error.Field))
            {
                general ??= error.Message;
            }
            else
            {
                fieldErrors.TryAdd(error.Field, error.Message);
            }
        }

        return fieldErrors.Count == 0
            ? ApiResult.Failure(general ?? UnexpectedError)
            : ApiResult.Invalid(fieldErrors, general);
    }
}

using System.Net.Http.Headers;
using System.Net.Http.Json;
using OzunuInkisaf.Contracts.Auth;
using OzunuInkisaf.Contracts.Books;
using OzunuInkisaf.Contracts.Common;
using OzunuInkisaf.Contracts.Duas;
using OzunuInkisaf.Contracts.Khatim;
using OzunuInkisaf.Contracts.Reports;
using OzunuInkisaf.Contracts.Tally;
using OzunuInkisaf.Contracts.Users;

namespace OzunuInkisaf.Web.Services;

/// <summary>
/// Backend Web API ilə bütün HTTP ünsiyyəti bu sinifdən keçir. Hər sorğudan
/// əvvəl <see cref="AuthState"/>-dəki JWT token avtomatik "Authorization"
/// başlığına əlavə olunur.
/// </summary>
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly AuthState _authState;

    public ApiClient(HttpClient http, AuthState authState)
    {
        _http = http;
        _authState = authState;
    }

    private void ApplyAuthHeader()
    {
        _http.DefaultRequestHeaders.Authorization = string.IsNullOrEmpty(_authState.AccessToken)
            ? null
            : new AuthenticationHeaderValue("Bearer", _authState.AccessToken);
    }

    private async Task<T> SendAsync<T>(Func<Task<HttpResponseMessage>> send)
    {
        ApplyAuthHeader();
        var response = await send();
        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<T>();
        return result ?? throw new ApiException("Serverdən boş cavab gəldi.");
    }

    private async Task SendNoContentAsync(Func<Task<HttpResponseMessage>> send)
    {
        ApplyAuthHeader();
        var response = await send();
        await EnsureSuccessAsync(response);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        string message = $"Sorğu uğursuz oldu ({(int)response.StatusCode}).";
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ApiError>();
            if (error is not null && !string.IsNullOrWhiteSpace(error.Message))
            {
                message = error.Message;
            }
        }
        catch
        {
            // Cavab JSON deyildi — defolt mesajla davam edirik.
        }

        throw new ApiException(message) { StatusCode = (int)response.StatusCode };
    }

    // ---- Auth --------------------------------------------------------

    public Task<LoginResponse> LoginAsync(string username, string password) =>
        SendAsync<LoginResponse>(() => _http.PostAsJsonAsync("api/auth/login", new LoginRequest(username, password)));

    public Task ChangePasswordAsync(string currentPassword, string newPassword) =>
        SendNoContentAsync(() => _http.PostAsJsonAsync("api/auth/change-password", new ChangePasswordRequest(currentPassword, newPassword)));

    // ---- Books ---------------------------------------------------------

    public Task<IReadOnlyList<BookSummaryDto>> GetBooksAsync(string? category = null) =>
        SendAsync<IReadOnlyList<BookSummaryDto>>(() => _http.GetAsync(BuildQuery("api/books", ("category", category))));

    public Task<ReadingProgressDto> SaveProgressAsync(Guid bookId, int currentPage) =>
        SendAsync<ReadingProgressDto>(() => _http.PutAsJsonAsync($"api/books/{bookId}/progress", new SaveReadingProgressRequest(currentPage)));

    public async Task<Stream> OpenPdfAsync(Guid bookId)
    {
        ApplyAuthHeader();
        var response = await _http.GetAsync($"api/books/{bookId}/pdf");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadAsStreamAsync();
    }

    public async Task<BookSummaryDto> UploadBookAsync(Stream pdfContent, string fileName, UploadBookRequest meta)
    {
        ApplyAuthHeader();
        using var form = new MultipartFormDataContent();
        using var fileContent = new StreamContent(pdfContent);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        form.Add(fileContent, "File", fileName);
        form.Add(new StringContent(meta.Title), "Title");
        if (meta.Author is not null) form.Add(new StringContent(meta.Author), "Author");
        form.Add(new StringContent(((int)meta.Category).ToString()), "Category");
        if (meta.AssignedMonthLabel is not null) form.Add(new StringContent(meta.AssignedMonthLabel), "AssignedMonthLabel");
        form.Add(new StringContent(meta.IsCurrentMonthPick.ToString()), "IsCurrentMonthPick");
        form.Add(new StringContent(meta.NotifyAllUsers.ToString()), "NotifyAllUsers");
        if (meta.TotalPages is not null) form.Add(new StringContent(meta.TotalPages.Value.ToString()), "TotalPages");

        var response = await _http.PostAsync("api/books/upload", form);
        await EnsureSuccessAsync(response);
        var result = await response.Content.ReadFromJsonAsync<BookSummaryDto>();
        return result ?? throw new ApiException("Serverdən boş cavab gəldi.");
    }

    public Task ArchiveBookAsync(Guid bookId) =>
        SendNoContentAsync(() => _http.PostAsync($"api/books/{bookId}/archive", null));

    // ---- Duas ----------------------------------------------------------

    public Task<IReadOnlyList<DuaDto>> GetDuasAsync(string? category = null, string? search = null) =>
        SendAsync<IReadOnlyList<DuaDto>>(() => _http.GetAsync(BuildQuery("api/duas", ("category", category), ("search", search))));

    public Task<DuaDto> CreateDuaAsync(UpsertDuaRequest request) =>
        SendAsync<DuaDto>(() => _http.PostAsJsonAsync("api/duas", request));

    public Task<DuaDto> UpdateDuaAsync(Guid duaId, UpsertDuaRequest request) =>
        SendAsync<DuaDto>(() => _http.PutAsJsonAsync($"api/duas/{duaId}", request));

    public Task DeleteDuaAsync(Guid duaId) =>
        SendNoContentAsync(() => _http.DeleteAsync($"api/duas/{duaId}"));

    // ---- Khatim ----------------------------------------------------------

    public Task<KhatimCycleStatusDto2> GetKhatimStatusAsync() =>
        SendAsync<KhatimCycleStatusDto2>(() => _http.GetAsync("api/khatim/status"));

    public Task<KhatimCycleStatusDto2> ClaimJuzAsync(int juzNumber) =>
        SendAsync<KhatimCycleStatusDto2>(() => _http.PostAsJsonAsync("api/khatim/claim", new ClaimJuzRequest(juzNumber)));

    public Task<KhatimCycleStatusDto2> CompleteJuzAsync(int juzNumber) =>
        SendAsync<KhatimCycleStatusDto2>(() => _http.PostAsJsonAsync("api/khatim/complete", new CompleteJuzRequest(juzNumber)));

    public Task<IReadOnlyList<KhatimHistoryItemDto>> GetKhatimHistoryAsync() =>
        SendAsync<IReadOnlyList<KhatimHistoryItemDto>>(() => _http.GetAsync("api/khatim/history"));

    // ---- Tally -----------------------------------------------------------

    public Task<TallyTemplateDto?> GetActiveTallyAsync() =>
        SendAsync<TallyTemplateDto?>(() => _http.GetAsync("api/tally/active"));

    public Task<TallyTemplateDto> SetTallyEntryAsync(Guid tallyItemId, DateOnly date, bool isCompleted) =>
        SendAsync<TallyTemplateDto>(() => _http.PutAsJsonAsync("api/tally/entry", new SetTallyEntryRequest(tallyItemId, date, isCompleted)));

    public Task<TallyTemplateDto> CreateTallyAsync(CreateTallyTemplateRequest request) =>
        SendAsync<TallyTemplateDto>(() => _http.PostAsJsonAsync("api/tally", request));

    public Task<TallyTemplateDto> PublishTallyAsync(Guid templateId) =>
        SendAsync<TallyTemplateDto>(() => _http.PostAsync($"api/tally/{templateId}/publish", null));

    public Task<IReadOnlyList<TallyTemplateSummaryDto>> GetTallySummariesAsync() =>
        SendAsync<IReadOnlyList<TallyTemplateSummaryDto>>(() => _http.GetAsync("api/tally/summaries"));

    public Task<TallyTemplateDto> GetTallyByIdAsync(Guid templateId) =>
        SendAsync<TallyTemplateDto>(() => _http.GetAsync($"api/tally/{templateId}"));

    // ---- Users (admin) -----------------------------------------------------

    public Task<IReadOnlyList<UserSummaryDto>> GetUsersAsync(string? search = null) =>
        SendAsync<IReadOnlyList<UserSummaryDto>>(() => _http.GetAsync(BuildQuery("api/users", ("search", search))));

    public Task<CreateUserResult> CreateUserAsync(CreateUserRequest request) =>
        SendAsync<CreateUserResult>(() => _http.PostAsJsonAsync("api/users", request));

    public Task<BulkCreateUsersResult> BulkCreateUsersAsync(int count) =>
        SendAsync<BulkCreateUsersResult>(() => _http.PostAsJsonAsync("api/users/bulk", new BulkCreateUsersRequest(count)));

    public Task<BulkCreateUsersResult> BulkCreateUsersFromListAsync(IReadOnlyList<BulkCreateUsersRowRequest> rows) =>
        SendAsync<BulkCreateUsersResult>(() => _http.PostAsJsonAsync("api/users/bulk-from-list", new BulkCreateUsersRequestFromList(rows)));

    public Task SetUserActiveAsync(Guid userId, bool isActive) =>
        SendNoContentAsync(() => _http.PostAsync($"api/users/{userId}/active?isActive={isActive}", null));

    public Task<CreateUserResult> ResetPasswordAsync(Guid userId) =>
        SendAsync<CreateUserResult>(() => _http.PostAsync($"api/users/{userId}/reset-password", null));

    public Task<LoginResponse> ImpersonateAsync(Guid userId) =>
        SendAsync<LoginResponse>(() => _http.PostAsync($"api/users/{userId}/impersonate", null));

    // ---- Reports (admin) ---------------------------------------------------

    public Task<AdminDashboardSummaryDto> GetDashboardSummaryAsync() =>
        SendAsync<AdminDashboardSummaryDto>(() => _http.GetAsync("api/reports/dashboard"));

    public Task<LeaderboardResultDto> GetLeaderboardAsync(ReportFilterRequest filter) =>
        SendAsync<LeaderboardResultDto>(() => _http.PostAsJsonAsync("api/reports/leaderboard", filter));

    public Task<WinnerAwardDto> DeclareWinnerAsync(DeclareWinnerRequest request) =>
        SendAsync<WinnerAwardDto>(() => _http.PostAsJsonAsync("api/reports/winners", request));

    public Task<IReadOnlyList<WinnerAwardDto>> GetWinnersAsync() =>
        SendAsync<IReadOnlyList<WinnerAwardDto>>(() => _http.GetAsync("api/reports/winners"));

    // ---- Helpers -----------------------------------------------------------

    private static string BuildQuery(string path, params (string Key, string? Value)[] parameters)
    {
        var query = string.Join('&', parameters
            .Where(p => !string.IsNullOrEmpty(p.Value))
            .Select(p => $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value!)}"));

        return query.Length == 0 ? path : $"{path}?{query}";
    }
}

public class ApiException : Exception
{
    public int StatusCode { get; set; }

    public ApiException(string message) : base(message)
    {
    }
}

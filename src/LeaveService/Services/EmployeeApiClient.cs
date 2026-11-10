using System.Net;
using System.Net.Http.Json;

namespace LeaveService.Services;

public class EmployeeApiClient
{
    private readonly HttpClient _httpClient;

    public EmployeeApiClient(
        HttpClient httpClient,
        IConfiguration configuration)
    {
        _httpClient = httpClient;

        var serviceApiKey = configuration[
            "ServiceAuthentication:ApiKey"];

        if (string.IsNullOrWhiteSpace(serviceApiKey))
        {
            throw new InvalidOperationException(
                "EmployeeService API key is not configured.");
        }

        _httpClient.DefaultRequestHeaders.Add(
            "X-Service-Api-Key",
            serviceApiKey);
    }

    public async Task<EmployeeInfo?> GetByIdAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(
            $"api/Employees/{employeeId}",
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<EmployeeInfo>(
            cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        Guid employeeId,
        CancellationToken cancellationToken = default)
    {
        var employee = await GetByIdAsync(
            employeeId,
            cancellationToken);

        return employee is not null;
    }
}

public record EmployeeInfo(
    Guid Id,
    string FullName,
    string Email,
    string Department,
    string Role,
    DateTime CreatedAtUtc
);
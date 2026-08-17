using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TrainingManagement.Api.DTOs;

namespace TrainingManagement.Api.Tests.Integration;

public class EmployeeApiTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EmployeeApiTests(
        CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient()
    {
        return _factory.CreateClient();
    }

    // ============================================================
    // GET /api/v1/employee
    // ============================================================

    [Fact]
    public async Task GetEmployees_ReturnsUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var client = CreateClient();

        // Act
        var response =
            await client.GetAsync("/api/v1/employee");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_ReturnsOk_WhenUserIsAuthenticated()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        // Act
        var response =
            await client.GetAsync("/api/v1/employee");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }

    [Fact]
    public async Task GetEmployees_ReturnsEmployees_WhenEmployeesExist()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        await _factory.SeedEmployeesAsync();

        // Act
        var response =
            await client.GetAsync("/api/v1/employee");

        // Assert
        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "John Doe",
            json);

        Assert.Contains(
            "Jane Smith",
            json);
    }

    [Fact]
    public async Task GetEmployees_ReturnsPagedResult()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        await _factory.SeedEmployeesAsync();

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/employee?page=1&pageSize=2");

        // Assert
        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadAsStringAsync();

        using var document =
            JsonDocument.Parse(json);

        var root =
            document.RootElement;

        Assert.True(
            root.GetProperty("success").GetBoolean());

        Assert.Equal(
            "Employees retrieved successfully.",
            root.GetProperty("message").GetString());

        var data =
            root.GetProperty("data");

        Assert.Equal(
            1,
            data.GetProperty("page").GetInt32());

        Assert.Equal(
            2,
            data.GetProperty("pageSize").GetInt32());

        Assert.True(
            data.GetProperty("items").GetArrayLength() <= 2);
    }

    [Fact]
    public async Task GetEmployees_FiltersByDepartment()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        await _factory.SeedEmployeesAsync();

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/employee?department=IT");

        // Assert
        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "John Doe",
            json);

        Assert.DoesNotContain(
            "Jane Smith",
            json);
    }

    [Fact]
    public async Task GetEmployees_SearchesEmployees()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        await _factory.SeedEmployeesAsync();

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/employee?search=John");

        // Assert
        response.EnsureSuccessStatusCode();

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "John Doe",
            json);

        Assert.DoesNotContain(
            "Jane Smith",
            json);
    }

    // ============================================================
    // GET /api/v1/employee/{id}
    // ============================================================

    [Fact]
    public async Task GetEmployeeById_ReturnsOk_WhenEmployeeExists()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        var employee =
            await _factory.SeedEmployeeAsync(
                "John",
                "Doe",
                "john@example.com",
                "IT");

        // Act
        var response =
            await client.GetAsync(
                $"/api/v1/employee/{employee.Id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "John Doe",
            json);

        Assert.Contains(
            "john@example.com",
            json);
    }

    [Fact]
    public async Task GetEmployeeById_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        // Act
        var response =
            await client.GetAsync(
                "/api/v1/employee/9999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Employee not found.",
            json);
    }

    // ============================================================
    // POST /api/v1/employee
    // ============================================================

    [Fact]
    public async Task CreateEmployee_ReturnsCreated_WhenRequestIsValid()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        var dto = new CreateEmployeeDto
        {
            FirstName = "David",
            LastName = "Miller",
            Email = "david@example.com",
            Department = "Finance"
        };

        // Act
        var response =
            await client.PostAsJsonAsync(
                "/api/v1/employee",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Employee created successfully.",
            json);

        Assert.Contains(
            "David Miller",
            json);
    }

    // ============================================================
    // PUT /api/v1/employee/{id}
    // ============================================================

    [Fact]
    public async Task UpdateEmployee_ReturnsOk_WhenEmployeeExists()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        var employee =
            await _factory.SeedEmployeeAsync(
                "John",
                "Doe",
                "john@example.com",
                "IT");

        var dto = new UpdateEmployeeDto
        {
            FirstName = "Jonathan",
            LastName = "Doe",
            Email = "jonathan@example.com",
            Department = "Management",
            IsActive = true
        };

        // Act
        var response =
            await client.PutAsJsonAsync(
                $"/api/v1/employee/{employee.Id}",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Employee updated successfully.",
            json);

        // Verify persistence through GET
        var getResponse =
            await client.GetAsync(
                $"/api/v1/employee/{employee.Id}");

        getResponse.EnsureSuccessStatusCode();

        var getJson =
            await getResponse.Content.ReadAsStringAsync();

        Assert.Contains(
            "Jonathan Doe",
            getJson);

        Assert.Contains(
            "jonathan@example.com",
            getJson);

        Assert.Contains(
            "Management",
            getJson);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        var dto = new UpdateEmployeeDto
        {
            FirstName = "Jonathan",
            LastName = "Doe",
            Email = "jonathan@example.com",
            Department = "Management",
            IsActive = true
        };

        // Act
        var response =
            await client.PutAsJsonAsync(
                "/api/v1/employee/9999",
                dto);

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Employee not found.",
            json);
    }

    // ============================================================
    // DELETE /api/v1/employee/{id}
    // ============================================================

    [Fact]
    public async Task DeleteEmployee_ReturnsOk_WhenEmployeeExists()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        var employee =
            await _factory.SeedEmployeeAsync(
                "John",
                "Doe",
                "john@example.com",
                "IT");

        // Act
        var response =
            await client.DeleteAsync(
                $"/api/v1/employee/{employee.Id}");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Employee deleted successfully.",
            json);

        // Verify employee is actually gone
        var getResponse =
            await client.GetAsync(
                $"/api/v1/employee/{employee.Id}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            getResponse.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var client = CreateClient();

        _factory.AuthenticateClient(client);

        await _factory.ResetDatabaseAsync();

        // Act
        var response =
            await client.DeleteAsync(
                "/api/v1/employee/9999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Employee not found.",
            json);
    }
}
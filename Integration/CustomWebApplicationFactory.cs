using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Options;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Microsoft.Data.Sqlite;

using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using TrainingManagement.Api.Data;
using TrainingManagement.Api.Models;

namespace TrainingManagement.Api.Tests.Integration;

public class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(
    IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // ============================================================
            // REMOVE PRODUCTION EF CORE DATABASE REGISTRATIONS
            // ============================================================

            services.RemoveAll<ApplicationDbContext>();

            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            services.RemoveAll<DbContextOptions>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<ApplicationDbContext>>();


            // ============================================================
            // CREATE SQLITE IN-MEMORY DATABASE
            // ============================================================

            _connection = new SqliteConnection(
                "Data Source=:memory:");

            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });


            // ============================================================
            // REPLACE JWT AUTHENTICATION
            // ============================================================

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                    TestAuthHandler.AuthenticationScheme;

                options.DefaultChallengeScheme =
                    TestAuthHandler.AuthenticationScheme;
            })
            .AddScheme<
                AuthenticationSchemeOptions,
                TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme,
                    _ =>
                    {
                    });
        });
    }

    // ================================================================
    // AUTHENTICATE TEST CLIENT
    // ================================================================

    public void AuthenticateClient(HttpClient client)
    {
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                TestAuthHandler.AuthenticationScheme);
    }

    // ================================================================
    // RESET DATABASE
    // ================================================================

    public async Task ResetDatabaseAsync()
    {
        using var scope = Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        await context.Database.EnsureDeletedAsync();

        await context.Database.EnsureCreatedAsync();
    }

    // ================================================================
    // SEED MULTIPLE EMPLOYEES
    // ================================================================

    public async Task SeedEmployeesAsync()
    {
        using var scope = Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var employees = new List<Employee>
        {
            new Employee
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Department = "IT",
                IsActive = true
            },

            new Employee
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Department = "HR",
                IsActive = true
            },

            new Employee
            {
                FirstName = "Michael",
                LastName = "Brown",
                Email = "michael@example.com",
                Department = "IT",
                IsActive = false
            }
        };

        await context.Employees.AddRangeAsync(employees);

        await context.SaveChangesAsync();
    }

    // ================================================================
    // SEED SINGLE EMPLOYEE
    // ================================================================

    public async Task<Employee> SeedEmployeeAsync(
        string firstName,
        string lastName,
        string email,
        string department)
    {
        using var scope = Services.CreateScope();

        var context =
            scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

        var employee = new Employee
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            Department = department,
            IsActive = true
        };

        await context.Employees.AddAsync(employee);

        await context.SaveChangesAsync();

        return employee;
    }

    // ================================================================
    // DISPOSE SQLITE CONNECTION
    // ================================================================

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }
}


// ====================================================================
// TEST AUTHENTICATION HANDLER
// ====================================================================

public class TestAuthHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string AuthenticationScheme =
        "TestAuthentication";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        // ------------------------------------------------------------
        // No Authorization header
        // ------------------------------------------------------------

        if (!Request.Headers.ContainsKey("Authorization"))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        // ------------------------------------------------------------
        // Fake authenticated user
        // ------------------------------------------------------------

        var claims = new[]
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                "test-user"),

            new Claim(
                ClaimTypes.Name,
                "Test User"),

            new Claim(
                ClaimTypes.Role,
                "Admin")
        };

        var identity =
            new ClaimsIdentity(
                claims,
                AuthenticationScheme);

        var principal =
            new ClaimsPrincipal(identity);

        var ticket =
            new AuthenticationTicket(
                principal,
                AuthenticationScheme);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}
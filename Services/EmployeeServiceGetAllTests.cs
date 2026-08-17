using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TrainingManagement.Api.Data;
using TrainingManagement.Api.DTOs;
using TrainingManagement.Api.Repositories;
using TrainingManagement.Api.Services;

namespace TrainingManagement.Api.Tests.Services;

public class EmployeeServiceGetAllTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly EmployeeRepository _repository;
    private readonly EmployeeService _service;

    public EmployeeServiceGetAllTests()
    {
        _connection = new SqliteConnection(
            "DataSource=:memory:");

        _connection.Open();

        var options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlite(_connection)
                .Options;

        _context = new ApplicationDbContext(options);

        _context.Database.EnsureCreated();

        _repository = new EmployeeRepository(_context);

        var logger =
            LoggerFactory
                .Create(builder => { })
                .CreateLogger<EmployeeService>();

        _service = new EmployeeService(
            _repository,
            logger);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEmployees()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            3,
            result.TotalRecords);

        Assert.Equal(
            3,
            result.Items.Count);

        Assert.Equal(
            1,
            result.Page);

        Assert.Equal(
            10,
            result.PageSize);
    }

    private async Task SeedEmployees()
    {
        await _context.Employees.AddRangeAsync(
            new Models.Employee
            {
                FirstName = "John",
                LastName = "Doe",
                Email = "john@example.com",
                Department = "IT",
                IsActive = true
            },
            new Models.Employee
            {
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane@example.com",
                Department = "HR",
                IsActive = true
            },
            new Models.Employee
            {
                FirstName = "Michael",
                LastName = "Brown",
                Email = "michael@example.com",
                Department = "IT",
                IsActive = false
            });

        await _context.SaveChangesAsync();
    }

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }

    [Fact]
    public async Task GetAllAsync_CalculatesPaginationMetadata()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            Page = 1,
            PageSize = 2
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(3, result.TotalRecords);

        Assert.Equal(2, result.TotalPages);

        Assert.False(result.HasPreviousPage);

        Assert.True(result.HasNextPage);

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsSecondPage()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            Page = 2,
            PageSize = 2
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(3, result.TotalRecords);

        Assert.Equal(2, result.TotalPages);

        Assert.True(result.HasPreviousPage);

        Assert.False(result.HasNextPage);

        Assert.Single(result.Items);

        Assert.Equal(
            "Michael Brown",
            result.Items[0].FullName);
    }

    [Fact]
    public async Task GetAllAsync_FiltersBySearch()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            Search = "Jane",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(1, result.TotalRecords);

        var employee =
            Assert.Single(result.Items);

        Assert.Equal(
            "Jane Smith",
            employee.FullName);
    }

    [Fact]
    public async Task GetAllAsync_FiltersByDepartment()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            Department = "IT",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(2, result.TotalRecords);

        Assert.Equal(
            2,
            result.Items.Count);

        Assert.All(
            result.Items,
            employee =>
                Assert.Equal(
                    "IT",
                    employee.Department));
    }

    [Fact]
    public async Task GetAllAsync_FiltersByActiveStatus()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            IsActive = true,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(2, result.TotalRecords);

        Assert.Equal(2, result.Items.Count);
    }

    [Fact]
    public async Task GetAllAsync_FiltersInactiveEmployees()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            IsActive = false,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(1, result.TotalRecords);

        var employee =
            Assert.Single(result.Items);

        Assert.Equal(
            "Michael Brown",
            employee.FullName);
    }

    [Fact]
    public async Task GetAllAsync_SortsByFirstNameAscending()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            Sort = "firstname",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(
            "Jane Smith",
            result.Items[0].FullName);

        Assert.Equal(
            "John Doe",
            result.Items[1].FullName);

        Assert.Equal(
            "Michael Brown",
            result.Items[2].FullName);
    }

    [Fact]
    public async Task GetAllAsync_SortsByFirstNameDescending()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            Sort = "-firstname",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(
            "Michael Brown",
            result.Items[0].FullName);

        Assert.Equal(
            "John Doe",
            result.Items[1].FullName);

        Assert.Equal(
            "Jane Smith",
            result.Items[2].FullName);
    }

    [Fact]
    public async Task GetAllAsync_UsesIdSort_WhenSortIsNotSpecified()
    {
        // Arrange
        await SeedEmployees();

        var query = new EmployeeQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _service.GetAllAsync(query);

        // Assert
        Assert.Equal(
            "John Doe",
            result.Items[0].FullName);

        Assert.Equal(
            "Jane Smith",
            result.Items[1].FullName);

        Assert.Equal(
            "Michael Brown",
            result.Items[2].FullName);
    }


}
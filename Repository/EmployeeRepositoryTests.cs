using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TrainingManagement.Api.Data;
using TrainingManagement.Api.DTOs;
using TrainingManagement.Api.Models;
using TrainingManagement.Api.Repositories;

namespace TrainingManagement.Api.Tests.Repository;

public class EmployeeRepositoryTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;
    private readonly EmployeeRepository _repository;

    public EmployeeRepositoryTests()
    {
        // Create an SQLite in-memory database.
        _connection = new SqliteConnection(
            "DataSource=:memory:");

        _connection.Open();

        // Configure EF Core to use the SQLite in-memory database.
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);

        // Create the database schema.
        _context.Database.EnsureCreated();

        _repository = new EmployeeRepository(_context);
    }

    // ============================================================
    // Add 3 Emmployees, then return them all(Basic Test)
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ReturnsAllEmployees()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        Assert.Equal(3, result.Count);
    }


    // ============================================================
    // Test ordering
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ReturnsEmployeesOrderedById()
    {
        // Arrange
        var employees = new List<Employee>
    {
        new Employee
        {
            Id = 3,
            FirstName = "Michael",
            LastName = "Brown",
            Email = "michael@example.com",
            Department = "IT",
            IsActive = true
        },

        new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Department = "IT",
            IsActive = true
        },

        new Employee
        {
            Id = 2,
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane@example.com",
            Department = "HR",
            IsActive = true
        }
    };

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }

    // ============================================================
    // Test search by first name
    // ============================================================
    [Fact]
    public async Task GetAllAsync_FiltersByFirstNameSearch()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Search = "John",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        var employee = Assert.Single(result);

        Assert.Equal("John", employee.FirstName);
    }

    // ============================================================
    // Test search by last name
    // ============================================================

    [Fact]
    public async Task GetAllAsync_FiltersByLastNameSearch()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Search = "Smith",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        var employee = Assert.Single(result);

        Assert.Equal("Smith", employee.LastName);
    }

    // ============================================================
    // Test search by email
    // ============================================================
    [Fact]
    public async Task GetAllAsync_FiltersByEmailSearch()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Search = "michael.brown",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        var employee = Assert.Single(result);

        Assert.Equal(
            "michael.brown@example.com",
            employee.Email);
    }
    // ============================================================
    // Test department filtering
    // ============================================================
    [Fact]
    public async Task GetAllAsync_FiltersByDepartment()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Department = "IT",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            employee =>
                Assert.Equal(
                    "IT",
                    employee.Department));
    }
    // ============================================================
    // Test active-status filtering
    // ============================================================
    [Fact]
    public async Task GetAllAsync_FiltersActiveEmployees()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            IsActive = true,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            employee =>
                Assert.True(employee.IsActive));
    }
    // ============================================================
    // Test Inactive-status filtering
    // ============================================================
    [Fact]
    public async Task GetAllAsync_FiltersInactiveEmployees()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            IsActive = false,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        var employee = Assert.Single(result);

        Assert.False(employee.IsActive);

        Assert.Equal(
            3,
            employee.Id);
    }

    // ============================================================
    // Test combined filters
    // ============================================================

    [Fact]
    public async Task GetAllAsync_AppliesMultipleFilters()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Department = "IT",
            IsActive = true,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        var employee = Assert.Single(result);

        Assert.Equal("IT", employee.Department);
        Assert.True(employee.IsActive);
        Assert.Equal(1, employee.Id);
    }

    // ============================================================
    // Test page beyond available records
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ReturnsEmptyList_WhenPageIsBeyondAvailableRecords()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Page = 10,
            PageSize = 10
        };

        // Act
        var result = await _repository.GetAllAsync(query);

        // Assert
        Assert.Empty(result);
    }

    // ============================================================
    // GET QUERY
    // ============================================================

    [Fact]
    public async Task GetQuery_ReturnsAllEmployees()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository
            .GetQuery()
            .ToListAsync();

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Contains(
            result,
            employee => employee.FirstName == "John");

        Assert.Contains(
            result,
            employee => employee.FirstName == "Jane");

        Assert.Contains(
            result,
            employee => employee.FirstName == "Michael");
    }

    // ============================================================
    // COUNT
    // ============================================================

    [Fact]
    public async Task CountAsync_ReturnsTotalEmployees()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery();

        // Act
        var result =
            await _repository.CountAsync(query);

        // Assert
        Assert.Equal(3, result);
    }

    [Fact]
    public async Task CountAsync_FiltersBySearch()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Search = "John"
        };

        // Act
        var result =
            await _repository.CountAsync(query);

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task CountAsync_FiltersByDepartment()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Department = "IT"
        };

        // Act
        var result =
            await _repository.CountAsync(query);

        // Assert
        Assert.Equal(2, result);
    }

    [Fact]
    public async Task CountAsync_FiltersByActiveStatus()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            IsActive = true
        };

        // Act
        var result =
            await _repository.CountAsync(query);

        // Assert
        Assert.Equal(2, result);
    }

    // ============================================================
    // GET ALL
    // ============================================================

    [Fact]
    public async Task GetAllAsync_ReturnsEmployees()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _repository.GetAllAsync(query);

        // Assert
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetAllAsync_AppliesSearchFilter()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Search = "Jane",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _repository.GetAllAsync(query);

        // Assert
        var employee = Assert.Single(result);

        Assert.Equal(
            "Jane",
            employee.FirstName);
    }

    [Fact]
    public async Task GetAllAsync_AppliesDepartmentFilter()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Department = "IT",
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _repository.GetAllAsync(query);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            employee =>
                Assert.Equal(
                    "IT",
                    employee.Department));
    }

    [Fact]
    public async Task GetAllAsync_AppliesActiveStatusFilter()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            IsActive = true,
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _repository.GetAllAsync(query);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.All(
            result,
            employee =>
                Assert.True(employee.IsActive));
    }

    [Fact]
    public async Task GetAllAsync_AppliesPagination()
    {
        // Arrange
        var employees = CreateEmployees();

        await _context.Employees.AddRangeAsync(employees);
        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Page = 2,
            PageSize = 1
        };

        // Act
        var result =
            await _repository.GetAllAsync(query);

        // Assert
        var employee = Assert.Single(result);

        Assert.Equal(
            2,
            employee.Id);
    }

    [Fact]
    public async Task GetAllAsync_OrdersEmployeesById()
    {
        // Arrange
        var employees = CreateEmployees();

        // Add in a deliberately different order.
        await _context.Employees.AddRangeAsync(
            employees[2],
            employees[0],
            employees[1]);

        await _context.SaveChangesAsync();

        var query = new EmployeeQuery
        {
            Page = 1,
            PageSize = 10
        };

        // Act
        var result =
            await _repository.GetAllAsync(query);

        // Assert
        Assert.Equal(3, result.Count);

        Assert.Equal(1, result[0].Id);
        Assert.Equal(2, result[1].Id);
        Assert.Equal(3, result[2].Id);
    }

    // ============================================================
    // GET BY ID
    // ============================================================

    [Fact]
    public async Task GetByIdAsync_ReturnsEmployee_WhenEmployeeExists()
    {
        // Arrange
        var employee = new Employee
        {
            Id = 1,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Department = "IT",
            IsActive = true
        };

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        // Act
        var result =
            await _repository.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            1,
            result.Id);

        Assert.Equal(
            "John",
            result.FirstName);

        Assert.Equal(
            "Doe",
            result.LastName);

        Assert.Equal(
            "john.doe@example.com",
            result.Email);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEmployeeDoesNotExist()
    {
        // Act
        var result =
            await _repository.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    // ============================================================
    // ADD
    // ============================================================

    [Fact]
    public async Task AddAsync_AddsEmployeeToDatabase()
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = "Alice",
            LastName = "Brown",
            Email = "alice.brown@example.com",
            Department = "Finance",
            IsActive = true
        };

        // Act
        await _repository.AddAsync(employee);
        await _repository.SaveChangesAsync();

        // Assert
        var savedEmployee =
            await _context.Employees
                .SingleAsync(e =>
                    e.Email == "alice.brown@example.com");

        Assert.NotEqual(0, savedEmployee.Id);

        Assert.Equal(
            "Alice",
            savedEmployee.FirstName);

        Assert.Equal(
            "Brown",
            savedEmployee.LastName);

        Assert.Equal(
            "Finance",
            savedEmployee.Department);

        Assert.True(
            savedEmployee.IsActive);
    }

    // ============================================================
    // UPDATE
    // ============================================================

    [Fact]
    public async Task UpdateAsync_UpdatesEmployeeInDatabase()
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Department = "IT",
            IsActive = true
        };

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        employee.FirstName = "John Updated";
        employee.Department = "Management";
        employee.IsActive = false;

        // Act
        await _repository.UpdateAsync(employee);
        await _repository.SaveChangesAsync();

        // Clear tracked entities so we verify
        // what is actually stored in the database.
        _context.ChangeTracker.Clear();

        var updatedEmployee =
            await _context.Employees
                .SingleAsync(e =>
                    e.Id == employee.Id);

        // Assert
        Assert.Equal(
            "John Updated",
            updatedEmployee.FirstName);

        Assert.Equal(
            "Management",
            updatedEmployee.Department);

        Assert.False(
            updatedEmployee.IsActive);
    }

    // ============================================================
    // DELETE
    // ============================================================

    [Fact]
    public async Task DeleteAsync_RemovesEmployeeFromDatabase()
    {
        // Arrange
        var employee = new Employee
        {
            FirstName = "Delete",
            LastName = "Me",
            Email = "delete.me@example.com",
            Department = "IT",
            IsActive = true
        };

        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();

        var employeeId = employee.Id;

        // Act
        await _repository.DeleteAsync(employee);
        await _repository.SaveChangesAsync();

        // Assert
        var deletedEmployee =
            await _context.Employees
                .FindAsync(employeeId);

        Assert.Null(deletedEmployee);
    }

    // ============================================================
    // TEST DATA
    // ============================================================

    private static List<Employee> CreateEmployees()
    {
        return new List<Employee>
        {
            new Employee
            {
                Id = 1,
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@example.com",
                Department = "IT",
                IsActive = true
            },

            new Employee
            {
                Id = 2,
                FirstName = "Jane",
                LastName = "Smith",
                Email = "jane.smith@example.com",
                Department = "HR",
                IsActive = true
            },

            new Employee
            {
                Id = 3,
                FirstName = "Michael",
                LastName = "Brown",
                Email = "michael.brown@example.com",
                Department = "IT",
                IsActive = false
            }
        };
    }

    // ============================================================
    // CLEANUP
    // ============================================================

    public void Dispose()
    {
        _context.Dispose();
        _connection.Dispose();
    }
}
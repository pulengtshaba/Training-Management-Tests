using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Extensions.Logging;
using Moq;
using TrainingManagement.Api.DTOs;
using TrainingManagement.Api.Interfaces;
using TrainingManagement.Api.Models;
using TrainingManagement.Api.Services;

namespace TrainingManagement.Api.Tests.Services;

public class EmployeeServiceTests
{
    private readonly Mock<IEmployeeRepository> _repositoryMock;
    private readonly Mock<ILogger<EmployeeService>> _loggerMock;
    private readonly EmployeeService _service;

    public EmployeeServiceTests()
    {
        _repositoryMock = new Mock<IEmployeeRepository>();
        _loggerMock = new Mock<ILogger<EmployeeService>>();

        _service = new EmployeeService(
            _repositoryMock.Object,
            _loggerMock.Object);
    }

    // ============================================================
    // GetByIdAsync
    // ============================================================

    [Fact]
    public async Task GetByIdAsync_ReturnsEmployeeDto_WhenEmployeeExists()
    {
        // Arrange
        var employeeId = 1;

        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            Department = "IT",
            IsActive = true
        };

        _repositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        // Act
        var result =
            await _service.GetByIdAsync(employeeId);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            employeeId,
            result.Id);

        Assert.Equal(
            "John Doe",
            result.FullName);

        Assert.Equal(
            "john.doe@example.com",
            result.Email);

        Assert.Equal(
            "IT",
            result.Department);

        _repositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(employeeId),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var employeeId = 999;

        _repositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(employeeId))
            .ReturnsAsync((Employee?)null);

        // Act
        var result =
            await _service.GetByIdAsync(employeeId);

        // Assert
        Assert.Null(result);

        _repositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(employeeId),
            Times.Once);
    }

    // ============================================================
    // CreateAsync
    // ============================================================

    [Fact]
    public async Task CreateAsync_CreatesEmployeeAndReturnsDto()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            Department = "HR"
        };

        _repositoryMock
            .Setup(repository =>
                repository.AddAsync(It.IsAny<Employee>()))
            .Callback<Employee>(employee =>
            {
                // Simulate database-generated ID
                employee.Id = 10;
            })
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(repository =>
                repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);

        Assert.Equal(
            10,
            result.Id);

        Assert.Equal(
            "Jane Smith",
            result.FullName);

        Assert.Equal(
            "jane.smith@example.com",
            result.Email);

        Assert.Equal(
            "HR",
            result.Department);

        _repositoryMock.Verify(
            repository =>
                repository.AddAsync(
                    It.Is<Employee>(employee =>
                        employee.FirstName == "Jane" &&
                        employee.LastName == "Smith" &&
                        employee.Email == "jane.smith@example.com" &&
                        employee.Department == "HR" &&
                        employee.IsActive)),
            Times.Once);

        _repositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(),
            Times.Once);
    }

    // ============================================================
    // UpdateAsync
    // ============================================================

    [Fact]
    public async Task UpdateAsync_ReturnsTrueAndUpdatesEmployee_WhenEmployeeExists()
    {
        // Arrange
        var employeeId = 1;

        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Department = "IT",
            IsActive = true
        };

        var dto = new UpdateEmployeeDto
        {
            FirstName = "John Updated",
            LastName = "Doe Updated",
            Email = "john.updated@example.com",
            Department = "Management",
            IsActive = false
        };

        _repositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        _repositoryMock
            .Setup(repository =>
                repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _service.UpdateAsync(
                employeeId,
                dto);

        // Assert
        Assert.True(result);

        Assert.Equal(
            "John Updated",
            employee.FirstName);

        Assert.Equal(
            "Doe Updated",
            employee.LastName);

        Assert.Equal(
            "john.updated@example.com",
            employee.Email);

        Assert.Equal(
            "Management",
            employee.Department);

        Assert.False(
            employee.IsActive);

        _repositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(employeeId),
            Times.Once);

        _repositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsFalse_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var employeeId = 999;

        var dto = new UpdateEmployeeDto
        {
            FirstName = "Updated",
            LastName = "Employee",
            Email = "updated@example.com",
            Department = "IT",
            IsActive = true
        };

        _repositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(employeeId))
            .ReturnsAsync((Employee?)null);

        // Act
        var result =
            await _service.UpdateAsync(
                employeeId,
                dto);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(employeeId),
            Times.Once);

        // SaveChanges must not be called
        // because the employee does not exist.
        _repositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(),
            Times.Never);
    }

    // ============================================================
    // DeleteAsync
    // ============================================================

    [Fact]
    public async Task DeleteAsync_ReturnsTrue_WhenEmployeeExists()
    {
        // Arrange
        var employeeId = 1;

        var employee = new Employee
        {
            Id = employeeId,
            FirstName = "John",
            LastName = "Doe",
            Email = "john@example.com",
            Department = "IT",
            IsActive = true
        };

        _repositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        _repositoryMock
            .Setup(repository =>
                repository.DeleteAsync(employee))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(repository =>
                repository.SaveChangesAsync())
            .Returns(Task.CompletedTask);

        // Act
        var result =
            await _service.DeleteAsync(employeeId);

        // Assert
        Assert.True(result);

        _repositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(employeeId),
            Times.Once);

        _repositoryMock.Verify(
            repository =>
                repository.DeleteAsync(employee),
            Times.Once);

        _repositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var employeeId = 999;

        _repositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(employeeId))
            .ReturnsAsync((Employee?)null);

        // Act
        var result =
            await _service.DeleteAsync(employeeId);

        // Assert
        Assert.False(result);

        _repositoryMock.Verify(
            repository =>
                repository.GetByIdAsync(employeeId),
            Times.Once);

        // Delete must not be called.
        _repositoryMock.Verify(
            repository =>
                repository.DeleteAsync(
                    It.IsAny<Employee>()),
            Times.Never);

        // SaveChanges must not be called.
        _repositoryMock.Verify(
            repository =>
                repository.SaveChangesAsync(),
            Times.Never);
    }
}

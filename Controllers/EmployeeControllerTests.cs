using Microsoft.AspNetCore.Mvc;
using Moq;
using TrainingManagement.Api.Common;
using TrainingManagement.Api.Controllers;
using TrainingManagement.Api.DTOs;
using TrainingManagement.Api.Interfaces;
using TrainingManagement.Api.Models.Common;

namespace TrainingManagement.Api.Tests.Controllers;

public class EmployeeControllerTests
{
    private readonly Mock<IEmployeeService> _employeeServiceMock;
    private readonly EmployeeController _controller;

    public EmployeeControllerTests()
    {
        _employeeServiceMock = new Mock<IEmployeeService>();

        _controller = new EmployeeController(
            _employeeServiceMock.Object);
    }

    // ============================================================
    // GET: /api/v1/employee
    // ============================================================

    [Fact]
    public async Task GetEmployees_ReturnsOk_WithEmployees()
    {
        // Arrange
        var query = new EmployeeQuery();

        var employees = new PagedResult<EmployeeDto>
        {
            Items = new List<EmployeeDto>
            {
                new EmployeeDto
                {
                    Id = 1,
                    FullName = "John Doe",
                    Email = "user@address.com",
                    Department = "HR"
                }
            },
            TotalRecords = 1
        };

        _employeeServiceMock
            .Setup(service => service.GetAllAsync(query))
            .ReturnsAsync(employees);

        // Act
        var result = await _controller.GetEmployees(query);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<ApiResponse<PagedResult<EmployeeDto>>>(
                okResult.Value);

        Assert.True(response.Success);

        Assert.Equal(
            "Employees retrieved successfully.",
            response.Message);

        Assert.NotNull(response.Data);

        Assert.Single(response.Data.Items);

        Assert.Equal(
            1,
            response.Data.Items.First().Id);

        // Verify service interaction
        _employeeServiceMock.Verify(
            service => service.GetAllAsync(query),
            Times.Once);
    }

    // ============================================================
    // GET: /api/v1/employee/{id}
    // ============================================================

    [Fact]
    public async Task GetEmployeeById_ReturnsOk_WhenEmployeeExists()
    {
        // Arrange
        var employeeId = 1;

        var employee = new EmployeeDto
        {
            Id = employeeId,
            FullName = "John Doe",
            Email = "user@address.com",
            Department = "HR"
        };

        _employeeServiceMock
            .Setup(service => service.GetByIdAsync(employeeId))
            .ReturnsAsync(employee);

        // Act
        var result =
            await _controller.GetEmployeeById(employeeId);

        // Assert
        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<ApiResponse<EmployeeDto>>(
                okResult.Value);

        Assert.True(response.Success);

        Assert.Equal(
            "Employee retrieved successfully.",
            response.Message);

        Assert.NotNull(response.Data);

        Assert.Equal(
            employeeId,
            response.Data.Id);


        Assert.Equal(
            "John Doe",
            response.Data.FullName);

        Assert.Equal(
            "user@address.com",
            response.Data.Email);

        // Verify service interaction
        _employeeServiceMock.Verify(
            service => service.GetByIdAsync(employeeId),
            Times.Once);
    }

    [Fact]
    public async Task GetEmployeeById_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var employeeId = 999;

        _employeeServiceMock
            .Setup(service => service.GetByIdAsync(employeeId))
            .ReturnsAsync((EmployeeDto?)null);

        // Act
        var result =
            await _controller.GetEmployeeById(employeeId);

        // Assert
        var notFoundResult =
            Assert.IsType<NotFoundObjectResult>(result);

        var response =
            Assert.IsType<ApiResponse<EmployeeDto>>(
                notFoundResult.Value);

        Assert.False(response.Success);

        Assert.Equal(
            "Employee not found.",
            response.Message);

        Assert.Null(response.Data);

        // Verify service interaction
        _employeeServiceMock.Verify(
            service => service.GetByIdAsync(employeeId),
            Times.Once);
    }

    // ============================================================
    // POST: /api/v1/employee
    // ============================================================

    [Fact]
    public async Task CreateEmployee_ReturnsCreatedAtAction_WhenEmployeeIsCreated()
    {
        // Arrange
        var dto = new CreateEmployeeDto
        {
            FirstName = "Jane",
            LastName = "Smith"
        };

        var employee = new EmployeeDto
        {
            Id = 10,
            FullName = "John Smith",
            Email = "user@address.com",
            Department = "HR"
        };

        _employeeServiceMock
            .Setup(service => service.CreateAsync(dto))
            .ReturnsAsync(employee);

        // Act
        var result =
            await _controller.CreateEmployee(dto);

        // Assert
        var createdResult =
            Assert.IsType<CreatedAtActionResult>(result);

        Assert.Equal(
            nameof(EmployeeController.GetEmployeeById),
            createdResult.ActionName);

        Assert.NotNull(createdResult.RouteValues);

        Assert.Equal(
            employee.Id,
            createdResult.RouteValues["id"]);

        var response =
            Assert.IsType<ApiResponse<EmployeeDto>>(
                createdResult.Value);

        Assert.True(response.Success);

        Assert.Equal(
            "Employee created successfully.",
            response.Message);

        Assert.NotNull(response.Data);

        Assert.Equal(
            employee.Id,
            response.Data.Id);

        Assert.Equal(
            "John Smith",
            response.Data.FullName);

        Assert.Equal(
            "user@address.com",
            response.Data.Email);

        // Verify service interaction
        _employeeServiceMock.Verify(
            service => service.CreateAsync(dto),
            Times.Once);
    }

    // ============================================================
    // PUT: /api/v1/employee/{id}
    // ============================================================

    [Fact]
    public async Task UpdateEmployee_ReturnsOk_WhenEmployeeIsUpdated()
    {
        // Arrange
        var employeeId = 1;

        var dto = new UpdateEmployeeDto
        {
            FirstName = "Updated",
            LastName = "Employee"
        };

        _employeeServiceMock
            .Setup(service =>
                service.UpdateAsync(employeeId, dto))
            .ReturnsAsync(true);

        // Act
        var result =
            await _controller.UpdateEmployee(
                employeeId,
                dto);

        // Assert
        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<ApiResponse<object>>(
                okResult.Value);

        Assert.True(response.Success);

        Assert.Equal(
            "Employee updated successfully.",
            response.Message);

        Assert.Null(response.Data);

        // Verify service interaction
        _employeeServiceMock.Verify(
            service =>
                service.UpdateAsync(employeeId, dto),
            Times.Once);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var employeeId = 999;

        var dto = new UpdateEmployeeDto
        {
            FirstName = "Updated",
            LastName = "Employee"
        };

        _employeeServiceMock
            .Setup(service =>
                service.UpdateAsync(employeeId, dto))
            .ReturnsAsync(false);

        // Act
        var result =
            await _controller.UpdateEmployee(
                employeeId,
                dto);

        // Assert
        var notFoundResult =
            Assert.IsType<NotFoundObjectResult>(result);

        var response =
            Assert.IsType<ApiResponse<object>>(
                notFoundResult.Value);

        Assert.False(response.Success);

        Assert.Equal(
            "Employee not found.",
            response.Message);

        Assert.Null(response.Data);

        // Verify service interaction
        _employeeServiceMock.Verify(
            service =>
                service.UpdateAsync(employeeId, dto),
            Times.Once);
    }

    // ============================================================
    // DELETE: /api/v1/employee/{id}
    // ============================================================

    [Fact]
    public async Task DeleteEmployee_ReturnsOk_WhenEmployeeIsDeleted()
    {
        // Arrange
        var employeeId = 1;

        _employeeServiceMock
            .Setup(service =>
                service.DeleteAsync(employeeId))
            .ReturnsAsync(true);

        // Act
        var result =
            await _controller.DeleteEmployee(employeeId);

        // Assert
        var okResult =
            Assert.IsType<OkObjectResult>(result);

        var response =
            Assert.IsType<ApiResponse<object>>(
                okResult.Value);

        Assert.True(response.Success);

        Assert.Equal(
            "Employee deleted successfully.",
            response.Message);

        Assert.Null(response.Data);

        // Verify service interaction
        _employeeServiceMock.Verify(
            service =>
                service.DeleteAsync(employeeId),
            Times.Once);
    }

    [Fact]
    public async Task DeleteEmployee_ReturnsNotFound_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var employeeId = 999;

        _employeeServiceMock
            .Setup(service =>
                service.DeleteAsync(employeeId))
            .ReturnsAsync(false);

        // Act
        var result =
            await _controller.DeleteEmployee(employeeId);

        // Assert
        var notFoundResult =
            Assert.IsType<NotFoundObjectResult>(result);

        var response =
            Assert.IsType<ApiResponse<object>>(
                notFoundResult.Value);

        Assert.False(response.Success);

        Assert.Equal(
            "Employee not found.",
            response.Message);

        Assert.Null(response.Data);

        // Verify service interaction
        _employeeServiceMock.Verify(
            service =>
                service.DeleteAsync(employeeId),
            Times.Once);
    }

    // ============================================================
    // GET: /api/v1/employee/test-error
    // ============================================================

    [Fact]
    public void TestError_ThrowsException()
    {
        // Act & Assert
        var exception =
            Assert.Throws<Exception>(
                () => _controller.TestError());

        Assert.Equal(
            "This is a test exception.",
            exception.Message);
    }
}
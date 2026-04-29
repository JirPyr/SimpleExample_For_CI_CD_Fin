using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using SimpleExample.API.Controllers;
using SimpleExample.Application.DTOs;
using SimpleExample.Application.Interfaces;
using Xunit;

namespace SimpleExample.Tests.API;

public class UsersControllerTests
{
    private readonly Mock<IUserService> _mockService;
    private readonly UsersController _controller;

    public UsersControllerTests()
    {
        _mockService = new Mock<IUserService>();
        _controller = new UsersController(_mockService.Object);
    }

    [Fact]
    public async Task GetAll_ShouldReturnOkWithUsers()
    {
        List<UserDto> users = new List<UserDto>
        {
            new UserDto { Id = Guid.NewGuid(), FirstName = "Matti", LastName = "Meikäläinen", Email = "matti@example.com" },
            new UserDto { Id = Guid.NewGuid(), FirstName = "Maija", LastName = "Virtanen", Email = "maija@example.com" }
        };

        _mockService.Setup(x => x.GetAllAsync()).ReturnsAsync(users);

        ActionResult<IEnumerable<UserDto>> result = await _controller.GetAll();

        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        IEnumerable<UserDto> returnedUsers = okResult.Value.Should().BeAssignableTo<IEnumerable<UserDto>>().Subject;
        returnedUsers.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetById_WhenUserExists_ShouldReturnOk()
    {
        Guid userId = Guid.NewGuid();
        UserDto user = new UserDto
        {
            Id = userId,
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        _mockService.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync(user);

        ActionResult<UserDto> result = await _controller.GetById(userId);

        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        UserDto returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.Id.Should().Be(userId);
    }

    [Fact]
    public async Task GetById_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        Guid userId = Guid.NewGuid();

        _mockService.Setup(x => x.GetByIdAsync(userId)).ReturnsAsync((UserDto?)null);

        ActionResult<UserDto> result = await _controller.GetById(userId);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Create_WithValidData_ShouldReturnCreated()
    {
        CreateUserDto createDto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        UserDto createdUser = new UserDto
        {
            Id = Guid.NewGuid(),
            FirstName = createDto.FirstName,
            LastName = createDto.LastName,
            Email = createDto.Email
        };

        _mockService.Setup(x => x.CreateAsync(createDto)).ReturnsAsync(createdUser);

        ActionResult<UserDto> result = await _controller.Create(createDto);

        CreatedAtActionResult createdResult = result.Result.Should().BeOfType<CreatedAtActionResult>().Subject;
        UserDto returnedUser = createdResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.Email.Should().Be("matti@example.com");
    }

    [Fact]
    public async Task Create_WithDuplicateEmail_ShouldReturnConflict()
    {
        CreateUserDto dto = new CreateUserDto
        {
            FirstName = "Matti",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        _mockService
            .Setup(x => x.CreateAsync(dto))
            .ThrowsAsync(new InvalidOperationException("Käyttäjä sähköpostilla matti@example.com on jo olemassa"));

        ActionResult<UserDto> result = await _controller.Create(dto);

        result.Result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_WithArgumentException_ShouldReturnBadRequest()
    {
        CreateUserDto dto = new CreateUserDto
        {
            FirstName = "Ma",
            LastName = "Meikäläinen",
            Email = "matti@example.com"
        };

        _mockService
            .Setup(x => x.CreateAsync(dto))
            .ThrowsAsync(new ArgumentException("Virheellinen etunimi"));

        ActionResult<UserDto> result = await _controller.Create(dto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenUserExists_ShouldReturnOk()
    {
        Guid userId = Guid.NewGuid();

        UpdateUserDto dto = new UpdateUserDto
        {
            FirstName = "Maija",
            LastName = "Virtanen",
            Email = "maija@example.com"
        };

        UserDto updatedUser = new UserDto
        {
            Id = userId,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email
        };

        _mockService.Setup(x => x.UpdateAsync(userId, dto)).ReturnsAsync(updatedUser);

        ActionResult<UserDto> result = await _controller.Update(userId, dto);

        OkObjectResult okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        UserDto returnedUser = okResult.Value.Should().BeOfType<UserDto>().Subject;
        returnedUser.FirstName.Should().Be("Maija");
    }

    [Fact]
    public async Task Update_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        Guid userId = Guid.NewGuid();

        UpdateUserDto dto = new UpdateUserDto
        {
            FirstName = "Maija",
            LastName = "Virtanen",
            Email = "maija@example.com"
        };

        _mockService.Setup(x => x.UpdateAsync(userId, dto)).ReturnsAsync((UserDto?)null);

        ActionResult<UserDto> result = await _controller.Update(userId, dto);

        result.Result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Update_WithArgumentException_ShouldReturnBadRequest()
    {
        Guid userId = Guid.NewGuid();

        UpdateUserDto dto = new UpdateUserDto
        {
            FirstName = "Ma",
            LastName = "Virtanen",
            Email = "maija@example.com"
        };

        _mockService
            .Setup(x => x.UpdateAsync(userId, dto))
            .ThrowsAsync(new ArgumentException("Virheellinen etunimi"));

        ActionResult<UserDto> result = await _controller.Update(userId, dto);

        result.Result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Delete_WhenUserExists_ShouldReturnNoContent()
    {
        Guid userId = Guid.NewGuid();

        _mockService.Setup(x => x.DeleteAsync(userId)).ReturnsAsync(true);

        ActionResult result = await _controller.Delete(userId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task Delete_WhenUserDoesNotExist_ShouldReturnNotFound()
    {
        Guid userId = Guid.NewGuid();

        _mockService.Setup(x => x.DeleteAsync(userId)).ReturnsAsync(false);

        ActionResult result = await _controller.Delete(userId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
}
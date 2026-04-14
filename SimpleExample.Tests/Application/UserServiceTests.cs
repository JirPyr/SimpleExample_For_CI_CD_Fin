using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using SimpleExample.Application.DTOs;
using SimpleExample.Application.Interfaces;
using SimpleExample.Application.Services;
using SimpleExample.Domain.Entities;
using Xunit;

namespace SimpleExample.Tests.Application;

public class UserServiceTests
{
	private readonly Mock<IUserRepository> _mockRepository;
	private readonly UserService _service;

	public UserServiceTests()
	{
		_mockRepository = new Mock<IUserRepository>();
		_service = new UserService(_mockRepository.Object);
	}

	[Fact]
	public async Task CreateAsync_WithValidData_ShouldCreateUser()
	{
		CreateUserDto dto = new CreateUserDto
		{
			FirstName = "Matti",
			LastName = "Meikäläinen",
			Email = "matti@example.com"
		};

		_mockRepository
			.Setup(x => x.AddAsync(It.IsAny<User>()))
			.ReturnsAsync((User user) =>
			{
				user.Id = Guid.NewGuid();
				user.CreatedAt = DateTime.UtcNow;
				user.UpdatedAt = DateTime.UtcNow;
				return user;
			});

		UserDto result = await _service.CreateAsync(dto);

		result.Should().NotBeNull();
		result.FirstName.Should().Be("Matti");
		result.LastName.Should().Be("Meikäläinen");
		result.Email.Should().Be("matti@example.com");

		_mockRepository.Verify(x => x.AddAsync(It.IsAny<User>()), Times.Once);
	}

	[Fact]
	public async Task GetByIdAsync_WhenUserExists_ShouldReturnUser()
	{
		Guid userId = Guid.NewGuid();
		User user = new User("Matti", "Meikäläinen", "matti@example.com")
		{
			Id = userId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_mockRepository
			.Setup(x => x.GetByIdAsync(userId))
			.ReturnsAsync(user);

		UserDto? result = await _service.GetByIdAsync(userId);

		result.Should().NotBeNull();
		result!.Id.Should().Be(userId);
		result.FirstName.Should().Be("Matti");
	}

	[Fact]
	public async Task GetByIdAsync_WhenUserDoesNotExist_ShouldReturnNull()
	{
		Guid userId = Guid.NewGuid();

		_mockRepository
			.Setup(x => x.GetByIdAsync(userId))
			.ReturnsAsync((User?)null);

		UserDto? result = await _service.GetByIdAsync(userId);

		result.Should().BeNull();
	}

	[Fact]
	public async Task GetAllAsync_ShouldReturnAllUsers()
	{
		List<User> users = new List<User>
		{
			new User("Matti", "Meikäläinen", "matti@example.com")
			{
				Id = Guid.NewGuid(),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			},
			new User("Maija", "Virtanen", "maija@example.com")
			{
				Id = Guid.NewGuid(),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			}
		};

		_mockRepository
			.Setup(x => x.GetAllAsync())
			.ReturnsAsync(users);

		IEnumerable<UserDto> result = await _service.GetAllAsync();

		result.Should().HaveCount(2);
	}

	[Fact]
	public async Task UpdateAsync_WhenUserExists_ShouldUpdateUser()
	{
		Guid userId = Guid.NewGuid();

		User existingUser = new User("Matti", "Meikäläinen", "matti@example.com")
		{
			Id = userId,
			CreatedAt = DateTime.UtcNow.AddDays(-1),
			UpdatedAt = DateTime.UtcNow.AddDays(-1)
		};

		UpdateUserDto dto = new UpdateUserDto
		{
			FirstName = "Maija",
			LastName = "Virtanen",
			Email = "maija@example.com"
		};

		_mockRepository
			.Setup(x => x.GetByIdAsync(userId))
			.ReturnsAsync(existingUser);

		_mockRepository
			.Setup(x => x.UpdateAsync(It.IsAny<User>()))
			.ReturnsAsync((User user) =>
			{
				user.UpdatedAt = DateTime.UtcNow;
				return user;
			});

		UserDto? result = await _service.UpdateAsync(userId, dto);

		result.Should().NotBeNull();
		result!.FirstName.Should().Be("Maija");
		result.LastName.Should().Be("Virtanen");
		result.Email.Should().Be("maija@example.com");
	}

	[Fact]
	public async Task UpdateAsync_WhenUserDoesNotExist_ShouldReturnNull()
	{
		Guid userId = Guid.NewGuid();

		UpdateUserDto dto = new UpdateUserDto
		{
			FirstName = "Maija",
			LastName = "Virtanen",
			Email = "maija@example.com"
		};

		_mockRepository
			.Setup(x => x.GetByIdAsync(userId))
			.ReturnsAsync((User?)null);

		UserDto? result = await _service.UpdateAsync(userId, dto);

		result.Should().BeNull();
	}

	[Fact]
	public async Task DeleteAsync_WhenUserExists_ShouldReturnTrue()
	{
		Guid userId = Guid.NewGuid();
		User user = new User("Matti", "Meikäläinen", "matti@example.com")
		{
			Id = userId,
			CreatedAt = DateTime.UtcNow,
			UpdatedAt = DateTime.UtcNow
		};

		_mockRepository
			.Setup(x => x.GetByIdAsync(userId))
			.ReturnsAsync(user);

		_mockRepository
			.Setup(x => x.DeleteAsync(userId))
			.Returns(Task.CompletedTask);

		bool result = await _service.DeleteAsync(userId);

		result.Should().BeTrue();
	}

	[Fact]
	public async Task DeleteAsync_WhenUserDoesNotExist_ShouldReturnFalse()
	{
		Guid userId = Guid.NewGuid();

		_mockRepository
			.Setup(x => x.GetByIdAsync(userId))
			.ReturnsAsync((User?)null);

		bool result = await _service.DeleteAsync(userId);

		result.Should().BeFalse();
	}
}
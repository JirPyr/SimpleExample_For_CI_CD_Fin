using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using SimpleExample.Domain.Entities;
using SimpleExample.Infrastructure.Data;
using SimpleExample.Infrastructure.Repositories;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SimpleExample.Tests.Infrastructure;

public class UserRepositoryIntegrationTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly UserRepository _repository;

    public UserRepositoryIntegrationTests()
    {
        DbContextOptions<ApplicationDbContext> options =
            new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

        _context = new ApplicationDbContext(options);
        _repository = new UserRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        User user = new User("Matti", "Meikäläinen", "matti@example.com");

        // Act
        User result = await _repository.AddAsync(user);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBe(Guid.Empty);

        User? savedUser = await _context.Users.FindAsync(result.Id);

        savedUser.Should().NotBeNull();
        savedUser!.FirstName.Should().Be("Matti");
        savedUser.LastName.Should().Be("Meikäläinen");
        savedUser.Email.Should().Be("matti@example.com");
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
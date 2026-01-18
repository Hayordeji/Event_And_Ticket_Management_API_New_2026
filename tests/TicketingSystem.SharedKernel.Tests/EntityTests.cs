using Xunit;

namespace TicketingSystem.SharedKernel.Tests;

public class EntityTests
{
    [Fact]
    public void Entity_ShouldGenerateUniqueId_WhenCreated()
    {
        // Arrange & Act
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Assert
        Assert.NotEqual(Guid.Empty, entity1.Id);
        Assert.NotEqual(Guid.Empty, entity2.Id);
        Assert.NotEqual(entity1.Id, entity2.Id);
    }

    [Fact]
    public void Entity_ShouldUseProvidedId_WhenCreatedWithId()
    {
        // Arrange
        var expectedId = Guid.NewGuid();

        // Act
        var entity = new TestEntity(expectedId);

        // Assert
        Assert.Equal(expectedId, entity.Id);
    }

    [Fact]
    public void Entity_ShouldBeEqual_WhenIdsMatch()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity1 = new TestEntity(id);
        var entity2 = new TestEntity(id);

        // Act & Assert
        Assert.Equal(entity1, entity2);
        Assert.True(entity1 == entity2);
    }

    [Fact]
    public void Entity_ShouldNotBeEqual_WhenIdsDiffer()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();

        // Act & Assert
        Assert.NotEqual(entity1, entity2);
        Assert.True(entity1 != entity2);
    }

    // Test entity for testing purposes
    private class TestEntity : Entity
    {
        public TestEntity() : base() { }
        public TestEntity(Guid id) : base(id) { }
    }
}
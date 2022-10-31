using FluentAssertions;
using MassTransit;
using Shop.Catalog.Service.Dtos;
using Shop.Catalog.Service.Entities;

namespace Shop.Catalog.UnitTest;

public class ItemsControllerTest
{

    private readonly Mock<IRepository<Item>> repositoryStub = new Mock<IRepository<Item>>();
    private readonly Mock<IPublishEndpoint> rabbitEvents = new Mock<IPublishEndpoint>();
    private readonly Random rand = new();

    [Fact]
    public async Task GetByIdAsync_WithUnexistingItem_ReturnsNotFound()
    {
        // Arrange        
        repositoryStub.Setup(repo => repo.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Item)null);

        var controller = new ItemsController(repositoryStub.Object, rabbitEvents.Object);

        // Act  
        var result = await controller.GetByIdAsync(Guid.NewGuid());

        // Assert        
        result.Result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetByIdAsync_WithExistingItem_ReturnsAnItem()
    {
        //Arrange
        var expectedItem = CreateItem();

        repositoryStub.Setup(repo => repo.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedItem);

        var controller = new ItemsController(repositoryStub.Object, rabbitEvents.Object);

        //Act
        var result = await controller.GetByIdAsync(Guid.NewGuid());

        //Assert
        result.Value.Should().BeEquivalentTo(expectedItem);
    }

    [Fact]
    public async Task GetAsync_WithExistingItem_ReturnsAllItems()
    {
        // Arrange
        var expectedItems = new[] { CreateItem(), CreateItem(), CreateItem() };

        repositoryStub.Setup(repo => repo.GetAllAsync()).ReturnsAsync(expectedItems);

        var controller = new ItemsController(repositoryStub.Object, rabbitEvents.Object);

        // Act
        var actualItems = await controller.GetAsync();

        // Assert        
        actualItems.Should().BeEquivalentTo(expectedItems);
    }

    [Fact]
    public async Task PostAsync_WithItemToCreate_ReturnsCreatedItem()
    {
        // Arrange
        var itemToCreate = new CreateItemDto("Nm", "Dpo", rand.Next(1000));

        var controller = new ItemsController(repositoryStub.Object, rabbitEvents.Object);

        // Act
        var result = await controller.PostAsync(itemToCreate);

        // Assert
        var createdItem = (result.Result as CreatedAtActionResult)!.Value as ItemDto;

        itemToCreate.Should().BeEquivalentTo(createdItem,
            options => options.ComparingByMembers<ItemDto>().ExcludingMissingMembers());

        createdItem!.Id.Should().NotBeEmpty();

        createdItem.CreatedDate.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(1000));
    }

    [Fact]
    public async Task PutAsync_WithExistingItem_ReturnsNoContent()
    {
        // Arrange        
        var existingItem = CreateItem();

        repositoryStub.Setup(repo => repo.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem);

        var itemId = existingItem.Id;

        var itemToUpdate = new UpdateItemDto("Name", "Description", existingItem.Price + 3);

        var controller = new ItemsController(repositoryStub.Object, rabbitEvents.Object);

        // Act
        var result = await controller.PutAsync(itemId, itemToUpdate);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task PutAsync_WithNoExistingItem_ReturnsNotFound()
    {
        // Arrange        
        repositoryStub.Setup(repo => repo.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Item)null);

        var notExistingItem = CreateItem();

        var itemId = notExistingItem.Id;

        var itemToUpdate = new UpdateItemDto("Name", "Description", notExistingItem.Price + 3);

        var controller = new ItemsController(repositoryStub.Object, rabbitEvents.Object);

        // Act
        var result = await controller.PutAsync(itemId, itemToUpdate);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task DeleteItemAsync_WithExistingItem_ReturnsNoContent()
    {
        // Arrange        
        var existingItem = CreateItem();

        repositoryStub.Setup(repo => repo.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync(existingItem);

        var controller = new ItemsController(repositoryStub.Object, rabbitEvents.Object);

        // Act
        var result = await controller.DeleteAsync(existingItem.Id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteItemAsync_WithNoExistingItem_ReturnsNotFound()
    {
        // Arrange

        var itemToDelete = CreateItem();

        repositoryStub.Setup(repo => repo.GetAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Item)null);

        var controller = new ItemsController(repositoryStub.Object, rabbitEvents.Object);

        // Act
        var result = await controller.DeleteAsync(itemToDelete.Id);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    private Item CreateItem()
    {
        return new()
        {
            Id = Guid.NewGuid(),
            Name = "Item",
            Price = rand.Next(1000),
            CreatedDate = DateTimeOffset.UtcNow
        };
    }
}
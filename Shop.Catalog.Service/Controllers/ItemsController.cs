using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Shop.Catalog.Service.Dtos;
using Shop.Catalog.Service.Entities;
using Shop.Common;
using Shop.Common.Contracts;

namespace Shop.Catalog.Service.Controllers;

[ApiController]
[Route("items")]
public class ItemsController : ControllerBase
{
    private readonly IRepository<Item> itemsRepository;

    private readonly IPublishEndpoint publishEndPoint;

    #region Constructors

    public ItemsController(IRepository<Item> repo, IPublishEndpoint _publishEndpoint)
    {
        this.itemsRepository = repo;
        this.publishEndPoint = _publishEndpoint;
    }

    #endregion

    [HttpGet]
    public async Task<IEnumerable<ItemDto>> GetAsync()
    {
        return (await itemsRepository.GetAllAsync())
                    .Select(item => item.AsDto());
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ItemDto>> GetByIdAsync(Guid id)
    {
        var item = await itemsRepository.GetAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        return item.AsDto();
    }

    [HttpPost]
    public async Task<ActionResult<ItemDto>> PostAsync(CreateItemDto createItemDto)
    {
        var item = new Item
        {
            Id = Guid.NewGuid(),
            Name = createItemDto.Name,
            Description = createItemDto.Description,
            Price = createItemDto.Price,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await itemsRepository.CreateAsync(item);
        await publishEndPoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));
        return CreatedAtAction(nameof(GetByIdAsync), new { id = item.Id }, item.AsDto());
    }

    // PUT /items/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> PutAsync(Guid id, UpdateItemDto updateItemDto)
    {
        var existingItem = await itemsRepository.GetAsync(id);

        if (existingItem == null)
        {
            return NotFound();
        }

        existingItem.Name = updateItemDto.Name;
        existingItem.Description = updateItemDto.Description;
        existingItem.Price = updateItemDto.Price;

        await itemsRepository.UpdateAsync(existingItem);

        await publishEndPoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

        return NoContent();
    }

    // DELETE /items/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var item = await itemsRepository.GetAsync(id);

        if (item == null)
        {
            return NotFound();
        }

        await itemsRepository.RemoveAsync(item.Id);

        await publishEndPoint.Publish(new CatalogItemDeleted(id));

        return NoContent();
    }

}

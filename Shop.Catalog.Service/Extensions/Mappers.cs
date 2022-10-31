using Shop.Catalog.Service.Dtos;
using Shop.Catalog.Service.Entities;

namespace Shop.Catalog.Service
{
    public static class Mappers
    {
        public static ItemDto AsDto(this Item item)
        {
            return new ItemDto(item.Id, item.Name, item.Description, item.Price, item.CreatedDate);
        }
    }
}
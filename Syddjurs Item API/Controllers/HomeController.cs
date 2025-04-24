using Microsoft.AspNetCore.Mvc;
using Syddjurs_Item_API.Data;
using Syddjurs_Item_API.Models;

namespace Syddjurs_Item_API.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }


        [HttpPost("uploaditem")]
        public async Task<IActionResult> UploadItem([FromBody] ItemFullDto itemDto)
        {
            if (itemDto == null)
                return BadRequest("No item data received.");

            Item item;

            if (itemDto.Id == 0)
            {
                // 🆕 Adding a new stamp
                item = CopyItemDtoToItem(itemDto, null);

                await _context.Items.AddAsync(item);
                await _context.SaveChangesAsync(); // ✅ Save here to get the generated Stamp.Id             
            }
            else
            {
                // 🔄 Updating an existing stamp
                var existingItem = await _context.Items.FindAsync(itemDto.Id);
                if (existingItem == null)
                    return NotFound($"Stamp with ID {itemDto.Id} not found.");

                item = CopyItemDtoToItem(itemDto, null);

                _context.Items.Update(item);                
            }

            await _context.SaveChangesAsync();
            return Ok();
        }



        [HttpPost("uploadItemCategory")]
        public async Task<IActionResult> UploadItemCategory([FromBody] ItemCategoryDto categoryDto)
        {
            if (categoryDto.Id == 0)
            {

                var category = new ItemCategory();
                category.Id = categoryDto.Id;
                category.Category = categoryDto.Category;

                await _context.Categories.AddAsync(category);
            }
            else
            {
                // 🔄 Updating an existing Item category
                var existingCategory = await _context.Categories.FindAsync(categoryDto.Id);
                if (existingCategory == null)
                    return NotFound($"Item with ID {categoryDto.Id} not found.");

                existingCategory.Category = categoryDto.Category;

                _context.Categories.Update(existingCategory);
            }

            await _context.SaveChangesAsync(); // ✅ Save here to get the generated Item category Id     
            return Ok();
        }

        [HttpGet("itemCategories")]
        public async Task<IActionResult> GetItemCategories()
        {
            var categoryList = _context.Categories.ToList();

            var returnList = new List<ItemCategoryDto>();

            foreach (var category in categoryList)
            {
                var dto = new ItemCategoryDto();
                dto.Id = category.Id;
                dto.Category = category.Category;

                returnList.Add(dto);
            }

            return Ok(returnList);
        }

        private Item CopyItemDtoToItem(ItemFullDto itemDto, Item item)
        {
            if (item == null)
            {
                item = new Item();
            }

            {
                item.Name = itemDto.Name;
                item.Number = itemDto.Number;
                item.Description = itemDto.Description;
                if (itemDto.Category != null)
                {
                    var itemCategory = new ItemCategory();
                    itemCategory.Id = itemDto.Category.Id;
                    itemCategory.Category = itemDto.Category.Category;
                    item.Categori = itemCategory;
                }

                item.Color = itemDto.Color;
                item.Sex = itemDto.Sex;
                item.Lendable = itemDto.Lendable;
                item.Size = itemDto.Size;
             
            }
           
            return item;
        }
    }


}


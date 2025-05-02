using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Syddjurs.Models;
using Syddjurs.Models;
using Syddjurs_Item_API.Data;
using Syddjurs_Item_API.Interfaces;
using Syddjurs_Item_API.Models;
using Syddjurs_Item_API.Services;
using System.Security.Claims;

namespace Syddjurs_Item_API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("[controller]")]
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUserContext _userContext;
        private readonly IEmailService _emailService;


        public HomeController(AppDbContext context, IEmailService emailService, IUserContext userContext)
        {
            _context = context;
            _emailService = emailService;
            _userContext = userContext;
        }


        [ServiceFilter(typeof(ResolveUserClaimsFilter))]
        [HttpPost("uploaditem")]
       
        public async Task<IActionResult> UploadItem([FromBody] ItemFullDto itemDto)
        {
            //var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            //var userEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            //var userName = User.Identity?.Name;

            var name = _userContext.UserName;
            var id = _userContext.UserId; 

            if (itemDto == null)
                return BadRequest("No item data received.");

            Item item;

            if (itemDto.Id == 0)
            {
                // 🆕 Adding a new stamp
                item = CopyItemDtoToItem(itemDto, null);

                await _context.Items.AddAsync(item);
                await _context.SaveChangesAsync(); // ✅ Save here to get the generated Stamp.Id             

                await _emailService.SendEmailAsync(
                toEmail: "johnpetersen1959@gmail.com",
                subject: "Test Email",
                message: "<h2>Hello from Syddjurs API!</h2>"
            );
            }
            else
            {
                // 🔄 Updating an existing stamp
                var existingItem = await _context.Items.FindAsync(itemDto.Id);
                if (existingItem == null)
                    return NotFound($"Stamp with ID {itemDto.Id} not found.");

                existingItem = CopyItemDtoToItem(itemDto, existingItem);

                _context.Items.Update(existingItem);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpGet("itemsforlist")]
        public async Task<IActionResult> GetItemsForList()
        {
            var itemList = _context.Items.ToList();

            var returnList = new List<ItemListDto>();

            foreach (var item in itemList)
            {
                var dto = new ItemListDto();
                dto.Id = item.Id;
                dto.Name = item.Name;
                dto.Lendable = item.Lendable;
                dto.Number = item.Number;   

                returnList.Add(dto);
            }

            return Ok(returnList);
        }


        [HttpGet("loansforlist")]
        public async Task<IActionResult> GetLoanForList(string userName)
        {
            var loanList = _context.Loans.ToList();

            var returnList = new List<LoanDto>();

            LoanDto dto = null;
            foreach (var item in loanList)
            {
                if (item.Lender == userName)
                {
                     dto = new LoanDto();
                    dto.Id = item.Id;
                    dto.Lender = item.Lender;
                    dto.LoanDate = item.LoanDate;
                    returnList.Add(dto);
                }                          
              
            }

            return Ok(returnList);
        }

        [HttpGet("loanitemlines")]
        public async Task<IActionResult> GetLoanItemLines(int loanId)
        {
            var returnList = new List<LoanItemLinesUploadDto>();

            // Materialize query to list before looping
            var loanLines = await _context.LoanItemLines
                                          .Where(p => p.LoanId == loanId)
                                          .ToListAsync();

            foreach (var loanLine in loanLines)
            {
                var dto = new LoanItemLinesUploadDto
                {
                    Id = loanLine.Id,
                    LoanId = loanLine.LoanId,
                    Note = loanLine.Note,
                    Number = loanLine.Number,
                    ItemId = loanLine.ItemId
                };

                var item = await _context.Items.FindAsync(loanLine.ItemId);
                if (item != null)
                {
                    dto.ItemName = item.Name;
                }

                returnList.Add(dto);
            }

            return Ok(returnList);
        }


        [HttpGet("itembyid")]
        public async Task<IActionResult> GetItemById(int id)
        {
            ItemFullDto itemDto = null;

            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                itemDto = new ItemFullDto();
                CopyItemToItemDto(itemDto, item);
            }

            return Ok(itemDto);
        }

        [HttpGet("itemdelete")]
        public async Task<IActionResult>DeletetItemById(int id)
        {
                      
            var item = await _context.Items.FindAsync(id);
            if (item != null)
            {
                 _context.Remove(item);
                await _context.SaveChangesAsync();
            }

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

        [HttpPost("uploadloan")]
        public async Task<IActionResult> UploadLoan([FromBody] LoanDto loanDto)
        {
            if (loanDto.Id == 0)
            {
                var loan = new Loan();
                loan.LoanDate = loanDto.LoanDate;
                loan.Lender = loanDto.Lender;                             
                await _context.Loans.AddAsync(loan);
                await _context.SaveChangesAsync();

                foreach (var loanItemLineDto in loanDto.LoanItemLines)
                {
                    var loanItemLine = new LoanItemLine();
                    loanItemLine.LoanId = loan.Id;
                    loanItemLine.ItemId = (int)loanItemLineDto.ItemId;
                    loanItemLine.Note = loanItemLineDto.Note;
                    loanItemLine.Number = (int)loanItemLineDto.Number;
                    var item = await _context.Items.FindAsync((int)loanItemLine.ItemId);

                    await AdjustNumberOnItem(item, loanItemLineDto.Number, false);
                    _context.LoanItemLines.Add(loanItemLine);   
                }

                await _context.SaveChangesAsync();
            }
            else
            {
                // 🔄 Updating an existing loan
                var existingLoan = await _context.Loans.FindAsync(loanDto.Id);
                if (existingLoan == null)
                    return NotFound($"Item with ID {loanDto.Id} not found.");

                existingLoan.LoanDate = loanDto.LoanDate;
                existingLoan.Lender = loanDto.Lender;
                _context.Loans.Update(existingLoan);
                await _context.SaveChangesAsync();

              
                var loanLines =  _context.LoanItemLines.Where(p => p.LoanId == existingLoan.Id).ToList();

                foreach (var loanLine in loanLines)
                {
                    var item = await _context.Items.FindAsync((int)loanLine.ItemId);
                    await AdjustNumberOnItem(item, loanLine.Number, true);
                    _context.Remove(loanLine);
                }

                await _context.SaveChangesAsync();


                foreach (var loanItemLineDto in loanDto.LoanItemLines)
                {
                    var loanItemLine = new LoanItemLine
                    {
                        LoanId = existingLoan.Id,
                        ItemId = (int)loanItemLineDto.ItemId,
                        Note = loanItemLineDto.Note,
                        Number = (int)loanItemLineDto.Number
                    };

                    var item = await _context.Items.FindAsync((int)loanItemLineDto.ItemId);
                    await AdjustNumberOnItem(item, loanItemLineDto.Number, false);
                    _context.LoanItemLines.Add(loanItemLine);
                }

                await _context.SaveChangesAsync();

            }

            await _context.SaveChangesAsync(); // ✅ Save here to get the generated Item category Id     
            return Ok();
        }

        private async Task<Item> AdjustNumberOnItem(Item item, int? loanItemNumber, bool addFlag)
        {
            // await using var separateContext = _contextFactory.CreateDbContext();
            //var item = await separateContext.Items.FindAsync(itemId);

           

            if (item == null)
                return null;

            if (addFlag)
                item.Number += loanItemNumber;
            else
                item.Number -= loanItemNumber;

            // await separateContext.SaveChangesAsync();
            await _context.SaveChangesAsync();
            return item;
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

        [HttpGet("categorybyid")]
        public async Task<IActionResult> GetCategoryById(int id)
        {
            ItemCategoryDto categoryDto = null;

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                categoryDto = new ItemCategoryDto();
                categoryDto.Id = id;
                categoryDto.Category = category.Category;   
            }

            return Ok(categoryDto);
        }

        [HttpGet("itemdcategorydelete")]
        public async Task<IActionResult> DeletetItemCategoryById(int id)
        {

            var category = await _context.Categories.FindAsync(id);
            if (category != null)
            {
                _context.Remove(category);
                await _context.SaveChangesAsync();
            }

            return Ok();
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
                item.CategoryId = itemDto.CategoryId;
                item.CategoryText = itemDto.CategoryText;
                item.Color = itemDto.Color;
                item.Sex = itemDto.Sex;
                item.Lendable = itemDto.Lendable;
                item.Size = itemDto.Size;

            }

            return item;
        }

        private Item CopyItemToItemDto(ItemFullDto itemDto, Item item)
        {


            itemDto.Id = item.Id;
            itemDto.Name = item.Name;
            itemDto.Description = item.Description;

            itemDto.Number = item.Number is null ? 0 : (int)item.Number;

             itemDto.Lendable = item.Lendable is null ? false : (bool)item.Lendable; ;


            itemDto.Description = item.Description;
            itemDto.CategoryId  = item.CategoryId;
            itemDto.CategoryText = item.CategoryText;

            itemDto.Color = item.Color ;
            itemDto.Sex = item.Sex;
          
            itemDto.Size = item.Size ;



            return item;
        }
    }


}


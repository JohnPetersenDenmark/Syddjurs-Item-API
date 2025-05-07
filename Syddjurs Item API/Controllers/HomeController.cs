using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Syddjurs.Models;
using Syddjurs.Models;
using Syddjurs_Item_API.Data;
using Syddjurs_Item_API.Interfaces;
using Syddjurs_Item_API.Models;
using Syddjurs_Item_API.Services;
using System.Data;
using System.Globalization;
using System.Security.Claims;
using System.Threading;

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
        private UserManager<ApplicationUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;


        public HomeController(AppDbContext context, IEmailService emailService, IUserContext userContext, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _emailService = emailService;
            _userContext = userContext;
            _userManager = userManager;
            _roleManager = roleManager; 
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
                await _context.SaveChangesAsync(); // ✅ Save here to get the generated Item id                                        
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

        [ServiceFilter(typeof(ResolveUserClaimsFilter))]
        [HttpGet("loansforlist")]
        public async Task<IActionResult> GetLoanForList(string userName)
        {                    
            var currentUserRoles = await _userManager.GetRolesAsync(_userContext.CurrentUser);

            var loanList = _context.Loans.ToList();

            var returnList = new List<LoanDto>();

            LoanDto dto = null;
            foreach (var item in loanList)
            {
                if (item.Lender == userName || currentUserRoles.Contains("Administrator") || currentUserRoles.Contains("Manager"))
                {
                    dto = new LoanDto();
                    dto.Id = item.Id;
                    dto.Lender = item.Lender;
                    dto.LoanDate = item.LoanDate;
                    returnList.Add(dto);
                }               
            }

            returnList = returnList.OrderBy(l => l.Lender).ToList();
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
        public async Task<IActionResult> DeletetItemById(int id)
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
            string emailBody = string.Empty;
            var userEmail = _userContext.CurrentUser.Email;

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


                emailBody = AddNewLoanEmailHeader(loan, emailBody);
                emailBody =  await AddLoanInfo(loan, emailBody);
              
                SendLoanInfoEmail(userEmail, emailBody);


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

                emailBody = AddEditedLoanEmailHeaderPart1(existingLoan, emailBody);
                emailBody = await AddLoanInfo(existingLoan, emailBody);

                var loanLines = _context.LoanItemLines.Where(p => p.LoanId == existingLoan.Id).ToList();

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

               // 
                emailBody =  AddEditedLoanEmailHeaderPart2(existingLoan, emailBody);
                emailBody = await AddLoanInfo(existingLoan, emailBody);
                SendLoanInfoEmail(userEmail, emailBody);
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

        [ServiceFilter(typeof(ResolveUserClaimsFilter))]
        [HttpGet("usersforlist")]
        public async Task<IActionResult> GetUsersForList()
        {
            if (_userContext.CurrentUser == null)
            {
                return BadRequest();
            }
            else
            {
                var currentUserRoles = await _userManager.GetRolesAsync(_userContext.CurrentUser);
                if (!currentUserRoles.Contains("Administrator"))
                {
                    return BadRequest();
                }
            }
                   
            var userList = await _userManager.Users.ToListAsync();
                              
                            
            var returnList = new List<UserDto>();

            foreach (var user in userList)
            {
                var dto = new UserDto();
                dto.Id = user.Id;
                dto.UserName = user.UserName;
                dto.Email = user.Email;

               

                var currentUserRoleNames = await _userManager.GetRolesAsync(user);
                foreach (var currentUserRoleName in currentUserRoleNames)
                {
                    if ( currentUserRoleName != "Administrator")
                    {
                        var role = await _roleManager.FindByNameAsync(currentUserRoleName);
                        var roleDto = new RoleDto();
                        roleDto.RoleName = currentUserRoleName;
                        roleDto.Id = role.Id;
                        roleDto.IsCheckBoxChecked = true;
                        dto.Roles.Add(roleDto);
                    }
                               
                }

                returnList.Add(dto);
            }

            returnList.OrderBy(u => u.UserName).ToList();
            return Ok(returnList);
        }


        [ServiceFilter(typeof(ResolveUserClaimsFilter))]
        [HttpGet("allroles")]
        public async Task<IActionResult> GetAllRoles()
        {
            if (_userContext.CurrentUser == null)
            {
                return BadRequest();
            }
            else
            {
                var currentUserRoles = await _userManager.GetRolesAsync(_userContext.CurrentUser);
                if (!currentUserRoles.Contains("Administrator"))
                {
                    return BadRequest();
                }
            }

           

            var roles = await _roleManager.Roles
                    .Select(role => new RoleDto
                        {
                            Id = role.Id,
                            RoleName = role.Name,
                            IsCheckBoxChecked = false
                        })
                    .ToListAsync();

            var roleToRemove = roles.FirstOrDefault(role => role.RoleName == "Administrator");

            roles.Remove(roleToRemove);

            return Ok(roles);
        }



        [ServiceFilter(typeof(ResolveUserClaimsFilter))]
        [HttpPost("uploaduser")]
        public async Task<IActionResult> UploadUser([FromBody] UserDto userDto)
        {

            if (_userContext.CurrentUser == null)
            {
                return BadRequest();
            }
            else
            {
                var currentUserRoles = await _userManager.GetRolesAsync(_userContext.CurrentUser);
                if (!currentUserRoles.Contains("Administrator"))
                {
                    return BadRequest();
                }
            }

            if ( String.IsNullOrWhiteSpace(userDto.Id))
            {
                return BadRequest();
            }

            var user = await _userManager.FindByIdAsync(userDto.Id);
            if (user == null)
            {
                return BadRequest();
            }

            var currentUserRoleNames = await _userManager.GetRolesAsync(user);
            foreach (var currentUserRoleName in currentUserRoleNames)
            {
                if (! (currentUserRoleName == "Administrator"))
                {
                    await _userManager.RemoveFromRoleAsync(user, currentUserRoleName);
                }
            }

            foreach (var dtoUserRole in userDto.Roles)
            {
                if (dtoUserRole.IsCheckBoxChecked) {
                    await _userManager.AddToRoleAsync(user, dtoUserRole.RoleName);
                }               
            }
                
           return Ok();
        }

        private Item CopyItemToItemDto(ItemFullDto itemDto, Item item)
        {


            itemDto.Id = item.Id;
            itemDto.Name = item.Name;
            itemDto.Description = item.Description;

            itemDto.Number = item.Number is null ? 0 : (int)item.Number;

            itemDto.Lendable = item.Lendable is null ? false : (bool)item.Lendable; ;


            itemDto.Description = item.Description;
            itemDto.CategoryId = item.CategoryId;
            itemDto.CategoryText = item.CategoryText;

            itemDto.Color = item.Color;
            itemDto.Sex = item.Sex;

            itemDto.Size = item.Size;



            return item;
        }

        private async Task SendNewLoanEmail(String SendTo, Loan loan)
        {
            var body = "<h2>Hej " + _userContext.CurrentUser.UserName + "</h2>";
            body += "<h2>Du har den " + loan.LoanDate + " lånt følgende:</h2>";

            body += AddLoanInfo(loan, body);

            await _emailService.SendEmailAsync(
                toEmail: SendTo,
                subject: "Nyt lån",
                 body);


        }

        private string AddNewLoanEmailHeader(Loan loan, string body)
        {
            body = "<h2>Hej " + _userContext.CurrentUser.UserName + "</h2>";
            body += "<h2>Du har den " + loan.LoanDate + " lånt følgende:</h2>";
            return body;
        }

        private string AddEditedLoanEmailHeaderPart1(Loan loan, string body)
        {
            body += "<h2>Dit lån fra den " + loan.LoanDate + "</h2>";
            return body;
        }

        private string AddEditedLoanEmailHeaderPart2(Loan loan, string body)
        {
            var now = DateTime.Now;
            var newLoanDate = now.ToString("d", new CultureInfo("da-DK"));
            body += "<h2>er ændret den " + newLoanDate + " til følgende:</h2>";
            return body;
        }

        private async Task SendLoanInfoEmail(String SendTo, string body)
        {

            await _emailService.SendEmailAsync(
                toEmail: SendTo,
                subject: "Vedrørende tøjlån",
                 body);
        }


        private async Task<string> AddLoanInfo(Loan loan, string emailBody)
        {
            var loanLines = _context.LoanItemLines.Where(p => p.LoanId == loan.Id).ToList();

            var body = emailBody;
            body += "<table width=\"50%\" border=\"1\" cellpadding=\"5\" cellspacing=\"0\" style=\"border-collapse: collapse;\">";

            body += "<tr>";
            body += "<th style=\"text-align: left;\" width=\"30%\">Antal</th>";
            body += "<th style=\"text-align: left;\" width=\"30%\">Beskrivelse</th>";
            body += "<th style=\"text-align: left;\" width=\"30%\">Notat</th>";
            body += "</tr>";

            foreach (var loanLine in loanLines)
            {
                var item = await _context.Items.FindAsync((int)loanLine.ItemId);

                body += "<tr>";
                body += "<td>" + loanLine.Number + "</td>";
                body += "<td>" + System.Net.WebUtility.HtmlEncode(item?.Name ?? "Ukendt") + "</td>";
                body += "<td>" + System.Net.WebUtility.HtmlEncode(loanLine.Note ?? "") + "</td>";
                body += "</tr>";
            }

            body += "</table>";
            return body;

        }
    }

}


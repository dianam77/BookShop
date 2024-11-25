using DataAccess.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DataAccess.Data.Persistence;
public class BookDbContextInitializer
{
    private readonly ILogger<BookDbContextInitializer> _logger;
    private readonly BookDbContext _context;
    private readonly UserManager<User> _userManager;

    public BookDbContextInitializer(ILogger<BookDbContextInitializer> logger, BookDbContext context, UserManager<User> userManager)
    {
        _logger = logger;
        _context = context;
        _userManager = userManager;
    }

    public void Initialize()
    {
        try
        {
            _context.Database.Migrate();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while initialising the database.");
            throw;
        }
    }

    public void Seed()
    {
        try
        {
            TrySeed();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while seeding the database.");
            throw;
        }
    }    

    public void TrySeed()
    {
        SeedUsers(_userManager).Wait();

        if (!_context.Authors.Any())
        {
            _context.Authors.AddRange(
                new Author { Name = " اریش ماریا رمارک" },
                new Author { Name = "کریستوفر پائولینی" }
            );
        }

        if (!_context.Books.Any())
        {
            _context.Books.AddRange(
                new Book
                {
                    Title = "به وقت عشق و مرگ",
                    Description = "اریش ماریا رمارک، نویسنده‌ی شهیر آلمانی در اثر داستانی خود کتاب به وقت عشق و مرگ، روشنایی زندگی و تاریکی مرگ را به مصاف هم درمی‌آورد. قهرمان و راوی این رمان تاریخی یک سرباز به نام ارنست گریبر است که پس از ماه‌ها خدمت در جبهه‌های جنگ حالا فرصت کوتاهی پیدا می‌کند تا به خانه و زندگی‌اش سری بزند. اما با ورود ارنست به شهر و دیار خود، مرد جوان با حقایقی رو‌به‌رو می‌شود که به‌سختی قابل باور کردن است.",
                    Price = 100000,
                    Img = "/uploads/test1.jpg",
                    Created = DateTime.Now,
                    IsAvail = true,
                    ShowHomePage = false,
                    AuthorId = 1
                },
                new Book
                {
                    Title = " الدست ",
                    Description = "داستان‌های فانتزی و هیجان انگیز...",
                    Price = 460000,
                    Img = "/uploads/test2.jpg",
                    Created = DateTime.Now,
                    IsAvail = true,
                    ShowHomePage = true,
                    AuthorId = 2
                },
                new Book
                {
                    Title = " من پیام رسان هستم ",
                    Description = "کتاب من پیام رسان هستم نوشته‌ی مارکوس زوساک، داستان زندگی راننده‌تاکسی جوان و ناامید و تنهایی است که ناخواسته وارد یک بازی پیچیده می‌شود و مأموریت‌هایی عجیب به او محول می‌گردد. اد کِندی پس‌ از اینکه به شکلی تصادفی مانع سرقت از یک بانک می‌شود و به شهرت می‌رسد، نامه‌ای ناشناس در صندوق پستی خود می‌یابد... گفتنی است که این کتاب برنده‌ی جایزه‌ی شورای کتاب کودک استرالیا شده است.",
                    Price = 58000,
                    Img = "/uploads/test3.jpg",
                    Created = DateTime.Now,
                    IsAvail = true,
                    ShowHomePage = true,
                    AuthorId = 2
                }
            );
        }

        _context.SaveChanges();        
    }

    private static async Task SeedUsers(UserManager<User> userManager)
    {
        var userName = "UserA";
        var userEmail = "usera@example.com";
        var userPassword = "zxcvb1234";

        // Check if the user already exists
        var user = await userManager.FindByNameAsync(userName);
        if (user == null)
        {
            // Create a new user
            user = new User
            {
                Id = "user1",
                UserName = userName,
                NormalizedUserName = userName.ToUpper(),
                Email = userEmail,
                NormalizedEmail = userEmail.ToUpper(),
                FullName = "User A",
                SecurityStamp = Guid.NewGuid().ToString()
            };

            var result = await userManager.CreateAsync(user, userPassword);

            if (result.Succeeded)
            {
                Console.WriteLine("User created successfully.");
            }
            else
            {
                Console.WriteLine("Error creating user:");
                foreach (var error in result.Errors)
                {
                    Console.WriteLine($"Code: {error.Code}, Description: {error.Description}");
                }
            }
        }
        else
        {
            Console.WriteLine("User already exists.");
        }
    }
}

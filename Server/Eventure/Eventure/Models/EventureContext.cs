using System.Reflection;
using Eventure.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Eventure.Models;

public class EventureContext : IdentityDbContext<User>
{
    public DbSet<Event> Events { get; set; }
    public DbSet<Location> Locations { get; set; }
    public DbSet<Category> Categories { get; set; }
    
    public EventureContext(DbContextOptions<EventureContext> options) : base(options)
    {
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Event>()
            .HasOne(e => e.Creator) // An Event has one Creator (User)
            .WithMany(u => u.CreatedEvents) // A User can have many CreatedEvents
            .HasForeignKey(e => e.CreatorId) // The foreign key for the relationship
            .OnDelete(DeleteBehavior.Restrict); // You can use Restrict, SetNull, or Cascade depending on your requirements

        modelBuilder.Entity<Event>()
            .HasMany(e => e.Participants) // An Event can have many Participants (Users)
            .WithMany(u => u.AppliedEvents) // A User can have many AppliedEvents
            .UsingEntity<Dictionary<string, object>>(
                "EventParticipants", // This is the name of the join table
                j => j
                    .HasOne<User>()
                    .WithMany()
                    .HasForeignKey("UserId")
                    .HasConstraintName("FK_EventParticipants_Users_UserId")
                    .OnDelete(DeleteBehavior.Cascade), // Use Cascade if you want to delete Participant records when a User is deleted
                j => j
                    .HasOne<Event>()
                    .WithMany()
                    .HasForeignKey("EventId")
                    .HasConstraintName("FK_EventParticipants_Events_EventId")
                    .OnDelete(DeleteBehavior.Cascade) // Use Cascade if you want to delete Participant records when an Event is deleted
            );
    }

     public static async Task Seed(EventureContext context, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
    {
        // Seed locationspublic
        if (!context.Locations.Any())
        {
            var fileName = "hu.csv";
            var locations = Location.LoadLocationsFromCsv(fileName);

            context.Locations.AddRange(locations);
            await context.SaveChangesAsync();
        }
        
        // Seed categories
        if (!context.Categories.Any())
        {
            var fileName = "Category.csv";

            var categories = Category.LoadCategoriesFromCsv(fileName);
            context.Categories.AddRange(categories);
            await context.SaveChangesAsync();
        }
        
        // Seed roles
        if (!roleManager.Roles.Any())
        {
            var roles = new List<IdentityRole>()
            {
                new() { Name = "User" },
                new() { Name = "Admin" }
            };
            
            foreach (var identityRole in roles)
            {
                await roleManager.CreateAsync(identityRole);
            }
            
        }

        // Seed users
        if (!userManager.Users.Any())
        {
            var newUsers = new List<User>()
            {
                new() { UserName = "Loci", Email = "loci@gmail.com" },
                new() { UserName = "Zsofi", Email = "zsofi@gmail.com" },
                new() { UserName = "Bianka", Email = "bianka@gmail.com" }
            };
            
            foreach (var newUser in newUsers)
            {
                await userManager.CreateAsync(newUser, "Abcd@1234");
                await userManager.AddToRolesAsync(newUser, new string[]{ "User","Admin" });
            }
        }
        
        //Seed Events

        if (!context.Events.Any())
        {
            var users = await userManager.Users.ToListAsync();
            int usersNumber = users.Count;
            var random = new Random();
            var events = new List<Event>()
            {
                new Event
                {
                    EventName = "Concert: Rock Legends",
                    Description = "A rocking concert featuring legendary rock bands.",
                    StartingDate = new DateTime(2023, 08, 24, 18, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 08, 24, 22, 00, 00).ToUniversalTime(),
                    HeadCount = 3,
                    RecommendedAge = 18,
                    Price = 30000,
                    LocationId = 1, 
                    CategoryId = 1,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                },
                new Event
                {
                    EventName = "Festival: Summer Vibes",
                    Description = "Enjoy the summer with music, food, and fun at this festival.",
                    StartingDate = new DateTime(2023, 08, 15, 12, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 08, 15, 12, 00, 00).ToUniversalTime(),
                    HeadCount = 5,
                    RecommendedAge = 18,
                    Price = 9000,
                    LocationId = 31, 
                    CategoryId = 2,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Exhibition: Art Gallery",
                    Description = "Explore stunning artworks from local and international artists.",
                    StartingDate = new DateTime(2023, 09, 10, 10, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 09, 10, 18, 00, 00).ToUniversalTime(),
                    HeadCount = 1,
                    RecommendedAge = 18,
                    Price = 4500,
                    LocationId = 21, 
                    CategoryId = 3,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName =  "Sports Event: Soccer Tournament",
                    Description = "Cheer for your favorite soccer teams in this thrilling tournament.",
                    StartingDate = new DateTime(2023, 09, 30, 14, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 09, 30, 20, 00, 00).ToUniversalTime(),
                    HeadCount = 6,
                    RecommendedAge = 18,
                    Price = 10000,
                    LocationId = 114, 
                    CategoryId = 4,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Fashion Show: Runway Glam",
                    Description = "Experience the latest fashion trends on the glamorous runway.",
                    StartingDate = new DateTime(2023, 08, 05, 19, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 08, 05, 22, 00, 00).ToUniversalTime(),
                    HeadCount = 2,
                    RecommendedAge = 18,
                    Price = 0,
                    LocationId = 1, 
                    CategoryId = 5,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Performance: Broadway Nights",
                    Description = "Be captivated by talented performers in this Broadway-style show.",
                    StartingDate = new DateTime(2023, 09, 08, 20, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 09, 08, 23, 00, 00).ToUniversalTime(),
                    HeadCount = 1,
                    RecommendedAge = 18,
                    Price = 20000,
                    LocationId = 122, 
                    CategoryId = 6,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Restaurant Opening: Fusion Delights",
                    Description = "Celebrate the grand opening of a new restaurant with delicious fusion cuisine.",
                    StartingDate = new DateTime(2023, 10, 20, 18, 30, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 10, 20, 22, 30, 00).ToUniversalTime(),
                    HeadCount = 2,
                    RecommendedAge = 18,
                    Price = 30000,
                    LocationId = 158, 
                    CategoryId = 7,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Book Launch: Mystery Thriller",
                    Description = "Meet the author and discover the suspenseful world of a mystery thriller.",
                    StartingDate = new DateTime(2023, 09, 12, 17, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 09, 12, 19, 00, 00).ToUniversalTime(),
                    HeadCount = 10,
                    RecommendedAge = 18,
                    Price = 0,
                    LocationId = 15, 
                    CategoryId = 8,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Photography Basics",
                    Description = "Learn the fundamentals of photography and capture stunning images.",
                    StartingDate =new DateTime(2023, 08, 08, 14, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 08, 08, 17, 00, 00).ToUniversalTime(),
                    HeadCount = 5,
                    RecommendedAge = 14,
                    Price = 5000,
                    LocationId = 14, 
                    CategoryId = 12,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Ultimate Shopping Spree",
                    Description = "Get ready for the shopping experience of a lifetime! Join us for an ultimate shopping spree at the city's best malls and stores.",
                    StartingDate = new DateTime(2023, 08, 12, 10, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 08, 12, 18, 00, 00).ToUniversalTime(),
                    HeadCount = 2,
                    RecommendedAge = 18,
                    Price = 0,
                    LocationId = 6, 
                    CategoryId = 4,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Mediterranean Feast",
                    Description = "Savor a delightful Mediterranean feast with friends and loved ones.",
                    StartingDate =  new DateTime(2023, 10, 25, 19, 00, 00).ToUniversalTime(),
                    EndingDate = new DateTime(2023, 10, 25, 23, 30, 00).ToUniversalTime(),
                    HeadCount = 15,
                    RecommendedAge = 18,
                    Price = 0,
                    LocationId = 8, 
                    CategoryId = 16,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                }, new Event
                {
                    EventName = "Monopoly Marathon",
                    Description = "Join us for an epic Monopoly board game night and showcase your real estate skills.",
                    StartingDate =  new DateTime(2023, 08, 05, 18, 00, 00).ToUniversalTime(),
                    EndingDate =  new DateTime(2023, 08, 06, 18, 00, 00).ToUniversalTime(),
                    HeadCount = 3,
                    RecommendedAge = 18,
                    Price = 0,
                    LocationId = 12, 
                    CategoryId = 17,
                    CreatorId = users[random.Next(0, usersNumber-1)].Id
                },
                
            };
            context.Events.AddRange(events);
            await context.SaveChangesAsync();
        }


    }
}
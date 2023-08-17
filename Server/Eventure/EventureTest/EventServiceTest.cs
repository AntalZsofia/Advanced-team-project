using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Eventure.Models;
using Eventure.Models.Entities;
using Eventure.Models.RequestDto;
using Eventure.Models.Results;
using Eventure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;


namespace EventureTest
{
    [TestFixture]
    public class EventServiceTest
    {
        private Mock<UserManager<User>> _mockUserManager;
        private Mock<IUserStore<User>> _mockUserStore;
        private IEventService _eventService;
        private EventureContext _context;
        private DbContextOptions<EventureContext> _dbContextOptions;


        [SetUp]
        public void Setup()
        {
            _mockUserStore = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(_mockUserStore.Object,null,null,null,null,null,null,null,null);
            _dbContextOptions = new DbContextOptionsBuilder<EventureContext>()
                .UseInMemoryDatabase(databaseName:"Eventureone")
                .Options;
            _context = new EventureContext(_dbContextOptions);
            _context.Database.EnsureCreated();
            SeedDb();
            _eventService = new EventService(_context, _mockUserManager.Object);

        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }
        

        [Test]
        public async Task CreateEventAsync_LocationNotFound_ReturnsFailedEventActionResult()
        {
            _mockUserManager.Setup(n => n.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());
            
            var createEventDto = new CreateEventDto
            {
                
            };

            var location = _context.Locations.First();
            
            _context.Locations.RemoveRange(_context.Locations);
            _context.SaveChanges();
            var result = await _eventService.CreateEventAsync(createEventDto, "");
            var exceptedResult = EventActionResult.Failed("Couldn't find location.");
            
            
            Assert.NotNull(location);
            Assert.IsInstanceOf<EventActionResult>(result);
            Assert.That(result.Succeeded, Is.EqualTo(exceptedResult.Succeeded));
            Assert.That(result.Response.Message,Is.EqualTo(exceptedResult.Response.Message));
        }

        [Test]
        public async Task CreateEventAsync_CategoryNotFound_ReturnsFailedEventActionResult()
        {
            _mockUserManager.Setup(n => n.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());
            
            var createEventDto = new CreateEventDto
            {
                Location =  "Budapest"
            };

            var location = _context.Locations.First();
            _context.Categories.RemoveRange(_context.Categories);
            _context.SaveChanges();
            var result = await _eventService.CreateEventAsync(createEventDto, "");
            var exceptedResult = EventActionResult.Failed("Couldn't find category.");
            
            Assert.NotNull(location);
            Assert.IsInstanceOf<EventActionResult>(result);
            Assert.That(result.Succeeded, Is.EqualTo(exceptedResult.Succeeded));
            Assert.That(result.Response.Message,Is.EqualTo(exceptedResult.Response.Message));

            
        }

        [Test]
        public async Task CreateEventAsync_EventCreated_EventPresentInDatabase()
        {
            _mockUserManager.Setup(n => n.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new User(){Id = "1"});
            
            var createEventDto = new CreateEventDto
            {
                EventName = "Test Event",
                Description = "Test event description",
                StartingDate = "2023-08-24 18:00:00+02",
                EndingDate = "2023-08-24 19:00:00+02",
                HeadCount = 100,
                RecommendedAge = 18,
                Price = 20,
                Location = "Budapest",
                Category = "Concert"
            };
            
            var result = await _eventService.CreateEventAsync(createEventDto, "");
            var createdEvent = _context.Events.FirstOrDefault(e =>e.CreatorId == "1" );
            var exceptedResult = EventActionResult.Succeed("Event created");

            
            Assert.NotNull(createdEvent);
            Assert.IsInstanceOf<EventActionResult>(result);
            Assert.That(result.Succeeded, Is.EqualTo(exceptedResult.Succeeded));
            Assert.That(result.Response.Message,Is.EqualTo(exceptedResult.Response.Message));

        }

        [Test]
        public async Task CreateEventAsync_ServerError_ThrowNewException()
        {
            _mockUserManager.Setup(n => n.FindByNameAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var exception = Assert.ThrowsAsync<Exception>(async () => await _eventService.CreateEventAsync(new CreateEventDto(), ""));
            Assert.That("An error occured on the server", Is.EqualTo(exception!.Message));
        }

        [Test]
        public void CalculateDistance_ValidInput_ReturnsCorrectDistance()
        {
            
        }

        [Test]
        public async Task UpdateEvent_EventNotFound_ReturnsEventNotFoundResult()
        {
            var updateEventDto = new UpdateEventDto();
            long id = 123456;
            var userName = "";
            
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(new User());

            var result = await _eventService.UpdateEvent(updateEventDto, id, userName);
            var exceptedResult = UpdateEventResult.Fail();
            
            Assert.IsInstanceOf<UpdateEventResult>(result);
            Assert.That(result.Succeeded, Is.EqualTo(exceptedResult.Succeeded));
            Assert.That(result.Message, Is.EqualTo(exceptedResult.Message));
        }

        [Test]
        public async Task UpdateEvent_EventUpdated_EventModifiedInDb()
        {
            var user = _context.Users.First();
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ReturnsAsync(user);
            var eventId = _context.Events.FirstOrDefault(e => e.EventName == "Concert: Rock Legends")!.Id;
            var updateEventDto = new UpdateEventDto()
            {
                EventName = "Concert: Rock Legends",
                Category = "Concert",
                Location = "Budapest",
                Description = "A rocking concert featuring legendary rock band.",
                StartingDate = new DateTime(2023, 08, 24, 18, 00, 00).ToUniversalTime().ToString(CultureInfo.CurrentCulture),
                EndingDate = new DateTime(2023, 08, 24, 22, 00, 00).ToUniversalTime().ToString(CultureInfo.CurrentCulture),
                HeadCount = 3,
                Price = 30000,
                RecommendedAge = 18
            };

            var result = await _eventService.UpdateEvent(updateEventDto, eventId, user.Name);
            var updatedEvent = _context.Events.FirstOrDefault(e => e.EventName == "Concert: Rock Legends");
            
            Assert.IsInstanceOf<UpdateEventResult>(result);
            Assert.That(updatedEvent!.Description, Is.EqualTo("A rocking concert featuring legendary rock band."));
        }

        [Test]
        public async Task UpdateEvent_ServerError_ThrowsException()
        {
            var updateEventDto = new UpdateEventDto();
            long id = 123456;
            var userName = "";
            
            _mockUserManager.Setup(m => m.FindByNameAsync(It.IsAny<string>()))
                .ThrowsAsync(new Exception());

            var exception = Assert.ThrowsAsync<Exception>(async () =>
                await _eventService.UpdateEvent(updateEventDto, id, userName));
            
            Assert.That(exception!.Message, Is.EqualTo("An error occured on the server."));
        }


        private void SeedDb()
        {
            var fileNameLocation = "hu.csv";
            var fileNameCategory = "Category.csv";
            var user = new User(){Id = "123", Name = "Bela" };
            
            var locations = Location.LoadLocationsFromCsv(fileNameLocation);
            var categories = Category.LoadCategoriesFromCsv(fileNameCategory);
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
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
                    CreatorId = user.Id
                },
                
            };

            _context.Add(user);
            _context.Categories.AddRange(categories);
            _context.Locations.AddRange(locations);
            _context.Events.AddRange(events);
            _context.SaveChanges();
        }

       
    }
}



        
   

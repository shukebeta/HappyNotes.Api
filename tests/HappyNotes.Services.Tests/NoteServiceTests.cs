using Api.Framework;
using Api.Framework.Exceptions;
using Api.Framework.Extensions;
using Api.Framework.Models;
using AutoMapper;
using HappyNotes.Common;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using EventId = HappyNotes.Common.Enums.EventId;

namespace HappyNotes.Services.Tests;

[TestFixture]
public class NoteServiceTests
{
    private Mock<ISearchService> _searchService;
    private Mock<ISyncNoteService> _mockSyncNoteService;
    private Mock<INoteTagService> _mockNoteTagService;
    private Mock<INoteRepository> _mockNoteRepository;
    private Mock<IRepositoryBase<LongNote>> _mockLongNoteRepository;
    private Mock<IMapper> _mockMapper;
    private Mock<ILogger<NoteService>> _mockLogger;
    private NoteService _noteService;

    [SetUp]
    public void Setup()
    {
        _searchService = new Mock<ISearchService>();
        _mockSyncNoteService = new Mock<ISyncNoteService>();
        var syncServices = new[] { _mockSyncNoteService.Object };
        _mockNoteTagService = new Mock<INoteTagService>();
        _mockNoteRepository = new Mock<INoteRepository>();
        _mockLongNoteRepository = new Mock<IRepositoryBase<LongNote>>();
        _mockMapper = new Mock<IMapper>();
        _mockLogger = new Mock<ILogger<NoteService>>();
        _mockSyncNoteService.Setup(s => s.SyncNewNote(It.IsAny<Note>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockSyncNoteService.Setup(s => s.SyncEditNote(It.IsAny<Note>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockSyncNoteService.Setup(s => s.SyncDeleteNote(It.IsAny<Note>()))
            .Returns(Task.CompletedTask);

        _noteService = new NoteService(
            _searchService.Object,
            syncServices,
            _mockNoteTagService.Object,
            _mockNoteRepository.Object,
            _mockLongNoteRepository.Object,
            _mockMapper.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Post_WithValidNote_ReturnsNoteId()
    {
        // Arrange
        var userId = 1L;
        var request = new PostNoteRequest { Content = "Test note" };
        var note = new Note
        {
            Id = 1,
            Content = "Test note",
            UserId = userId,
            CreatedAt = DateTime.UtcNow.ToUnixTimeSeconds()
        };

        _mockMapper.Setup(m => m.Map<PostNoteRequest, Note>(request))
            .Returns(note);
        _mockNoteRepository.Setup(r => r.InsertAsync(note))
            .ReturnsAsync(true);

        // Act
        var result = await _noteService.Post(userId, request);

        // Assert
        Assert.That(result, Is.EqualTo(note.Id));
        _mockNoteRepository.Verify(r => r.InsertAsync(It.IsAny<Note>()), Times.Once);
    }

    [Test]
    public void Post_WithEmptyContent_ThrowsArgumentException()
    {
        // Arrange
        var userId = 1L;
        var request = new PostNoteRequest { Content = "" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _noteService.Post(userId, request));
        Assert.That(ex.Message, Is.EqualTo("Nothing was submitted"));
    }

    [Test]
    public async Task Get_WithExistingPublicNote_ReturnsNote()
    {
        // Arrange
        var userId = 1L;
        var noteId = 1L;
        var note = new Note
        {
            Id = noteId,
            Content = "Test note",
            UserId = 2,
            IsPrivate = false,
            DeletedAt = null
        };

        _mockNoteRepository.Setup(r => r.Get(noteId))
            .ReturnsAsync(note);

        // Act
        var result = await _noteService.Get(userId, noteId);

        // Assert
        Assert.That(result, Is.EqualTo(note));
    }

    [Test]
    public void Get_WithNonExistentNote_ThrowsException()
    {
        // Arrange
        var userId = 1L;
        var noteId = 1L;
        _mockNoteRepository.Setup(r => r.Get(noteId))
            .ReturnsAsync((Note)null!);

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException<object>>(async () =>
            await _noteService.Get(userId, noteId));
        Assert.That(ex.CustomData!.ErrorCode, Is.EqualTo((int)EventId._00100_NoteNotFound));
    }

    [Test]
    public void Get_WithPrivateNoteFromOtherUser_ThrowsException()
    {
        // Arrange
        var userId = 1L;
        var noteId = 1L;
        var note = new Note
        {
            Id = noteId,
            Content = "Private note",
            UserId = 2,
            IsPrivate = true
        };

        _mockNoteRepository.Setup(r => r.Get(noteId))
            .ReturnsAsync(note);

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException<object>>(async () =>
            await _noteService.Get(userId, noteId));
        Assert.That(ex.CustomData!.ErrorCode, Is.EqualTo((int)EventId._00101_NoteIsPrivate));
    }

    [Test]
    public async Task Update_WithValidNote_ReturnsTrue()
    {
        // Arrange
        var userId = 1L;
        var noteId = 1L;
        var request = new PostNoteRequest { Content = "Updated content" };
        var existingNote = new Note
        {
            Id = noteId,
            Content = "Original content",
            UserId = userId
        };
        var updatedNote = new Note
        {
            Id = noteId,
            Content = "Updated content",
            UserId = userId
        };

        _mockNoteRepository.Setup(r => r.Get(noteId)) .ReturnsAsync(existingNote);
        _mockMapper.Setup(m => m.Map<PostNoteRequest, Note>(request))
            .Returns(updatedNote);
        _mockNoteRepository.Setup(r => r.UpdateAsync(It.IsAny<Note>()))
            .ReturnsAsync(true);

        // Act
        var result = await _noteService.Update(userId, noteId, request);

        // Assert
        Assert.That(result, Is.True);
        _mockNoteRepository.Verify(r => r.UpdateAsync(It.IsAny<Note>()), Times.Once);
    }

    [Test]
    public async Task Delete_WithOwnedNote_ReturnsTrue()
    {
        // Arrange
        var userId = 1L;
        var noteId = 1L;
        var note = new Note
        {
            Id = noteId,
            Content = "Test note",
            UserId = userId,
            DeletedAt = null
        };

        _mockNoteRepository.Setup(r => r.GetFirstOrDefaultAsync(x => x.Id == noteId, null)) .ReturnsAsync(note);
        _mockNoteRepository.Setup(r => r.UpdateAsync(It.IsAny<Note>()))
            .ReturnsAsync(true);

        // Act
        var result = await _noteService.Delete(userId, noteId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(note.DeletedAt, Is.Not.Null);
    }

    [Test]
    public async Task Undelete_WithDeletedOwnedNote_ReturnsTrue()
    {
        // Arrange
        var userId = 1L;
        var noteId = 1L;
        var note = new Note
        {
            Id = noteId,
            Content = "Test note",
            UserId = userId,
            DeletedAt = DateTime.UtcNow.ToUnixTimeSeconds()
        };

        _mockNoteRepository.Setup(r => r.GetFirstOrDefaultAsync(x => x.Id == noteId, null)) .ReturnsAsync(note);
        _mockNoteRepository.Setup(r => r.UpdateAsync(It.IsAny<Note>()))
            .ReturnsAsync(true);

        // Act
        var result = await _noteService.Undelete(userId, noteId);

        // Assert
        Assert.That(result, Is.True);
        _mockNoteRepository.Verify(r => r.UndeleteAsync(noteId), Times.Once);
    }

    [Test]
    public async Task GetUserNotes_WithValidParameters_ReturnsPageData()
    {
        // Arrange
        var userId = 1L;
        var pageSize = 10;
        var pageNumber = 1;
        var expectedNotes = new PageData<Note>
        {
            DataList = new List<Note> { new Note { Id = 1, Content = "Test" } },
            TotalCount = 1
        };

        _mockNoteRepository.Setup(r => r.GetUserNotes(userId, pageSize, pageNumber, false, false))
            .ReturnsAsync(expectedNotes);

        // Act
        var result = await _noteService.GetUserNotes(userId, pageSize, pageNumber);

        // Assert
        Assert.That(result, Is.EqualTo(expectedNotes));
    }

    [Test]
    public async Task GetPublicNotes_WithinMaxPage_ReturnsPageData()
    {
        // Arrange
        var pageSize = 10;
        var pageNumber = 1;
        var expectedNotes = new PageData<Note>
        {
            DataList = new List<Note> { new Note { Id = 1, Content = "Test" } },
            TotalCount = 1
        };

        _mockNoteRepository.Setup(r => r.GetPublicNotes(pageSize, pageNumber, false))
            .ReturnsAsync(expectedNotes);

        // Act
        var result = await _noteService.GetPublicNotes(pageSize, pageNumber);

        // Assert
        Assert.That(result, Is.EqualTo(expectedNotes));
    }

    [Test]
    public void GetPublicNotes_ExceedingMaxPage_ThrowsException()
    {
        // Arrange
        var pageSize = 10;
        var pageNumber = Constants.PublicNotesMaxPage + 1;

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await _noteService.GetPublicNotes(pageSize, pageNumber));
        Assert.That(ex.Message, Does.Contain($"We only provide at most {Constants.PublicNotesMaxPage} page"));
    }

    [TestFixture]
    public class GetTimestampsTests
    {
        [Test]
        public void GetTimestamps_WithRecentStartDate_ReturnsExpectedCount()
        {
            // Arrange - Start date 2 months ago
            var timeZone = "UTC";
            var startDate = DateTimeOffset.UtcNow.AddMonths(-2);
            var startTimestamp = startDate.ToUnixTimeSeconds();

            // Act
            var result = NoteService._GetTimestamps(startTimestamp, timeZone);

            // Assert
            Assert.That(result.Length, Is.GreaterThanOrEqualTo(2), "Should include at least yesterday and today");
            Assert.That(result.Length, Is.LessThanOrEqualTo(10), "Should not return excessive timestamps");
            
            // Verify results are sorted (older dates first)
            for (int i = 1; i < result.Length; i++)
            {
                Assert.That(result[i], Is.GreaterThanOrEqualTo(result[i-1]), 
                    "Timestamps should be in ascending order");
            }
        }

        [Test]
        public void GetTimestamps_WithOldStartDate_IncludesYearlyMilestones()
        {
            // Arrange - Start date 3 years ago, same month/day as today
            var timeZone = "UTC";
            var today = DateTimeOffset.UtcNow;
            var startDate = new DateTimeOffset(today.Year - 3, today.Month, today.Day, 
                12, 0, 0, TimeSpan.Zero);
            var startTimestamp = startDate.ToUnixTimeSeconds();

            // Act
            var result = NoteService._GetTimestamps(startTimestamp, timeZone);

            // Assert
            Assert.That(result.Length, Is.GreaterThanOrEqualTo(5), 
                "Should include yearly anniversaries plus recent periods");
            
            // Should contain timestamps that could be yearly anniversaries
            var yearlyCount = result.Count(ts => 
            {
                var date = DateTimeOffset.FromUnixTimeSeconds(ts);
                return date.Month == today.Month && date.Day == today.Day && date.Year < today.Year;
            });
            Assert.That(yearlyCount, Is.GreaterThanOrEqualTo(1), 
                "Should contain at least one yearly anniversary");
        }

        [Test]
        public void GetTimestamps_WithValidTimeZone_DoesNotThrow()
        {
            // Arrange
            var timeZones = new[] { "UTC", "America/New_York", "Europe/London", "Asia/Shanghai" };
            var startTimestamp = DateTimeOffset.UtcNow.AddMonths(-6).ToUnixTimeSeconds();

            foreach (var timeZone in timeZones)
            {
                // Act & Assert
                Assert.DoesNotThrow(() =>
                {
                    var result = NoteService._GetTimestamps(startTimestamp, timeZone);
                    Assert.That(result, Is.Not.Null);
                    Assert.That(result.Length, Is.GreaterThan(0));
                }, $"Should handle timezone {timeZone} without throwing");
            }
        }

        [Test]
        public void GetTimestamps_WithSameDayStartDate_ReturnsMinimumTimestamps()
        {
            // Arrange - Start date is today
            var timeZone = "UTC";
            var startTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            // Act
            var result = NoteService._GetTimestamps(startTimestamp, timeZone);

            // Assert
            Assert.That(result.Length, Is.GreaterThanOrEqualTo(2), 
                "Should always include yesterday and today");
        }

        [Test]
        public void GetTimestamps_WithFarFutureStartDate_HandlesGracefully()
        {
            // Arrange - Start date in future (edge case)
            var timeZone = "UTC";
            var futureStartTimestamp = DateTimeOffset.UtcNow.AddYears(1).ToUnixTimeSeconds();

            // Act
            var result = NoteService._GetTimestamps(futureStartTimestamp, timeZone);

            // Assert
            Assert.That(result.Length, Is.GreaterThanOrEqualTo(2), 
                "Should handle future start dates gracefully");
        }

        [Test]
        public void GetTimestamps_ConsistentResults_ForSameInput()
        {
            // Arrange
            var timeZone = "UTC";
            var startTimestamp = DateTimeOffset.UtcNow.AddMonths(-1).ToUnixTimeSeconds();

            // Act - Call multiple times
            var result1 = NoteService._GetTimestamps(startTimestamp, timeZone);
            var result2 = NoteService._GetTimestamps(startTimestamp, timeZone);

            // Assert - Results should be identical (within same second)
            Assert.That(result1.Length, Is.EqualTo(result2.Length));
            for (int i = 0; i < result1.Length; i++)
            {
                Assert.That(Math.Abs(result1[i] - result2[i]), Is.LessThanOrEqualTo(1), 
                    "Results should be consistent within 1 second");
            }
        }

        [Test]
        public void GetTimestamps_AnniversaryDates_RespectHistoricalTimezoneOffsets()
        {
            // Arrange - Use realistic dates and timezone
            var timeZone = "America/New_York";
            // Use an old enough start date to ensure we get yearly anniversaries
            var startDate = DateTimeOffset.UtcNow.AddYears(-5); 
            var startTimestamp = startDate.ToUnixTimeSeconds();

            // Act
            var result = NoteService._GetTimestamps(startTimestamp, timeZone);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Length, Is.GreaterThan(0));
            
            // The method should return timestamps for today's month/day in previous years
            var today = DateTimeOffset.UtcNow;
            var currentMonth = today.Month;
            var currentDay = today.Day;
            
            // Find timestamps that match today's month/day from previous years
            var anniversaryTimestamps = result.Where(ts => 
            {
                var nyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                var date = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(ts), nyTimeZone);
                return date.Month == currentMonth && date.Day == currentDay && date.Year < today.Year;
            }).ToList();

            // Should have at least one anniversary if start date is old enough
            if (startDate.Year < today.Year)
            {
                Assert.That(anniversaryTimestamps.Count, Is.GreaterThanOrEqualTo(1), 
                    "Should contain anniversary dates for same month/day in previous years");

                // Verify that anniversary dates are correctly set to start of day
                foreach (var ts in anniversaryTimestamps)
                {
                    var nyTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZone);
                    var date = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(ts), nyTimeZone);
                    Assert.That(date.Month, Is.EqualTo(currentMonth), $"Anniversary should be in month {currentMonth}, got {date:yyyy-MM-dd}");
                    Assert.That(date.Day, Is.EqualTo(currentDay), $"Anniversary should be on day {currentDay}, got {date:yyyy-MM-dd}");
                    Assert.That(date.Hour, Is.EqualTo(0), "Anniversary should be at start of day");
                    Assert.That(date.Minute, Is.EqualTo(0), "Anniversary should be at start of day");
                }
            }
        }

        [Test]
        public void TryCreateDate_WithDifferentYears_UsesCorrectTimezoneOffset()
        {
            // Arrange - Test with a timezone that has DST changes
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("America/New_York");
            
            // Test different scenarios
            var testCases = new[]
            {
                new { Year = 2023, Month = 6, Day = 29, Description = "Summer 2023 (EDT)" },
                new { Year = 2023, Month = 12, Day = 29, Description = "Winter 2023 (EST)" },
                new { Year = 2020, Month = 6, Day = 29, Description = "Summer 2020 (EDT)" }
            };

            foreach (var testCase in testCases)
            {
                // Act
                var success = NoteService._TryCreateDate(testCase.Year, testCase.Month, testCase.Day, timeZone, out var timestamp);

                // Assert
                Assert.That(success, Is.True, $"Should successfully create date for {testCase.Description}");
                
                var resultDate = TimeZoneInfo.ConvertTime(DateTimeOffset.FromUnixTimeSeconds(timestamp), timeZone);
                Assert.That(resultDate.Year, Is.EqualTo(testCase.Year), $"Year mismatch for {testCase.Description}");
                Assert.That(resultDate.Month, Is.EqualTo(testCase.Month), $"Month mismatch for {testCase.Description}");
                Assert.That(resultDate.Day, Is.EqualTo(testCase.Day), $"Day mismatch for {testCase.Description}");
                Assert.That(resultDate.Hour, Is.EqualTo(0), $"Hour should be 0 for {testCase.Description}");
                Assert.That(resultDate.Minute, Is.EqualTo(0), $"Minute should be 0 for {testCase.Description}");
            }
        }
    }
}

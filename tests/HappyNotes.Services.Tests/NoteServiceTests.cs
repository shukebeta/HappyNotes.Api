using Api.Framework;
using Api.Framework.Exceptions;
using Api.Framework.Extensions;
using Api.Framework.Models;
using HappyNotes.Entities;
using HappyNotes.Models;
using HappyNotes.Services.interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using AutoMapper;
using HappyNotes.Repositories.interfaces;
using HappyNotes.Common;
using EventId = HappyNotes.Common.Enums.EventId;

namespace HappyNotes.Services.Tests;

[TestFixture]
public class NoteServiceTests
{
    private Mock<ISyncNoteService> _mockSyncNoteService;
    private Mock<INoteTagService> _mockNoteTagService;
    private Mock<INoteRepository> _mockNoteRepository;
    private Mock<IRepositoryBase<LongNote>> _mockLongNoteRepository;
    private Mock<IMapper> _mockMapper;
    private Mock<ICurrentUser> _mockCurrentUser;
    private Mock<ILogger<NoteService>> _mockLogger;
    private NoteService _noteService;

    [SetUp]
    public void Setup()
    {
        _mockSyncNoteService = new Mock<ISyncNoteService>();
        var syncServices = new[] { _mockSyncNoteService.Object };
        _mockNoteTagService = new Mock<INoteTagService>();
        _mockNoteRepository = new Mock<INoteRepository>();
        _mockLongNoteRepository = new Mock<IRepositoryBase<LongNote>>();
        _mockMapper = new Mock<IMapper>();
        _mockCurrentUser = new Mock<ICurrentUser>();
        _mockLogger = new Mock<ILogger<NoteService>>();
        _mockSyncNoteService.Setup(s => s.SyncNewNote(It.IsAny<Note>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockSyncNoteService.Setup(s => s.SyncEditNote(It.IsAny<Note>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        _mockSyncNoteService.Setup(s => s.SyncDeleteNote(It.IsAny<Note>()))
            .Returns(Task.CompletedTask);

        _noteService = new NoteService(
            syncServices,
            _mockNoteTagService.Object,
            _mockNoteRepository.Object,
            _mockLongNoteRepository.Object,
            _mockMapper.Object,
            _mockCurrentUser.Object,
            _mockLogger.Object
        );
    }

    [Test]
    public async Task Post_WithValidNote_ReturnsNoteId()
    {
        // Arrange
        var request = new PostNoteRequest { Content = "Test note" };
        var note = new Note
        {
            Id = 1,
            Content = "Test note",
            UserId = 1,
            CreatedAt = DateTime.UtcNow.ToUnixTimeSeconds()
        };

        _mockMapper.Setup(m => m.Map<PostNoteRequest, Note>(request))
            .Returns(note);
        _mockCurrentUser.Setup(u => u.Id).Returns(1);
        _mockNoteRepository.Setup(r => r.InsertAsync(note))
            .ReturnsAsync(true);

        // Act
        var result = await _noteService.Post(request);

        // Assert
        Assert.That(result, Is.EqualTo(note.Id));
        _mockNoteRepository.Verify(r => r.InsertAsync(It.IsAny<Note>()), Times.Once);
    }

    [Test]
    public void Post_WithEmptyContent_ThrowsArgumentException()
    {
        // Arrange
        var request = new PostNoteRequest { Content = "" };

        // Act & Assert
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await _noteService.Post(request));
        Assert.That(ex.Message, Is.EqualTo("Nothing was submitted"));
    }

    [Test]
    public async Task Get_WithExistingPublicNote_ReturnsNote()
    {
        // Arrange
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
        _mockCurrentUser.Setup(u => u.Id).Returns(1);

        // Act
        var result = await _noteService.Get(noteId);

        // Assert
        Assert.That(result, Is.EqualTo(note));
    }

    [Test]
    public void Get_WithNonExistentNote_ThrowsException()
    {
        // Arrange
        var noteId = 1L;
        _mockNoteRepository.Setup(r => r.Get(noteId))
            .ReturnsAsync((Note)null!);

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException<object>>(async () =>
            await _noteService.Get(noteId));
        Assert.That(ex.CustomData!.ErrorCode, Is.EqualTo((int)EventId._00100_NoteNotFound));
    }

    [Test]
    public void Get_WithPrivateNoteFromOtherUser_ThrowsException()
    {
        // Arrange
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
        _mockCurrentUser.Setup(u => u.Id).Returns(1);

        // Act & Assert
        var ex = Assert.ThrowsAsync<CustomException<object>>(async () =>
            await _noteService.Get(noteId));
        Assert.That(ex.CustomData!.ErrorCode, Is.EqualTo((int)EventId._00101_NoteIsPrivate));
    }

    [Test]
    public async Task Update_WithValidNote_ReturnsTrue()
    {
        // Arrange
        var noteId = 1L;
        var request = new PostNoteRequest { Content = "Updated content" };
        var existingNote = new Note
        {
            Id = noteId,
            Content = "Original content",
            UserId = 1
        };
        var updatedNote = new Note
        {
            Id = noteId,
            Content = "Updated content",
            UserId = 1
        };

        _mockNoteRepository.Setup(r => r.GetFirstOrDefaultAsync(x => x.Id == noteId, null)) .ReturnsAsync(existingNote);
        _mockMapper.Setup(m => m.Map<PostNoteRequest, Note>(request))
            .Returns(updatedNote);
        _mockCurrentUser.Setup(u => u.Id).Returns(1);
        _mockNoteRepository.Setup(r => r.UpdateAsync(It.IsAny<Note>()))
            .ReturnsAsync(true);

        // Act
        var result = await _noteService.Update(noteId, request);

        // Assert
        Assert.That(result, Is.True);
        _mockNoteRepository.Verify(r => r.UpdateAsync(It.IsAny<Note>()), Times.Once);
    }

    [Test]
    public async Task Delete_WithOwnedNote_ReturnsTrue()
    {
        // Arrange
        var noteId = 1L;
        var note = new Note
        {
            Id = noteId,
            Content = "Test note",
            UserId = 1,
            DeletedAt = null
        };

        _mockNoteRepository.Setup(r => r.GetFirstOrDefaultAsync(x => x.Id == noteId, null)) .ReturnsAsync(note);
        _mockCurrentUser.Setup(u => u.Id).Returns(1);
        _mockNoteRepository.Setup(r => r.UpdateAsync(It.IsAny<Note>()))
            .ReturnsAsync(true);

        // Act
        var result = await _noteService.Delete(noteId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(note.DeletedAt, Is.Not.Null);
    }

    [Test]
    public async Task Undelete_WithDeletedOwnedNote_ReturnsTrue()
    {
        // Arrange
        var noteId = 1L;
        var note = new Note
        {
            Id = noteId,
            Content = "Test note",
            UserId = 1,
            DeletedAt = DateTime.UtcNow.ToUnixTimeSeconds()
        };

        _mockNoteRepository.Setup(r => r.GetFirstOrDefaultAsync(x => x.Id == noteId, null)) .ReturnsAsync(note);
        _mockCurrentUser.Setup(u => u.Id).Returns(1);
        _mockNoteRepository.Setup(r => r.UpdateAsync(It.IsAny<Note>()))
            .ReturnsAsync(true);

        // Act
        var result = await _noteService.Undelete(noteId);

        // Assert
        Assert.That(result, Is.True);
        Assert.That(note.DeletedAt, Is.Null);
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
}

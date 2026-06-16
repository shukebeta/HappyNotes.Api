using AutoMapper;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Models;
using Microsoft.Extensions.Logging.Abstractions;

namespace HappyNotes.Services.Tests;

// Issue #15: lock the convention-based PostNoteRequest ↔ Note IsPrivate mapping.
// The fan-out path in NoteService.SetIsPrivate feeds IsPrivate from the request into
// Update, which carries it into SyncEditNote. If this map ever drops IsPrivate
// (e.g. someone refactors AutoMapperProfile), Telegram/Mastodon will silently desync.
[TestFixture]
public class AutoMapperProfileTests
{
    private IMapper _mapper;

    [SetUp]
    public void Setup()
    {
        // Skip AssertConfigurationIsValid: the production profile intentionally leaves
        // several members (Id, UserId, Tags, …) to be assigned by the service layer.
        var config = new MapperConfiguration(cfg => cfg.AddProfile<AutoMapperProfile>(), NullLoggerFactory.Instance);
        _mapper = config.CreateMapper();
    }

    [TestCase(true)]
    [TestCase(false)]
    public void PostNoteRequest_ToNote_IsPrivate_IsPreserved(bool isPrivate)
    {
        var request = new PostNoteRequest { Content = "note body", IsPrivate = isPrivate };

        var note = _mapper.Map<PostNoteRequest, Note>(request);

        Assert.That(note.IsPrivate, Is.EqualTo(isPrivate));
    }

    [TestCase(true)]
    [TestCase(false)]
    public void Note_ToPostNoteRequest_IsPrivate_IsPreserved(bool isPrivate)
    {
        var note = new Note { Content = "note body", IsPrivate = isPrivate };

        var request = _mapper.Map<Note, PostNoteRequest>(note);

        Assert.That(request.IsPrivate, Is.EqualTo(isPrivate));
    }
}

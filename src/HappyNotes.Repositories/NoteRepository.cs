using System.Linq.Expressions;
using Api.Framework;
using Api.Framework.Models;
using HappyNotes.Dto;
using HappyNotes.Entities;
using HappyNotes.Repositories.interfaces;
using SqlSugar;

namespace HappyNotes.Repositories;

public class NoteRepository(ISqlSugarClient db) : RepositoryBase<Note>(db), INoteRepository
{
    private readonly ISqlSugarClient _db = db;

    public async Task<Note> Get(long noteId)
    {
        return await _db.Queryable<Note, LongNote, User>((n, l, u) => new JoinQueryInfos(
                JoinType.Left, n.Id == l.Id,
                JoinType.Inner, n.UserId == u.Id
            ))
            .Where((n, l, u) => n.Id == noteId)
            .Select((n, l, u) => new Note
            {
                Id = n.Id,
                Content = n.IsLong ? l.Content : n.Content,
                FavoriteCount = n.FavoriteCount,
                Status = n.Status,
                IsLong = n.IsLong,
                IsMarkdown = n.IsMarkdown,
                IsPrivate = n.IsPrivate,
                UserId = n.UserId,
                User = u
            })
            .FirstAsync();
    }

    public async Task<PageData<Note>> GetUserNotes(long userId, int pageSize, int pageNumber,
        bool all = false, bool isAsc = false)
    {
        return await _GetPageDataAsync(pageSize, pageNumber,
            n => n.DeleteAt == null && n.UserId == userId && (all || n.IsPrivate == false),
            n => n.CreateAt, isAsc);
    }

    public async Task<PageData<Note>> GetPublicNotes(int pageSize, int pageNumber, bool isAsc = false)
    {
        return await _GetPageDataAsync(pageSize, pageNumber,
            n => n.DeleteAt == null && n.IsPrivate == false,
            n => n.CreateAt, isAsc);
    }

    private async Task<PageData<Note>> _GetPageDataAsync(int pageSize = 20, int pageNumber = 1,
        Expression<Func<Note, bool>>? where = null, Expression<Func<Note, object>>? orderBy = null,
        bool isAsc = true)
    {
        var pageData = new PageData<Note>
        {
            PageIndex = pageNumber,
            PageSize = pageSize,
        };
        RefAsync<int> totalCount = 0;
        var result = await _db.Queryable<Note, User>((n, u) => new JoinQueryInfos(
                JoinType.Inner, n.UserId == u.Id
            )).WhereIF(null != where, where)
            .OrderByIF(orderBy != null, orderBy, isAsc ? OrderByType.Asc : OrderByType.Desc)
            .Mapper(n => n.User, n => n.UserId)
            .ToPageListAsync(pageNumber, pageSize, totalCount);
        pageData.TotalCount = totalCount;
        pageData.DataList = result;
        return pageData;
    }
}

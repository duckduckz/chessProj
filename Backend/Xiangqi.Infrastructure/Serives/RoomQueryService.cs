namespace Xiangqi.Infrastructure.Services;

public interface IRoomQueryService
{
    Task<PagedResult<RoomListItem>> ListAsync(RoomListQuery q);
}

public sealed class RoomQueryService : IRoomQueryService
{
    private readonly IRoomRepository _repo;
    public RoomQueryService(IRoomRepository repo) => _repo = repo;

    public async Task<PagedResult<RoomListItem>> ListAsync(RoomListQuery q)
    {
        var rooms = await _repo.ListAsync(q.Visibility);

        // project + compute flags
        var proj = rooms.Select(r => new RoomListItem(
            r.Id,
            r.Code,
            r.Name,
            r.Playing,
            r.Settings.Visibility.ToString(),
            r.Settings.Visibility == Visibility.Private,
            r.RedPlayerId,
            r.BlackPlayerId,
            r.RedPlayerId is not null && r.BlackPlayerId is not null,
            r.Waiting.Count,
            r.Spectators.Count,
            r.OwnerId
        ));

        // full filter
        proj = q.Full switch
        {
            FullFilter.OnlyFull => proj.Where(x => x.IsFull),
            FullFilter.OnlyOpen => proj.Where(x => !x.IsFull),
            _ => proj
        };

        // owner filter
        if (q.OwnerId is Guid own) proj = proj.Where(x => x.OwnerId == own);

        // participant filter
        if (q.ParticipantId is Guid pid)
        {
            // need original room collections; re-enumerate once to avoid N+1
            var dict = rooms.ToDictionary(r => r.Id);
            proj = proj.Where(x =>
            {
                if (x.RedPlayerId == pid || x.BlackPlayerId == pid) return true;
                var r = dict[x.Id];
                return r.Spectators.Contains(pid) || r.Waiting.Contains(pid);
            });
        }

        // search
        if (!string.IsNullOrWhiteSpace(q.Search))
            proj = proj.Where(x => x.Name.Contains(q.Search, StringComparison.OrdinalIgnoreCase));

        // pagination + ordering
        var ordered = proj
            .OrderBy(x => x.IsFull)           // open first
            .ThenByDescending(x => x.Playing) // playing after open
            .ThenBy(x => x.Code);

        var total  = ordered.Count();
        var page   = Math.Max(1, q.Page);
        var size   = Math.Max(1, q.PageSize);
        var items  = ordered.Skip((page - 1) * size).Take(size).ToList();

        return new PagedResult<RoomListItem>(page, size, total, items);
    }
}
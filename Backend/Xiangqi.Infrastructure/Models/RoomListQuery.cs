namespace Xiangqi.Infrastructure.Models;

public sealed record RoomListQuery(
    Visibility Visibility,     
    FullFilter Full = FullFilter.All,
    Guid? OwnerId = null,
    Guid? ParticipantId = null,
    string Search = null,
    int Page = 1,
    int PageSize = 50
);
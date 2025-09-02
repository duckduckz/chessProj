namespace Xiangqi.Api.Dto;

public record CreateRoomDto(
    Guid OwnerId,
    string Name,
    RoomSettings Settings
);
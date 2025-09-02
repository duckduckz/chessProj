namespace Xiangqi.Api.Dto;

public record SeatDto (
    Guid OwnerId,
    Guid UserId,
    string Role
);
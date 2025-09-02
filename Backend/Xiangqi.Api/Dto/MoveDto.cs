namespace Xiangqi.Api.Dto;

public record MoveDto(
    Guid ByUserId,
    int From,
    int To
);
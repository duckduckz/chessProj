namespace Xiangqi.Api.Dto;

public record ChangePasswordDto(
    string OldPassword,
    string NewPassword
);
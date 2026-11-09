
using Microsoft.AspNetCore.Identity;
using Backend.Application.DTOs;
using Backend.Domain.Entities;

namespace Backend.Application.Mappings;

public static class AuthMapping
{
    public static AuthResponseDto ToResponseDto(
        this UserEntity user,
        string? token = null)
    {
        return new AuthResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Role = user.Role,
            Email = user.Email,
            Token = token
        };
    }
}

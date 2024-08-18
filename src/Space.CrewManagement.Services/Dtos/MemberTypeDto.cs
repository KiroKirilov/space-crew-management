using Space.CrewManagement.Data.Models;

namespace Space.CrewManagement.Services.Dtos;
public record MemberTypeDto(Guid Id, string Name, MemberTypeEnum Type);
namespace DepotDirectApi.Models.DTOs;

public class UserRegionDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int RegionId { get; set; }
    public object? Metadata { get; set; }
    public int? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    
    // Navigation properties
    public UserDto? User { get; set; }
    public RegionDto? Region { get; set; }
}

public class UserRegionCreateDto
{
    public int UserId { get; set; }
    public int RegionId { get; set; }
    public object? Metadata { get; set; }
}

public class UserRegionUpdateDto
{
    public object? Metadata { get; set; }
}

public class AssignUserToRegionDto
{
    public int RegionId { get; set; }
}

public class AssignRegionToUserDto
{
    public int UserId { get; set; }
}

public class UserWithRegionsDto : UserDto
{
    public List<RegionListItemDto> Regions { get; set; } = new List<RegionListItemDto>();
}

public class RegionWithUsersDto : RegionResponseDto
{
    public List<UserListItemDto> Users { get; set; } = new List<UserListItemDto>();
}

public class UserListItemDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Active { get; set; }
}
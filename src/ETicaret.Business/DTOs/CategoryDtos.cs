namespace ETicaret.Business.DTOs;

public record CategoryDto(
    int Id,
    string Name,
    string Slug,
    string? Description,
    string? IconUrl,
    int? ParentCategoryId,
    int DisplayOrder,
    bool IsActive
);

public record CategoryCreateDto(
    string Name,
    string? Description,
    string? IconUrl,
    int? ParentCategoryId,
    int DisplayOrder,
    bool IsActive
);

public record CategoryUpdateDto(
    string Name,
    string? Description,
    string? IconUrl,
    int? ParentCategoryId,
    int DisplayOrder,
    bool IsActive
);

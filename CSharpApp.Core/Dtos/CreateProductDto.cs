namespace CSharpApp.Core.Dtos;

public record class CreateProductDto(
    string? Title,
    decimal? Price,
    string? Description,
    int? CategoryId,
    IEnumerable<string>? Images
);
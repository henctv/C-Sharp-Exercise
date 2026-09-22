using FluentValidation.TestHelper;
using CSharpApp.Core.Dtos;

public class CreateProductDtoValidatorTests
{
    private readonly CreateProductDtoValidator _validator = new();

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ShouldHaveError_ForTitle(string? title)
    {
        // Arrange
        var dto = CreateDto() with { Title = title };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.Title);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void ShouldHaveError_ForDescription(string? description)
    {
        // Arrange
        var dto = CreateDto() with { Description = description };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.Description);
    }

    public static TheoryData<decimal?> InvalidPrices => new TheoryData<decimal?>
    {
        null,
        0m,
        -1m
    };

    [Theory]
    [MemberData(nameof(InvalidPrices))]
    public void ShouldHaveError_ForPrice(decimal? price)
    {
        // Arrange
        var dto = CreateDto() with { Price = price };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.Price);
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-1)]
    public void ShouldHaveError_ForCategoryId(int? categoryId)
    {
        // Arrange
        var dto = CreateDto() with { CategoryId = categoryId };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.CategoryId);
    }

    public static TheoryData<IEnumerable<string>?> InvalidImages => new TheoryData<IEnumerable<string>?>
    {
        null,
        Array.Empty<string>()
    };

    [Theory]
    [MemberData(nameof(InvalidImages))]
    public void ShouldHaveError_ForImages_WhenNullOrEmpty(IEnumerable<string>? images)
    {
        // Arrange
        var dto = CreateDto() with { Images = images };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.Images);
    }

    public static TheoryData<string?> InvalidImageUrls => new TheoryData<string?>
    {
        null,
        "",
        " ",
        "invalid-url"
    };

    [Theory]
    [MemberData(nameof(InvalidImageUrls))]
    public void ShouldHaveError_ForInvalidImageUrls(string? imageUrl)
    {
        // Arrange
        var dto = CreateDto() with { Images = [imageUrl] };

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldHaveValidationErrorFor(dto => dto.Images);
    }

    [Fact]
    public void ShouldNotHaveError()
    {
        // Arrange
        var dto = CreateDto();

        // Act
        var result = _validator.TestValidate(dto);

        // Assert
        result.ShouldNotHaveValidationErrorFor(dto => dto.Images);
    }

    private CreateProductDto CreateDto() => new(
        Title: "Test Title",
        Description: "Test Description",
        Price: 10m,
        CategoryId: 1,
        Images: ["http://example.com/image.jpg"]
    );
}
using FluentValidation;

public class CreateProductDtoValidator : AbstractValidator<CreateProductDto>
{
    public CreateProductDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required.");
        RuleFor(x => x.Price)
            .NotNull()
            .WithMessage("Price is required.")
            .GreaterThan(0m)
            .WithMessage("Price must be greater than 0.");
        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required.");
        RuleFor(x => x.CategoryId)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("CategoryId is required.");
        RuleFor(x => x.Images)
            .NotNull()
            .NotEmpty()
            .WithMessage("Images are required.");

        RuleForEach(x => x.Images)
            .NotEmpty()
            .WithMessage("Each image URL is required.")
            .Must(ValidateImageUrl)
            .WithMessage("Each image URL must be a valid URL.");
    }

    private static bool ValidateImageUrl(string imageUrl)
    {
        return string.IsNullOrWhiteSpace(imageUrl) is false
            && Uri.TryCreate(imageUrl, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
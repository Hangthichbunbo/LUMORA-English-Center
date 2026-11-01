namespace StoryHub.Application.DTOs.Author;

public class AuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Biography { get; set; }
    public int StoryCount { get; set; }
}

public class CreateOrUpdateAuthorDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Biography { get; set; }
}

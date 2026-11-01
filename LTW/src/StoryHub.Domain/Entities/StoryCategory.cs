namespace StoryHub.Domain.Entities;

public class StoryCategory
{
    public int StoryId { get; set; }
    public virtual Story Story { get; set; } = null!;

    public int CategoryId { get; set; }
    public virtual Category Category { get; set; } = null!;
}

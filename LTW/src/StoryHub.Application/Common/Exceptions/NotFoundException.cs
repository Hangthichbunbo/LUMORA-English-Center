namespace StoryHub.Application.Common.Exceptions;

public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message)
    {
    }

    public NotFoundException(string name, object key)
        : base($"Không tìm thấy đối tượng '{name}' với khóa ({key}).")
    {
    }
}

namespace StoryHub.Application.Common.Exceptions;

public class ForbiddenAccessException : Exception
{
    public ForbiddenAccessException(string message = "Bạn không có quyền thực hiện thao tác này.")
        : base(message)
    {
    }
}

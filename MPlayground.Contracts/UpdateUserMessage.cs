namespace MPlayground.Contracts;

public class UpdateUserMessage
{
    public Guid UserId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public DateTime UpdatedAt { get; set; }
}
namespace AudioTravelling.Core.Entities;

public class AccessCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Code { get; set; } = string.Empty;
    public string QrImageUrl { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<AccessSession> Sessions { get; set; } = [];
}

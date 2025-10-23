namespace CoNexus.Api.Domain;

public abstract class BaseEntity
{
	public int Id { get; set; }
	public DateTime Created { get; set; }
	public DateTime LastUpdated { get; set; }
}

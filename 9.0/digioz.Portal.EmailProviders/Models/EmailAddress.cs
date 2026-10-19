namespace digioz.Portal.EmailProviders.Models;

/// <summary>
/// Represents an email address with optional display name
/// </summary>
public class EmailAddress
{
    /// <summary>
    /// Email address (required)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Display name (optional)
    /// </summary>
    public string? Name { get; set; }

    public EmailAddress()
    {
    }

    public EmailAddress(string email, string? name = null)
    {
        Email = email;
        Name = name;
    }

    public override string ToString()
    {
        return string.IsNullOrWhiteSpace(Name) ? Email : $"{Name} <{Email}>";
    }
}

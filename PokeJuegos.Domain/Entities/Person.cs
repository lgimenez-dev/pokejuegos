namespace PokeJuegos.Domain.Entities;

/// <summary>
/// Represents the profile data associated with a user account.
/// </summary>
public sealed class Person : AuditableEntity
{
    /// <summary>Gets the ID of the user this profile belongs to.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Gets the person's full name.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Gets the national identity document number. <see langword="null"/> for OAuth2 users who have not completed their profile.</summary>
    public string? NationalId { get; private set; }

    /// <summary>Gets the phone number. <see langword="null"/> if not provided.</summary>
    public string? Phone { get; private set; }

    /// <summary>Gets the date of birth. <see langword="null"/> if not provided.</summary>
    public DateOnly? BirthDate { get; private set; }

    private Person() { }

    /// <summary>
    /// Creates a new <see cref="Person"/> profile linked to the specified user.
    /// </summary>
    /// <param name="userId">The ID of the owning user.</param>
    /// <param name="name">The person's full name.</param>
    /// <param name="nationalId">The national identity document number, or <see langword="null"/>.</param>
    /// <param name="phone">The phone number, or <see langword="null"/>.</param>
    /// <param name="birthDate">The date of birth, or <see langword="null"/>.</param>
    /// <returns>A new <see cref="Person"/> instance.</returns>
    public static Person Create(
        Guid userId,
        string name,
        string? nationalId,
        string? phone,
        DateOnly? birthDate)
    {
        Person person = new Person();
        person.InitAudit();
        person.UserId = userId;
        person.Name = name;
        person.NationalId = nationalId;
        person.Phone = phone;
        person.BirthDate = birthDate;
        return person;
    }

    /// <summary>
    /// Replaces the person's full name.
    /// </summary>
    /// <param name="name">The new full name.</param>
    public void UpdateName(string name)
    {
        Name = name;
        SetUpdatedAt();
    }
}

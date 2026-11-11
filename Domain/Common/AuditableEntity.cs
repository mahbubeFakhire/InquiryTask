using System;

namespace Domain.Common;

/// <summary>
/// A base class definition for Entities / Domain Models.
/// Perhaps in the future we will changed this to include a list of complete audit records! But not just yet.
/// </summary>
public abstract class AuditableEntity : CreateEntity
{

    /// <summary>
    /// UTC time for when the entity was last modified
    /// </summary>
    public DateTime? LastModifiedAtUtc { get; set; }
}

/// <summary>
/// A base class definition for Entities / Domain Models.
/// Perhaps in the future we will changed this to include a list of complete audit records! But not just yet.
/// </summary>
public abstract class CreateEntity : Entity
{

    /// <summary>
    /// UTC time for when the entity was created
    /// </summary>
    public DateTime CreatedAtUtc { get; set; }
}
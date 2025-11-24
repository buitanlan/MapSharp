namespace MapSharp;

/// <summary>
/// Specifies a custom mapping for a property.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class MapPropertyAttribute : Attribute
{
    /// <summary>
    /// Gets the name of the source property to map from.
    /// </summary>
    public string? SourcePropertyName { get; }

    /// <summary>
    /// Gets a value indicating whether to ignore this property during mapping.
    /// </summary>
    public bool Ignore { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapPropertyAttribute"/> class.
    /// </summary>
    public MapPropertyAttribute()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MapPropertyAttribute"/> class.
    /// </summary>
    /// <param name="sourcePropertyName">The name of the source property to map from.</param>
    public MapPropertyAttribute(string sourcePropertyName)
    {
        SourcePropertyName = sourcePropertyName;
    }
}


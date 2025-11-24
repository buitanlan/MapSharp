namespace MapSharp;

/// <summary>
/// Marks a class to generate mapping methods from the specified source type.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MapFromAttribute"/> class.
/// </remarks>
/// <param name="sourceType">The source type to map from.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class MapFromAttribute(Type sourceType) : Attribute
{
    /// <summary>
    /// Gets the source type to map from.
    /// </summary>
    public Type SourceType { get; } = sourceType;
}


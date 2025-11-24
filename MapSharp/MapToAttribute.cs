namespace MapSharp;

/// <summary>
/// Marks a class to generate mapping methods to the specified target type.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="MapToAttribute"/> class.
/// </remarks>
/// <param name="targetType">The target type to map to.</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public sealed class MapToAttribute(Type targetType) : Attribute
{
    /// <summary>
    /// Gets the target type to map to.
    /// </summary>
    public Type TargetType { get; } = targetType;
}


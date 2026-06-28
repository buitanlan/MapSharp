using Microsoft.CodeAnalysis;

namespace MapSharp.SourceGenerator;

internal static class DiagnosticDescriptors
{
    public static readonly DiagnosticDescriptor ClassMustBePartial = new(
        id: "MAP001",
        title: "Mapping class must be partial",
        messageFormat: "Class '{0}' must be declared partial to use MapSharp mapping attributes",
        category: "MapSharp",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor IncompatiblePropertyMapping = new(
        id: "MAP002",
        title: "Incompatible property mapping",
        messageFormat: "Property '{0}' cannot be mapped between '{1}' and '{2}'",
        category: "MapSharp",
        defaultSeverity: DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    public static readonly DiagnosticDescriptor DuplicateMapToTarget = new(
        id: "MAP003",
        title: "Duplicate MapTo target",
        messageFormat: "Multiple MapTo attributes specify the same target type '{0}'",
        category: "MapSharp",
        defaultSeverity: DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}

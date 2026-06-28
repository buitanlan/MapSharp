#nullable enable
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;

namespace MapSharp.SourceGenerator;

[Generator]
public class MapperGenerator : IIncrementalGenerator
{
    private const string MapFromAttributeName = "MapSharp.MapFromAttribute";
    private const string MapToAttributeName = "MapSharp.MapToAttribute";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var mapFromClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            MapFromAttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => GetSemanticTargetForGeneration(ctx));

        var mapToClasses = context.SyntaxProvider.ForAttributeWithMetadataName(
            MapToAttributeName,
            static (node, _) => node is ClassDeclarationSyntax,
            static (ctx, _) => GetSemanticTargetForGeneration(ctx));

        var classDeclarations = mapFromClasses
            .Collect()
            .Combine(mapToClasses.Collect())
            .Select(static (pair, _) => MergeTargets(pair.Left, pair.Right))
            .SelectMany(static (targets, _) => targets);

        context.RegisterSourceOutput(classDeclarations, static (spc, source) => Execute(source, spc));
    }

    private static IEnumerable<ClassToGenerate> MergeTargets(
        ImmutableArray<ClassToGenerate?> left,
        ImmutableArray<ClassToGenerate?> right)
    {
        return left.Concat(right)
            .Where(static m => m is not null)
            .GroupBy(static m => m!.ClassSymbol, SymbolEqualityComparer.Default)
            .Select(static g => g.First()!);
    }

    private static ClassToGenerate? GetSemanticTargetForGeneration(GeneratorAttributeSyntaxContext context)
    {
        if (context.TargetSymbol is not INamedTypeSymbol classSymbol)
            return null;

        var classDeclaration = (ClassDeclarationSyntax)context.TargetNode;
        var diagnostics = new List<Diagnostic>();

        if (!classDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.ClassMustBePartial,
                classDeclaration.Identifier.GetLocation(),
                classSymbol.Name));
        }

        var mapFromAttributes = new List<INamedTypeSymbol>();
        var mapToAttributes = new List<INamedTypeSymbol>();

        foreach (var attribute in classSymbol.GetAttributes())
        {
            if (IsMapFromAttribute(attribute.AttributeClass) &&
                attribute.ConstructorArguments.Length > 0 &&
                attribute.ConstructorArguments[0].Value is INamedTypeSymbol sourceType)
            {
                mapFromAttributes.Add(sourceType);
            }
            else if (IsMapToAttribute(attribute.AttributeClass) &&
                     attribute.ConstructorArguments.Length > 0 &&
                     attribute.ConstructorArguments[0].Value is INamedTypeSymbol targetType)
            {
                mapToAttributes.Add(targetType);
            }
        }

        foreach (var duplicate in mapToAttributes
                     .GroupBy(static t => t, SymbolEqualityComparer.Default)
                     .Where(static g => g.Count() > 1))
        {
            diagnostics.Add(Diagnostic.Create(
                DiagnosticDescriptors.DuplicateMapToTarget,
                classDeclaration.Identifier.GetLocation(),
                duplicate.Key!.ToDisplayString()));
        }

        if (mapFromAttributes.Count == 0 && mapToAttributes.Count == 0)
            return null;

        ValidatePropertyMappings(
            classSymbol,
            mapFromAttributes,
            mapToAttributes,
            context.SemanticModel.Compilation,
            diagnostics);

        return new ClassToGenerate(
            classSymbol,
            mapFromAttributes,
            mapToAttributes,
            GetPropertyMappings(classSymbol),
            context.SemanticModel.Compilation,
            diagnostics);
    }

    private static void ValidatePropertyMappings(
        INamedTypeSymbol classSymbol,
        List<INamedTypeSymbol> mapFromTypes,
        List<INamedTypeSymbol> mapToTypes,
        Compilation compilation,
        List<Diagnostic> diagnostics)
    {
        var propertyMappings = GetPropertyMappings(classSymbol);

        foreach (var sourceType in mapFromTypes)
        {
            var sourceProperties = GetReadableProperties(sourceType);

            foreach (var mapping in propertyMappings.Values)
            {
                if (mapping.Ignore)
                    continue;

                var sourcePropName = mapping.SourcePropertyName ?? mapping.PropertyName;
                if (!sourceProperties.TryGetValue(sourcePropName, out var sourceProperty))
                    continue;

                if (GetMappingExpression(
                        $"source.{sourcePropName}",
                        sourceProperty.Type,
                        mapping.PropertyType,
                        isMapFrom: true,
                        compilation,
                        sourceType,
                        classSymbol) == null)
                {
                    diagnostics.Add(Diagnostic.Create(
                        DiagnosticDescriptors.IncompatiblePropertyMapping,
                        mapping.Location,
                        mapping.PropertyName,
                        sourceProperty.Type.ToDisplayString(),
                        mapping.PropertyType.ToDisplayString()));
                }
            }
        }

        foreach (var targetType in mapToTypes)
        {
            var targetProperties = GetWritableProperties(targetType);

            foreach (var mapping in propertyMappings.Values)
            {
                if (mapping.Ignore)
                    continue;

                var targetPropName = mapping.SourcePropertyName ?? mapping.PropertyName;
                if (!targetProperties.TryGetValue(targetPropName, out var targetProperty))
                    continue;

                if (GetMappingExpression(
                        $"this.{mapping.PropertyName}",
                        mapping.PropertyType,
                        targetProperty.Type,
                        isMapFrom: false,
                        compilation,
                        classSymbol,
                        targetType) == null)
                {
                    diagnostics.Add(Diagnostic.Create(
                        DiagnosticDescriptors.IncompatiblePropertyMapping,
                        mapping.Location,
                        mapping.PropertyName,
                        mapping.PropertyType.ToDisplayString(),
                        targetProperty.Type.ToDisplayString()));
                }
            }
        }
    }

    private static Dictionary<string, PropertyMapping> GetPropertyMappings(INamedTypeSymbol classSymbol)
    {
        var mappings = new Dictionary<string, PropertyMapping>();

        foreach (var member in classSymbol.GetMembers().OfType<IPropertySymbol>())
        {
            var mapPropertyAttr = member.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass?.Name == "MapPropertyAttribute");

            if (mapPropertyAttr != null)
            {
                var ignore = false;
                string? sourcePropertyName = null;

                foreach (var namedArg in mapPropertyAttr.NamedArguments)
                {
                    if (namedArg.Key == "Ignore" && namedArg.Value.Value is bool ignoreValue)
                    {
                        ignore = ignoreValue;
                    }
                }

                if (mapPropertyAttr.ConstructorArguments.Length > 0 &&
                    mapPropertyAttr.ConstructorArguments[0].Value is string sourceName)
                {
                    sourcePropertyName = sourceName;
                }

                mappings[member.Name] = new PropertyMapping(
                    member.Name,
                    member.Type,
                    sourcePropertyName,
                    ignore,
                    member.Locations.FirstOrDefault() ?? Location.None);
            }
            else
            {
                mappings[member.Name] = new PropertyMapping(
                    member.Name,
                    member.Type,
                    null,
                    false,
                    member.Locations.FirstOrDefault() ?? Location.None);
            }
        }

        return mappings;
    }

    private static void Execute(ClassToGenerate? classToGenerate, SourceProductionContext context)
    {
        if (classToGenerate == null)
            return;

        foreach (var diagnostic in classToGenerate.Diagnostics)
        {
            context.ReportDiagnostic(diagnostic);
        }

        if (classToGenerate.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error))
            return;

        var source = GenerateMappingCode(classToGenerate);
        context.AddSource(GetHintFileName(classToGenerate.ClassSymbol), SourceText.From(source, Encoding.UTF8));
    }

    private static string GetHintFileName(INamedTypeSymbol classSymbol)
    {
        var metadataName = classSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", string.Empty);

        var sanitized = new StringBuilder(metadataName.Length);
        foreach (var ch in metadataName)
        {
            sanitized.Append(char.IsLetterOrDigit(ch) ? ch : '_');
        }

        return $"{sanitized}_Mappings.g.cs";
    }

    private static string GenerateMappingCode(ClassToGenerate classToGenerate)
    {
        var sb = new StringBuilder();
        var className = classToGenerate.ClassSymbol.Name;
        var namespaceName = classToGenerate.ClassSymbol.ContainingNamespace.ToDisplayString();
        var accessibility = GetAccessibilityKeyword(classToGenerate.ClassSymbol);

        sb.AppendLine("// <auto-generated/>");
        sb.AppendLine("#nullable enable");
        sb.AppendLine("using System.Linq;");
        sb.AppendLine();
        sb.AppendLine($"namespace {namespaceName}");
        sb.AppendLine("{");
        sb.AppendLine($"    {accessibility} partial class {className}");
        sb.AppendLine("    {");

        foreach (var sourceType in classToGenerate.MapFromTypes)
        {
            GenerateMapFromMethod(sb, classToGenerate, sourceType);
        }

        foreach (var targetType in classToGenerate.MapToTypes)
        {
            GenerateMapToMethod(sb, classToGenerate, targetType);
        }

        sb.AppendLine("    }");
        sb.AppendLine("}");

        return sb.ToString();
    }

    private static string GetAccessibilityKeyword(INamedTypeSymbol symbol) =>
        symbol.DeclaredAccessibility switch
        {
            Accessibility.Public => "public",
            Accessibility.Internal => "internal",
            Accessibility.Protected => "protected",
            Accessibility.ProtectedOrInternal => "protected internal",
            Accessibility.Private => "private",
            Accessibility.ProtectedAndInternal => "private protected",
            _ => "internal"
        };

    private static void GenerateMapFromMethod(
        StringBuilder sb,
        ClassToGenerate classToGenerate,
        INamedTypeSymbol sourceType)
    {
        var className = classToGenerate.ClassSymbol.Name;
        var sourceTypeName = sourceType.ToDisplayString();

        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine($"        /// Maps from <see cref=\"{sourceTypeName}\"/> to this instance.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine($"        public static {className} MapFrom({sourceTypeName} source)");
        sb.AppendLine("        {");
        sb.AppendLine("            if (source == null) throw new System.ArgumentNullException(nameof(source));");
        sb.AppendLine();
        sb.AppendLine($"            return new {className}");
        sb.AppendLine("            {");

        var sourceProperties = GetReadableProperties(sourceType);
        var mappedProperties = new List<string>();

        foreach (var mapping in classToGenerate.PropertyMappings.Values)
        {
            if (mapping.Ignore)
                continue;

            var sourcePropName = mapping.SourcePropertyName ?? mapping.PropertyName;
            if (!sourceProperties.TryGetValue(sourcePropName, out var sourceProperty))
                continue;

            var mappingExpression = GetMappingExpression(
                $"source.{sourcePropName}",
                sourceProperty.Type,
                mapping.PropertyType,
                isMapFrom: true,
                classToGenerate.Compilation,
                sourceType,
                classToGenerate.ClassSymbol);

            if (mappingExpression != null)
            {
                mappedProperties.Add($"                {mapping.PropertyName} = {mappingExpression}");
            }
        }

        sb.AppendLine(string.Join(",\n", mappedProperties));
        sb.AppendLine("            };");
        sb.AppendLine("        }");

        GenerateListMapFromMethod(sb, className, sourceType);
    }

    private static void GenerateMapToMethod(
        StringBuilder sb,
        ClassToGenerate classToGenerate,
        INamedTypeSymbol targetType)
    {
        var className = classToGenerate.ClassSymbol.Name;
        var targetTypeName = targetType.ToDisplayString();
        var methodName = GetMapToMethodName(classToGenerate.ClassSymbol, targetType);

        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine($"        /// Maps this instance to <see cref=\"{targetTypeName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine($"        public {targetTypeName} {methodName}()");
        sb.AppendLine("        {");
        sb.AppendLine($"            return new {targetTypeName}");
        sb.AppendLine("            {");

        var targetProperties = GetWritableProperties(targetType);
        var mappedProperties = new List<string>();

        foreach (var mapping in classToGenerate.PropertyMappings.Values)
        {
            if (mapping.Ignore)
                continue;

            var targetPropName = mapping.SourcePropertyName ?? mapping.PropertyName;
            if (!targetProperties.TryGetValue(targetPropName, out var targetProperty))
                continue;

            var mappingExpression = GetMappingExpression(
                $"this.{mapping.PropertyName}",
                mapping.PropertyType,
                targetProperty.Type,
                isMapFrom: false,
                classToGenerate.Compilation,
                classToGenerate.ClassSymbol,
                targetType);

            if (mappingExpression != null)
            {
                mappedProperties.Add($"                {targetPropName} = {mappingExpression}");
            }
        }

        sb.AppendLine(string.Join(",\n", mappedProperties));
        sb.AppendLine("            };");
        sb.AppendLine("        }");

        GenerateListMapToMethod(sb, className, targetType, methodName);
    }

    private static Dictionary<string, IPropertySymbol> GetReadableProperties(INamedTypeSymbol type) =>
        type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && p.GetMethod != null)
            .ToDictionary(p => p.Name, p => p);

    private static Dictionary<string, IPropertySymbol> GetWritableProperties(INamedTypeSymbol type) =>
        type.GetMembers()
            .OfType<IPropertySymbol>()
            .Where(p => p.DeclaredAccessibility == Accessibility.Public && p.SetMethod != null)
            .ToDictionary(p => p.Name, p => p);

    private static string? GetMappingExpression(
        string sourceExpression,
        ITypeSymbol sourceType,
        ITypeSymbol targetType,
        bool isMapFrom,
        Compilation compilation,
        INamedTypeSymbol sourceModelType,
        INamedTypeSymbol targetModelType)
    {
        var unwrappedSourceType = GetUnwrappedType(sourceType);
        var unwrappedTargetType = GetUnwrappedType(targetType);
        var isSourceNullable = IsNullableType(sourceType);

        if (SymbolEqualityComparer.Default.Equals(unwrappedSourceType, unwrappedTargetType) ||
            IsImplicitlyConvertible(unwrappedSourceType, unwrappedTargetType, compilation))
        {
            return sourceExpression;
        }

        var sourceElementType = GetCollectionElementType(unwrappedSourceType);
        var targetElementType = GetCollectionElementType(unwrappedTargetType);

        if (sourceElementType != null && targetElementType != null)
        {
            return GenerateCollectionMapping(
                sourceExpression,
                sourceElementType,
                targetElementType,
                unwrappedTargetType,
                isMapFrom,
                compilation);
        }

        if (isMapFrom)
        {
            if (HasMapFromAttribute(unwrappedTargetType, unwrappedSourceType))
            {
                var targetTypeName = GetTypeNameWithoutNullable(unwrappedTargetType);
                return isSourceNullable
                    ? $"{sourceExpression} != null ? {targetTypeName}.MapFrom({sourceExpression}) : null"
                    : $"{targetTypeName}.MapFrom({sourceExpression})";
            }
        }
        else
        {
            var mapToMethodName = GetMapToMethodNameForNested(unwrappedSourceType, unwrappedTargetType);
            if (mapToMethodName != null)
            {
                return isSourceNullable
                    ? $"{sourceExpression}?.{mapToMethodName}()"
                    : $"{sourceExpression}.{mapToMethodName}()";
            }
        }

        if (AreTypesCompatible(unwrappedSourceType, unwrappedTargetType))
        {
            return sourceExpression;
        }

        return null;
    }

    private static ITypeSymbol GetUnwrappedType(ITypeSymbol type)
    {
        if (type is INamedTypeSymbol namedType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0];
        }

        return type;
    }

    private static bool IsNullableType(ITypeSymbol type)
    {
        if (type.NullableAnnotation == NullableAnnotation.Annotated)
            return true;

        if (type is INamedTypeSymbol namedType &&
            namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            return true;
        }

        return false;
    }

    private static string? GenerateCollectionMapping(
        string sourceExpression,
        ITypeSymbol sourceElementType,
        ITypeSymbol targetElementType,
        ITypeSymbol targetCollectionType,
        bool isMapFrom,
        Compilation compilation)
    {
        var unwrappedSourceElement = GetUnwrappedType(sourceElementType);
        var unwrappedTargetElement = GetUnwrappedType(targetElementType);

        string elementMapping;

        if (SymbolEqualityComparer.Default.Equals(unwrappedSourceElement, unwrappedTargetElement) ||
            IsImplicitlyConvertible(unwrappedSourceElement, unwrappedTargetElement, compilation))
        {
            elementMapping = "x";
        }
        else if (isMapFrom && HasMapFromAttribute(unwrappedTargetElement, unwrappedSourceElement))
        {
            var targetElementTypeName = GetTypeNameWithoutNullable(unwrappedTargetElement);
            elementMapping = $"{targetElementTypeName}.MapFrom(x)";
        }
        else if (!isMapFrom)
        {
            var mapToMethodName = GetMapToMethodNameForNested(unwrappedSourceElement, unwrappedTargetElement);
            if (mapToMethodName == null)
            {
                return null;
            }

            elementMapping = $"x.{mapToMethodName}()";
        }
        else if (AreTypesCompatible(unwrappedSourceElement, unwrappedTargetElement))
        {
            elementMapping = "x";
        }
        else
        {
            return null;
        }

        var targetTypeString = targetCollectionType.ToDisplayString();
        var selectExpression = $"{sourceExpression}?.Select(x => {elementMapping})";

        if (targetTypeString.StartsWith("System.Collections.Generic.List<") ||
            targetTypeString.Contains("List<"))
        {
            return $"{selectExpression}?.ToList()";
        }

        if (targetTypeString.Contains("[]"))
        {
            return $"{selectExpression}?.ToArray()";
        }

        if (targetTypeString.StartsWith("System.Collections.Generic.IList<") ||
            targetTypeString.StartsWith("System.Collections.Generic.ICollection<"))
        {
            return $"{selectExpression}?.ToList()";
        }

        return selectExpression;
    }

    private static ITypeSymbol? GetCollectionElementType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol arrayType)
        {
            return arrayType.ElementType;
        }

        if (type is INamedTypeSymbol namedType && namedType.IsGenericType)
        {
            var typeDefinition = namedType.ConstructedFrom.ToDisplayString();

            if (typeDefinition.Contains("IEnumerable<") ||
                typeDefinition.Contains("ICollection<") ||
                typeDefinition.Contains("IList<") ||
                typeDefinition.Contains("List<") ||
                typeDefinition.Contains("IReadOnlyList<") ||
                typeDefinition.Contains("IReadOnlyCollection<") ||
                typeDefinition.Contains("HashSet<") ||
                typeDefinition.Contains("ISet<"))
            {
                return namedType.TypeArguments.FirstOrDefault();
            }

            foreach (var iface in namedType.AllInterfaces)
            {
                if (iface.IsGenericType &&
                    iface.ConstructedFrom.ToDisplayString().Contains("IEnumerable<"))
                {
                    return iface.TypeArguments.FirstOrDefault();
                }
            }
        }

        return null;
    }

    private static bool HasMapFromAttribute(ITypeSymbol targetType, ITypeSymbol sourceType)
    {
        var namedTargetType = GetNamedTypeSymbol(targetType);
        if (namedTargetType == null)
            return false;

        var namedSourceType = GetNamedTypeSymbol(GetUnwrappedType(sourceType));
        if (namedSourceType == null)
            return false;

        var sourceTypeName = namedSourceType.ToDisplayString();

        foreach (var attr in namedTargetType.GetAttributes())
        {
            if (IsMapFromAttribute(attr.AttributeClass) &&
                attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is INamedTypeSymbol attrSourceType &&
                (attrSourceType.ToDisplayString() == sourceTypeName ||
                 SymbolEqualityComparer.Default.Equals(attrSourceType, namedSourceType)))
            {
                return true;
            }
        }

        return false;
    }

    private static INamedTypeSymbol? GetNamedTypeSymbol(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol namedType)
            return null;

        if (namedType.ConstructedFrom.SpecialType == SpecialType.System_Nullable_T)
        {
            return namedType.TypeArguments[0] as INamedTypeSymbol;
        }

        return namedType;
    }

    private static string GetTypeNameWithoutNullable(ITypeSymbol type)
    {
        var namedType = GetNamedTypeSymbol(type);
        if (namedType == null)
            return type.ToDisplayString();

        return namedType.ToDisplayString(new SymbolDisplayFormat(
            globalNamespaceStyle: SymbolDisplayGlobalNamespaceStyle.Omitted,
            typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameAndContainingTypesAndNamespaces,
            genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters,
            miscellaneousOptions: SymbolDisplayMiscellaneousOptions.None));
    }

    private static List<INamedTypeSymbol> GetMapToTargets(INamedTypeSymbol sourceType)
    {
        var targets = new List<INamedTypeSymbol>();

        foreach (var attr in sourceType.GetAttributes())
        {
            if (IsMapToAttribute(attr.AttributeClass) &&
                attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is INamedTypeSymbol targetType)
            {
                targets.Add(targetType);
            }
        }

        return targets;
    }

    private static string GetMapToMethodName(INamedTypeSymbol sourceType, INamedTypeSymbol targetType)
    {
        var mapToTargets = GetMapToTargets(sourceType);
        if (mapToTargets.Count <= 1)
            return "MapTo";

        return $"MapTo{GetSimpleTypeName(targetType)}";
    }

    private static string? GetMapToMethodNameForNested(ITypeSymbol sourceType, ITypeSymbol targetType)
    {
        var namedSourceType = GetNamedTypeSymbol(sourceType);
        var namedTargetType = GetNamedTypeSymbol(GetUnwrappedType(targetType));
        if (namedSourceType == null || namedTargetType == null)
            return null;

        var targetTypeName = namedTargetType.ToDisplayString();

        foreach (var attr in namedSourceType.GetAttributes())
        {
            if (IsMapToAttribute(attr.AttributeClass) &&
                attr.ConstructorArguments.Length > 0 &&
                attr.ConstructorArguments[0].Value is INamedTypeSymbol attrTargetType &&
                (attrTargetType.ToDisplayString() == targetTypeName ||
                 SymbolEqualityComparer.Default.Equals(attrTargetType, namedTargetType)))
            {
                return GetMapToMethodName(namedSourceType, namedTargetType);
            }
        }

        return null;
    }

    private static string GetSimpleTypeName(INamedTypeSymbol type)
    {
        if (type.IsGenericType)
        {
            var baseName = type.Name.Split('`')[0];
            var typeArgs = string.Concat(type.TypeArguments.Select(arg =>
                arg is INamedTypeSymbol namedArg ? GetSimpleTypeName(namedArg) : arg.Name));
            return baseName + typeArgs;
        }

        return type.Name;
    }

    private static bool IsImplicitlyConvertible(ITypeSymbol source, ITypeSymbol target, Compilation compilation)
    {
        if (SymbolEqualityComparer.Default.Equals(source, target))
            return true;

        var conversion = compilation.ClassifyCommonConversion(source, target);
        return conversion.Exists && conversion.IsImplicit;
    }

    private static bool AreTypesCompatible(ITypeSymbol source, ITypeSymbol target)
    {
        if (SymbolEqualityComparer.Default.Equals(source, target))
            return true;

        if (source is INamedTypeSymbol namedSource && target is INamedTypeSymbol namedTarget)
        {
            var currentBase = namedSource.BaseType;
            while (currentBase != null)
            {
                if (SymbolEqualityComparer.Default.Equals(currentBase, namedTarget))
                    return true;

                currentBase = currentBase.BaseType;
            }

            if (namedSource.AllInterfaces.Any(i => SymbolEqualityComparer.Default.Equals(i, namedTarget)))
                return true;
        }

        return false;
    }

    private static void GenerateListMapFromMethod(StringBuilder sb, string className, INamedTypeSymbol sourceType)
    {
        var sourceTypeName = sourceType.ToDisplayString();

        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine($"        /// Maps a collection of <see cref=\"{sourceTypeName}\"/> to a list of {className}.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine($"        public static System.Collections.Generic.List<{className}> MapFrom(System.Collections.Generic.IEnumerable<{sourceTypeName}>? sources)");
        sb.AppendLine("        {");
        sb.AppendLine($"            if (sources == null) return new System.Collections.Generic.List<{className}>();");
        sb.AppendLine("            return sources.Select(MapFrom).ToList();");
        sb.AppendLine("        }");
    }

    private static void GenerateListMapToMethod(
        StringBuilder sb,
        string className,
        INamedTypeSymbol targetType,
        string methodName)
    {
        var targetTypeName = targetType.ToDisplayString();

        sb.AppendLine();
        sb.AppendLine("        /// <summary>");
        sb.AppendLine($"        /// Maps a collection of {className} to a list of <see cref=\"{targetTypeName}\"/>.");
        sb.AppendLine("        /// </summary>");
        sb.AppendLine($"        public static System.Collections.Generic.List<{targetTypeName}> {methodName}(System.Collections.Generic.IEnumerable<{className}>? sources)");
        sb.AppendLine("        {");
        sb.AppendLine($"            if (sources == null) return new System.Collections.Generic.List<{targetTypeName}>();");
        sb.AppendLine($"            return sources.Select(x => x.{methodName}()).ToList();");
        sb.AppendLine("        }");
    }

    private static bool IsMapFromAttribute(INamedTypeSymbol? attributeClass) =>
        attributeClass?.ToDisplayString() == MapFromAttributeName ||
        attributeClass?.Name == "MapFromAttribute";

    private static bool IsMapToAttribute(INamedTypeSymbol? attributeClass) =>
        attributeClass?.ToDisplayString() == MapToAttributeName ||
        attributeClass?.Name == "MapToAttribute";

    private sealed class ClassToGenerate(
        INamedTypeSymbol classSymbol,
        List<INamedTypeSymbol> mapFromTypes,
        List<INamedTypeSymbol> mapToTypes,
        Dictionary<string, PropertyMapping> propertyMappings,
        Compilation compilation,
        List<Diagnostic> diagnostics)
    {
        public INamedTypeSymbol ClassSymbol { get; } = classSymbol;
        public List<INamedTypeSymbol> MapFromTypes { get; } = mapFromTypes;
        public List<INamedTypeSymbol> MapToTypes { get; } = mapToTypes;
        public Dictionary<string, PropertyMapping> PropertyMappings { get; } = propertyMappings;
        public Compilation Compilation { get; } = compilation;
        public List<Diagnostic> Diagnostics { get; } = diagnostics;
    }

    private sealed class PropertyMapping(
        string propertyName,
        ITypeSymbol propertyType,
        string? sourcePropertyName,
        bool ignore,
        Location location)
    {
        public string PropertyName { get; } = propertyName;
        public ITypeSymbol PropertyType { get; } = propertyType;
        public string? SourcePropertyName { get; } = sourcePropertyName;
        public bool Ignore { get; } = ignore;
        public Location Location { get; } = location;
    }
}

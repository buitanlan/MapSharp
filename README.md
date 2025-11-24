# MapSharp

A high-performance, compile-time auto mapper for .NET 10 using source generators.

## Features

- ✨ **Zero runtime overhead** - All mapping code is generated at compile time
- 🚀 **High performance** - No reflection, no IL emit, just pure generated code
- 🎯 **Type-safe** - Compile-time errors for invalid mappings
- 🔧 **Customizable** - Support for custom property mappings and ignored properties
- 📦 **Lightweight** - Minimal dependencies
- 🧪 **Well-tested** - Comprehensive unit test coverage with xUnit

## Installation

Add the MapSharp package reference to your project:

```xml
<ItemGroup>
  <ProjectReference Include="MapSharp\MapSharp.csproj" />
  <ProjectReference Include="MapSharp.SourceGenerator\MapSharp.SourceGenerator.csproj"
                    OutputItemType="Analyzer"
                    ReferenceOutputAssembly="false" />
</ItemGroup>
```

## Quick Start

### 1. Define your models

```csharp
public class PersonDto
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}
```

### 2. Add mapping attributes

```csharp
using MapSharp;

[MapFrom(typeof(PersonDto))]
[MapTo(typeof(PersonDto))]
public partial class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}
```

**Note:** The class must be marked as `partial` for the source generator to add the mapping methods.

### 3. Use the generated methods

```csharp
// Map from DTO to domain model
var dto = new PersonDto 
{ 
    FirstName = "John", 
    LastName = "Doe", 
    Age = 30,
    Email = "john.doe@example.com"
};

var person = Person.MapFrom(dto);

// Map from domain model to DTO
var resultDto = person.MapTo();
```

## Advanced Usage

### Custom Property Mapping

Map properties with different names using the `MapProperty` attribute:

```csharp
[MapFrom(typeof(UserDto))]
public partial class UserProfile
{
    [MapProperty("Username")]
    public string Name { get; set; }
    
    public string Email { get; set; }
}
```

### Ignore Properties

Exclude properties from mapping using the `Ignore` flag:

```csharp
[MapFrom(typeof(UserDto))]
public partial class UserProfile
{
    public string Name { get; set; }
    
    public string Email { get; set; }
    
    [MapProperty(Ignore = true)]
    public DateTime CreatedAt { get; set; }
}
```

### Multiple Mappings

You can add multiple mapping attributes to support multiple source/target types:

```csharp
[MapFrom(typeof(PersonDto))]
[MapFrom(typeof(PersonViewModel))]
[MapTo(typeof(PersonDto))]
public partial class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
}
```

## Attributes

### MapFromAttribute

Generates a static `MapFrom` method to create an instance from the source type.

```csharp
[MapFrom(typeof(SourceType))]
public partial class TargetType
{
    // Generated method:
    // public static TargetType MapFrom(SourceType source)
}
```

### MapToAttribute

Generates an instance `MapTo` method to create a target type instance.

```csharp
[MapTo(typeof(TargetType))]
public partial class SourceType
{
    // Generated method:
    // public TargetType MapTo()
}
```

### MapPropertyAttribute

Customizes property mapping behavior.

```csharp
// Map from a different source property name
[MapProperty("OldPropertyName")]
public string NewPropertyName { get; set; }

// Ignore a property during mapping
[MapProperty(Ignore = true)]
public string IgnoredProperty { get; set; }
```

## How It Works

MapSharp uses C# source generators to analyze your code at compile time and generate mapping methods. This approach provides several benefits:

1. **No runtime overhead** - Mapping code is generated during compilation
2. **Type safety** - Compilation fails if mappings are invalid
3. **Debuggable** - You can step through the generated code
4. **IDE support** - Full IntelliSense and code navigation

## Project Structure

- **MapSharp** - Core library with attribute definitions
- **MapSharp.SourceGenerator** - Source generator implementation
- **MapSharp.Tests** - Comprehensive xUnit test suite

## Building from Source

```bash
# Build the solution
dotnet build MapSharp.slnx

# Run tests
dotnet test MapSharp.Tests/MapSharp.Tests.csproj

# Clean build artifacts
dotnet clean MapSharp.slnx
```

## Requirements

- .NET 10 SDK or later
- C# 13 or later

## Test Coverage

The library includes comprehensive unit tests covering:

- ✅ Basic property mapping
- ✅ Nullable value handling
- ✅ Custom property name mapping
- ✅ Ignored properties
- ✅ Round-trip mapping
- ✅ Multiple instance mapping
- ✅ Special characters and edge cases
- ✅ Attribute validation
- ✅ Null argument validation

Run tests with:

```bash
dotnet test MapSharp.Tests/MapSharp.Tests.csproj
```

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

## License

This project is available for use under your chosen license.

## Example Output

The source generator creates code like this:

```csharp
// <auto-generated/>
#nullable enable

namespace YourNamespace
{
    public partial class Person
    {
        /// <summary>
        /// Maps from <see cref="PersonDto"/> to this instance.
        /// </summary>
        public static Person MapFrom(PersonDto source)
        {
            if (source == null) throw new System.ArgumentNullException(nameof(source));

            return new Person
            {
                FirstName = source.FirstName,
                LastName = source.LastName,
                Age = source.Age,
                Email = source.Email
            };
        }

        /// <summary>
        /// Maps this instance to <see cref="PersonDto"/>.
        /// </summary>
        public PersonDto MapTo()
        {
            return new PersonDto
            {
                FirstName = this.FirstName,
                LastName = this.LastName,
                Age = this.Age,
                Email = this.Email
            };
        }
    }
}
```

## Performance

Because MapSharp uses source generators, the mapping code is as fast as hand-written mapping code. There is no reflection, no dynamic code generation, and no runtime overhead.

## Comparison with Other Mappers

| Feature | MapSharp | AutoMapper | Mapster |
|---------|----------|------------|---------|
| Compile-time generation | ✅ | ❌ | ❌ |
| Zero runtime overhead | ✅ | ❌ | ❌ |
| No reflection | ✅ | ❌ | ❌ |
| Type safety | ✅ | ⚠️ | ⚠️ |
| Configuration required | ❌ | ✅ | ✅ |

---

**MapSharp** - Fast, type-safe, compile-time object mapping for .NET 10


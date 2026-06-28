# MapSharp

[![CI](https://github.com/buitanlan/MapSharp/actions/workflows/ci.yml/badge.svg)](https://github.com/buitanlan/MapSharp/actions/workflows/ci.yml)
[![NuGet](https://img.shields.io/nuget/v/MapSharp.svg)](https://www.nuget.org/packages/MapSharp)
[![NuGet Downloads](https://img.shields.io/nuget/dt/MapSharp.svg)](https://www.nuget.org/packages/MapSharp)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A high-performance, compile-time auto mapper for .NET using source generators. Supports .NET 8, .NET 9, and .NET 10.

## Features

- ✨ **Zero runtime overhead** - All mapping code is generated at compile time
- 🚀 **High performance** - No reflection, no IL emit, just pure generated code
- 🎯 **Type-safe** - Compile-time diagnostics for invalid mappings
- 🔧 **Customizable** - Support for custom property mappings and ignored properties
- 🔄 **Nested mapping** - Automatically maps complex nested objects
- 📋 **Collection mapping** - Seamlessly maps lists and collections of objects
- 🙈 **Auto-ignore** - Unmatched properties are automatically skipped
- 📦 **Lightweight** - Minimal dependencies
- 🧪 **Well-tested** - Comprehensive unit test coverage with xUnit (49 tests)

## Installation

Install MapSharp via NuGet:

```bash
dotnet add package MapSharp
```

Or via the Package Manager Console:

```powershell
Install-Package MapSharp
```

Or add directly to your `.csproj`:

```xml
<ItemGroup>
  <PackageReference Include="MapSharp" Version="1.0.0" />
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

### Auto-Ignore Unmatched Properties

Properties that don't exist in the source/target type are automatically ignored - no explicit configuration needed:

```csharp
public class CustomerDto
{
    public string Name { get; set; }
    public string Email { get; set; }
}

[MapFrom(typeof(CustomerDto))]
public partial class Customer
{
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; } // Auto-ignored - not in CustomerDto
}
```

### Nested Object Mapping

Complex nested objects are automatically mapped if they have mapping attributes:

```csharp
public class AddressDto { /* ... */ }
public class CustomerDto
{
    public string Name { get; set; }
    public AddressDto? ShippingAddress { get; set; }
}

[MapFrom(typeof(AddressDto))]
[MapTo(typeof(AddressDto))]
public partial class Address { /* ... */ }

[MapFrom(typeof(CustomerDto))]
[MapTo(typeof(CustomerDto))]
public partial class Customer
{
    public string Name { get; set; }
    public Address? ShippingAddress { get; set; } // Auto-maps using Address.MapFrom()
}
```

### Collection/List Mapping

Collections of mapped types are automatically transformed:

```csharp
public class OrderItemDto { /* ... */ }
public class CustomerDto
{
    public string Name { get; set; }
    public List<OrderItemDto>? Orders { get; set; }
    public List<string>? Tags { get; set; } // Simple types copied directly
}

[MapFrom(typeof(OrderItemDto))]
[MapTo(typeof(OrderItemDto))]
public partial class OrderItem { /* ... */ }

[MapFrom(typeof(CustomerDto))]
public partial class Customer
{
    public string Name { get; set; }
    public List<OrderItem>? Orders { get; set; } // Each item mapped using OrderItem.MapFrom()
    public List<string>? Tags { get; set; }      // Copied directly
}
```

The generator also creates bulk mapping methods:

```csharp
// Map a list of DTOs to domain models
var dtos = new List<CustomerDto> { /* ... */ };
var customers = Customer.MapFrom(dtos);

// Map a list of domain models to DTOs
var customers = new List<Customer> { /* ... */ };
var dtos = Customer.MapTo(customers);
```

### Multiple Mappings

You can add multiple mapping attributes to support multiple source/target types:

```csharp
[MapFrom(typeof(PersonDto))]
[MapFrom(typeof(PersonViewModel))]
[MapTo(typeof(PersonDto))]
[MapTo(typeof(PersonSummaryDto))]
public partial class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public int Age { get; set; }
    public string Email { get; set; }
}
```

When a single `[MapTo]` attribute is used, the generated method is named `MapTo()`. With multiple `[MapTo]` attributes, methods are named after the target type (e.g. `MapToPersonDto()`, `MapToPersonSummaryDto()`).

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
    // Single MapTo: public TargetType MapTo()
    // Multiple MapTo: public TargetType MapToTargetType(), public OtherTarget MapToOtherTarget()
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
2. **Type safety** - Diagnostics warn about incompatible mappings; non-partial classes produce compile errors
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

- .NET 8, .NET 9, or .NET 10 runtime (library targets all three)
- .NET 10 SDK recommended for building all target frameworks locally

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

## CI/CD

This project uses GitHub Actions for continuous integration and deployment:

| Workflow | Trigger | Purpose |
|----------|---------|---------|
| **CI** | Push/PR to main/develop | Build, test on multiple platforms |
| **Publish** | Release published | Build and publish to NuGet.org |
| **Prerelease** | Manual workflow dispatch | Publish prerelease versions to NuGet.org |

### Publishing a Release

1. Update version in `Directory.Build.props`
2. Create a new GitHub Release with a tag (e.g., `v1.0.0`)
3. The publish workflow will automatically build and push to NuGet.org

### Required Secrets

- `NUGET_API_KEY`: Your NuGet.org API key with push permissions

## Contributing

Contributions are welcome! Please feel free to submit issues and pull requests.

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

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
| No runtime configuration | ✅ | ❌ | ❌ |
| Custom mapping configuration | ✅ | ✅ | ✅ |

---

**MapSharp** - Fast, type-safe, compile-time object mapping for .NET 8+


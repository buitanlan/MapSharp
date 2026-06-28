namespace MapSharp.Tests.Models;

[MapFrom(typeof(PersonDto))]
[MapFrom(typeof(PersonViewModel))]
[MapTo(typeof(PersonDto))]
[MapTo(typeof(PersonSummaryDto))]
public partial class MultiMapPerson
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public string Email { get; set; } = string.Empty;
}

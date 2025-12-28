namespace MapSharp.Tests.Models;

[MapFrom(typeof(AddressDto))]
[MapTo(typeof(AddressDto))]
public partial class Address
{
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
}

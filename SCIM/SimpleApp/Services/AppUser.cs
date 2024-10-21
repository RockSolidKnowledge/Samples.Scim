using System.Drawing;
using Rsk.AspNetCore.Scim.Models;

namespace SimpleApp.Services;

public class AppUser
{
    public AppUser()
    {
        Id = Guid.NewGuid().ToString();
        Roles = new List<AppRole>();
    }

    public AppUser(string id) : this()
    {
        Id = id;
    }

    public string Id { get;  }
    public string? Username { get; set; }
    public string? Locale { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Department { get; set; }
    public bool IsDisabled { get; set; }

    public ICollection<AppRole> Roles { get; set; }

    public string? NormalizedUsername { get; set; }
    public string? Nickname { get; set; }
    public string? Title { get; set; }
    public string? ProfileUrl { get; set; }
    public string? Timezone { get; set; }
    public string? DisplayName { get; set; }

    public string? Formatted { get; set; }
    public string? HonorificSuffix { get; set; }
    public string? HonorificPrefix { get; set; }
    public string? MiddleName { get; set; }
    public string? PreferredLanguage { get; set; }
    public string? EmployeeNumber { get; set; }
    public string? Organization { get; set; }
    public string? Division { get; set; }
    public string? CostCenter { get; set; }
    public string? UserType { get; set; }
    public string? Email { get; set; }
    public AppAddress? Address { get; set; }
    public ICollection<AppPhoneNumber>? Phones { get; set; }
}

public class AppAddress
{
    public int Id { get; set; }
    public string Country { get;set; }
    public string PostalCode { get;set; }
    public string Region { get;set; }
    public string Locality { get;set; }
    public string StreetAddress { get;set; }
    public string Formatted { get;set; }
}

public class AppPhoneNumber
{
    public AppPhoneNumber(string type)
    {
        Type = type;
    }

    public AppPhoneNumber()
    {

    }

    public int Id { get; set; }
    public string Value { get; set; }
    public bool Primary { get; set; }
    public string Type { get; set; }
}
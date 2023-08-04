using Rsk.AspNetCore.Scim.Models;

namespace SimpleApp;

public class Employment : ResourceExtension
{
    public DateTime StartDate { get; set; }
}
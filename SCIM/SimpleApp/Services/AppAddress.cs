namespace SimpleApp.Services;

public class AppAddress
{
    public int Id { get; set; }
    public string Country { get;set; }
    public string PostalCode { get;set; }
    public string Region { get;set; }
    public string Locality { get;set; }
    public string StreetAddress { get;set; }
    public string Formatted { get;set; }
    public bool Primary { get; set; }
    public string Type { get; set; }
}
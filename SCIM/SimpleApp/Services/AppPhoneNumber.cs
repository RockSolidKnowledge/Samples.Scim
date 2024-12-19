namespace SimpleApp.Services;

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
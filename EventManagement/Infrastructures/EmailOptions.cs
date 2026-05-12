namespace EventManagement.Infrastructures;

public class EmailOptions
{
    public bool IsEnabled { get; set; }
    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool EnableSsl { get; set; } = true;
    public string FromName { get; set; } = "GetTicket";
    public string FromEmail { get; set; } = "no-reply@getticket.local";
}

public class FrontendOptions
{
    public string BaseUrl { get; set; } = "http://localhost:3000";
}

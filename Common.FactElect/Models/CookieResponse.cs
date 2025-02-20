using System.Net;

namespace Common.FactElect.Models;

public class CookieResponse
{
    public CookieContainer Cookie { get; set; }
    public string Html { get; set; }
}

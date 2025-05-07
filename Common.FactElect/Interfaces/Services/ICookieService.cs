namespace Common.FactElect.Interfaces.Services;

public interface ICookieService
{
    Task<CookieResponse> GetAsync();
}
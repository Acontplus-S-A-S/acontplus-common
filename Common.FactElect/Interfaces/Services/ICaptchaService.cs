namespace Common.FactElect.Interfaces.Services;

public interface ICaptchaService
{
    Task<string> ValidateAsync(string html, CookieContainer cookies);
}
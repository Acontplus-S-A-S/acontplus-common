namespace Common.FactElect.Interfaces.Services;

public interface ICedulaService
{
    Task<CedulaModel> GetCedulaSriAsync(string numeroCedula);
}
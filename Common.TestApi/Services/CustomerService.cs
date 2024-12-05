using Common.Infrastructure.Repository.Interfaces;
using System.Data;

namespace Common.TestApi.Services;

public interface ICustomerService
{
    Task<DataTable> GetByIdCardAsync(Dictionary<string, object> parameters);
}
public class CustomerService(IAdoSqlServer adoSqlServer) : ICustomerService
{
    public Task<DataTable> GetByIdCardAsync(Dictionary<string, object> parameters)
    {
        return adoSqlServer.GetDataTableAsync("Customer.Customer_Get", parameters);
    }
}

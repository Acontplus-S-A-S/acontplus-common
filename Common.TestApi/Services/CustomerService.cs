using Common.Infrastructure.Repository.Interfaces;

namespace Common.TestApi.Services;

public interface ICustomerService
{
    Task<DataTable> GetByIdCardAsync(Dictionary<string, object> parameters);
}
public class CustomerService(IAdoRepository adoRepository) : ICustomerService
{
    public Task<DataTable> GetByIdCardAsync(Dictionary<string, object> parameters)
    {
        return adoRepository.GetDataTableAsync("Customer.Customer_IDCard_Get", parameters);
    }
}

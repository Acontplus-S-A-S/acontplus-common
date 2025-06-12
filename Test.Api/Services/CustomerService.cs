using System.Data;
using Common.Core.Abstractions;
namespace Test.Api.Services;

public interface ICustomerService
{
    Task<DataTable> GetByIdCardAsync(Dictionary<string, object> parameters);
}
public class CustomerService(IAdoRepository adoRepository) : ICustomerService
{
    public async Task<DataTable> GetByIdCardAsync(Dictionary<string, object> parameters)
    {
        var ds = await adoRepository.GetDataSetAsync("Customer.Customer_IDCard_Get", parameters, withTableNames: false);
        return ds.Tables[0];
    }
}

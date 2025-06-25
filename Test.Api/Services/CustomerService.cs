using System.Data;
using Common.Core.Abstractions;
using Common.Core.DTOs.Ado;
namespace Test.Api.Services;

public interface ICustomerService
{
    Task<DataTable> GetByIdCardAsync(Dictionary<string, object> parameters);
}
public class CustomerService(IAdoRepository adoRepository) : ICustomerService
{
    public async Task<DataTable> GetByIdCardAsync(Dictionary<string, object> parameters)
    {
        var options = new CommandOptionsDto
        {
            CommandTimeout = 0, // No timeout
            WithTableNames = false // Do not include table names in the result
        };
        var ds = await adoRepository.GetDataSetAsync("Customer.Customer_IDCard_Get", parameters, options);
        return ds.Tables[0];
    }
}

using Common.Core.Validation;
using Common.FactElect.Services;

namespace Common.TestApi.Controllers;

public class CustomerController(
    IRucService rucService,
    ICedulaService cedulaService,
    ICustomerService customerService)
    : BaseApiController
{
    [HttpGet("GetRucSri")]
    public async Task<RucModel> GetRucSri(string ruc, bool sriOnly = false)
    {
        if (sriOnly)
        {
            return await rucService.GetRucSriAsync(ruc);
        }

        var parameters = new Dictionary<string, object> { { "id", ruc } };
        var dt = await customerService.GetByIdCardAsync(parameters);

        if (!DataValidation.DataTableIsNull(dt))
        {
            return DataTableMapper.MapDataRowToModel<RucModel>(dt.Rows[0]);
        }

        return await rucService.GetRucSriAsync(ruc);
    }
    [HttpGet("GetCedulaSri")]
    public async Task<CedulaModel> GetCedulaSri(string ruc, bool sriOnly = false)
    {
        if (sriOnly)
        {
            return await cedulaService.GetCedulaSriAsync(ruc);
        }

        var parameters = new Dictionary<string, object> { { "id", ruc }, { "IDType", "05" } };
        var dt = await customerService.GetByIdCardAsync(parameters);

        if (!DataValidation.DataTableIsNull(dt))
        {
            return DataTableMapper.MapDataRowToModel<CedulaModel>(dt.Rows[0]);
        }

        return await cedulaService.GetCedulaSriAsync(ruc);
    }
}

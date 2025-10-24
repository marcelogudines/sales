using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Sales123.Sales.Application.Abstractions;
using Sales123.Sales.Application.DTOs;
using Sales123.Sales.Application.Services;
using Sales123.Sales.Domain.Abstractions;
using Sales123.Sales.Domain.Shared;
using Sales123.Sales.WebApi.Support;

namespace Sales123.Sales.WebApi.Controllers;

[ApiController]
[Route("api/sales")] 
[Produces("application/json")]
public class SalesController : ControllerBase
{
    private readonly ISaleService _service;
    private readonly ISaleQueryRepository _query;

    public SalesController(ISaleService service, ISaleQueryRepository query)
    {
        _service = service;
        _query = query;
    }

    /// <summary>Cria uma nova venda</summary>
    [HttpPost]
    [SwaggerResponse(StatusCodes.Status201Created, "Venda criada com sucesso.", typeof(ApiResponse<SaleView>))]
    [SwaggerResponse(StatusCodes.Status409Conflict, "Venda já existe", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Regras de negócio violadas.", typeof(ApiResponse<object>))]
    public IActionResult Create([FromBody] CreateSaleCommand body)
    {
        var res = _service.Create(body);
        var trace = HttpContext.TraceIdentifier;

        var already = res.Notifications.Items.Any(n => n.Code == "sale.already_exists");
        if (already)
            return Conflict(ApiResponse<object>.Error(res.Notifications, trace));

        if (!res.IsValid)
            return UnprocessableEntity(ApiResponse<object>.Error(res.Notifications, trace));

        var view = SaleViewMapper.ToView(res.Value!);
        var apiRes = ApiResponse<SaleView>.Ok(view, trace);
        return CreatedAtAction(nameof(GetById), new { id = view.Id }, apiRes);
    }

    /// <summary>Obtem venda por Id.</summary>
    [HttpGet("{id}")]
    [SwaggerResponse(StatusCodes.Status200OK, "Venda encontrado.", typeof(ApiResponse<SaleView>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Venda não encontrada.", typeof(ApiResponse<object>))]
    public IActionResult GetById([FromRoute] string id)
    {
        var (items, _) = _query.List(1, int.MaxValue);
        var sale = items.FirstOrDefault(x => x.Id == id);
        if (sale is null) return NotFound(ApiResponse<object>.Error(new NotificationsBag(), HttpContext.TraceIdentifier));
        return Ok(ApiResponse<SaleView>.Ok(SaleViewMapper.ToView(sale), HttpContext.TraceIdentifier));
    }

    /// <summary>Lista vendas (paginado).</summary>
    [HttpGet]
    [SwaggerResponse(StatusCodes.Status200OK, "Lista de vendas.", typeof(ApiResponse<object>))]
    public IActionResult List([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var (items, total) = _query.List(page <= 0 ? 1 : page, pageSize <= 0 ? 50 : pageSize);
        var data = new { total, page, pageSize, items = items.Select(SaleViewMapper.ToView) };
        return Ok(ApiResponse<object>.Ok(data, HttpContext.TraceIdentifier));
    }

    /// <summary>Adiciona item à venda.</summary>
    [HttpPost("{id}/items")]
    [SwaggerResponse(StatusCodes.Status200OK, "Item adicionado.", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Venda não encontrada.", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Regras de negócio violadas.", typeof(ApiResponse<object>))]
    public IActionResult AddItem([FromRoute] string id, [FromBody] AddItemCommand body)
    {
        var cmd = body with { SaleId = id };
        var res = _service.AddItem(cmd);
        if (!res.IsValid) return MapResult(res);
        return Ok(ApiResponse<object>.Ok(new { itemId = res.Value!.Id }, HttpContext.TraceIdentifier));
    }

    /// <summary>Altera quantidade de um item.</summary>
    [HttpPut("{id}/items/{itemId}/quantity")]
    [SwaggerResponse(StatusCodes.Status200OK, "Quantidade alterada.", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Venda/Item não encontrado.", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status422UnprocessableEntity, "Regras de negócio violadas.", typeof(ApiResponse<object>))]
    public IActionResult ReplaceQuantity([FromRoute] string id, [FromRoute] string itemId, [FromBody] ReplaceItemQuantityCommand body)
    {
        var cmd = body with { SaleId = id, ItemId = itemId };
        var res = _service.ReplaceItemQuantity(cmd);
        if (!res.IsValid) return MapResult(res);
        return Ok(ApiResponse<object>.Ok(new { itemId }, HttpContext.TraceIdentifier));
    }

    /// <summary>Cancela um item.</summary>
    [HttpPost("{id}/items/{itemId}/cancel")]
    [SwaggerResponse(StatusCodes.Status200OK, "Item cancelado.", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Venda/Item não encontrado.", typeof(ApiResponse<object>))]
    public IActionResult CancelItem([FromRoute] string id, [FromRoute] string itemId)
    {
        var res = _service.CancelItem(new CancelItemCommand(id, itemId));
        if (!res.IsValid) return MapResult(res);
        return Ok(ApiResponse<object>.Ok(new { itemId }, HttpContext.TraceIdentifier));
    }

    /// <summary>Cancela uma venda.</summary>
    [HttpPost("{id}/cancel")]
    [SwaggerResponse(StatusCodes.Status200OK, "Venda cancelada.", typeof(ApiResponse<object>))]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Venda não encontrada.", typeof(ApiResponse<object>))]
    public IActionResult Cancel([FromRoute] string id, [FromBody] CancelSaleCommand body)
    {
        var cmd = body with { SaleId = id };
        var res = _service.CancelSale(cmd);
        if (!res.IsValid) return MapResult(res);
        return Ok(ApiResponse<object>.Ok(new { saleId = id }, HttpContext.TraceIdentifier));
    }

    /// <summary>Exclui uma venda.</summary>
    [HttpDelete("{id}")]
    [SwaggerResponse(StatusCodes.Status204NoContent, "Venda excluída.")]
    [SwaggerResponse(StatusCodes.Status404NotFound, "Venda não encontrada.", typeof(ApiResponse<object>))]
    public IActionResult Delete([FromRoute] string id)
    {
        var ok = _service.Delete(id);
        if (!ok) return NotFound(ApiResponse<object>.Error(new NotificationsBag(), HttpContext.TraceIdentifier));
        return NoContent();
    }

    private IActionResult MapResult<T>(Result<T> r)
    {
        var trace = HttpContext.TraceIdentifier;
        var notFound = r.Notifications.Items.Any(n => n.Code.Contains("not_found"));
        if (notFound) return NotFound(ApiResponse<object>.Error(r.Notifications, trace));
        return UnprocessableEntity(ApiResponse<object>.Error(r.Notifications, trace));
    }
}

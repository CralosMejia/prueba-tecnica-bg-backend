using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ShoppingCart.Application.Products;

namespace ShoppingCart.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<ProductResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status503ServiceUnavailable
    )]
    public async Task<ActionResult<IReadOnlyList<ProductResponse>>> GetAll(
        [FromQuery] string? search,
        CancellationToken cancellationToken)
    {
        var products = await _productService.GetAllAsync(
            search,
            cancellationToken
        );

        return Ok(products);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(ProductResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status500InternalServerError
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status503ServiceUnavailable
    )]
    public async Task<ActionResult<ProductResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var product = await _productService.GetByIdAsync(
            id,
            cancellationToken
        );

        if (product is null)
        {
            return NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Product not found",
                Detail = $"Product with id '{id}' was not found."
            });
        }

        return Ok(product);
    }
}
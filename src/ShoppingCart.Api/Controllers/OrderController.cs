using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.Extensions;
using ShoppingCart.Application.Orders;

namespace ShoppingCart.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/orders")]
public sealed class OrderController(
    IOrderService orderService)
    : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(
        typeof(OrderResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict
    )]
    public async Task<ActionResult<OrderResponse>> Checkout(
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var response = await orderService.CheckoutAsync(
            userId,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(GetById),
            new { id = response.Id },
            response
        );
    }

    [HttpGet]
    [ProducesResponseType(
        typeof(IReadOnlyList<OrderResponse>),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    public async Task<
        ActionResult<IReadOnlyList<OrderResponse>>
    > GetAll(
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var response = await orderService.GetAllAsync(
            userId,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(OrderResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound
    )]
    public async Task<ActionResult<OrderResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var response = await orderService.GetByIdAsync(
            userId,
            id,
            cancellationToken
        );

        return Ok(response);
    }
}
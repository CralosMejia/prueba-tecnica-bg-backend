using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ShoppingCart.Api.Extensions;
using ShoppingCart.Application.Carts;

namespace ShoppingCart.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/cart")]
public sealed class CartController(
    ICartService cartService)
    : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(
        typeof(CartResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    public async Task<ActionResult<CartResponse>> Get(
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var response = await cartService.GetAsync(
            userId,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpPost("items")]
    [ProducesResponseType(
        typeof(CartResponse),
        StatusCodes.Status201Created
    )]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    public async Task<ActionResult<CartResponse>> AddItem(
        [FromBody] AddCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var response = await cartService.AddItemAsync(
            userId,
            request,
            cancellationToken
        );

        return CreatedAtAction(
            nameof(Get),
            response
        );
    }

    [HttpPut("items/{productId:guid}")]
    [ProducesResponseType(
        typeof(CartResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ValidationProblemDetails),
        StatusCodes.Status400BadRequest
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status409Conflict
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    public async Task<ActionResult<CartResponse>> UpdateQuantity(
        Guid productId,
        [FromBody] UpdateCartItemRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var response =
            await cartService.UpdateQuantityAsync(
                userId,
                productId,
                request,
                cancellationToken
            );

        return Ok(response);
    }

    [HttpDelete("items/{productId:guid}")]
    [ProducesResponseType(
        typeof(CartResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        typeof(ProblemDetails),
        StatusCodes.Status404NotFound
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    public async Task<ActionResult<CartResponse>> RemoveItem(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var response = await cartService.RemoveItemAsync(
            userId,
            productId,
            cancellationToken
        );

        return Ok(response);
    }

    [HttpDelete]
    [ProducesResponseType(
        typeof(CartResponse),
        StatusCodes.Status200OK
    )]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized
    )]
    public async Task<ActionResult<CartResponse>> Clear(
        CancellationToken cancellationToken)
    {
        var userId = User.GetRequiredUserId();

        var response = await cartService.ClearAsync(
            userId,
            cancellationToken
        );

        return Ok(response);
    }
}
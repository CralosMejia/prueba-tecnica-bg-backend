# Modelo del dominio

## Entidades

- User
- Product
- Cart
- CartItem
- Order
- OrderItem

## Reglas del producto

- El precio no puede ser negativo.
- El stock no puede ser negativo.
- El código del producto debe ser único.

## Reglas del carrito

- Las cantidades deben ser mayores que cero.
- El subtotal corresponde a la suma de precio por cantidad.
- El descuento del 10 % se aplica cuando el subtotal supera los $100.
- No se puede agregar más cantidad que el stock disponible.

## Reglas de la orden

- Una orden debe contener al menos un producto.
- La orden debe conservar el precio de compra de cada producto.
- La generación de la orden y la disminución de stock deben ser transaccionales.
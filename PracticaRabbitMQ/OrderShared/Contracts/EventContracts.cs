using System;

namespace OrderShared.Contracts;

public record OrderCreated(Guid OrderId, int ProductId, decimal Quantity, decimal Price);
public record OrderCancelled(Guid OrderId);

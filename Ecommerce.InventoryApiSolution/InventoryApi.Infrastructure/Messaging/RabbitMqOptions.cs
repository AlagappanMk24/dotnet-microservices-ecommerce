namespace InventoryApi.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string Exchange { get; set; } = "ecommerce.events";
    public string Queue { get; set; } = "inventory.order-placed";
    public string RoutingKey { get; set; } = "order.placed";
}
//using InventoryApi.Application.Abstractions;
//using InventoryApi.Application.Contracts;
//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System.Text;
//using System.Text.Json;

//namespace InventoryApi.Infrastructure.Messaging;

//public sealed class OrderPlacedConsumer(
//    IOptions<RabbitMqOptions> options,
//    IServiceProvider serviceProvider,
//    ILogger<OrderPlacedConsumer> logger) : BackgroundService
//{
//    private IConnection? _connection;
//    private IChannel? _channel;

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        var settings = options.Value;
//        var factory = new ConnectionFactory
//        {
//            HostName = settings.HostName,
//            Port = settings.Port,
//            UserName = settings.UserName,
//            Password = settings.Password,
//            VirtualHost = settings.VirtualHost,
//            AutomaticRecoveryEnabled = true
//        };

//        while (!stoppingToken.IsCancellationRequested)
//        {
//            try
//            {
//                if (_connection is null || !_connection.IsOpen)
//                {
//                    _connection = await factory.CreateConnectionAsync("inventory-api", stoppingToken);
//                    _channel = await _connection.CreateChannelAsync(cancellationToken: stoppingToken);

//                    await _channel.ExchangeDeclareAsync(settings.Exchange, ExchangeType.Topic, durable: true, autoDelete: false, cancellationToken: stoppingToken);
//                    await _channel.QueueDeclareAsync(settings.Queue, durable: true, exclusive: false, autoDelete: false, arguments: null, cancellationToken: stoppingToken);
//                    await _channel.QueueBindAsync(settings.Queue, settings.Exchange, settings.RoutingKey, cancellationToken: stoppingToken);
//                    await _channel.BasicQosAsync(0, 1, false, stoppingToken);

//                    var consumer = new AsyncEventingBasicConsumer(_channel);

//                    consumer.ReceivedAsync += async (_, ea) =>
//                    {
//                        try
//                        {
//                            var json = Encoding.UTF8.GetString(ea.Body.Span);

//                            var message = JsonSerializer.Deserialize<OrderPlacedMessage>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
//                                ?? throw new JsonException("Empty OrderPlacedEvent payload.");

//                            using var scope = serviceProvider.CreateScope();
//                            var inventoryService = scope.ServiceProvider.GetRequiredService<IInventoryService>();
//                            await inventoryService.ProcessOrderPlacedAsync(message, stoppingToken);

//                            if (_channel is not null)
//                            {
//                                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
//                            }
//                        }
//                        catch (InvalidOperationException ex)
//                        {
//                            logger.LogWarning(ex, "Order event rejected due to business rules. Delivery will be acknowledged to prevent endless retries.");

//                            if (_channel is not null)
//                            {
//                                await _channel.BasicAckAsync(ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
//                            }
//                        }
//                        catch (Exception ex)
//                        {
//                            logger.LogError(ex, "Failed processing OrderPlacedEvent. Message will be requeued.");

//                            try
//                            {
//                                if (_channel is not null)
//                                {
//                                    await _channel.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
//                                }
//                            }
//                            catch (Exception ackEx)
//                            {
//                                logger.LogError(ackEx, "Could not nack RabbitMQ delivery.");
//                            }
//                        }
//                    };

//                    await _channel.BasicConsumeAsync(settings.Queue, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);

//                    logger.LogInformation("Inventory consumer connected to RabbitMQ queue {Queue}.", settings.Queue);
//                }

//                await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
//            }
//            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
//            {
//                break;
//            }
//            catch (Exception ex)
//            {
//                logger.LogError(ex, "RabbitMQ consumer connection failed; retrying in five seconds.");
//                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
//            }
//        }
//    }

//    public override void Dispose()
//    {
//        try
//        {
//            _channel?.Dispose();
//            _connection?.Dispose();
//        }
//        catch { }

//        base.Dispose();
//    }
//}


using Ecommerce.SharedLibrary.Events;
using InventoryApi.Application.Abstractions;
using InventoryApi.Application.Contracts;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace InventoryApi.Infrastructure.Messaging;

/// <summary>
/// Consumes OrderPlacedEvent messages published by OrderApi.
/// MassTransit manages the RabbitMQ consumer lifecycle.
/// </summary>
public sealed class OrderPlacedConsumer(
    IInventoryService inventoryService,
    ILogger<OrderPlacedConsumer> logger)
    : IConsumer<OrderPlacedEvent>
{
    public async Task Consume(ConsumeContext<OrderPlacedEvent> context)
    {
        var orderEvent = context.Message;

        logger.LogInformation(
            "Received OrderPlacedEvent for order {OrderId}.",
            orderEvent.OrderId);

        var message = new OrderPlacedMessage(
            orderEvent.OrderId,
            orderEvent.CustomerId,
            orderEvent.Total,
            orderEvent.Items.Select(item =>
                new OrderPlacedMessageItem(
                    item.ProductId,
                    item.Quantity,
                    item.UnitPrice))
                .ToArray());

        await inventoryService.ProcessOrderPlacedAsync(
            message,
            context.CancellationToken);

        logger.LogInformation(
            "Successfully processed OrderPlacedEvent for order {OrderId}.",
            orderEvent.OrderId);
    }
}

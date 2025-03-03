// OrderProcessing.Shared/Services/RabbitMqService.cs
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderProcessing.Shared.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace OrderProcessing.Shared.Services
{
    public class RabbitMqService : IDisposable
    {
        private readonly RabbitMqConfig _config;
        private readonly ILogger<RabbitMqService> _logger;
        private IConnection? _connection;
        private IChannel? _channel;

        public RabbitMqService(RabbitMqConfig config, ILogger<RabbitMqService> logger)
        {
            _config = config;
            _logger = logger;
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                var factory = new ConnectionFactory() { HostName = _config.HostName };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: _config.OrderExchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);
                await _channel.ExchangeDeclareAsync(exchange: _config.PaymentExchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);
                await _channel.ExchangeDeclareAsync(exchange: _config.NotificationExchangeName, type: ExchangeType.Fanout, durable: true, autoDelete: false, arguments: null);

                //Declare Queues
                await _channel.QueueDeclareAsync(queue: _config.OrderPaymentQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                await _channel.QueueDeclareAsync(queue: _config.PaymentNotificationQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);
                await _channel.QueueDeclareAsync(queue: _config.OrderUpdateQueueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

                //Bind Queues to Exchanges
                await _channel.QueueBindAsync(queue: _config.OrderPaymentQueueName, exchange: _config.OrderExchangeName, routingKey: "");
                await _channel.QueueBindAsync(queue: _config.PaymentNotificationQueueName, exchange: _config.PaymentExchangeName, routingKey: "");
                await _channel.QueueBindAsync(queue: _config.OrderUpdateQueueName, exchange: _config.PaymentExchangeName, routingKey: "");

                _logger.LogInformation("RabbitMQ Connection and Channel initialized successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error initializing RabbitMQ: {ex.Message}");
            }
        }

        public void PublishMessage(string exchangeName, object message)
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ Channel is not initialized.  Cannot publish message.");
                return;
            }

            try
            {
                var json = JsonConvert.SerializeObject(message);
                var body = Encoding.UTF8.GetBytes(json);

                var properties = new BasicProperties() { Persistent = true };

                _channel.BasicPublishAsync(exchange: exchangeName,
                                     routingKey: "",
                                     mandatory: true,
                                     basicProperties: properties,
                                     body: body);

                _logger.LogInformation($"Published message to exchange {exchangeName}: {json}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error publishing message to exchange {exchangeName}: {ex.Message}");
            }
        }

        public void ConsumeMessage(string queueName, Func<string, Task> processMessage)
        {
            if (_channel == null)
            {
                _logger.LogError("RabbitMQ Channel is not initialized. Cannot consume messages.");
                return;
            }

            var consumer = new AsyncEventingBasicConsumer(_channel);

            consumer.ReceivedAsync += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                _logger.LogInformation($"Received message from queue {queueName}: {message}");

                try
                {
                    await processMessage(message);
                    await _channel.BasicAckAsync(ea.DeliveryTag, false); // Acknowledge message
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error processing message from queue {queueName}: {ex.Message}");
                    await _channel.BasicNackAsync(ea.DeliveryTag, false, false); // Reject message, don't requeue
                }
            };

            _channel.BasicConsumeAsync(queue: queueName,
                                 autoAck: false,
                                 consumer: consumer);
            _logger.LogInformation($"Started consuming messages from queue {queueName}.");
        }

        public void Dispose()
        {
            _channel?.CloseAsync();
            _channel?.Dispose();
            _connection?.CloseAsync();
            _connection?.Dispose();
        }
    }
}
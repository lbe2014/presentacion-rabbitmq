using OrderShared.Contracts;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace OrderConsumer;

public class Program
{
    static async Task Main(string[] args)
    {
        // Lanza la tarea de conexión en segundo plano
        var consumerTask = Task.Run(async () =>
        {
            while (true)
            {
                try
                {
                    var factory = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "admin", Password = "admin" };

                    using var connection = await factory.CreateConnectionAsync();
                    using var channel = await connection.CreateChannelAsync();

                    await channel.QueueDeclareAsync(queue: "order_create", durable: true, exclusive: false, autoDelete: false, arguments: null);
                    await channel.BasicQosAsync(0, 1, false);

                    var consumer = new AsyncEventingBasicConsumer(channel);

                    consumer.ReceivedAsync += async (model, ea) =>
                    {
                        var body = ea.Body.ToArray();
                        var message = Encoding.UTF8.GetString(body);
                        var order = JsonSerializer.Deserialize<OrderCreated>(message);

                        Console.WriteLine($"Recibido orden: {order.OrderId}, Producto: {order.ProductId}, Cantidad: {order.Quantity}, Precio: {order.Price}");
                        Console.WriteLine($"Procesando Orden {order.OrderId}");
                        await Task.Delay(1000);
                        Console.WriteLine($"Orden {order.OrderId} procesada");
                        await channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false);
                    };

                    await channel.BasicConsumeAsync(queue: "order_create", autoAck: false, consumer: consumer);

                    Console.WriteLine("Esperando mensajes para validacion de prodcutos");

                    // Espera indefinidamente mientras la conexión esté activa
                    await Task.Delay(-1);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"No se pudo conectar a RabbitMQ: {ex.Message}");
                    Console.WriteLine("Reintentando en 5 segundos...");
                    await Task.Delay(5000); // Espera 5 segundos antes de reintentar
                }
            }
        });

        // El hilo principal puede seguir haciendo otras cosas o simplemente esperar
        Console.WriteLine("El consumidor está en espera de RabbitMQ...");
        Console.ReadLine();
    }
}

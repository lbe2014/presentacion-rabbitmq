using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text;
using OrderShared.Contracts;
using RabbitMQ.Client;

namespace OrderService.Controllers
{
    /// <summary>
    /// Controlador para gestionar las operaciones relacionadas con órdenes.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController : ControllerBase
    {
        /// <summary>
        /// Publica un evento para crear una orden en RabbitMQ.
        /// </summary>
        /// <param name="order">El objeto <see cref="OrderCreated"/> que contiene los datos de la orden a crear.</param>
        /// <returns>Un <see cref="IActionResult"/> indicando el resultado de la operación.</returns>
        [HttpPost("Create")]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreated order)
        {
            // Crear una nueva instancia de OrderCreated con un nuevo OrderId
            var newOrder = order with { OrderId = Guid.NewGuid() };

            // Configuración de la conexión a RabbitMQ
            var factory = new ConnectionFactory()
            {
                HostName = "rabbitmq", // Dirección del servidor RabbitMQ
                Port = 5672,           // Puerto del servidor RabbitMQ
                UserName = "admin",    // Usuario para autenticación
                Password = "admin"     // Contraseña para autenticación
            };

            // Nombre del exchange y routingKey
            var exchangeName = "order_exchange";
            var routingKey = "order.create";

            // Crear una conexión asíncrona con RabbitMQ
            using var connection = await factory.CreateConnectionAsync();

            // Crear un canal asíncrono para enviar mensajes
            using var channel = await connection.CreateChannelAsync();

            // Declarar el intercambio en RabbitMQ (si no existe, se crea)
            await channel.ExchangeDeclareAsync(exchangeName, type: "direct", durable: true);

            // Crear una cola para recibir mensajes para creacion de ordenes
            await channel.QueueDeclareAsync(queue: "order_create", durable: true, exclusive: false, autoDelete: false, arguments: null);

            // Enlazar la cola al exchange con la clave de enrutamiento
            await channel.QueueBindAsync(queue: "order_create", exchange: exchangeName, routingKey: routingKey);

            // Serializar el objeto de la orden a un arreglo de bytes
            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(newOrder));

            // Configurar las propiedades del mensaje
            var properties = new BasicProperties { };
            properties.Persistent = true; // Hacer que el mensaje sea persistente

            // Publicar el mensaje en el exchange con el routingKey
            await channel.BasicPublishAsync(
                exchange: exchangeName,
                routingKey: routingKey,
                mandatory: true,
                basicProperties: properties,
                body: body);

            // Retornar una respuesta indicando que el evento fue publicado
            return Ok("Evento de crear orden publicado.");
        }
    }
}

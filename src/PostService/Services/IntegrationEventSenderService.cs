using Microsoft.Extensions.Hosting;
using Newtonsoft.Json.Linq;
using PostService.Data;
using PostService.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PostService.Services
{
    public class IntegrationEventListenerService : BackgroundService
    {
        private async Task ListenForIntegrationEvents(CancellationToken stoppingToken)
        {
            try
            {
                ConnectionFactory factory = new ConnectionFactory
                {
                    UserName = "test",
                    Password = "test"
                };
                var endpoints = new System.Collections.Generic.List<AmqpTcpEndpoint>
                {
                  new AmqpTcpEndpoint("host.docker.internal"),
                  new AmqpTcpEndpoint("localhost")
                };
                var connection = factory.CreateConnection(endpoints);
                var channel = connection.CreateModel();
                var consumer = new EventingBasicConsumer(channel);

                var arguments = new Dictionary<String, object>
                {
                    { "x-single-active-consumer", true }
                };
                channel.QueueDeclare("user.postservicesingleactiveconsumer", false, false, false, arguments);

                channel.ExchangeDeclare("userloadtest", "fanout");
                channel.QueueBind("user.postservicesingleactiveconsumer", "userloadtest", "");

                consumer.Received += async(model, ea)  =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    Console.WriteLine("IntegrationEvent {0}", message);

                    var data = JObject.Parse(message);
                    var type = ea.RoutingKey;
                    var user = new User()
                    {
                        ID = data["id"].Value<int>(),
                        Name = data["name"].Value<string>(),
                        Version = data["version"].Value<int>()
                    };
                    if (type == "user.add")
                    {
                        await _dataAccess.AddUser(user);

                    }
                    else if (type == "user.update")
                    {
                        await _dataAccess.UpdateUser(user);
                    }
                    channel.BasicAck(ea.DeliveryTag, false);
                };
                channel.BasicConsume(queue: "user.postservicesingleactiveconsumer",
                                         autoAck: false,
                                         consumer: consumer);
                try
                {
                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    Console.WriteLine("Shutting down.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private readonly DataAccess _dataAccess;

        public IntegrationEventListenerService(DataAccess dataAccess)
        {
            _dataAccess = dataAccess;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ListenForIntegrationEvents(stoppingToken);
            }
        }
    }
}
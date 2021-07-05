//using Microsoft.Extensions.DependencyInjection;
//using Microsoft.Extensions.Hosting;
//using PostService.Data;
//using RabbitMQ.Client;
//using System;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;
//namespace UserService.Services
//{
//    public class IntegrationEventSenderService : BackgroundService
//    {
//        private readonly IServiceScopeFactory _scopeFactory;
//        private CancellationTokenSource _wakeupCancelationTokenSource = new CancellationTokenSource();
//        public IntegrationEventSenderService(IServiceScopeFactory scopeFactory)
//        {
//            _scopeFactory = scopeFactory;
//            using var scope = _scopeFactory.CreateScope();
//            using var dbContext = scope.ServiceProvider.GetRequiredService<PostServiceContext>();
//            dbContext.Database.EnsureCreated();
//        }
//        public void StartPublishingOutstandingIntegrationEvents()
//        {
//            _wakeupCancelationTokenSource.Cancel();
//        }
//        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//        {
//            while (!stoppingToken.IsCancellationRequested)
//            {
//                await PublishOutstandingIntegrationEvents(stoppingToken);
//            }
//        }
//        private async Task PublishOutstandingIntegrationEvents(CancellationToken stoppingToken)
//        {
//            try
//            {
//                var factory = new ConnectionFactory();
//                var connection = factory.CreateConnection();
//                var channel = connection.CreateModel();

//                channel.ConfirmSelect(); // enable publisher confirms
//                IBasicProperties props = channel.CreateBasicProperties();
                
//                props.DeliveryMode = 2; // persist message


//                while (!stoppingToken.IsCancellationRequested)
//                {
//                    {
//                        using var scope = _scopeFactory.CreateScope();
//                        using var dbContext = scope.ServiceProvider.GetRequiredService<PostServiceContext>();
//                        var events = dbContext.IntegrationEventOutbox.OrderBy(o => o.ID).ToList();
//                        foreach (var e in events)
//                        {
//                            var body = Encoding.UTF8.GetBytes(e.Data);
//                            channel.BasicPublish(exchange: "user",
//                                                             routingKey: e.Event,
//                                                             basicProperties: props,
//                                                             body: body);
                            
//                            channel.WaitForConfirmsOrDie(new TimeSpan(0, 0, 5)); // wait 5 seconds for publisher confirm
//                            Console.WriteLine("Published: " + e.Event + " " + e.Data);

//                            dbContext.Remove(e);
//                            dbContext.SaveChanges();
//                        }
//                    }

//                    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
//                        _wakeupCancelationTokenSource.Token, 
//                        stoppingToken);
//                    try
//                    {
//                        await Task.Delay(Timeout.Infinite, linkedCts.Token);
//                    }
//                    catch (OperationCanceledException)
//                    {
//                        if (_wakeupCancelationTokenSource.Token.IsCancellationRequested)
//                        {
//                            Console.WriteLine("Publish requested");
//                            var tmp = _wakeupCancelationTokenSource;
//                            _wakeupCancelationTokenSource = new CancellationTokenSource();
//                            tmp.Dispose();
//                        }
//                        else if (stoppingToken.IsCancellationRequested)
//                        {
//                            Console.WriteLine("Shutting down.");
//                        }
//                    }
//                }
//            }
//            catch (Exception e)
//            {
//                Console.WriteLine(e.ToString());
//            }
//        }

//        private static void ListenForIntegrationEvents()
//        {
//            var factory = new ConnectionFactory();
//            var connection = factory.CreateConnection();
//            var channel = connection.CreateModel();
//            var consumer = new EventingBasicConsumer(channel);

//            consumer.Received += (model, ea) =>
//            {
//                var contextOptions = new DbContextOptionsBuilder<PostServiceContext>()
//                    .UseSqlite(@"Data Source=post.db")
//                    .Options;
//                var dbContext = new PostServiceContext(contextOptions);

//                var body = ea.Body.ToArray();
//                var message = Encoding.UTF8.GetString(body);
//                Console.WriteLine(" [x] Received {0}", message);

//                var data = JObject.Parse(message);
//                var type = ea.RoutingKey;
//                if (type == "user.add")
//                {
//                    if (dbContext.User.Any(a => a.ID == data["id"].Value<int>()))
//                    {
//                        Console.WriteLine("Ignoring old/duplicate entity");
//                    }
//                    else
//                    {
//                        dbContext.User.Add(new User()
//                        {
//                            ID = data["id"].Value<int>(),
//                            Name = data["name"].Value<string>(),
//                            Version = data["version"].Value<int>()
//                        });
//                        dbContext.SaveChanges();
//                    }
//                }
//                else if (type == "user.update")
//                {
//                    int newVersion = data["version"].Value<int>();
//                    var user = dbContext.User.First(a => a.ID == data["id"].Value<int>());
//                    if (user.Version >= newVersion)
//                    {
//                        Console.WriteLine("Ignoring old/duplicate entity");
//                    }
//                    else
//                    {
//                        user.Name = data["newname"].Value<string>();
//                        user.Version = newVersion;
//                        dbContext.SaveChanges();
//                    }
//                }
//                channel.BasicAck(ea.DeliveryTag, false);
//            };
//            channel.BasicConsume(queue: "user.postservice",
//                                     autoAck: false,
//                                     consumer: consumer);
//        }

//    }
//}
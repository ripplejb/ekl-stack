// See https://aka.ms/new-console-template for more information

using BackupDisks;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.RabbitMQ;

var loggerConfig = new LoggerConfiguration()
    .WriteTo.RabbitMQ((clientConfig, sinksConfig) =>
    {
        clientConfig.RouteKey = "backup-rk";
        clientConfig.Hostnames.Add("localhost");
        clientConfig.Exchange = "app-logging";
        clientConfig.ExchangeType = "direct";
        clientConfig.Username = "guest";
        clientConfig.Password = "guest";
        clientConfig.Port = 5672;
        clientConfig.DeliveryMode = RabbitMQDeliveryMode.Durable;

        sinksConfig.TextFormatter = new JsonFormatter();
    })
    .Enrich.WithProperty("Environment", "My Local PC")
    .Enrich.WithThreadId();

var asyncBackup = new AsyncBackup(loggerConfig.CreateLogger());

asyncBackup.Copy("/home/ripalbarot/WebstormProjects", "/home/ripalbarot/Temp");

Console.WriteLine("Type 'Quit' to exit.");
var inStr = string.Empty;
while (inStr is not "Quit")
{
    inStr = Console.ReadLine();
}
asyncBackup.CancelCopy();
Thread.Sleep(60000);


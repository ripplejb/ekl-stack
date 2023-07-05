// See https://aka.ms/new-console-template for more information

using BackupDisks;
using Serilog;
using Serilog.Formatting.Json;
using Serilog.Sinks.RabbitMQ;

if (Environment.GetCommandLineArgs().Length != 3) 
{
    Console.Error.WriteLine("On valid command line arguments");
    Console.Error.WriteLine("\t\tBackupDisks /source/path/ /destination/path/ ");
    return;
}

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

Console.Clear();
Console.WriteLine("Create a file named 'Quit' to exit in the same folder as app.");

asyncBackup.Copy(Environment.GetCommandLineArgs()[1], Environment.GetCommandLineArgs()[2]);

while (!File.Exists("Quit"))
{
    Thread.Sleep(10000);
}
asyncBackup.CancelCopy();
Thread.Sleep(60000);


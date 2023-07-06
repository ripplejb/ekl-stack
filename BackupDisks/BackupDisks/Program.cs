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

var taskUserInput = new Task(() =>
{
    Console.WriteLine("Type 'Quit' to exit.");
    var strIn = "";
    while (!strIn.Equals("Quit"))
    {
        strIn = Console.ReadLine() ?? "";
    }
    Console.WriteLine("Shutting Down...");
    asyncBackup.CancelCopy();
});
taskUserInput.Start();


var tasks = new[] { taskUserInput, 
    asyncBackup.Copy(Environment.GetCommandLineArgs()[1], Environment.GetCommandLineArgs()[2]) 
};
await Task.WhenAll(tasks);


Thread.Sleep(60000);


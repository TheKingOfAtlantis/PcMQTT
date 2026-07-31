
using MQTTnet;
using PcMQTT;

string  hostname = "";
Int16?  port;
string? username;
string? password = null;

string ReadPassword()
{
    var password = new System.Text.StringBuilder();

    while (true)
    {
        var key = Console.ReadKey(true);

        if (key.Key == ConsoleKey.Enter)
            break;

        if (key.Key == ConsoleKey.Backspace)
        {
            if (password.Length > 0)
            {
                password.Length--;
                Console.Write("\b \b");
            }
        }
        else if (!char.IsControl(key.KeyChar))
        {
            password.Append(key.KeyChar);
            Console.Write("*");
        }
    }

    Console.WriteLine();
    return password.ToString();
}

Console.Write("IP/Hostname: ");
hostname = Console.ReadLine() ?? "";
Console.Write($"port (Default: {PcMQTT.MqttClient.defaultPort}): ");
port = short.TryParse(Console.ReadLine(), out var parsedPort) ? parsedPort : null;
Console.Write("Username: ");
username = Console.ReadLine();
if(!string.IsNullOrEmpty(username))
{
    Console.Write("Password: ");
    password = ReadPassword();
}

var mqttClient = new PcMQTT.MqttClient(
    hostname: hostname,
    port: port ?? PcMQTT.MqttClient.defaultPort,
    clientID: "PcMQTTClient",
    username: username,
    password: password
);

await mqttClient.connect();

var powerStateSensor = new PowerStateSensor(mqttClient);

var powerManager = new PowerManager(
    onResume: async () => {
        await mqttClient.awaitConnected();
        await powerStateSensor.publish(PcMQTT.PowerState.Running);
    },
    onSuspend: async () => {
        await mqttClient.publish(Topics.avaliabilityTopic, "offline");
        if(powerStateSensor.lastState == PcMQTT.PowerState.Running)
            await powerStateSensor.publish(PcMQTT.PowerState.Hibernating);    
    },
    onShutdown: async () => {
        await powerStateSensor.publish(PcMQTT.PowerState.Shutdown);
    }
);

var sensors = new ISensor[]
{
    powerStateSensor
};

var commands = new IButton[]
{
    new HibernateCommand(mqttClient, powerStateSensor),
    new ShutdownCommand(mqttClient, powerStateSensor),
    new SleepCommand(mqttClient, powerStateSensor)
};

foreach(var discoverable in (sensors as IDiscoverable[]).Union(commands))
    await discoverable.discover();

mqttClient.mqttClient.ApplicationMessageReceivedAsync += async args => {
    var topic   = args.ApplicationMessage.Topic;
    var payload = args.ApplicationMessage.ConvertPayloadToString();

    foreach(var command in commands)
        if(topic == command.topic)
            await command.handleSubscription(payload);
};

foreach(var command in commands)
    await command.subscribe();

await powerStateSensor.publish(PcMQTT.PowerState.Running);

// Now we sleep the main thread and handle mqtt in the background
var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
};

try
{
    await Task.Delay(Timeout.Infinite, cts.Token);
} 
catch(TaskCanceledException)
{
    
}

await mqttClient.disconnect();

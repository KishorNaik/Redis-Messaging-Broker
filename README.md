# Redis-Messaging-Broker
This package offers a comprehensive Redis Messaging Broker, utilizing both the Publish/Subscribe and Request/Reply patterns. It integrates seamlessly with .NET applications, enabling efficient asynchronous communication through the Publish/Subscribe pattern, which supports decoupled message broadcasting to multiple subscribers, enhancing scalability and flexibility. Additionally, the Request/Reply pattern facilitates direct, asynchronous communication, ideal for scenarios requiring immediate feedback or acknowledgment. This package is perfect for developers aiming to build responsive, event-driven systems with Redis as the core messaging infrastructure.

## Prerequisites
Before using this NuGet package, ensure you have Redis installed. You can either install the Redis Docker image or set up Redis on Windows. 

For Windows installation, follow the instructions provided here.

https://github.com/tporadowski/redis/releases

For Redis Docker installation, follow the instructions provided here.
```powershell
docker run --name myredis -p 6379:6379 -d redis
```

## Package Info

### Using Nuget Package Manager:
```powershell
PM> Install-Package RedisBrokerBus -Version 1.0.0
```

### Using .Net CLI:
```powershell
> dotnet add package RedisBrokerBus --version 1.0.0
```

## Publisher and Subscriber Pattern.
The Publish/Subscribe (Pub/Sub) pattern is a messaging paradigm where senders (publishers) broadcast messages to a channel without knowing who will receive them. Receivers (subscribers) listen to specific channels and receive messages sent to those channels. This decouples the sender and receiver, allowing for scalable and flexible communication. Subscribers receive only the messages they are interested in, enabling efficient and targeted message delivery.

Let's see how to use the above pattern with a simple example.

### Step 1: Create an ASP.NET Core API Project for Producer
To create an ASP.NET Core API project named “Producer,” you can use the .NET CLI. Open your terminal or command prompt and run the following command:
```powershell
dotnet new webapi -n Producer
```
This command will scaffold a new ASP.NET Core Web API project named “Producer.”

### Step 2: Install `RedisBrokerBus` NuGet Package
Next, navigate to the project directory and install the `RedisBrokerBus` NuGet package by running the following command:
```powershell
dotnet add package RedisBrokerBus --version 1.0.0
```

### Step 3: Register Redis Message Broker service in Producer API.
Go to the Program.cs file and add the following code:
```C#
builder.Services.AddRedisMessageBroker("localhost:6379", (config) =>
{
    // Enable message publisher bus
    config.AddPublisher();
});
```
- `builder.Services.AddRedisMessageBroker("localhost:6379", (config) => { ... });`: This line registers the Redis Message Broker with the specified Redis server address (localhost:6379).
- `config.AddPublisher();`: This line enables the publisher bus, allowing the application to publish messages to Redis channels.

### Step 4: Create a Class Library for Communication
Create a class library to facilitate communication between the Producer and Consumer by transmitting messages via classes or records. Certainly! To create a class library named Message, you can use the following .NET CLI command:
```powershell
dotnet new classlib -n Message
```
This command will create a new class library project named Message.

Define a simple message class as shown below:
```powershell
public class DemoMessage
{
    public string Message { get; set; }
}
```
This class will be used to encapsulate the messages that are sent and received between the Producer and Consumer.

### Step 5: Create a Producer Controller
The `ProducerController` is an ASP.NET Core API controller responsible for publishing messages to a Redis channel. Below is the code:
```C#
using Message.Consumer_Producer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisBrokerBus.Core.Broker.Producer_Consumer;

namespace Producer.Controllers
{
    [Route("api/producer")]
    [ApiController]
    public class ProducerController : ControllerBase
    {
        private readonly IRedisPublisher _redisPublisher = null;

        public ProducerController(IRedisPublisher redisPublisher)
        {
            _redisPublisher = redisPublisher;
        }

        [HttpPost]
        public async Task<IActionResult> PublishMessage([FromBody] DemoMessage message)
        {
            Console.WriteLine($"Producer Message: {message.Message}");
            await _redisPublisher.PublishAsync<DemoMessage>("demo-channel", message);
            return Ok();
        }
    }
}

```
- The `ProducerController` has a constructor that takes an `IRedisPublisher` instance, which is injected by the ASP.NET Core dependency injection system. This instance is used to publish messages to Redis.
- The message is published to the `demo-channel` using the `PublishAsync` method of the `IRedisPublisher` interface. The channel name can be anything.

### Step 6: Create an ASP.NET Core API Project for Consumer
To create an ASP.NET Core API project named “Consumer,” you can use the .NET CLI. Open your terminal or command prompt and run the following command:
```powershell
dotnet new webapi -n Consumer
```
This command will scaffold a new ASP.NET Core Web API project named “Producer.”

### Step 7: Install `RedisBrokerBus` NuGet Package
Next, navigate to the project directory and install the RedisBrokerBus NuGet package by running the following command:
```powershell
dotnet add package RedisBrokerBus --version 1.0.0
```

### Step 8: Create a Consumer Class
The `DemoConsumer` class is designed to receive messages transmitted by the Producer. Below is the code.
```C#
using Message.Consumer_Producer;
using RedisBrokerBus.Core.Interface.Consumers;

namespace Consumer.Consumers;

public class DemoConsumer : IRedisConsumer<DemoMessage>
{
    Task IRedisConsumer<DemoMessage>.HandleAsync(string channel, DemoMessage message)
    {
        Console.WriteLine($"Consumer Received Channel: {channel}");
        Console.WriteLine($"Consumer Received Message: {message.Message}");

        return Task.CompletedTask;
    }
}

```
- The `DemoConsumer` class implements the `IRedisConsumer<DemoMessage>` interface, which requires implementing the `HandleAsync` method.
- This class listens for messages on a specified channel and processes them as they are received, making it an essential part of the consumer side of the Publish/Subscribe pattern.

### Step 9: Register Redis Message Broker service in Consumer API.
Go to the Program.cs file and add the following code:
```C#
builder.Services.AddRedisMessageBroker("localhost:6379", (config) =>
{
    config.AddSubscriber();
    config.AddConsumer<DemoConsumer, DemoMessage>("demo-channel");
});

```
- `builder.Services.AddRedisMessageBroker("localhost:6379", (config) => { ... });`: This line registers the Redis Message Broker with the specified Redis server address (localhost:6379).
- `config.AddSubscriber();`: This line enables the subscriber bus, allowing the application to subscribe to messages on Redis channels.
- `config.AddConsumer<DemoConsumer, DemoMessage>("demo-channel");`: This line adds the `DemoConsumer` class as a consumer and specifies the `DemoMessage` message class in the generic parameter for the `demo-channel`.

### 10: Run both projects and call the `Producer` API.
To send a message from the Producer and have the Consumer receive it, you can use the following curl command:
```curl
curl -X POST "http://localhost:5000/api/producer" -H "Content-Type: application/json" -d '{"message":"Your message here"}'

```
This command sends a POST request to the `/api/producer` endpoint with a JSON body containing the message.

## Request and Reply Pattern
The Request and Reply pattern is a messaging paradigm where a client sends a request message to a server and waits for a reply. This pattern facilitates direct, synchronous communication between distributed components, ensuring that the client receives an immediate response or acknowledgment. It is particularly useful for scenarios requiring real-time interaction and confirmation, making it ideal for tasks that need immediate feedback or processing results.

Let's see how to use the above pattern with a simple example.

### Step 1: Create an ASP.NET Core API Project for Request API
To create an ASP.NET Core API project named “RequestAPI,” you can use the .NET CLI. Open your terminal or command prompt and run the following command:
```powershell
dotnet new webapi -n RequestAPI
```
This command will scaffold a new ASP.NET Core Web API project named “RequestAPI.”

### Step 2: Install `RedisBrokerBus` NuGet Package
Next, navigate to the project directory and install the `RedisBrokerBus` NuGet package by running the following command:
```powershell
dotnet add package RedisBrokerBus --version 1.0.0
```
### Step 3: Register Redis Message Broker service for the Request API.
Go to the Program.cs file and add the following code:
```C#
builder.Services.AddRedisMessageBroker("localhost:6379", (config) =>
{
    config.AddRequest();
});
```
- `builder.Services.AddRedisMessageBroker("localhost:6379", (config) => { ... });`: This line registers the Redis Message Broker with the specified Redis server address (localhost:6379).
- `config.AddRequest();`: This line enables the request bus, allowing the application to send request messages on a Redis channel.

### Step 4: Create a Class Library for Communication
Create a class library to facilitate communication between the Request and Reply by transmitting messages via classes or records. Certainly! To create a class library named Message, you can use the following .NET CLI command:
```powershell
dotnet new classlib -n Message
```
This command will create a new class library project named Message.

Define a simple message class as shown below:
```powershell
public class DemoRequest
{
    public string Name { get; set; }
}

public class DemoResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; }

```
This class will be used to encapsulate the messages that are sent and received between the Request and Reply API.

### Step 5: Create a Request Controller
The RequestController is an ASP.NET Core API controller responsible for sending request messages to a Redis channel and handling the responses. Below is the code
```C#
using Message.Request_Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RedisBrokerBus.Core.Broker.Request_Response;

namespace RequestAPI.Controllers
{
    [Route("api/request")]
    [ApiController]
    public class RequestController : ControllerBase
    {
        private readonly IRedisRequest _redisRequest;

        public RequestController(IRedisRequest redisRequest)
        {
            _redisRequest = redisRequest;
        }

        [HttpPost]
        public async Task<IActionResult> PublishMessage([FromBody] DemoRequest message)
        {
            Console.WriteLine($"Request Message: {message.Name}");
            var response = await _redisRequest.RequestAsync<DemoRequest, DemoResponse>("request-channel", message, TimeSpan.FromSeconds(5));
            Console.WriteLine($"Response Message: {response.Id} - {response.Name}");
            return Ok(response);
        }
    }
}
```
- The `RequestController` has a constructor that takes an `IRedisRequest` instance, which is injected by the ASP.NET Core dependency injection system. This instance is used to send request messages to Redis.
- The message is sent to the request-channel using the `RequestAsync` method of the `IRedisRequest` interface. The method waits for a response for up to 5 seconds.

This setup allows the RequestController to send request messages and handle responses, facilitating synchronous communication between distributed components.

### Step 6: Create an ASP.NET Core API Project for Reply
To create an ASP.NET Core API project named “ReplyAPI,” you can use the .NET CLI. Open your terminal or command prompt and run the following command:
```powershell
dotnet new webapi -n ReplyAPI
```
This command will scaffold a new ASP.NET Core Web API project named “ReplyAPI.”

### Step 7: Install `RedisBrokerBus` NuGet Package
Next, navigate to the project directory and install the RedisBrokerBus NuGet package by running the following command:
```powershell
dotnet add package RedisBrokerBus --version 1.0.0
```

### Step 8: Create a Responder Class
The `DemoReply` class is designed to receive messages transmitted by the Request and reply to the request. Below is the code.
```C#
using Message.Request_Response;
using RedisBrokerBus.Core.Interface.Responders;

namespace ReplyAPI.Reply;

public class DemoReplay : IRedisResponder<DemoRequest, DemoResponse>
{
    Task<DemoResponse> IRedisResponder<DemoRequest, DemoResponse>.HandleAsync(DemoRequest request)
    {
        var response = new DemoResponse();
        response.Id = Guid.NewGuid();
        response.Name = request.Name;

        Console.WriteLine($"Reply Message: {response.Id} - {response.Name}");

        return Task.FromResult(response);
    }
}
```
- The `DemoReply` class implements the `IRedisResponder<DemoRequest, DemoResponse>` interface, which requires the implementation of the `HandleAsync` method.
- This class listens for request messages on a specified channel, processes them, and returns a response, making it an essential part of the consumer side of the Request/Reply pattern.

### Step 9: Register Redis Message Broker Service in ReplyAPI
Go to the Program.cs file in your ReplyAPI project and add the following code:
```C#
builder.Services.AddRedisMessageBroker("localhost:6379", (config) =>
{
    config.AddResponse();
    config.AddReply<DemoRequest, DemoResponse, DemoReplay>("request-channel");
});

```
- `builder.Services.AddRedisMessageBroker("localhost:6379", (config) => { ... })`;: This line registers the Redis Message Broker with the specified Redis server address (localhost:6379).
- `config.AddResponse();`: This line enables the reply bus, allowing the application to subscribe to messages on Redis channels.
- `config.AddReply<DemoRequest, DemoResponse, DemoReplay>("request-channel");` 
- `config.AddReply<DemoRequest, DemoResponse, DemoReplay>("request-channel");`: This line registers the DemoReply class as a responder for the DemoRequest and DemoResponse message types on the specified request-channel.

Step 10: Run Both Projects and Call the Request API.
To send a message from the Request API and have the Reply API receive it and send a response, you can use the following curl command:
```curl
curl -X POST "http://localhost:5000/api/request" -H "Content-Type: application/json" -d '{"name":"Your name here"}'

```
This command sends a POST request to the /api/request endpoint with a JSON body containing the message. The Reply API will process the request and return a response.

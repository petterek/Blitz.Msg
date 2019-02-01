
# What does this do?

It creates the infrastructure for reciving messages from RabbitMq. 
Also handles faild messages..

Look in the Demo folder to se the usage as wellgit

# Prerequisites

* The Exchange must be manually declared on rabbit instance for the Rabbit adapter to work

# Workflow of the Rabbit Adapter

* Create 2 connections, one for incomming and one for outgoing messages
* Creates a {ExchangeName}_GlobalErrors exchange for notifying. 
* Create "Dead Letter Exchang" for this consumer type. (many instances of the same type can be run at the same time, but they share the queue infrastructure).
* Create "Dead Letter Queue" for this consumer type and attach to "Dead Letter Exchange". 
* Create queue for all registerd handlers, if not exists(operation is idempotent).
    * The Dead Letter Exchange from step 2 is set as the DLE of the queue, meaning that "NACK"ing a message from the queue will send it to this exchange. 
	* Attached queue to exchange provided in the connectioninfo, this exchange has to exists prior to staring the consumer. 
    * Start to listen to messages from queue

## Incomming type safe messages is handled in this way. 

* Get the routing key for the message, read from properties in the "BasicDevliveryEventArgs"
* Deserialize the message to the type matching the routing key. You can controll the how the serializer works by implementing the `ISerializer` interface
* Create instance of handler assosiated with the message, using the provided `Action` delegate.
* Invoke the Handle method on the instance with the message.
* ACK is sendt to rabbit, this removes the message from the queue;

## Incomming wildcard routings 

* Create a generic message with the body as string.
* Instanciate the handler that was registerd with routing key
* Call the Handle method
* ACK is sendt to rabbit, this removes the message from the queue;

## Naming of queues
Queues will be named by convention. 

`string QueueName = $"{clientName}_{exchangeName}_{routingKey}";`


## When handling fails

* Message is NACK'ed. 
* Rabbit will push the message to the Dead Letter Exchange set on the queue, when the queue was created. 
* A message will be sent to the exchange in step 2 in of the setup.
  * The message is of type  
```csharp
public class ErrorModel
    {
        public string CorrelationId;
        public string MessageType;
        public string HandlerName;
        public string ServerName;
    }
```
This can be used to survie the infratructure. 

### Small example. 
```csharp
static void Main(string[] args)
{
    //Use any container.. 
    var container = new SimpleFactory.Container(LifeTimeEnum.PerGraph);
    container.Register<MyHandler>();

    //Connectioninfo to the rabbit server. 
    //The ClientName is important, as it is used in the infrastructure to indentify the host. 
    RabbitConnectionInfo connectionInfo = new RabbitConnectionInfo { UserName = "guest", Password = "guest", Server = "localhost", ExchangeName = "Simployer", ClientName = "MyTestingApp" };

    //Create the RabbitAdapter. This is a spesific implementation for Rabbit.
    IMessageAdapter messageAdapter = new RabbitMessageAdapter(
            connectionInfo,
            //The serializer that will be used by the adapter. This must implement the ISerializer from Itas.Infrastructure.
            new Serializer(),
            //This Func<BasicDeliveryEventArgs> gives you the chance to create a context value for your eventhandler.
            //Setting the ClientContext e.g
            (eventArgs) => new ClientContext
            {
                CorrelationId = eventArgs.GetCorrelationId(),
                CompanyGuid = eventArgs.GetCustomerGuid(),
                UserGuid = eventArgs.GetUserGuid()
            }
        );

    //Then instanciate the MessageHandler.. Passing in the Adapter. 
    var server = new MessageHandlerEngine(
        messageAdapter,
        //This Func<Type,object> is used instead of taking a dependency on a Container. 
        //Here you can create your scope to for your context

        (t, c) => container.CreateAnonymousInstance(t, c));

    //Register a typed handler for the Engine. 
    //The engine will ask for an instance of  MessageHandle<MyEventClass> using the above Action<Type,object>. 
    server.AttachMessageHandler<SomethingOccured, MyHandler>();

    //Registering an untyped handler. 
    //Will ask for an instance of the type mapped against this bindingkey. 
    server.AttachGenericMessageHandler<GenericEventHandler>("#");

    //Start the server. 
    //The infrastructure will be created on the rabbit server and the adapter will start to recieve the messages. 
    server.StartServer();

    Console.ReadLine();

    //Stop the server to dispose the connections to Rabbit. 
    server.StopServer();
}
```

## Producing messages for RabbitMQ
This is a piece of software that takes the burden off the developer when publishing messages/events to RabbitMQ. 

It is made as a "singleton" component and should only be instanciated once. Use a "scoped" or "transient" objct that takes this object as a dependency to capture the customer/user context.

```csharp

static void Main(string[] args)
{
    var container = new SimpleFactory.Container();
    var con = new RabbitConnectionInfo
    {
        ClientName = "Listner.Demo",
        ExchangeName = "Simployer",
        Server = "localhost",
        UserName = "guest",
        Password = "guest",
        VirtualHost = "/"
    };

    var pub = new PublishEventToRabbit(con, new Serializer());

    container.Register<PublishEventToRabbit>(() => pub).AsSingleton(); //This is singleton to hold the connection stuff for rabbit. Must be disposed
    container.Register<CustomPublisher>().Transient(); //This is the wrapper to capture the context of the current call
    container.Register<ApplicationContext>(); // this is the actual context.. Very simplefied :) 

    for (var x = 0; x < 10; x++)
    {
        var sender = container.CreateInstance<CustomPublisher>();
        sender.Publish(new SomethingOccured());
    }

    pub.Dispose();
}


public class CustomPublisher
{
    readonly PublishEventToRabbit toRabbit;
    readonly ApplicationContext context;

    public CustomPublisher(PublishEventToRabbit toRabbit, ApplicationContext context)
    {
        this.context = context;
        this.toRabbit = toRabbit;
    }


    public void Publish(object message)
    {
        var ctx = new RabbitEventContext {CorrelationId=context.CorrelationId, CustomerId=context.CompanyGuid, UserId=context.UserId};
        toRabbit.Publish(ctx, message);
    }

}

public class ApplicationContext
{
    public Guid CorrelationId = Guid.NewGuid();
    public Guid UserId = Guid.NewGuid();
    public Guid CompanyGuid = Guid.NewGuid();
}

```
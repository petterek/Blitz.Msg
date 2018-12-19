
# What does this do?

It creates the infrastructure for reciving messages from RabbitMq. 
Also handles faild messages..

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
    container.Register<MessageHandler<MyEventClass>, MyHandler>();
    container.Register<GenericEventHandler>();
            
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
        (e)=> new ClientContext {
            CorrelationId =Guid.Parse(e.BasicProperties.CorrelationId),
            CompanyGuid = new Guid(System.Text.Encoding.UTF8.GetString( (byte[]) e.BasicProperties.Headers[HeaderNames.Company] ) )}
        );

    //Then instanciate the MessageHandler.. Passing in the Adapter. 
    var server = new MessageHandlerEngine(
        messageAdapter,
        //This Func<Type,object> is used instead of taking a dependency on a Container. 
        //Here you can create your scope to for your context
        (t,c)=> container.CreateAnonymousInstance(t,c));

    //Register a typed handler for the Engine. 
    //The engine will ask for an instance of  MessageHandle<MyEventClass> using the above Action<Type,object>. 
    server.AttachMessageHandler<MyEventClass, MyHandler>();
            
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

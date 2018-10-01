# What does this do?

It creates the infrastructure for reciving messages from RabbitMq. 
Also handles faild messages, they are posted to an Dead Letter Queue, 




# Workflow of the RabbitServer

* Create connection
* Creat channel for AdminUse
* Create exchange if not exist(operation is idempotent)
* Create "Dead Letter Exchang" for the exchange
* Create "Dead Letter Queue" for this process and attach to "Dead Letter Exchange"
* Create queue for all registerd handlers, if not exists(operation is idempotent)
** Attached queue to exchange from step 3
** Start to listen to messages from queue

## Incomming type safe messages is handled in this way. 

* Get the routing key for the message
* Deserialize the message to the type matching the routing key
* Create instance of handler assosiated with the message, using the container provided. 
* Invoke the Handle method on the instance with the message
* ACK is sendt to rabbit, this removes the message from the queue;

## Incomming wildcard routings 

* Create a generic message with the body as string.
* Instanciate the handler that was registerd with routing key
* Call the Handle method
* ACK is sendt to rabbit, this removes the message from the queue;

## When handling fails

* Create a HandlingOfEventFaild message. 
* Push this to the configured "Dead Letter Exchange" with the routing key = "{ClientName}_{HandlingOfEventFaild}"
* All faild message hanling will accumelated in the same queue for the same "ClientName"


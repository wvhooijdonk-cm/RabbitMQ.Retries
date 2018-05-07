# RabbitMQ.Retries

A small library containing an implementation of a EventingBasicConsumer (called RetyConsumer) 
which supports exponential backoff retries.

In general this means that, when an exception is thrown in the Recieved event, the message can be retried in the following manner:

- 1st retry: after 1 second
- 2st retry: after 1 second
- 3st retry: after 1 second
- 4st retry: after 2 seconds
- 5st retry: after 2 seconds
- 6st retry: after 2 seconds
- 7st retry: after 4 seconds
- 8st retry: after 4 seconds
- 9st retry: after 4 seconds
and so on...

Note the exponential retry backoff time.
The number of attempts per backoff time can be configured as well as the number of backoff layers (3 in this example).


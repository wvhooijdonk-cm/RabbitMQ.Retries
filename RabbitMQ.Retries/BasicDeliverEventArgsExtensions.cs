using RabbitMQ.Client.Events;
using System.Collections.Generic;

namespace RabbitMQ.Retries {
    internal static class BasicDeliverEventArgsExtensions {
        internal static IDictionary<string, object> GetOrInitializeHeaders(this BasicDeliverEventArgs eventArgs) {
            eventArgs.BasicProperties.Headers = eventArgs.BasicProperties.Headers ?? new Dictionary<string, object>();
            return eventArgs.BasicProperties.Headers;
        }

        internal static int GetRetryCount(this BasicDeliverEventArgs eventArgs) {
            return eventArgs.GetOrInitializeHeaders().GetInt(HeaderNames.RetryCount) ?? 0;
        }
        internal static void SetRetryCount(this BasicDeliverEventArgs eventArgs, int value) {
            eventArgs.GetOrInitializeHeaders()[HeaderNames.RetryCount] = value;
        }

        internal static int GetRetryExpirationBase(this BasicDeliverEventArgs eventArgs) {
            return eventArgs.GetOrInitializeHeaders().GetInt(HeaderNames.RetryExpirationBase) ?? 1000;
        }

    }

}

using System;
using System.Collections.Generic;

namespace RabbitMQ.Retries {
    internal static class IDictionaryStringObjectExtensions {
        internal static int? GetInt(this IDictionary<string, object> headers, string headerName) {
            return headers.ContainsKey(headerName) ? (int?)Convert.ToInt32(headers[headerName]) : null;
        }
    }
}

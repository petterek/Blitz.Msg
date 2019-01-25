namespace Itas.Infrastructure.MessageHost
{
    public class HandlingOfEventFailed
    {
        public HandlingOfEventFailed(string innnerMessage, string eventType, string exceptionType, string exceptionMsg)
        {
            InnnerMessage = innnerMessage;
            EventType = eventType;
            ExceptionType = exceptionType;
            ExceptionMsg = exceptionMsg;
        }

        public string InnnerMessage { get; }
        public string EventType { get; }
        public string ExceptionType { get; }
        public string ExceptionMsg { get; }
    }

}
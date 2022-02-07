namespace TicketManagementSystem
{
    public static class PriorityExtensions
    {
        public static Priority RaisePriority(this Priority priority)
        {
            return priority switch
            {
                Priority.Low => Priority.Medium,
                Priority.Medium => Priority.High,
                _ => priority
            };
        }
    }
}

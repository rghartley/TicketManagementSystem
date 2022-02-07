using System;

namespace TicketManagementSystem
{
    public static class StringExtensions
    {
        public static void WhenNotPopulated(this string value, Action notPopulatedAction)
        {
            if (string.IsNullOrEmpty(value))
            {
                notPopulatedAction();
            }
        }
    }
}

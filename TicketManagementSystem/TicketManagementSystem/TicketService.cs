using System;
using EmailService;

namespace TicketManagementSystem
{
    public class TicketService
    {
        private readonly IEmailService emailService;
        private readonly IUserRepository userRepository;
        private readonly ITicketRepository ticketRepository;

        public TicketService(IEmailService emailService = null, IUserRepository userRepository = null, ITicketRepository ticketRepository = null)
        {
            this.emailService = emailService ?? new EmailServiceProxy();
            this.userRepository = userRepository ?? new UserRepository();
            this.ticketRepository = ticketRepository ?? new TicketRepositoryProxy();
        }

        public int CreateTicket(string title, Priority priority, string assignedTo, string description, DateTime date, bool isPayingCustomer)
        {
            const string InvalidTicketMessage = "Title or description were null";
            
            title.WhenNotPopulated(() => throw new InvalidTicketException(InvalidTicketMessage));
            description.WhenNotPopulated(() => throw new InvalidTicketException(InvalidTicketMessage));

            priority = CalculatePriority(priority, date, title);

            if (priority == Priority.High)
            {
                emailService.SendEmailToAdministrator(title, assignedTo);
            }

            var user = GetUser(assignedTo, $"User {assignedTo} not found");
            (var price, var accountManager) = GetAccountPaymentDetails(isPayingCustomer, priority);

            var ticket = new Ticket()
            {
                Title = title,
                AssignedUser = user,
                Priority = priority,
                Description = description,
                Created = date,
                PriceDollars = price,
                AccountManager = accountManager
            };

            var id = ticketRepository.CreateTicket(ticket);

            return id;
        }

        public void AssignTicket(int id, string username)
        {
            var user = GetUser(username, "User not found");
            var ticket = ticketRepository.GetTicket(id)
                ?? throw new ApplicationException($"No ticket found for id {id}");

            ticket.AssignedUser = user;

            ticketRepository.UpdateTicket(ticket);
        }

        private User GetUser(string username, string unknownUserExceptionMessage)
        {
            var user = username is not null
                ? userRepository.GetUser(username)
                : null;

            return user ?? throw new UnknownUserException(unknownUserExceptionMessage);
        }

        private Priority CalculatePriority(Priority currentPriority, DateTime date, string title)
        {
            var isTicketOutdated = date < DateTime.UtcNow - TimeSpan.FromHours(1);
            var isTicketImportant = title.Contains("Crash") || title.Contains("Important") || title.Contains("Failure");

            return isTicketOutdated || isTicketImportant
                ? currentPriority.RaisePriority()
                : currentPriority;
        }

        private (double price, User accountManager) GetAccountPaymentDetails(bool isPayingCustomer, Priority priority)
        {
            if (!isPayingCustomer)
            {
                return (0, null);
            }

            // Only paid customers have an account manager.
            var accountManager = userRepository.GetAccountManager();
            var price = priority is Priority.High ? 100 : 50;

            return (price, accountManager);
        }
    }
}

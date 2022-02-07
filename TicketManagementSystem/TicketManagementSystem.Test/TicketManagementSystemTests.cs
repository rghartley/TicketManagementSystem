using EmailService;
using FakeItEasy;
using NUnit.Framework;
using System;

namespace TicketManagementSystem.Test
{
    [TestFixture]
    public class TicketManagementSystemTests
    {
        [Test]
        public void ShallCreateTicket()
        {
            const string TicketTitle = "System Crash";
            const string TicketDescription = "The system crashed when user performed a search";
            const string AccountManagerName = "Sarah";
            const string AssignedToUser = "Johan";

            var emailService = A.Fake<IEmailService>();
            var userRepository = A.Fake<IUserRepository>();
            var ticketRepository = A.Fake<ITicketRepository>();

            A.CallTo(() => userRepository.GetAccountManager()).Returns(new User { Username = AccountManagerName });
            A.CallTo(() => userRepository.GetUser(AssignedToUser)).Returns(new User { Username = AssignedToUser });

            A.CallTo(() => ticketRepository.CreateTicket(A<Ticket>.That.Matches(t =>
                t.Title == TicketTitle &&
                t.Description == TicketDescription &&
                t.AssignedUser.Username == AssignedToUser &&
                t.AccountManager.Username == AccountManagerName &&
                t.Created == DateTime.MinValue &&
                t.PriceDollars == 100 &&
                t.Priority == Priority.High)))
                .Returns(1);

            var ticketService = new TicketService(emailService, userRepository, ticketRepository);

            var ticketId = ticketService.CreateTicket(
                TicketTitle,
                Priority.Medium,
                AssignedToUser,
                TicketDescription,
                DateTime.MinValue,
                true);

            Assert.AreEqual(1, ticketId);
            A.CallTo(() => emailService.SendEmailToAdministrator(TicketTitle, AssignedToUser)).MustHaveHappenedOnceExactly();
        }
    }
}
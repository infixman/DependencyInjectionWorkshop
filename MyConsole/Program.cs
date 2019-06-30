using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    class Program
    {
        private static AuthenticationService _authenticationService;

        private static void Main(string[] args)
        {
            Verify("joey", "9487", "9527");
        }

        private static void Verify(string account, string password, string otp)
        {
            var failedCounter = new FailedCounter();
            var authentication = 
                new AuthenticationService(
                    new ProfileDao(), 
                    new Sha256Adapter(), 
                    new OtpService());

            var notificationDecorator = 
                new NotificationDecorator(authentication, 
                new SlackAdapter());

            var failedCounterDecorator = 
                new FailedCounterDecorator(notificationDecorator, 
                failedCounter);

            var logFailedCounterDecorator = 
                new LogFailedCounterDecorator(failedCounterDecorator,
                failedCounter, new NLogAdapter());
            
            Console.WriteLine(logFailedCounterDecorator.Verify(account, password, otp));
        }
    }
}

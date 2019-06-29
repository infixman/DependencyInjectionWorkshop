namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;
        private static INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification)
        {
            _authentication = authentication;
            _notification = notification;
        }

        public bool Verify(string account, string password, string otp)
        {
            var isValid = _authentication.Verify(account, password, otp);
            if (!isValid)
            {
                _notification.PushMessage(account);
            }

            return isValid;
        }
    }
}
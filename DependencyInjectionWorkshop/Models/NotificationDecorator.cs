namespace DependencyInjectionWorkshop.Models
{
    public class NotificationDecorator : BaseAuthenticationDecorator
    {
        private readonly INotification _notification;

        public NotificationDecorator(IAuthentication authentication, INotification notification) : base(authentication)
        {
            _notification = notification;
        }

        public override bool Verify(string account, string password, string otp)
        {
            var isValid = base.Verify(account, password, otp);
            if (!isValid)
            {
                _notification.PushMessage(account);
            }

            return isValid;
        }
    }
}
namespace DependencyInjectionWorkshop.Models
{
    public class BaseAuthenticationDecorator : IAuthentication
    {
        private readonly IAuthentication _authentication;

        public BaseAuthenticationDecorator(IAuthentication authentication)
        {
            _authentication = authentication;
        }

        public virtual bool Verify(string account, string password, string otp)
        {
            return _authentication.Verify(account, password, otp);
        }
    }
}
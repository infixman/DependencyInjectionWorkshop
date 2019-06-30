namespace DependencyInjectionWorkshop.Models
{
    public class FailedCounterDecorator : BaseAuthenticationDecorator
    {
        private readonly IFailedCounter _failedCounter;

        public FailedCounterDecorator(IAuthentication authentication, IFailedCounter failedCounter) : base(authentication)
        {
            _failedCounter = failedCounter;
        }

        public override bool Verify(string account, string password, string otp)
        {
            CheckAccountIsLocked(account);
            return base.Verify(account, password, otp);
        }

        private void CheckAccountIsLocked(string account)
        {
            if (_failedCounter.IsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }
        }
    }
}
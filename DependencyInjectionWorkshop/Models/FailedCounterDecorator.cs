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
            var isValid = base.Verify(account, password, otp);
            if (isValid)
            {
                //驗證成功，歸零錯誤次數
                _failedCounter.ResetFailedCount(account);
            }
            else
            {
                //增加錯誤次數
                _failedCounter.AddFailedCount(account);
            }

            return isValid;
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
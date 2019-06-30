using DependencyInjectionWorkshop.Models;

namespace MyConsole
{
    internal class LogMethodInfoDecorator : BaseAuthenticationDecorator
    {
        private readonly ILogger _logger;

        public LogMethodInfoDecorator(IAuthentication authentication, ILogger logger) : base(authentication)
        {
            _logger = logger;
        }

        public override bool Verify(string account, string password, string otp)
        {
            _logger.Info($"{nameof(LogMethodInfoDecorator)}.{nameof(Verify)}({account},{password},{otp})");
            var isValid = base.Verify(account, password, otp);
            _logger.Info($"{nameof(LogMethodInfoDecorator)}.{nameof(Verify)}(isValid = {isValid})");
            return isValid;
        }
    }
}
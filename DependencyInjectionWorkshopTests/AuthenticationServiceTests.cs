using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private IFailedCounter _failedCounter;
        private INotification _notification;
        private ILogger _logger;
        private AuthenticationService _authenticationService;

        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();

            _authenticationService = 
                new AuthenticationService(_profile, _hash, _otpService, _failedCounter, 
                    _notification, _logger);
        }


        [Test]
        public void is_valid()
        {
            _profile.GetPassword("joey").ReturnsForAnyArgs("abc");
            _hash.Compute("9487").ReturnsForAnyArgs("abc");
            _otpService.GetCurrentOtp("joey").ReturnsForAnyArgs("9527");

            var isValid = _authenticationService.Verify("joey", "9487", "9527");
            Assert.IsTrue(isValid);
        }
    }
}
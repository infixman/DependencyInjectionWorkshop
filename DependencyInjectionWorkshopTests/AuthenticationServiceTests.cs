using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultPassword = "9487";
        private const string DefaultHashPassword = "abc";
        private const string DefaultAccount = "joey";
        private const string DefaultOtp = "9527";
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
            GivenPasswordFromDb(DefaultAccount, DefaultHashPassword);
            GivenHashedPassword(DefaultPassword, DefaultHashPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenValid(DefaultAccount, DefaultPassword, DefaultOtp);
            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid_when_otp_iw_wrong()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashPassword);
            GivenHashedPassword(DefaultPassword, DefaultHashPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenValid(DefaultAccount, DefaultPassword, "wrong otp");
            ShouldBeInvalid(isValid);
        }

        private static void ShouldBeValid(bool isValid)
        {
            Assert.IsTrue(isValid);
        }

        private static void ShouldBeInvalid(bool isValid)
        {
            Assert.IsFalse(isValid);
        }

        private bool WhenValid(string account, string password, string otp)
        {
            return _authenticationService.Verify(account, password, otp);
        }

        private void GivenOtp(string account, string otp)
        {
            _otpService.GetCurrentOtp(account).ReturnsForAnyArgs(otp);
        }

        private void GivenHashedPassword(string password, string hashedPassword)
        {
            _hash.Compute(password).ReturnsForAnyArgs(hashedPassword);
        }

        private void GivenPasswordFromDb(string account, string hashedPassword)
        {
            _profile.GetPassword(account).ReturnsForAnyArgs(hashedPassword);
        }
    }
}
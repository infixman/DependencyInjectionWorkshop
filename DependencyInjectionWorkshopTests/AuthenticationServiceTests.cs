using System;
using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        private const string DefaultInputPassword = "9487";
        private const string DefaultHashPassword = "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad";
        private const string DefaultAccount = "joey";
        private const string DefaultOtp = "9527";

        private IProfile _profile;
        private IHash _hash;
        private IOtpService _otpService;
        private IFailedCounter _failedCounter;
        private INotification _notification;
        private ILogger _logger;
        private IAuthentication _authentication;

        [SetUp]
        public void SetUp()
        {
            _profile = Substitute.For<IProfile>();
            _hash = Substitute.For<IHash>();
            _otpService = Substitute.For<IOtpService>();
            _failedCounter = Substitute.For<IFailedCounter>();
            _notification = Substitute.For<INotification>();
            _logger = Substitute.For<ILogger>();
            
            //先初始化一個最基本的Service
            var authentication = 
                new AuthenticationService(_profile, _hash, _otpService, _failedCounter, 
                    _notification, _logger);
            
            //然後裝飾他
            var notificationDecorator = new NotificationDecorator(authentication, _notification); 
            _authentication = new FailedCounterDecorator(notificationDecorator, _failedCounter);
        }

        [Test]
        public void is_valid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashPassword);
            GivenHashedPassword(DefaultInputPassword, DefaultHashPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenValid(DefaultAccount, DefaultInputPassword, DefaultOtp);
            ShouldBeValid(isValid);
        }

        [Test]
        public void is_invalid_when_otp_is_wrong()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashPassword);
            GivenHashedPassword(DefaultInputPassword, DefaultHashPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenValid(DefaultAccount, DefaultInputPassword, "wrong otp");
            ShouldBeInvalid(isValid);
        }

        [Test]
        public void should_notify_when_invalid()
        {
            WhenInvalid();
            ShouldNotify(DefaultAccount);
        }

        [Test]
        public void should_add_fail_count_when_invalid()
        {
            WhenInvalid();
            ShouldAddFailCount(DefaultAccount);
        }

        [Test]
        public void should_reset_fail_count_when_valid()
        {
            WhenValid();
            ShouldResetFailCount(DefaultAccount);
        }

        [Test]
        public void should_log_when_valid()
        {
            WhenInvalid();
            ShouldLog(DefaultAccount);
        }

        [Test]
        public void account_is_lock()
        {
            GivenAccountIsLocked();
            ShouldThrowsException<FailedTooManyTimesException>();
        }

        private void ShouldThrowsException<TException>() where TException : Exception
        {
            TestDelegate action = () => WhenValid();
            Assert.Throws<TException>(action);
        }

        private void GivenAccountIsLocked()
        {
            _failedCounter.IsAccountLocked(DefaultAccount).ReturnsForAnyArgs(true);
        }

        private void ShouldLog(string account)
        {
            _logger.Received().Info(Arg.Is<string>(m => m.Contains(account)));
        }


        private void ShouldResetFailCount(string account)
        {
            _failedCounter.Received().ResetFailedCount(account);
        }

        private bool WhenValid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashPassword);
            GivenHashedPassword(DefaultInputPassword, DefaultHashPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenValid(DefaultAccount, DefaultInputPassword, DefaultOtp);
            return isValid;
        }

        private void ShouldAddFailCount(string account)
        {
            _failedCounter.Received().AddFailedCount(account);
        }

        private void ShouldNotify(string account)
        {
            _notification.Received().PushMessage(Arg.Is<string>(account));
        }

        private bool WhenInvalid()
        {
            GivenPasswordFromDb(DefaultAccount, DefaultHashPassword);
            GivenHashedPassword(DefaultInputPassword, DefaultHashPassword);
            GivenOtp(DefaultAccount, DefaultOtp);

            var isValid = WhenValid(DefaultAccount, DefaultInputPassword, "wrong otp");
            return isValid;
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
            return _authentication.Verify(account, password, otp);
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
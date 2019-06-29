using DependencyInjectionWorkshop.Models;
using NSubstitute;
using NUnit.Framework;

namespace DependencyInjectionWorkshopTests
{
    [TestFixture]
    public class AuthenticationServiceTests
    {
        [Test]
        public void is_valid()
        {
            var profile = Substitute.For<IProfile>();
            var hash = Substitute.For<IHash>();
            var otpService = Substitute.For<IOtpService>();
            var failedCounter = Substitute.For<IFailedCounter>();
            var notification = Substitute.For<INotification>();
            var logger = Substitute.For<ILogger>();

            profile.GetPassword("joey").ReturnsForAnyArgs("abc");
            hash.Compute("9487").ReturnsForAnyArgs("abc");
            otpService.GetCurrentOtp("joey").ReturnsForAnyArgs("9527");

            var authenticationService = new AuthenticationService(profile, hash, otpService,
                failedCounter, notification, logger);

            Assert.IsTrue(authenticationService.Verify("joey", "9487", "9527"));
        }
    }
}
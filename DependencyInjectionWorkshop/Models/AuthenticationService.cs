using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;
        private readonly IFailedCounter _failedCounter;
        private readonly ILogger _logger;

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService, IFailedCounter failedCounter, INotification notification, ILogger logger)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _logger = new NLogAdapter();
        }

        public bool Verify(string account, string password, string otp)
        {
            //從DB撈使用者密碼
            var pwdFromDb = _profile.GetPassword(account);

            //將使用者輸入的密碼HASH一下
            var hashPwd = _hash.Compute(password);

            //從API取得目前的OTP
            var otpFromApi = _otpService.GetCurrentOtp(account);

            //檢查使用者輸入的密碼 & OTP正確性
            if (pwdFromDb == hashPwd && otpFromApi == otp)
            {
                //驗證成功，歸零錯誤次數
                _failedCounter.ResetFailedCount(account);
                return true;
            }
            else //驗證失敗
            {
                //增加錯誤次數
                _failedCounter.AddFailedCount(account);

                //紀錄錯誤次數
                var failedCount = _failedCounter.GetFailedCount(account);
                _logger.Info($"accountId:{account} failed times:{failedCount}");

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
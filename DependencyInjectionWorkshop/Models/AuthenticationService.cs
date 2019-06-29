using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao;
        private readonly Sha256Adapter _sha256Adapter;
        private readonly OtpService _otpService;
        private readonly FailedCounter _failedCounter;
        private readonly SlackAdapter _slackAdapter;
        private readonly ILogger _logger;

        public AuthenticationService(ProfileDao profileDao, Sha256Adapter sha256Adapter, OtpService otpService, FailedCounter failedCounter, SlackAdapter slackAdapter, ILogger logger)
        {
            _profileDao = profileDao;
            _sha256Adapter = sha256Adapter;
            _otpService = otpService;
            _failedCounter = failedCounter;
            _slackAdapter = slackAdapter;
            _logger = logger;
        }

        public AuthenticationService()
        {
            _profileDao = new ProfileDao();
            _sha256Adapter = new Sha256Adapter();
            _otpService = new OtpService();
            _failedCounter = new FailedCounter();
            _slackAdapter = new SlackAdapter();
            _logger = new NLogAdapter();
        }

        public bool Verify(string account, string password, string otp)
        {
            //檢查帳號是否被鎖定
            if (_failedCounter.IsAccountLocked(account))
            {
                throw new FailedTooManyTimesException();
            }

            //從DB撈使用者密碼
            var pwdFromDb = _profileDao.GetPassword(account);

            //將使用者輸入的密碼HASH一下
            var hashPwd = _sha256Adapter.Hash(password);

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
                //打SLACK通知使用者
                _slackAdapter.PushMessage(account);

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
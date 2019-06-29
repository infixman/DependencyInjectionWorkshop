using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();
        private readonly Sha256Adapter _sha256Adapter = new Sha256Adapter();
        private readonly OtpService _otpService = new OtpService();
        private readonly FailedCounter _failedCounter = new FailedCounter();
        private readonly SlackAdapter _slackAdapter = new SlackAdapter();
        private readonly NLogAdapter _nLogAdapter = new NLogAdapter();

        public bool Verify(string account, string password, string otp)
        {
            //檢查帳號是否被鎖定
            var isLocked = _failedCounter.IsAccountLocked(account);
            if (isLocked)
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
                _nLogAdapter.Info($"accountId:{account} failed times:{failedCount}");

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
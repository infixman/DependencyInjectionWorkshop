using System;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService : IAuthentication
    {
        private readonly IProfile _profile;
        private readonly IHash _hash;
        private readonly IOtpService _otpService;

        public AuthenticationService(IProfile profile, IHash hash, IOtpService otpService)
        {
            _profile = profile;
            _hash = hash;
            _otpService = otpService;
        }

        public AuthenticationService()
        {
            _profile = new ProfileDao();
            _hash = new Sha256Adapter();
            _otpService = new OtpService();
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
                return true;
            }
            else //驗證失敗
            {
                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
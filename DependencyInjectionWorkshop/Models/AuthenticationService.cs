using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class ProfileDao
    {
        public string GetPwdFromDb(string account)
        {
            string pwdHashFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                pwdHashFromDb = connection.Query<string>("spGetUserPassword", new {Id = account},
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            return pwdHashFromDb;
        }
    }

    public class AuthenticationService
    {
        private readonly ProfileDao _profileDao = new ProfileDao();

        public bool Verify(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            //檢查帳號是否被鎖定
            var isLocked = IsAccountLocked(account, httpClient);
            if (isLocked)
            {
                throw new FailedTooManyTimesException();
            }

            //從DB撈使用者密碼
            var pwdFromDb = _profileDao.GetPwdFromDb(account);

            //將使用者輸入的密碼HASH一下
            var hashPwd = GetHashPwd(password);

            //從API取得目前的OTP
            var otpFromApi = GetCurrentOtp(account, httpClient);

            //檢查使用者輸入的密碼&OTP正確性
            if (pwdFromDb == hashPwd && otpFromApi == otp)
            {
                //驗證成功，歸零錯誤次數
                ResetFailedCount(account, httpClient);
                return true;
            }
            else //驗證失敗
            {
                //打SLACK通知使用者
                PushMessage();

                //增加錯誤次數
                AddFailedCount(account, httpClient);

                //紀錄錯誤次數
                LogFailedCount(account, httpClient);

                return false;
            }
        }

        private static bool IsAccountLocked(string account, HttpClient httpClient)
        {
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsAccountLocked", account).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }

        private static void LogFailedCount(string account, HttpClient httpClient)
        {
            var failedCountResponse =
                httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;

            //LOG錯誤次數
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info($"accountId:{account} failed times:{failedCount}");
        }

        private static void AddFailedCount(string account, HttpClient httpClient)
        {
            var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        private static void PushMessage()
        {
            var slackClient = new SlackClient("my api token");
            slackClient.PostMessage(slackResponse => { }, "my channel", "my message", "my bot name");
        }

        private static void ResetFailedCount(string account, HttpClient httpClient)
        {
            var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        private static string GetCurrentOtp(string account, HttpClient httpClient)
        {
            string otpFromApi;
            var response = httpClient.PostAsJsonAsync("api/otps", account).Result;
            if (response.IsSuccessStatusCode)
            {
                otpFromApi = response.Content.ReadAsAsync<string>().Result;
            }
            else
            {
                throw new Exception($"web api error, account:{account}");
            }

            return otpFromApi;
        }

        private static string GetHashPwd(string password)
        {
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }

            var pwdHashFromInput = hash.ToString();
            return pwdHashFromInput;
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
﻿using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using Dapper;
using SlackAPI;

namespace DependencyInjectionWorkshop.Models
{
    public class AuthenticationService
    {
        public bool Verify(string account, string password, string otp)
        {
            var httpClient = new HttpClient() { BaseAddress = new Uri("http://joey.com/") };

            //檢查帳號是否被鎖定
            var isLockedResponse = httpClient.PostAsJsonAsync("api/failedCounter/IsLocked", account).Result;
            isLockedResponse.EnsureSuccessStatusCode();

            if (isLockedResponse.Content.ReadAsAsync<bool>().Result)
            {
                throw new FailedTooManyTimesException();
            }

            //從DB撈使用者密碼
            string pwdHashFromDb;
            using (var connection = new SqlConnection("my connection string"))
            {
                pwdHashFromDb = connection.Query<string>("spGetUserPassword", new { Id = account },
                    commandType: CommandType.StoredProcedure).SingleOrDefault();
            }

            //將使用者輸入的密碼HASH一下
            var crypt = new System.Security.Cryptography.SHA256Managed();
            var hash = new StringBuilder();
            var crypto = crypt.ComputeHash(Encoding.UTF8.GetBytes(password));
            foreach (var theByte in crypto)
            {
                hash.Append(theByte.ToString("x2"));
            }
            var pwdHashFromInput = hash.ToString();

            //從API取得目前的OTP
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

            //檢查使用者輸入的密碼&OTP正確性
            if (pwdHashFromDb == pwdHashFromInput && otpFromApi == otp)
            {
                //驗證成功，歸零錯誤次數
                var resetResponse = httpClient.PostAsJsonAsync("api/failedCounter/Reset", account).Result;
                resetResponse.EnsureSuccessStatusCode();
                return true;
            }
            else //驗證失敗
            {
                //打SLACK通知使用者
                var slackClient = new SlackClient("my api token");
                slackClient.PostMessage(slackResponse => { }, "my channel", "my message", "my bot name");
                
                //增加錯誤次數
                var addFailedCountResponse = httpClient.PostAsJsonAsync("api/failedCounter/Add", account).Result;
                addFailedCountResponse.EnsureSuccessStatusCode();
                
                //取得最新錯誤次數
                var failedCountResponse =
                    httpClient.PostAsJsonAsync("api/failedCounter/GetFailedCount", account).Result;
                failedCountResponse.EnsureSuccessStatusCode();
                var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
                
                //LOG錯誤次數
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info($"accountId:{account} failed times:{failedCount}");

                return false;
            }
        }
    }

    public class FailedTooManyTimesException : Exception
    {
    }
}
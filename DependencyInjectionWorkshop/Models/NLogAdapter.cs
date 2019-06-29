namespace DependencyInjectionWorkshop.Models
{
    public interface ILogger
    {
        void Info(string message);
    }

    public class NLogAdapter : ILogger
    {
        public void Info(string message)
        {
            //LOG錯誤次數
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}
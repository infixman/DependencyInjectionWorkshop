namespace DependencyInjectionWorkshop.Models
{
    public class NLogAdapter
    {
        public void Info(string message)
        {
            //LOG錯誤次數
            var logger = NLog.LogManager.GetCurrentClassLogger();
            logger.Info(message);
        }
    }
}
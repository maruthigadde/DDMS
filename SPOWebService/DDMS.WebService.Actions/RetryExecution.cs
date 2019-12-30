using System;
using System.Net;
using System.Threading;
using log4net;
using Microsoft.SharePoint.Client;

namespace DDMS.WebService.SPOActions
{
    public static class RetryExecution
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(RetryExecution));
        public static void ExecuteQueryWithRetry(this ClientContext clientContext, int retryCount, int delayTime)
        {
            Log.Info("In ExecuteQueryWithRetry method");
            int attemptCount = 0;
            if (retryCount <= 0)
                throw new ArgumentException("Provide a retry count greater than zero.");
            if (delayTime <= 0)
                throw new ArgumentException("Provide a delay greater than zero.");
            while (attemptCount < retryCount)
            {
                try
                {
                    clientContext.ExecuteQuery();
                    Log.Info("In ExecuteQueryWithRetry method - Query executed successfully in retry counts :" + attemptCount);
                    return;
                }
                catch (WebException wex)
                {
                    Log.ErrorFormat("Error in ExecuteQueryWithRetry method :{0}", wex.Message);
                    if (wex.Response is HttpWebResponse response && (response.StatusCode == (HttpStatusCode)429 || response.StatusCode == (HttpStatusCode)503 || response.StatusCode == (HttpStatusCode)407))
                    {
                        Thread.Sleep(delayTime);
                        attemptCount++;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            throw new Exception(string.Format("Maximum retry attempts {0}, has be attempted.", retryCount));
        }
    }
}

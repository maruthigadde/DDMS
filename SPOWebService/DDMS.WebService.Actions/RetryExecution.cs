﻿using System;
using System.Net;
using System.Threading;
using log4net;
using Microsoft.SharePoint.Client;

namespace DDMS.WebService.SPOActions
{
    public static class RetryExecution
    {
        private static readonly ILog Log = log4net.LogManager.GetLogger(typeof(RetryExecution));
        public static void ExecuteQueryWithRetry(this ClientContext clientContext, int retryCount, int delayTime, string LoggerId)
        {
            Log.DebugFormat("In ExecuteQueryWithRetry method for MessageId - {0}", LoggerId);
            int attemptCount = 0;
            if (retryCount <= 0)
            {
                Log.DebugFormat("In ExecuteQueryWithRetry method Provide a retry count greater than zero for MessageId - {0}", LoggerId);
                throw new ArgumentException("Provide a retry count greater than zero.");
            }
            if (delayTime <= 0)
            {
                Log.DebugFormat("In ExecuteQueryWithRetry method Provide a delay greater than zero for MessageId - {0}", LoggerId);
                throw new ArgumentException("Provide a delay greater than zero.");
            }
            while (attemptCount <= retryCount)
            {
                try
                {
                    clientContext.ExecuteQuery();
                    Log.DebugFormat("In ExecuteQueryWithRetry method - Query executed successfully in retry counts :{0}  for MessageId - {1}", attemptCount, LoggerId);
                    return;
                }
                catch (WebException wex)
                {
                    Log.ErrorFormat("Error in ExecuteQueryWithRetry method for MessageId - {0} :{1}", LoggerId, wex.Message);
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
            Log.ErrorFormat("Maximum retry attempts {0}, has be attempted for MessageId {1}", retryCount, LoggerId);
            throw new Exception(string.Format("Maximum retry attempts {0}, has be attempted.", retryCount));
        }
    }
}

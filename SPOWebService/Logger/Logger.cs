using DDMS.WebService.Models.Common;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogManager
{
    public class Logger
    {
        public static void LogSetup(string FileId)
        {
            string logDirectory = "", logPath = "", logLevel = "", filePath = "", dateTime = "";
            try
            {
                logDirectory = ConfigurationManager.AppSettings.Get(LoggerConfigurationConstants.LogDirectory);
                logPath = ConfigurationManager.AppSettings.Get(LoggerConfigurationConstants.LogPath);
                logLevel = ConfigurationManager.AppSettings.Get(LoggerConfigurationConstants.LogLevel);
                dateTime = DateTime.Now.Date.ToString(LoggerConfigurationConstants.DateTimeFormat);

                Hierarchy hierarchy = (Hierarchy)log4net.LogManager.GetRepository();
                PatternLayout patternLayout = new PatternLayout();
                RollingFileAppender roller = new RollingFileAppender();

                filePath = logDirectory + logPath + "\\" + dateTime + "\\" + FileId + LoggerConfigurationConstants.FileExtension;

                patternLayout.ConversionPattern = LoggerConfigurationConstants.ConversionPattern;
                patternLayout.ActivateOptions();

                roller.AppendToFile = true;
                roller.File = filePath;
                roller.Layout = patternLayout;
                roller.MaxSizeRollBackups = 5;
                roller.RollingStyle = RollingFileAppender.RollingMode.Size;
                roller.StaticLogFileName = true;
                roller.ActivateOptions();
                hierarchy.Root.AddAppender(roller);

                switch (logLevel.ToUpper())
                {
                    case LoggerConfigurationConstants.Info:
                        hierarchy.Root.Level = Level.Info;
                        break;
                    case LoggerConfigurationConstants.Debug:
                        hierarchy.Root.Level = Level.Debug;
                        break;
                    case LoggerConfigurationConstants.Error:
                        hierarchy.Root.Level = Level.Error;
                        break;
                    case LoggerConfigurationConstants.Off:
                        hierarchy.Root.Level = Level.Off;
                        break;
                    default:
                        hierarchy.Root.Level = Level.All;
                        break;

                }
                hierarchy.Configured = true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}

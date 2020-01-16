using System.Diagnostics.CodeAnalysis;

namespace DDMS.WebService.Constants
{
    [ExcludeFromCodeCoverage]
    public static class ConfigurationConstants
    {
      
        public const string SPOUserName = "SPOUserName";
        public const string SPOUserNameKey = "SPOUserNameKey";
        public const string SPOUserNameIv = "SPOUserNameIv";
        public const string SPOPassword = "SPOPassword";
        public const string SPOPasswordKey = "SPOPasswordKey";
        public const string SPOPasswordIv = "SPOPasswordIv";

        public const string SPOSiteURL = "SPOSiteURL";
        public const string SPOFolder = "SPOFolder";
        public const string SPORelativeURL = "SPORelativeURL";
    }

    [ExcludeFromCodeCoverage]
    public static class LoggerConfigurationConstants
    {
        public const string LogDirectory = "LogDirectory";
        public const string LogPath = "LogPath";
        public const string LogLevel = "LogLevel";
        public const string ConversionPattern = "%date [%thread] %-5level %logger - %message%newline";
        public const string Info = "INFO";
        public const string Debug = "DEBUG";
        public const string Error = "ERROR";
        public const string Off = "OFF";
        public const string DateTimeFormat = "dd-MM-yyyy";
        public const string FileExtension = ".txt";
    }

    [ExcludeFromCodeCoverage]
    public static class SpoConstants
    {
        public const string Title = "Title";
        public const string Name = "FileLeafRef";
        public const string DealerNumber = "DealerNumber";
        public const string ClaimNumber = "ClaimNumber";
        public const string ClaimSubmDate = "ClaimSubmDate";
        public const string RepairOrderDate = "RepairOrderDate";
        public const string RepairOrderNumber = "RepairOrderNumber";
        public const string VinNumber = "VinNumber";
        public const string ClaimId = "ClaimID";
        public const string DivisionId = "DivisionID";
        public const string Version = "_UIVersionString";
        public const string RequestUser = "RequestUser";
        public const string DocumentumId = "DocumentumId";
        public const string DocumentumVersion = "DocumentumVersion";
        public const bool OverRideExistingVersion = true;
        public const string MaxFileSize = "MaximumFileSizeAllowed";
        public const string DocumentId = "documentId";
    }
    [ExcludeFromCodeCoverage]
    public static class HeaderConstants
    {
        public const string Code = "Code";
        public const string SiteId = "siteId";
        public const string BusinessId = "businessId";
        public const string MessageId = "messageId";
        public const string Node = "Node";
        public const string CollectedTimeStamp = "collectedTimestamp";
        public const string ErrorCode = "ErrorCode";
        public const string Status = "Status";
        public const string ErrorType = "ErrorType";
        public const string ErrorDescription = "ErrorDescription";
    }

    [ExcludeFromCodeCoverage]
    public static class HeaderValueConstants
    {
        public const string SiteId = "DDMS";
        public const string BusinessId = "DDMS Documents";
        public const string Success = "Success";
        public const string Failed = "Failed";
    }

    [ExcludeFromCodeCoverage]
    public static class HeaderErrorConstants
    {
        public const string CodeSender = "Sender";
        public const string ErrorTypeSecurity = "SECURITY";
        public const string ErrorDescriptionInvalidRequest = "Invalid request";
        public const string ErrorDescriptionCollectedTimeStampRequired = "Invalid request collectedTimestamp required";
        public const string ErrorDescriptionBusinessIdRequired = "Invalid request businessId required";
        public const string ErrorDescriptionSiteIdRequired = "Invalid request siteId required";
        public const string ErrorDescriptionMessageIdRequired = "Invalid request security token messageId required";
        public const string ErrorDescriptionMessageIdInvalid = "Invalid request security token messageId invalid";
        public const string ErrorDescriptionCollectedTimeStampInvalid = "Invalid request collectedTimestamp invalid";
        public const string ErrorDescriptionBusinessIdInvalid = "Invalid request businessId invalid";
        public const string ErrorDescriptionSiteIdInvalid = "Invalid request siteId invalid";
    }
    [ExcludeFromCodeCoverage]
    public static class ExecuteQueryConstants
    {
        public const int RetryCount = 3;
        public const int RetryDelayTime = 30000;
    }

    [ExcludeFromCodeCoverage]
    public static class LogLevel
    {
        public const string Info = "INFO";
        public const string Debug = "DEBUG";
        public const string Error = "ERROR";
    }

    [ExcludeFromCodeCoverage]
    public static class ErrorMessage
    {
        public const string DifferentFileExtension = "Cannot update a file with different extension,please upload the file with same extension";
        public const string UploadFailed = "Failed to upload file and metadata";
        public const string UpdateFailed = "Failed to update file and metadata";
        public const string FileNotFound = "File does not exist";
        public const string RemoteName = "Remote name could not be resolved";
        public const string RequiredMandatoryFields = "Please send all required mandatory fields";
        public const string FieldValueNotValid = "Field value is not valid";
        public const string VersionRequired = "Please send the version number,not current version";
        public const string GreaterVersionProvided = "Version not available,version is greater than major version available";
        public const string InvalidSearchMethod = "Invalid search method";
        public const string ValueEmpty = "Required field {0} value is empty";
        public const string MaxFileSizeContentReached = "Unable to upload,file size should not be more than 30MB";
    }

    [ExcludeFromCodeCoverage]
    public static class ErrorException
    {
        public const string SystemIoFileNotFound = "System.IO.FileNotFoundException";
        public const string ArgumentException = "System.ArgumentException";
        public const string SpFieldValueException = "Microsoft.SharePoint.SPFieldValueException";
    }

    [ExcludeFromCodeCoverage]
    public static class Constants
    {
        public const string ContentType = "application/json;odata=nometadata";
        public const string MediaType = "application/json";
    }
}

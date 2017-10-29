namespace HWC_UserApp.UserApp.Models
{
    #region General constants

    /// <summary>
    /// General constants
    /// </summary>
    public static class Constants
    {
        // Directory and file related constants
        public const string RootDirectoryName = "HwConscious";
        public const string Level1DirectoryName = "UserApp";
        public const string LogDirectoryName = "Logs";
        public const string LogFileName = "Log";

        // REST API related constants
        public const string RestApiEndpoint = "https://oz3yqvjaik.execute-api.us-east-1.amazonaws.com/v1";
        public const string XApiKeyValue = "kHnzbQx6PX6sLZIIwwP2E58QlLKKUHeAao4fzoX0";

        // Miscellaneous constants
        public const int UserLocationPingFrequencyInMs = 1000 * 3; // 3 seconds
    }

    #endregion
}

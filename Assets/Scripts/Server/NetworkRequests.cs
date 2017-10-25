namespace platformer.network
{
    public class NetworkRequests
    {
        public const string LoginRequest = "{0}/api/Account/ChekUserReg";
        public const string UserProfileRequest = "{0}/api/Account/UserInfo";

        public const string NickNameChangeRequest = "{0}/api/Account/SetNameAndBirth";

        public const string TokenRequest = "{0}/token";

        public const string GetTargetsRequest = "{0}/api/Target/GetTargets?";
        public const string SetTargetLink = "{0}/api/Target/SetTargetLinks?";
        public const string SearchTarget = "{0}/api/Target/FindBySymbolsInName?";
        public const string SetTargetStatusRequest = "{0}/api/Target/SetTargetActive?";

        public const string GetSchedule = "{0}/api/Schedule/GetSchedules?";
        public const string GetActiveSchedule = "{0}/api/Schedule/GetActiveScheduleGroups?";
        public const string AddScheduleWithAllGroups = "{0}api/Schedule/CreateNewScheduleWithAllGroups?";
        public const string SetScheduleActive = "{0}/api/Schedule/SetScheduleActive?";
        public const string DeleteSchedule = "{0}/api/Schedule/DeleteSchedule?";
        public const string AddCopySchedule = "{0}/api/Schedule/CreateCopyOfDefaultSchedule?";

        public const string GetScheduleGroups = "{0}/api/Group/GetScheduleGroups?";
        public const string SetGroupActive = "{0}/api/Group/SetGroupActive?";
        public const string SetGroupTime = "{0}/api/Group/SetGroupTime";
        public const string SetScheduleGroupTime = "{0}/api/Group/SetScheduleGroupsTime";


        public const string SetSettingActive = "{0}/api/Target/SetTargetInGroupActive?";
        public const string SetSettingLink = "{0}/api/Target/SetTargetLinksInGroup?";

        public const string OnCleanReport = "{0}/api/Report/ClearUserReports?";
        public const string OnGetReport = "{0}/api/Report/GetReports?";

        public const string OnRecognizeTarget = "{0}/api/Target/RecognizeTarget?";
        public const string OnRecognizeVuMark = "{0}/api/Target/RecognizeVumark?";
        public const string OnRecognizeImages = "{0}/api/Target/RecognizeBookvikTarget?";

        public const string GetBookvikImages = "{0}/api/Target/GetBukvikTargets?";
        public const string GetUserByFirstNameSymbolsRequest = "{0}api/User/getUsersByFirstNameSymbols?symbols=";

        public const string GetAdditionalGroups = "{0}/api/Group/GetUserAdditionalGroups?";
        public const string SetAdditionalGroupStatus = "{0}/api/Group/ChangeAdditionalGroupStatus?";
        public const string SetAdditionalGroupName = "{0}/api/Group/ChangeAdditionalGroupName?";
        public const string SetAdditionalGroupTime = "{0}/api/Group/ChangeAdditionalGroupTime?";
        //public const string GetUserByFirstNameSymbolsRequest = "{0}api/User/getUsersByFirstNameSymbols?symbols=";

    }
}

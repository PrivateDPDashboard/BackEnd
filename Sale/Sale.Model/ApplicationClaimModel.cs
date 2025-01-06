namespace Sale.Model
{
    public class ApplicationClaimModel
    {
        public ApplicationClaimModel(string claimType, string claimValue, string groupName, string displayName, string description) {
            ClaimType = claimType;
            ClaimValue = claimValue;
            GroupName = groupName;
            DisplayName = displayName;
            Description = description;
        }

        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
        public string GroupName { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
    }
}

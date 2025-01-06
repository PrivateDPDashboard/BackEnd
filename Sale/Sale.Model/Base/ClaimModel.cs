namespace Sale.Model.Base
{
    public class ClaimModel(string claimType, string claimValue)
    {
        public string ClaimType { get; set; } = claimType;
        public string ClaimValue { get; set; } = claimValue;
    }
}

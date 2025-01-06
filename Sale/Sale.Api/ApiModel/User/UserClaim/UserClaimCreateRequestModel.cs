namespace Sale.Api.ApiModel.User.UserClaim
{
    public class UserClaimCreateRequestModel
    {
        public string UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}

namespace Sale.Api.ApiModel.User.UserClaim
{
    public class UserClaimModifyRequestModel
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string ClaimType { get; set; }
        public string ClaimValue { get; set; }
    }
}

namespace Sale.Api.ApiModel.User
{
    public class UserResetPasswordRequestModel
    {
        public string ApplicationUserId { get; set; }
        public string NewPassword { get; set; }
    }
}

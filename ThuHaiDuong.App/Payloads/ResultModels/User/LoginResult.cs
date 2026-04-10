using ThuHaiDuong.Application.Payloads.ResultModels.User;

namespace ThuHaiDuong.Application.Payloads.ResultModels.User
{
    public class LoginResult
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public UserResult User { get; set; }
    }
}

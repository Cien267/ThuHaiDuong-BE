namespace ThuHaiDuong.Application.Payloads.InputModels.Admin.Auth
{
    public class ChangePasswordInput
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}

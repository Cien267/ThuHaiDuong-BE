namespace ThuHaiDuong.Application.Payloads.Responses
{
    public static class ResponseMessage
    {
        public static string GetEmailSuccessMessage(string email)
        {
            return $"Email is sent to: {email}.";
        }
    }
}

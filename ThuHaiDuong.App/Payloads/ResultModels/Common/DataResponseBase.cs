namespace ThuHaiDuong.Application.Payloads.ResultModels.Common
{
    public class DataResponseBase
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}

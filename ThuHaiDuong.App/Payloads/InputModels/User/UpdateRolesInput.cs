namespace ThuHaiDuong.Application.Payloads.InputModels.User;

public class UpdateRolesInput
{
    public Guid AssignerId { get; set; }
    public List<string> Roles { get; set; }
}
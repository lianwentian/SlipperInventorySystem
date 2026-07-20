namespace SlipperIS.Core.Models;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }
    public DateTime AssignedAt { get; set; } = DateTime.Now;

    public virtual Role? Role { get; set; }
    public virtual Permission? Permission { get; set; }
}

namespace SimpleApp.Tenancy;
public interface IScimTenancyAccessor
{
    string? TenantId { get; }
}

internal interface IScimTenancyContext : IScimTenancyAccessor
{
    void SetTenantId(string tenantId);
}

internal class ScopedTenancyAccessor : IScimTenancyContext
{
    private string? tenantId = null;

    public string? TenantId => tenantId;
    public void SetTenantId(string tenantId)
    {
        this.tenantId = tenantId;
    }
}
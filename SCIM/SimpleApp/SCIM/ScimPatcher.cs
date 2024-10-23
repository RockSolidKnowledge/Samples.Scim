using Rsk.AspNetCore.Scim.Stores;

namespace SimpleApp.SCIM;

public interface IScimPatcher<in TEntity> where TEntity : class
{
    public bool TryPatch(TEntity user, PatchCommand command);
}

public class ScimPatcher<TEntity> : IScimPatcher<TEntity> where TEntity : class
{
    private readonly IScimPatchMap<TEntity> patchMap;

    public ScimPatcher(IScimPatchMap<TEntity> patchMap)
    {
        this.patchMap = patchMap;
    }

    public bool TryPatch(TEntity user, PatchCommand command)
    {
        if (patchMap.TryGetPatchMethod(command.Path, out IScimPatchOperationExecutor<TEntity>? patchOp))
        {
            patchOp.Execute(user, command);
            return true;
        }

        return false;
    }
}
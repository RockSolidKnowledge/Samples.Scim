using Rsk.AspNetCore.Scim.Stores;

namespace SimpleApp.SCIM;

public interface IScimPatcher<in TEntity> where TEntity : class
{
    public bool TryPatch(TEntity user, PatchCommand command);
}

public class ScimPatcher<TEntity> : IScimPatcher<TEntity> where TEntity : class
{
    private readonly IEnumerable<IScimPatchMap<TEntity>> maps;

    public ScimPatcher(IEnumerable<IScimPatchMap<TEntity>> maps)
    {
        this.maps = maps;
    }

    public bool TryPatch(TEntity user, PatchCommand command)
    {
        foreach (var map in maps)
        {
            if (map.TryGetPatchExecutor(command.Path, out IScimPatchOperationExecutor<TEntity>? patchOp))
            {
                patchOp.Execute(user, command);
                return true;
            }
        }

        return false;
    }
}
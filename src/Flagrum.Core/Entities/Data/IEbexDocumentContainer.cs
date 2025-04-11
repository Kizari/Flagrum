namespace Flagrum.Core.Scripting.Ebex.Data;

public interface IDocumentContainer
{
    bool IsSettingNewScene { get; set; }

    void LatestUseNodeUpdate();

    void FavoriteNodeUpdate();

    void FavoritePackageUpdate();

    void Select(DataItem item);

    void SelectRemove(DataItem item, bool packageRemovable);

    DataItem FindDataItem(ItemPath path, bool isAllowLazyLoading = true);

    void beginDisposeDataItem(DataItem item);

    void endDisposeDataItem(DataItem item);

    void doOnEntityNameChanged(Entity entity);

    void doOnEntityModifiedChanged(Entity entity);

    void doOnEntityPackageModifiedChanged(EntityPackage entityPackage);

    void doOnEntityPackageEarcModifiedChanged(EntityPackage entityPackage);

    void doOnEntityPackageOpened(EntityPackage entityPackage);

    void doOnEntityPackageSaving(EntityPackage entityPackage, string path);

    void doOnEntityPackageSaved(EntityPackage entityPackage, string path, bool result);

    DataItem GetEntities();

    void LazyLoading(EntityPackageReference entityPackageReference);
}
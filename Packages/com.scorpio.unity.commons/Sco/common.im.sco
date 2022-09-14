function CreatePool_impl(value) {
    return ResourceManager.InstantiateResource(value.assetBundleName, value.resourceName)
}
function CreatePool(name, assetBundleName, resourceName) {
    return PoolManager.CreatePrefabPool(name, CreatePool_impl, { assetBundleName : assetBundleName, resourceName : resourceName })
}
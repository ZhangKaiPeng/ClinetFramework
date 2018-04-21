using UnityEngine;

public interface IAssetContainer : IReleasable
{
	GameObject CreateAssetGO(int assetID, Transform parent, bool usePool);

	GameObject CreateAssetGO(int assetID);

	string GetAssetName(int assetID);

	string GetAssetBundleName(int assetID);

	void ReleaseAsset(int assetID, bool release);
}

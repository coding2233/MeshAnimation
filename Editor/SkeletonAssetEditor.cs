using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(SkeletonAsset))]
public class SkeletonAssetEditor : Editor
{
    private SkeletonAsset _SkeletonAsset;
    public void OnEnable()
    {
        _SkeletonAsset = (SkeletonAsset)target;
    }
    public override void OnInspectorGUI()
    {
        GUILayout.Label(_SkeletonAsset._SkeletonType + " 变形动画骨架");
        GUILayout.Label("骨骼数量：" + _SkeletonAsset._Bones.Count);
    }
}

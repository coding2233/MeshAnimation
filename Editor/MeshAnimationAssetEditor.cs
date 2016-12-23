using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MeshAnimationAsset))]
public class MeshAnimationAssetEditor : Editor
{
    private MeshAnimationAsset _MeshAnimationAsset;
    public void OnEnable()
    {
        _MeshAnimationAsset = (MeshAnimationAsset)target;
    }
    public override void OnInspectorGUI()
    {
        GUILayout.Label(_MeshAnimationAsset._TargetName + " 变形动画数据文件");
        GUILayout.Label("动画顶点：" + _MeshAnimationAsset._VertexNumber);
        GUILayout.Label("动画帧数：" + _MeshAnimationAsset._FrameNumber);
    }
}

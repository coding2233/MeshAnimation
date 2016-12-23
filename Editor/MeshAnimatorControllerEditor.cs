using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshAnimatorController))]
public class MeshAnimatorControllerEditor : Editor
{
    private MeshAnimatorController _MeshAnimatorController;
    public void OnEnable()
    {
        _MeshAnimatorController = (MeshAnimatorController)target;
    }
    public override void OnInspectorGUI()
    {
        GUILayout.Label("变形动画状态机");
        GUILayout.Label("菜单栏打开：Window/变形动画状态机");
    }
}

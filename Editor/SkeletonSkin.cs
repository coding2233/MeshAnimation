using UnityEngine;
using UnityEditor;
/// <summary>
/// 变形动画骨骼蒙皮
/// </summary>
public class SkeletonSkin : EditorWindow
{
    //当前的动画编辑器
    public MeshAnimation _MeshAnimation;
    
    void OnGUI()
    {
        if (_MeshAnimation != null)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("快速蒙皮"))
            {
                if (_MeshAnimation._Skeleton == null)
                    return;

                GenericMenu genericMenu = new GenericMenu();
                for (int i = 0; i < _MeshAnimation._Skeleton._Bones.Count; i++)
                {
                    int j = i;
                    genericMenu.AddItem(new GUIContent(_MeshAnimation._Skeleton._Bones[i]._BoneName), false, delegate () { Skin(j); });
                }
                genericMenu.ShowAsContext();
            }
            GUILayout.EndHorizontal();
        }
    }
    /// <summary>
    /// 快速蒙皮
    /// </summary>
    void Skin(int number)
    {
        if (Selection.objects.Length <= 0)
        {
            EditorUtility.DisplayDialog("警告", "请先选中需要蒙皮的顶点！", "确定");
            return;
        }
        for (int i = 0; i < Selection.objects.Length; i++)
        {
            GameObject obj = Selection.objects[i] as GameObject;
            if (obj.GetComponent<Vertex>() != null)
            {
                obj.GetComponent<Vertex>().SetBone(number);
            }
        }
    }
}

using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(Vertex))]
public class VertexEditor : Editor
{
    private Vertex _Vertex;
    public void OnEnable()
    {
        _Vertex = (Vertex)target;
    }
    public override void OnInspectorGUI()
    {
        if (_Vertex == null)
            return;

        if (_Vertex._MeshAnimation == null || _Vertex._Identity == -1)
        {
            GUILayout.Label("请注意！这是一个不合法的顶点！", "ErrorLabel");
        }
        else
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("当前所属骨骼：");
            if (GUILayout.Button(_Vertex._TheBoneName))
            {
                if (_Vertex._MeshAnimation._Skeleton == null)
                    return;

                GenericMenu genericMenu = new GenericMenu();
                for (int i = 0; i < _Vertex._MeshAnimation._Skeleton._Bones.Count; i++)
                {
                    int j = i;
                    genericMenu.AddItem(new GUIContent(_Vertex._MeshAnimation._Skeleton._Bones[i]._BoneName), false, delegate () { _Vertex.SetBone(j); });
                }
                genericMenu.ShowAsContext();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("返回至模型"))
            {
                if (_Vertex._MeshAnimation != null)
                {
                    Selection.objects = new UnityEngine.Object[] { _Vertex._MeshAnimation.gameObject };
                }
            }
            GUILayout.EndHorizontal();
        }
    }
}

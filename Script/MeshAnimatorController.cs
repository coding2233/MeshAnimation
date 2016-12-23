using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 变形动画状态机
/// </summary>
public class MeshAnimatorController : ScriptableObject{

    //此状态机下所有的动画片段相同的顶点数目
    public int _MeshAnimationVertexNumber;
    //动画片段集合
    public List<MeshAnimationAsset> _MeshAnimationAssets;
    //动画片段位置集合（用于在编辑器中显示）
    public List<Vector2> _MeshAnimationAssetPosition;

    //动画片段过渡关系集合（一根连线就是一个过渡关系，每一个过渡关系对应一个过渡条件）
    public List<Vector2> _MeshAnimationTransitions;
    //动画片段过渡条件集合
    public List<bool> _MeshAnimationTransitionType;
    //动画片段过渡条件索引集合（通过索引设置过渡条件）
    public List<string> _MeshAnimationTransitionName;
    //动画片段是否是自动过渡（是自动过渡则忽略过渡条件）
    public List<bool> _HasExitTimes;
    
    //初始动画片段
    public MeshAnimationAsset _DefaultAnimation;

#if UNITY_EDITOR
    [MenuItem("Assets/Create/Mesh Animator Controller")]
    static void Create()
    {
        //创建一个新的状态机
        MeshAnimatorController meshAnimatorController = CreateInstance<MeshAnimatorController>();
        meshAnimatorController._MeshAnimationVertexNumber = 0;
        meshAnimatorController._MeshAnimationAssets = new List<MeshAnimationAsset>();
        meshAnimatorController._MeshAnimationAssetPosition = new List<Vector2>();
        meshAnimatorController._MeshAnimationTransitions = new List<Vector2>();
        meshAnimatorController._MeshAnimationTransitionType = new List<bool>();
        meshAnimatorController._MeshAnimationTransitionName = new List<string>();
        meshAnimatorController._HasExitTimes = new List<bool>();
        
        //创建本地文件
        string path = "Assets/New Mesh Animator Controller.asset";
        AssetDatabase.CreateAsset(meshAnimatorController, path);
    }
#endif
}

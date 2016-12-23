using UnityEngine;
/// <summary>
/// 顶点
/// </summary>
public class Vertex : MonoBehaviour {

    //所属的动画编辑器
    [System.NonSerialized]
    public MeshAnimation _MeshAnimation;
    //顶点索引
    [System.NonSerialized]
    public int _Identity = -1;
    //所属骨骼名称
    [System.NonSerialized]
    public string _TheBoneName;

    /// <summary>
    /// 设置骨骼（蒙皮）
    /// </summary>
    public void SetBone(int number)
    {
        _MeshAnimation._Skeleton._Bones[number]._Vertex.Add(_Identity);
        transform.SetParent(_MeshAnimation._Skeleton._Bones[number]._BoneObj.transform);
        _TheBoneName = _MeshAnimation._Skeleton._Bones[number]._BoneName;
    }
}

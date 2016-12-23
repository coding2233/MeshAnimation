using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 骨头信息文件
/// </summary>
[System.Serializable]
public class BoneAsset
{
    //骨头的名字
    public string _BoneName;
    //骨头的位置
    public Vector3 _BonePositon;
    //是否是核心骨头
    public bool _IsRoot;
    //父骨头
    public int _LastBone;
    //子骨头
    public List<int> _NextBone;
    //骨头关联的所有顶点
    public List<int> _Vertex;
    //骨头的层级
    public int _Hierarchy;
    //是否在Inspector面板绘制子骨骼
    public bool _IsDrawSon;
}

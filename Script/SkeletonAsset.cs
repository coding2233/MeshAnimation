using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 骨架信息文件
/// </summary>
public class SkeletonAsset : ScriptableObject
{
    //骨架所属网格类型
    public string _SkeletonType;
    //所有骨头
    public List<BoneAsset> _Bones;
}

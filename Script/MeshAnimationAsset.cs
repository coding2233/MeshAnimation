using UnityEngine;
/// <summary>
/// 变形动画数据文件
/// </summary>
public class MeshAnimationAsset : ScriptableObject {

    public string _TargetName;

    public int _VertexNumber;

    public int _FrameNumber;

    public Vector3[] _VerticesAnimationArray;
}

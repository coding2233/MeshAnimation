using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 变形动画播放器
/// </summary>
[AddComponentMenu("变形动画/MeshAnimationPlayer")]
public class MeshAnimationPlayer : MonoBehaviour
{

    #region 变量
    //动画数据文件
    public MeshAnimationAsset _MeshAnimationAsset;
    //循环播放
    public bool _IsLoop = false;
    //启动时自动播放
    public bool _IsPlayOnAwake = false;
    //动画播放速度（每帧刷新1/30个片段）,越小播放越快
    public int _AnimationPlaySpeed = 30;

    //本物体是否可以播放变形动画
    private bool _IsCanPlay = false;
    //动画目标网格
    private Mesh _Mesh;
    //动画目标顶点
    private Vector3[] _Vertice;
    //动画是否播放中
    private bool _IsPlaying = false;
    //动画播放序列
    private int _AnimationIndex;
    //动画播放上一序列
    private int _AnimationLastIndex;
    //动画播放片段
    private Vector3[] _AnimationFragment;
    //动画播放控制器
    private int _AnimationPlayControl;
    //顶点动画数组
    private List<Vector3> _VerticesAnimationArraySave;
    private List<Vector3[]> _VerticesAnimationArray;
    #endregion

    #region 函数
    void Start () {
        //无法播放
        if (_MeshAnimationAsset == null)
        {
            Debug.LogWarning("Warning:缺少动画数据文件！");
            return;
        }
        if (GetComponent<MeshFilter>() == null)
        {
            Debug.LogWarning("Warning:目标物体缺少组件 MeshFilter！");
            return;
        }
        if (_MeshAnimationAsset._VertexNumber != GetComponent<MeshFilter>().mesh.vertices.Length)
        {
            Debug.LogError("Error:动画数据文件与目标网格不匹配！");
            return;
        }
        _Mesh = GetComponent<MeshFilter>().mesh;
        _Vertice = _Mesh.vertices;
        //动画片段
        _AnimationFragment = new Vector3[_MeshAnimationAsset._VertexNumber];
        //导入动画数据
        _VerticesAnimationArraySave = new List<Vector3>(_MeshAnimationAsset._VerticesAnimationArray);
        _VerticesAnimationArray = new List<Vector3[]>();
        Vector3[] vertice;
        while (_VerticesAnimationArraySave.Count > 0)
        {
            vertice = new Vector3[_MeshAnimationAsset._VertexNumber];
            for (int i = 0; i < vertice.Length; i++)
            {
                vertice[i] = _VerticesAnimationArraySave[0];
                _VerticesAnimationArraySave.RemoveAt(0);
            }
            _VerticesAnimationArray.Add(vertice);
        }
        _IsCanPlay = true;
        //启动时播放
        if (_IsPlayOnAwake)
        {
            Play();
        }
	}
	void Update () {
        if (_IsCanPlay && _IsPlaying)
        {
            //动画播放至最后一帧，动画播放完毕
            if (_AnimationIndex + 1 >= _VerticesAnimationArray.Count)
            {
                //判断是否循环播放
                if (_IsLoop)
                {
                    Play();
                    return;
                }
                Stop();
                return;
            }
            //当前动画播放序列不等于上一帧序列，则进入下一帧
            if (_AnimationIndex != _AnimationLastIndex)
            {
                _AnimationLastIndex = _AnimationIndex;
                //分割动画片段
                for (int i = 0; i < _AnimationFragment.Length; i++)
                {
                    _AnimationFragment[i] = (_VerticesAnimationArray[_AnimationIndex + 1][i] - _VerticesAnimationArray[_AnimationIndex][i]) / _AnimationPlaySpeed;
                }
            }
            //动画进行中
            for (int i = 0; i < _Vertice.Length; i++)
            {
                _Vertice[i] += _AnimationFragment[i];
            }
            //动画控制器计数
            _AnimationPlayControl += 1;
            //动画控制器记录的一个动画帧播放完毕
            if (_AnimationPlayControl >= _AnimationPlaySpeed)
            {
                _AnimationPlayControl = 0;
                _AnimationIndex += 1;
            }
            RefishMesh();
        }
	}
    /// <summary>
    /// 动画状态跳转到指定帧
    /// </summary>
    void SelectFrame(int number)
    {
        if (_VerticesAnimationArray[number] == null)
            return;

        GetComponent<MeshFilter>().mesh.vertices = _VerticesAnimationArray[number];
        GetComponent<MeshFilter>().mesh.RecalculateNormals();
        _Mesh = GetComponent<MeshFilter>().mesh;
        _Vertice = _Mesh.vertices;
    }
    /// <summary>
    /// 刷新网格
    /// </summary>
    void RefishMesh()
    {
        if (_Mesh != null)
        {
            //刷新网格
            _Mesh.vertices = _Vertice;
            _Mesh.RecalculateNormals();
        }
    }
    #endregion

    #region 控制开关
    /// <summary>
    /// 播放动画
    /// </summary>
    public void Play()
    {
        if (_IsCanPlay)
        {
            //从第一帧开始播放（顶点动画数组下标0）
            _AnimationIndex = 0;
            //重置记录动画播放上一序列的变量
            _AnimationLastIndex = -1;
            //重置动画播放控制器
            _AnimationPlayControl = 0;
            //动画跳转到第一帧
            SelectFrame(_AnimationIndex);
            _IsPlaying = true;
        }
    }
    /// <summary>
    /// 停止播放
    /// </summary>
    public void Stop()
    {
        if (_IsCanPlay)
        {
            _IsPlaying = false;
            //动画回归到第一帧
            SelectFrame(0);
        }
    }
    #endregion
}

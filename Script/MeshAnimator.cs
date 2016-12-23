using UnityEngine;
using System.Collections.Generic;
/// <summary>
/// 变形动画播放者
/// </summary>
[AddComponentMenu("变形动画/MeshAnimator")]
public class MeshAnimator : MonoBehaviour
{

    #region 变量
    //变形动画状态机
    public MeshAnimatorController _MeshAnimatorController;
    //动画播放速度（每帧刷新1/30个片段）,越小播放越快
    public int _AnimationPlaySpeed = 30;

    //当前的动画片段索引
    private int _PlayingIndex;
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
    //所有动画片段的数据
    private List<List<Vector3[]>> _VerticesAnimationArray;
    #endregion

    #region 函数
    void Start(){
        //无法播放
        if (_MeshAnimatorController == null)
        {
            Debug.LogWarning("Warning:缺少动画数据文件！");
            return;
        }
        if (GetComponent<MeshFilter>() == null)
        {
            Debug.LogWarning("Warning:目标物体缺少组件 MeshFilter！");
            return;
        }
        if (_MeshAnimatorController._MeshAnimationVertexNumber != GetComponent<MeshFilter>().mesh.vertices.Length)
        {
            Debug.LogError("Error:动画数据文件与目标网格不匹配！");
            return;
        }
        if (_MeshAnimatorController._MeshAnimationAssets.Count <= 0)
        {
            Debug.LogError("Error:没有动画数据！");
            return;
        }
        _Mesh = GetComponent<MeshFilter>().mesh;
        _Vertice = _Mesh.vertices;
        //动画片段
        _AnimationFragment = new Vector3[_MeshAnimatorController._MeshAnimationVertexNumber];
        //导入动画数据
        _VerticesAnimationArray = new List<List<Vector3[]>>();

        ImportAllAnimationClip();

        _PlayingIndex = _MeshAnimatorController._MeshAnimationAssets.IndexOf(_MeshAnimatorController._DefaultAnimation);
        _IsCanPlay = true;
        Play();
	}
    void Update()
    {
        if (_IsCanPlay && _IsPlaying)
        {
            //动画播放至最后一帧，动画播放完毕
            if (_AnimationIndex + 1 >= _VerticesAnimationArray[_PlayingIndex].Count)
            {
                OneClipPlayEnd();
                return;
            }
            //当前动画播放序列不等于上一帧序列，则进入下一帧
            if (_AnimationIndex != _AnimationLastIndex)
            {
                _AnimationLastIndex = _AnimationIndex;
                //分割动画片段
                for (int i = 0; i < _AnimationFragment.Length; i++)
                {
                    _AnimationFragment[i] = (_VerticesAnimationArray[_PlayingIndex][_AnimationIndex + 1][i] - _VerticesAnimationArray[_PlayingIndex][_AnimationIndex][i]) / _AnimationPlaySpeed;
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
    /// 一个动画片段播放完毕
    /// </summary>
    void OneClipPlayEnd()
    {
        //过渡目标
        List<int> target = new List<int>();
        //过渡条件的索引
        List<int> targetIndex = new List<int>();
        //寻找是否能够过渡到其他动画片段
        for (int i = 0; i < _MeshAnimatorController._MeshAnimationTransitions.Count; i++)
        {
            if ((int)_MeshAnimatorController._MeshAnimationTransitions[i].x == _PlayingIndex)
            {
                target.Add((int)_MeshAnimatorController._MeshAnimationTransitions[i].y);
                targetIndex.Add(i);
            }
        }
        //不能够过渡到其他动画片段，重复播放当前动画片段
        if (target.Count <= 0)
        {
            Play();
            return;
        }
        //是否自动过渡到下一片段
        for (int i = 0; i < target.Count; i++)
        {
            if (_MeshAnimatorController._HasExitTimes[targetIndex[i]] || _MeshAnimatorController._MeshAnimationTransitionType[targetIndex[i]])
            {
                _PlayingIndex = target[i];
                Play();
                return;
            }
        }
        //没有一个过渡条件满足，重复播放当前动画片段
        Play();
    }
    /// <summary>
    /// 动画状态跳转到指定帧
    /// </summary>
    void SelectFrame(int number)
    {
        if (_VerticesAnimationArray[_PlayingIndex][number] == null)
            return;

        GetComponent<MeshFilter>().mesh.vertices = _VerticesAnimationArray[_PlayingIndex][number];
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
    /// <summary>
    /// 导入所有动画片段
    /// </summary>
    void ImportAllAnimationClip()
    {
        List<Vector3[]> verticesAnimationArray;
        List<Vector3> verticesAnimationArraySave;
        for (int i = 0; i < _MeshAnimatorController._MeshAnimationAssets.Count; i++)
        {
            verticesAnimationArray = new List<Vector3[]>();
            verticesAnimationArraySave = new List<Vector3>(_MeshAnimatorController._MeshAnimationAssets[i]._VerticesAnimationArray);
            Vector3[] vertice;
            while (verticesAnimationArraySave.Count > 0)
            {
                vertice = new Vector3[_MeshAnimatorController._MeshAnimationVertexNumber];
                for (int j = 0; j < vertice.Length; j++)
                {
                    vertice[j] = verticesAnimationArraySave[0];
                    verticesAnimationArraySave.RemoveAt(0);
                }
                verticesAnimationArray.Add(vertice);
            }
            _VerticesAnimationArray.Add(verticesAnimationArray);
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
    /// <summary>
    /// 设置过渡条件
    /// </summary>
    public void SetBool(string boolName, bool value)
    {
        int index = _MeshAnimatorController._MeshAnimationTransitionName.IndexOf(boolName);
        if (index <= 0) return;
        _MeshAnimatorController._MeshAnimationTransitionType[index] = value;
    }
    /// <summary>
    /// 获取当前动画状态
    /// </summary>
    public string GetCurrentState()
    {
        if (_PlayingIndex < 0) return string.Empty;
        return _MeshAnimatorController._MeshAnimationAssets[_PlayingIndex].name;
    }
    /// <summary>
    /// 设置当前动画状态，立即切换到当前动画
    /// </summary>
    public void SetCurrentState(string AnimationName)
    {
        if (_IsCanPlay)
        {
            for (int i = 0; i < _MeshAnimatorController._MeshAnimationAssets.Count; i++)
            {
                if (_MeshAnimatorController._MeshAnimationAssets[i].name == AnimationName)
                {
                    _PlayingIndex = i;
                    Play();
                    return;
                }
            }
            Debug.LogWarning("Warning:状态机中不存在此动画片段！");
        }
    }
    #endregion
}

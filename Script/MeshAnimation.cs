using UnityEngine;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif
/// <summary>
/// 变形动画编辑器
/// </summary>
#region 前缀
[ExecuteInEditMode, DisallowMultipleComponent, AddComponentMenu("变形动画/MeshAnimation")]
#endregion
public class MeshAnimation : MonoBehaviour {

    //注：编辑完成之后，导出动画文件，并移除此脚本，在场景运行前，最好确保此脚本不再存在于任一物体上
    #region 变量
    //骨骼与蒙皮（骨架）
    public Skeleton _Skeleton;
    //骨骼与蒙皮资源（骨架）
    public SkeletonAsset _SkeletonAsset;
    //骨头大小
    [System.NonSerialized]
    public float _BoneSize = 0.1f;

    //顶点大小
    [SerializeField, Range(0, 1)]
    public float _VertexSize = 0.05f;
    //顶点大小缓存
    [System.NonSerialized]
    public float _LastVertexSize;
    //顶点数量
    [SerializeField]
    public int _VertexNumber = 0;
    //最大处理顶点数
    [System.NonSerialized]
    public int _VertexNum = 5000;
    //所有顶点物体集合
    [System.NonSerialized]
    public GameObject[] _Vertices = new GameObject[0];
    //物体网格
    [System.NonSerialized]
    public Mesh _Mesh;
    //顶点物体数量
    [System.NonSerialized]
    public int _VerticesNum = 0;
    //所有重复顶点集合
    [System.NonSerialized]
    public List<List<int>> _AllVerticesGroupList;
    //所有顶点集合
    [System.NonSerialized]
    public List<Vector3> _AllVerticesList;
    //记录所有顶点集合
    [System.NonSerialized]
    public List<Vector3> _RecordAllVerticesList;
    //筛选后的顶点集合
    [System.NonSerialized]
    public List<Vector3> _VerticesList;
    //所有需移除的顶点集合
    [System.NonSerialized]
    public List<int> _VerticesRemoveList;
    //用于筛选的顶点集合
    [System.NonSerialized]
    public List<int> _VerticesSubList;
    //顶点动画数组
    [System.NonSerialized]
    public List<Vector3[]> _VerticesAnimationArray = new List<Vector3[]>();
    //动画播放中
    [System.NonSerialized]
    public bool _IsPlay = false;
    //当前选中的帧
    [System.NonSerialized]
    public int _NowSelectFrame = -1;

    //动画播放序列
    private int _AnimationIndex;
    //动画播放上一序列
    private int _AnimationLastIndex;
    //动画播放片段
    private Vector3[] _AnimationFragment;
    //动画播放控制器
    private int _AnimationPlayControl;
    //动画播放速度（每帧刷新1/30个片段）,越小播放越快
    private int _AnimationPlaySpeed = 30;
    #endregion

    #region 函数
    void Start()
    {
#if UNITY_EDITOR
        #region 无法处理
        if (GetComponent<MeshFilter>() == null)
        {
            DestroyImmediate(GetComponent<MeshAnimation>());
            EditorUtility.DisplayDialog("警告", "游戏物体缺少组件 MeshFilter！", "确定");
            return;
        }
        if (GetComponent<MeshRenderer>() == null)
        {
            DestroyImmediate(GetComponent<MeshAnimation>());
            EditorUtility.DisplayDialog("警告", "游戏物体缺少组件 MeshRenderer！", "确定");
            return;
        }
        if (GetComponent<MeshRenderer>().sharedMaterial == null)
        {
            DestroyImmediate(GetComponent<MeshAnimation>());
            EditorUtility.DisplayDialog("警告", "游戏物体无材质！", "确定");
            return;
        }
        _VerticesNum = GetComponent<MeshFilter>().sharedMesh.vertices.Length;
        if (_VerticesNum > _VertexNum)
        {
            _VerticesNum = 0;
            DestroyImmediate(GetComponent<MeshAnimation>());
            EditorUtility.DisplayDialog("警告", "游戏物体顶点太多，我无法处理！", "确定");
            return;
        }
        if (transform.localScale != Vector3.one)
        {
            transform.localScale = Vector3.one;
            Debug.Log("游戏物体的缩放已归为初始值");
        }
#endregion

        #region 识别顶点
        _AllVerticesGroupList = new List<List<int>>();
        _AllVerticesList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices);
        _RecordAllVerticesList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices);
        _VerticesList = new List<Vector3>(GetComponent<MeshFilter>().sharedMesh.vertices);
        _VerticesRemoveList = new List<int>();

        //循环遍历并记录重复顶点
        for (int i = 0; i < _VerticesList.Count; i++)
        {
            EditorUtility.DisplayProgressBar("识别顶点", "正在识别顶点（" + i + "/" + _VerticesList.Count + "）......", 1.0f / _VerticesList.Count * i);
            //已存在于删除集合的顶点不计算
            if (_VerticesRemoveList.IndexOf(i) >= 0)
                continue;

            _VerticesSubList = new List<int>();
            _VerticesSubList.Add(i);
            int j = i + 1;
            //发现重复顶点，将之记录在内，并加入待删除集合
            while (j < _VerticesList.Count)
            {
                if (_VerticesList[i] == _VerticesList[j])
                {
                    _VerticesSubList.Add(j);
                    _VerticesRemoveList.Add(j);
                }
                j++;
            }
            //记录重复顶点集合
            _AllVerticesGroupList.Add(_VerticesSubList);
        }
        //整理待删除集合
        _VerticesRemoveList.Sort();
        //删除重复顶点
        for (int i = _VerticesRemoveList.Count - 1; i >= 0; i--)
        {
            _VerticesList.RemoveAt(_VerticesRemoveList[i]);
        }
        _VerticesRemoveList.Clear();
        #endregion

        #region 创建顶点
        _VerticesNum = _VerticesList.Count;
        _VertexNumber = _VerticesNum;
        //创建顶点，应用顶点大小设置，顶点位置为删除重复顶点之后的集合
        _Vertices = new GameObject[_VerticesNum];
        for (int i = 0; i < _VerticesNum; i++)
        {
            EditorUtility.DisplayProgressBar("创建顶点", "正在创建顶点（" + i + "/" + _VerticesNum + "）......", 1.0f / _VerticesNum * i);
            _Vertices[i] = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            _Vertices[i].name = "Vertex";
            _Vertices[i].transform.localScale = new Vector3(_VertexSize, _VertexSize, _VertexSize);
            _Vertices[i].transform.position = transform.localToWorldMatrix.MultiplyPoint3x4(_VerticesList[i]);
            _Vertices[i].GetComponent<MeshRenderer>().sharedMaterial = GetComponent<MeshRenderer>().sharedMaterial;
            _Vertices[i].AddComponent<Vertex>();
            _Vertices[i].GetComponent<Vertex>()._Identity = i;
            _Vertices[i].GetComponent<Vertex>()._MeshAnimation = this;
            _Vertices[i].GetComponent<Vertex>()._TheBoneName = "None";
            _Vertices[i].transform.SetParent(transform);
        }
        _LastVertexSize = _VertexSize;
        //重构网格
        _Mesh = new Mesh();
        _Mesh.Clear();
        _Mesh.vertices = _AllVerticesList.ToArray();
        _Mesh.triangles = GetComponent<MeshFilter>().sharedMesh.triangles;
        _Mesh.uv = GetComponent<MeshFilter>().sharedMesh.uv;
        _Mesh.name = GetComponent<MeshFilter>().sharedMesh.name;
        _Mesh.RecalculateNormals();
        GetComponent<MeshFilter>().sharedMesh = _Mesh;
        EditorUtility.ClearProgressBar();
        #endregion
#endif
    }
    void Update()
    {
#if UNITY_EDITOR
        RefishMesh();
        if (_LastVertexSize != _VertexSize)
            RefishVertexSize();
        if (_Skeleton == null && _SkeletonAsset != null)
            ImportSkeleton();
#endif
    }
    /// <summary>
    /// 导入骨架
    /// </summary>
    void ImportSkeleton()
    {
#if UNITY_EDITOR
        if (_SkeletonAsset._SkeletonType != _Mesh.name)
        {
            _SkeletonAsset = null;
            EditorUtility.DisplayDialog("警告", "这份骨架所包含的骨骼信息无法应用于当前网格！", "确定");
            return;
        }

        _Skeleton = new Skeleton();
        _Skeleton._Bones = new List<Bone>();
        //导入骨头信息
        for (int i = 0; i < _SkeletonAsset._Bones.Count; i++)
        {
            EditorUtility.DisplayProgressBar("导入骨架", "正在导入骨骼信息（" + i + "/" + _SkeletonAsset._Bones.Count + "）......", 1.0f / _SkeletonAsset._Bones.Count * i);

            Bone bone = new Bone();
            bone._BoneName = _SkeletonAsset._Bones[i]._BoneName;
            bone._BoneObj = new GameObject(bone._BoneName);
            bone._BonePositon = _SkeletonAsset._Bones[i]._BonePositon;
            bone._IsRoot = _SkeletonAsset._Bones[i]._IsRoot;
            bone._Vertex = new List<int>(_SkeletonAsset._Bones[i]._Vertex);
            bone._Hierarchy = _SkeletonAsset._Bones[i]._Hierarchy;
            bone._IsDrawSon = _SkeletonAsset._Bones[i]._IsDrawSon;

            _Skeleton._Bones.Add(bone);
        }
        //建立骨头关联信息
        for (int i = 0; i < _SkeletonAsset._Bones.Count; i++)
        {
            EditorUtility.DisplayProgressBar("导入骨架", "正在导入骨骼关联信息（" + i + "/" + _SkeletonAsset._Bones.Count + "）......", 1.0f / _SkeletonAsset._Bones.Count * i);

            if (_SkeletonAsset._Bones[i]._LastBone != -1)
            {
                _Skeleton._Bones[i]._LastBone = _Skeleton._Bones[_SkeletonAsset._Bones[i]._LastBone];
            }
            else
                _Skeleton._Bones[i]._LastBone = null;

            _Skeleton._Bones[i]._NextBone = new List<Bone>();
            if (_SkeletonAsset._Bones[i]._NextBone.Count > 0)
            {
                for (int j = 0; j < _SkeletonAsset._Bones[i]._NextBone.Count; j++)
                {
                    _Skeleton._Bones[i]._NextBone.Add(_Skeleton._Bones[_SkeletonAsset._Bones[i]._NextBone[j]]);
                }
            }
        }
        //导入骨头蒙皮信息
        for (int i = 0; i < _Skeleton._Bones.Count; i++)
        {
            EditorUtility.DisplayProgressBar("导入骨架", "正在应用骨骼信息（" + i + "/" + _Skeleton._Bones.Count + "）......", 1.0f / _Skeleton._Bones.Count * i);

            _Skeleton._Bones[i]._BoneObj.transform.position = _Skeleton._Bones[i]._BonePositon;
            if (_Skeleton._Bones[i]._LastBone != null)
            {
                _Skeleton._Bones[i]._BoneObj.transform.SetParent(_Skeleton._Bones[i]._LastBone._BoneObj.transform);
            }
            else
                _Skeleton._Bones[i]._BoneObj.transform.SetParent(transform);

            if (_Skeleton._Bones[i]._Vertex.Count > 0)
            {
                for (int j = 0; j < _Skeleton._Bones[i]._Vertex.Count; j++)
                {
                    _Vertices[_Skeleton._Bones[i]._Vertex[j]].transform.SetParent(_Skeleton._Bones[i]._BoneObj.transform);
                    _Vertices[_Skeleton._Bones[i]._Vertex[j]].GetComponent<Vertex>()._TheBoneName = _Skeleton._Bones[i]._BoneName;
                }
            }
        }

        EditorUtility.ClearProgressBar();
#endif
    }
    void OnDrawGizmos()
    {
#if UNITY_EDITOR
        if (_Skeleton != null && !_IsPlay)
        {
            DrawSkeleton();
        }
#endif
    }
    /// <summary>
    /// 画整个骨架
    /// </summary>
    void DrawSkeleton()
    {
        for (int i = 0; i < _Skeleton._Bones.Count; i++)
        {
            //画出核心骨头
            if (_Skeleton._Bones[i]._IsRoot)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_Skeleton._Bones[i]._BoneObj.transform.position, _BoneSize);
            }
            //画出其他骨头
            else
            {
                DrawBone(_Skeleton._Bones[i]._BoneObj.transform.position, _Skeleton._Bones[i]._LastBone._BoneObj.transform.position);
            }
        }
    }
    /// <summary>
    /// 画一条骨头
    /// </summary>
    void DrawBone(Vector3 start, Vector3 end)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(start, _BoneSize);
        Gizmos.DrawLine(start, end);
    }
    /// <summary>
    /// 删除骨头，并将此骨头下所有顶点、和子骨头下所有顶点归位
    /// </summary>
    void VertexHoming(Bone bone)
    {
        _Skeleton._Bones.Remove(bone);
        //此骨头关联的所有顶点归位
        for (int i = 0; i < bone._Vertex.Count; i++)
        {
            _Vertices[bone._Vertex[i]].transform.SetParent(transform);
            _Vertices[bone._Vertex[i]].GetComponent<Vertex>()._TheBoneName = "None";
        }
        //此骨头的子骨头关联的所有顶点归位
        for (int i = 0; i < bone._NextBone.Count; i++)
        {
            VertexHoming(bone._NextBone[i]);
        }
    }
    /// <summary>
    /// 刷新网格
    /// </summary>
    void RefishMesh()
    {
        if (_Mesh != null)
        {
            //重新应用顶点位置
            for (int i = 0; i < _Vertices.Length; i++)
            {
                for (int j = 0; j < _AllVerticesGroupList[i].Count; j++)
                {
                    _AllVerticesList[_AllVerticesGroupList[i][j]] = transform.worldToLocalMatrix.MultiplyPoint3x4(_Vertices[i].transform.position);
                }
            }
            //刷新网格
            _Mesh.vertices = _AllVerticesList.ToArray();
            _Mesh.RecalculateNormals();
        }
    }
    /// <summary>
    /// 刷新顶点大小
    /// </summary>
    void RefishVertexSize()
    {
        //刷新顶点大小
        if (_Vertices.Length > 0)
            if (_LastVertexSize != _VertexSize)
            {
                for (int i = 0; i < _Vertices.Length; i++)
                {
                    _Vertices[i].transform.localScale = new Vector3(_VertexSize, _VertexSize, _VertexSize);
                }
                _LastVertexSize = _VertexSize;
            }
    }
    /// <summary>
    /// 编辑结束
    /// </summary>
    void OnDestroy()
    {
#if UNITY_EDITOR
        for (int i = 0; i < _VerticesNum; i++)
        {
            EditorUtility.DisplayProgressBar("清除顶点", "正在清除顶点（" + i + "/" + _VerticesNum + "）......", 1.0f / _VerticesNum * i);
            if (_Vertices[i] != null)
                DestroyImmediate(_Vertices[i]);
        }
        if (_Skeleton != null)
        {
            for (int i = 0; i < _Skeleton._Bones.Count; i++)
            {
                if (_Skeleton._Bones[i]._BoneObj != null)
                    DestroyImmediate(_Skeleton._Bones[i]._BoneObj);
            }
        }
        if (_RecordAllVerticesList != null && _RecordAllVerticesList.Count > 0)
        {
            GetComponent<MeshFilter>().sharedMesh.vertices = _RecordAllVerticesList.ToArray();
            GetComponent<MeshFilter>().sharedMesh.RecalculateNormals();
        }
        EditorUtility.ClearProgressBar();
#endif
    }
    #endregion

    #region 操作功能
#if UNITY_EDITOR
    /// <summary>
    /// 新建骨架
    /// </summary>
    public void CreateSkeleton()
    {
        _Skeleton = new Skeleton();
        _Skeleton._Bones = new List<Bone>();
        //自动创建核心骨头
        CreateBone("RootBone",null);
    }
    /// <summary>
    /// 新建骨头
    /// </summary>
    public void CreateBone(string name,Bone lastBone)
    {
        Bone bone = new Bone();
        bone._BoneName = name;
        bone._BoneObj = new GameObject(name);
        bone._BonePositon = bone._BoneObj.transform.position;
        bone._IsRoot = (lastBone == null);
        bone._LastBone = lastBone;
        bone._NextBone = new List<Bone>();
        bone._Vertex = new List<int>();
        bone._Hierarchy = (bone._IsRoot ? 0 : lastBone._Hierarchy + 1);
        bone._IsDrawSon = true;

        bone._BoneObj.transform.position = (bone._IsRoot ? transform.position : (bone._LastBone._BoneObj.transform.position + new Vector3(0, 2, 0)));
        bone._BoneObj.transform.SetParent(bone._IsRoot ? transform : bone._LastBone._BoneObj.transform);

        if (lastBone != null)
        {
            lastBone._NextBone.Add(bone);
        }

        _Skeleton._Bones.Add(bone);
    }
    /// <summary>
    /// 删除骨头
    /// </summary>
    public void DeleteBone(Bone bone)
    {
        bone._LastBone._NextBone.Remove(bone);

        VertexHoming(bone);

        DestroyImmediate(bone._BoneObj);
    }
    /// <summary>
    /// 保存原型骨架
    /// </summary>
    public void SaveSkeleton()
    {
        //创建骨架数据文件
        _SkeletonAsset = ScriptableObject.CreateInstance<SkeletonAsset>();
        //记录骨架网格类型
        _SkeletonAsset._SkeletonType = _Mesh.name;
        //记录骨头信息
        _SkeletonAsset._Bones = new List<BoneAsset>();
        for (int i = 0; i < _Skeleton._Bones.Count; i++)
        {
            EditorUtility.DisplayProgressBar("保存原型骨架", "正在记录骨骼信息（" + i + "/" + _Skeleton._Bones.Count + "）......", 1.0f / _Skeleton._Bones.Count * i);

            BoneAsset boneAsset = new BoneAsset();
            boneAsset._BoneName = _Skeleton._Bones[i]._BoneName;
            boneAsset._BonePositon = _Skeleton._Bones[i]._BoneObj.transform.position;
            boneAsset._IsRoot = _Skeleton._Bones[i]._IsRoot;
            boneAsset._Vertex = new List<int>(_Skeleton._Bones[i]._Vertex);
            boneAsset._Hierarchy = _Skeleton._Bones[i]._Hierarchy;
            boneAsset._IsDrawSon = _Skeleton._Bones[i]._IsDrawSon;
            _SkeletonAsset._Bones.Add(boneAsset);
        }
        //记录骨头关联信息
        for (int i = 0; i < _Skeleton._Bones.Count; i++)
        {
            EditorUtility.DisplayProgressBar("保存原型骨架", "正在记录骨骼关联信息（" + i + "/" + _Skeleton._Bones.Count + "）......", 1.0f / _Skeleton._Bones.Count * i);

            if (_Skeleton._Bones[i]._LastBone != null)
            {
                int index = _Skeleton._Bones.IndexOf(_Skeleton._Bones[i]._LastBone);
                _SkeletonAsset._Bones[i]._LastBone = index;
            }
            else
                _SkeletonAsset._Bones[i]._LastBone = -1;

            _SkeletonAsset._Bones[i]._NextBone = new List<int>();
            if (_Skeleton._Bones[i]._NextBone.Count > 0)
            {
                for (int j = 0; j < _Skeleton._Bones[i]._NextBone.Count; j++)
                {
                    int index = _Skeleton._Bones.IndexOf(_Skeleton._Bones[i]._NextBone[j]);
                    _SkeletonAsset._Bones[i]._NextBone.Add(index);
                }
            }
        }
        //创建本地文件
        string path = "Assets/" + _Mesh.name + "SkeletonData.asset";
        AssetDatabase.CreateAsset(_SkeletonAsset, path);

        EditorUtility.ClearProgressBar();
    }
    /// <summary>
    /// 添加动画帧
    /// </summary>
    public void AddFrame()
    {
        //记录当前各顶点位置，并添加为新的动画帧数据到动画数组
        Vector3[] vertices = new Vector3[_Vertices.Length];
        for (int i = 0; i < _Vertices.Length; i++)
        {
            vertices[i] = _Vertices[i].transform.position;
        }
        _VerticesAnimationArray.Add(vertices);
    }
    /// <summary>
    /// 删除动画帧
    /// </summary>
    public void DeleteFrame()
    {
        //如果当前动画帧数据存在，则删除当前动画帧数据
        if (_NowSelectFrame >= 0 && _NowSelectFrame < _VerticesAnimationArray.Count)
        {
            _VerticesAnimationArray.RemoveAt(_NowSelectFrame);
            _NowSelectFrame = -1;
        }
    }
    /// <summary>
    /// 应用动画帧
    /// </summary>
    public void ApplyFrame()
    {
        //如果当前动画帧数据存在，则应用当前物体的各顶点数据至当前动画帧
        if (_NowSelectFrame >= 0 && _NowSelectFrame < _VerticesAnimationArray.Count)
        {
            for (int i = 0; i < _Vertices.Length; i++)
            {
                _VerticesAnimationArray[_NowSelectFrame][i] = _Vertices[i].transform.position;
            }
        }
    }
    /// <summary>
    /// 选定指定帧
    /// </summary>
    public void SelectFrame(int frameIndex)
    {
        //如果当前动画帧数据存在，则选定当前动画帧，所有顶点应用当前动画帧数据
        if (frameIndex >= 0 && frameIndex < _VerticesAnimationArray.Count)
        {
            _NowSelectFrame = frameIndex;
            for (int i = 0; i < _Vertices.Length; i++)
            {
                _Vertices[i].transform.position = _VerticesAnimationArray[frameIndex][i];
            }
        }
    }
    /// <summary>
    /// 预览动画
    /// </summary>
    public void PlayAnimation()
    {
        //没有动画可以预览
        if (_VerticesAnimationArray.Count <= 0)
        {
            return;
        }
        //预览从第一帧开始（顶点动画数组下标0）
        _AnimationIndex = 0;
        //重置记录动画播放上一序列的变量
        _AnimationLastIndex = -1;
        //重建新的动画片段
        _AnimationFragment = new Vector3[_Vertices.Length];
        //重置动画播放控制器
        _AnimationPlayControl = 0;
        //动画进入到第一帧
        for (int i = 0; i < _Vertices.Length; i++)
        {
            _Vertices[i].transform.position = _VerticesAnimationArray[0][i];
        }
        _IsPlay = true;
        //将刷新动画函数注册到Unity编辑器帧执行模块
        EditorApplication.update += PlayingAnimation;
    }
    /// <summary>
    /// 动画预览中
    /// </summary>
    void PlayingAnimation()
    {
        if (_IsPlay)
        {
            //动画播放至最后一帧，动画播放完毕
            if (_AnimationIndex + 1 >= _VerticesAnimationArray.Count)
            {
                //动画播放完毕
                _IsPlay = false;
                //清除刷新动画函数的注册
                EditorApplication.update -= PlayingAnimation;
                //动画回归到第一帧
                for (int i = 0; i < _Vertices.Length; i++)
                {
                    _Vertices[i].transform.position = _VerticesAnimationArray[0][i];
                }
                return;
            }
            //当前动画播放序列不等于上一帧序列，则进入下一帧
            if (_AnimationIndex != _AnimationLastIndex)
            {
                _AnimationLastIndex = _AnimationIndex;
                //分割动画片段
                for (int i = 0; i < _AnimationFragment.Length; i++)
                {
                    _AnimationFragment[i] = (_VerticesAnimationArray[_AnimationIndex + 1][i] - _VerticesAnimationArray[_AnimationIndex][i])/ _AnimationPlaySpeed;
                }
            }
            //动画进行中
            for (int i = 0; i < _Vertices.Length; i++)
            {
                _Vertices[i].transform.position += _AnimationFragment[i];
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
    /// 导出动画
    /// </summary>
    public void ExportAnimation()
    {
        //动画帧数小于等于1不允许导出
        if (_VerticesAnimationArray.Count <= 1)
            return;
        //创建动画数据文件
        MeshAnimationAsset meshAnimationAsset = ScriptableObject.CreateInstance<MeshAnimationAsset>();
        //记录目标名称
        meshAnimationAsset._TargetName = gameObject.name;
        //记录动画顶点数
        meshAnimationAsset._VertexNumber = _RecordAllVerticesList.Count;
        //记录动画帧数
        meshAnimationAsset._FrameNumber = _VerticesAnimationArray.Count;
        //记录动画帧数据
        meshAnimationAsset._VerticesAnimationArray = new Vector3[_VerticesAnimationArray.Count * _RecordAllVerticesList.Count];
        for (int n = 0; n < _VerticesAnimationArray.Count; n++)
        {
            for (int i = 0; i < _VerticesAnimationArray[n].Length; i++)
            {
                for (int j = 0; j < _AllVerticesGroupList[i].Count; j++)
                {
                    int number = n * _RecordAllVerticesList.Count + _AllVerticesGroupList[i][j];
                    EditorUtility.DisplayProgressBar("导出动画", "正在导出顶点数据（" + number + "/" + meshAnimationAsset._VerticesAnimationArray.Length + "）......", 1.0f / meshAnimationAsset._VerticesAnimationArray.Length * number);
                    meshAnimationAsset._VerticesAnimationArray[number] = transform.worldToLocalMatrix.MultiplyPoint3x4(_VerticesAnimationArray[n][i]);
                }
            }
        }
        //创建本地文件
        string path = "Assets/" + GetComponent<MeshFilter>().sharedMesh.name + "AnimationData.asset";
        AssetDatabase.CreateAsset(meshAnimationAsset, path);

        EditorUtility.ClearProgressBar();
    }
#endif
    #endregion
}

using UnityEngine;
using UnityEditor;
/// <summary>
/// 变形动画状态机界面
/// </summary>
public class MeshAnimationStateMachine : EditorWindow{

    [@MenuItem("Window/变形动画状态机")]
    static void main()
    {
        MeshAnimationStateMachine _MeshAnimationStateMachine = EditorWindow.GetWindow<MeshAnimationStateMachine>(false, "变形动画状态机");
        _MeshAnimationStateMachine.titleContent = new GUIContent(EditorGUIUtility.IconContent("Mirror"));
        _MeshAnimationStateMachine.titleContent.text = "变形动画状态机";
        _MeshAnimationStateMachine.autoRepaintOnSceneChange = true;
        _MeshAnimationStateMachine.Show();
    }

    //当前编辑的状态机
    public MeshAnimatorController _MeshAnimatorController;
    //当前选中的状态
    private MeshAnimationAsset _Selected;
    //当前选中的过渡连线
    private int _SelectedLine;
    //是否记录动画片段的位置
    private bool _RecordMousePosition = false;

    void Update()
    {
        #region 选中变形动画状态机
        if (Selection.objects.Length == 1 && Selection.objects[0].GetType() == typeof(MeshAnimatorController) && _MeshAnimatorController != Selection.objects[0])
        {
            _MeshAnimatorController = (MeshAnimatorController)Selection.objects[0];
            _Selected = null;
            _SelectedLine = -1;
            _RecordMousePosition = false;
            Focus();
        }
        #endregion
    }
    void OnGUI()
    {
        if (_MeshAnimatorController == null)
            return;

        #region 鼠标拖拽动画片段至窗口中
        //拖拽变形动画片段至本界面
        if (Event.current != null && Event.current.type == EventType.DragUpdated)
        {
            if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0].GetType() == typeof(MeshAnimationAsset))
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
            }
        }
        //放置变形动画片段至本界面
        if (Event.current != null && Event.current.type == EventType.DragExited)
        {
            if (DragAndDrop.objectReferences.Length == 1 && DragAndDrop.objectReferences[0].GetType() == typeof(MeshAnimationAsset))
            {
                Focus();
                MeshAnimationAsset meshAnimationAsset = (MeshAnimationAsset)DragAndDrop.objectReferences[0];
                //筛选不匹配的动画片段
                if (_MeshAnimatorController._MeshAnimationAssets.Count > 0 && meshAnimationAsset._VertexNumber != _MeshAnimatorController._MeshAnimationVertexNumber)
                {
                    Debug.LogWarning("Warning:不匹配的动画片段！");
                    return;
                }
                //筛选已经存在的动画片段
                if (_MeshAnimatorController._MeshAnimationAssets.IndexOf(meshAnimationAsset) >= 0)
                {
                    Debug.LogWarning("Warning:已经存在此动画片段！");
                    _Selected = meshAnimationAsset;
                    return;
                }
                //合格的动画片段，加入状态机
                _MeshAnimatorController._MeshAnimationAssets.Add(meshAnimationAsset);
                _RecordMousePosition = true;
                _Selected = meshAnimationAsset;
                //如果是第一个动画片段就记录动画顶点数并设为初始动画片段
                if (_MeshAnimatorController._MeshAnimationAssets.Count == 1)
                {
                    _MeshAnimatorController._MeshAnimationVertexNumber = _MeshAnimatorController._MeshAnimationAssets[0]._VertexNumber;
                    _MeshAnimatorController._DefaultAnimation = _MeshAnimatorController._MeshAnimationAssets[0];
                }
            }
        }
        //记录放置的动画片段位置
        if (_RecordMousePosition && Event.current != null && Event.current.type != EventType.DragExited)
        {
            _MeshAnimatorController._MeshAnimationAssetPosition.Add(Event.current.mousePosition);
            _RecordMousePosition = false;
        }
        #endregion

        #region 绘制过渡关系
        for (int i = 0; i < _MeshAnimatorController._MeshAnimationTransitions.Count; i++)
        {
            Handles.DrawBezier(_MeshAnimatorController._MeshAnimationAssetPosition[(int)_MeshAnimatorController._MeshAnimationTransitions[i].x] + new Vector2(75,0),
                _MeshAnimatorController._MeshAnimationAssetPosition[(int)_MeshAnimatorController._MeshAnimationTransitions[i].y] - new Vector2(75,0),
                _MeshAnimatorController._MeshAnimationAssetPosition[(int)_MeshAnimatorController._MeshAnimationTransitions[i].x] + new Vector2(275,0),
                _MeshAnimatorController._MeshAnimationAssetPosition[(int)_MeshAnimatorController._MeshAnimationTransitions[i].y] - new Vector2(275, 0),
                i == _SelectedLine ? Color.red : Color.white, null, 2);
        }
        #endregion

        #region 所有动画片段的显示按钮
        try
        {
            for (int i = 0; i < _MeshAnimatorController._MeshAnimationAssets.Count; i++)
            {
                //所有动画片段的显示风格
                GUIStyle style;
                if (_MeshAnimatorController._DefaultAnimation == _MeshAnimatorController._MeshAnimationAssets[i])
                {
                    if (_Selected == _MeshAnimatorController._MeshAnimationAssets[i]) style = "flow node 5 on";
                    else style = "flow node 5";
                }
                else
                {
                    if (_Selected == _MeshAnimatorController._MeshAnimationAssets[i]) style = "flow node 0 on";
                    else style = "flow node 0";
                }

                //所有动画片段的操控按钮
                if (GUI.RepeatButton(new Rect(_MeshAnimatorController._MeshAnimationAssetPosition[i].x - 75, _MeshAnimatorController._MeshAnimationAssetPosition[i].y - 15, 150, 30),
                    _MeshAnimatorController._MeshAnimationAssets[i].name, style))
                {
                    _Selected = _MeshAnimatorController._MeshAnimationAssets[i];
                    _SelectedLine = -1;
                    if (Event.current.button == 1)
                    {
                        //右击动画片段
                        int j = i;
                        GenericMenu genericMenu = new GenericMenu();

                        for (int n = 0; n < _MeshAnimatorController._MeshAnimationAssets.Count; n++)
                        {
                            int m = n;
                            if (j != m)
                                genericMenu.AddItem(new GUIContent("过渡至/" + _MeshAnimatorController._MeshAnimationAssets[n].name), false, delegate() { ConnectAnimation(j, m); });
                        }

                        if (_MeshAnimatorController._DefaultAnimation == _MeshAnimatorController._MeshAnimationAssets[j])
                            genericMenu.AddDisabledItem(new GUIContent("设为初始状态"));
                        else
                            genericMenu.AddItem(new GUIContent("设为初始状态"), false, delegate() { SetDefaultAnimation(j); });

                        genericMenu.AddItem(new GUIContent("删除"), false, delegate() { DeleteAnimation(j); });

                        genericMenu.ShowAsContext();
                        continue;
                    }
                    _MeshAnimatorController._MeshAnimationAssetPosition[i] = Event.current.mousePosition;
                    Repaint();
                }
            }
        }
        catch
        {
        }
        #endregion

        #region 窗口标题栏
        GUILayout.BeginHorizontal("toolbarbutton");
        GUILayout.Label(_MeshAnimatorController.name);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Number Of Vertex:" + _MeshAnimatorController._MeshAnimationVertexNumber.ToString(), "GV Gizmo DropDown"))
        {
            if (Event.current.button == 0)
            {
                //窗口右上角的重置当前顶点数
                GenericMenu genericMenu = new GenericMenu();
                genericMenu.AddItem(new GUIContent("重置"), false, delegate() { ReSet(); });
                genericMenu.ShowAsContext();
            }
        }
        if (GUILayout.Button("Number Of AnimationClip:" + _MeshAnimatorController._MeshAnimationAssets.Count.ToString(), "GV Gizmo DropDown"))
        {
            if (Event.current.button == 0)
            {
                //窗口右上角的所有动画片段索引
                GenericMenu genericMenu = new GenericMenu();
                for (int i = 0; i < _MeshAnimatorController._MeshAnimationAssets.Count; i++)
                {
                    int j = i;
                    genericMenu.AddItem(new GUIContent(_MeshAnimatorController._MeshAnimationAssets[i].name), false, delegate(){ SelectAnimationIndex(j); });
                }
                genericMenu.ShowAsContext();
            }
        }
        GUILayout.EndHorizontal();
        #endregion

        #region 当前选中动画片段的属性
        try
        {
            if (_Selected != null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("AnimationClip Name:" + _Selected.name, "PR PrefabLabel");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Number Of Frames:" + _Selected._FrameNumber, "PR PrefabLabel");
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                int number = _MeshAnimatorController._MeshAnimationAssets.IndexOf(_Selected);
                for (int i = 0; i < _MeshAnimatorController._MeshAnimationTransitions.Count; i++)
                {
                    //只显示当前选中片段过渡到其他片段的连线
                    if (_MeshAnimatorController._MeshAnimationTransitions[i].x == number)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button(_Selected.name + " —> " + _MeshAnimatorController._MeshAnimationAssets[(int)_MeshAnimatorController._MeshAnimationTransitions[i].y].name, "LargeButton"))
                        {
                            if (_SelectedLine == i)
                            {
                                _SelectedLine = -1;
                            }
                            else
                            {
                                _SelectedLine = i;
                            }
                        }
                        GUILayout.FlexibleSpace();
                        GUILayout.EndHorizontal();

                        if (_SelectedLine == i)
                        {
                            GUILayout.BeginHorizontal();
                            if (GUILayout.Button("delete", "button"))
                            {
                                DeleteTransition(i);
                            }
                            _MeshAnimatorController._HasExitTimes[i] = GUILayout.Toggle(_MeshAnimatorController._HasExitTimes[i], "Has Exit Time", "toggle");
                            if (!_MeshAnimatorController._HasExitTimes[i])
                            {
                                _MeshAnimatorController._MeshAnimationTransitionName[i] = GUILayout.TextField(_MeshAnimatorController._MeshAnimationTransitionName[i], "textfield", GUILayout.Width(100));
                                _MeshAnimatorController._MeshAnimationTransitionType[i] = GUILayout.Toggle(_MeshAnimatorController._MeshAnimationTransitionType[i], "", "toggle");
                                if (_MeshAnimatorController._MeshAnimationTransitionType[i] && _MeshAnimatorController._MeshAnimationTransitionName[i] == "")
                                {
                                    Debug.LogWarning("Warning:过渡条件的bool判断值名称不能为空！特别注意：名称可以重复，但可能会出现你不想要的效果！");
                                    _MeshAnimatorController._MeshAnimationTransitionType[i] = false;
                                }
                            }
                            GUILayout.FlexibleSpace();
                            GUILayout.EndHorizontal();
                        }
                    }
                }
            }
        }
        catch
        { 
        }
        #endregion

        #region 如果某一动画片段为空，则该动画片段文件在工程中被删除
        for (int i = 0; i < _MeshAnimatorController._MeshAnimationAssets.Count; i++)
        {
            if (_MeshAnimatorController._MeshAnimationAssets[i] == null)
            {
                DeleteAnimation(i);
            }
        }
        #endregion
    }
    /// <summary>
    /// 重置当前顶点数
    /// </summary>
    void ReSet()
    {
        if (_MeshAnimatorController._MeshAnimationAssets.Count > 0)
        {
            Debug.LogWarning("Warning:当前状态机内还存在动画片段！若要重置，请删除所有片段！");
            return;
        }
        _MeshAnimatorController._MeshAnimationVertexNumber = 0;
    }
    /// <summary>
    /// 选中动画片段索引
    /// </summary>
    void SelectAnimationIndex(int index)
    {
        _Selected = _MeshAnimatorController._MeshAnimationAssets[index];
        if (_MeshAnimatorController._MeshAnimationAssetPosition[index].x > position.width || _MeshAnimatorController._MeshAnimationAssetPosition[index].x < 0 ||
            _MeshAnimatorController._MeshAnimationAssetPosition[index].y > position.height || _MeshAnimatorController._MeshAnimationAssetPosition[index].y < 0)
            _MeshAnimatorController._MeshAnimationAssetPosition[index] = new Vector2(position.width / 2, position.height / 2);
        Repaint();
    }
    /// <summary>
    /// 连接动画片段（建立过渡方式）
    /// </summary>
    void ConnectAnimation(int start, int end)
    {
        Vector2 vec = new Vector2(start,end);
        for (int i = 0; i < _MeshAnimatorController._MeshAnimationTransitions.Count; i++)
        {
            if (_MeshAnimatorController._MeshAnimationTransitions[i] == vec)
            {
                Debug.LogWarning("Warning:已经存在此过渡关系！" + _MeshAnimatorController._MeshAnimationAssets[start].name + " 过渡至 " + _MeshAnimatorController._MeshAnimationAssets[end].name);
                return;
            }
        }
        _MeshAnimatorController._MeshAnimationTransitions.Add(vec);
        _MeshAnimatorController._HasExitTimes.Add(true);
        _MeshAnimatorController._MeshAnimationTransitionType.Add(false);
        _MeshAnimatorController._MeshAnimationTransitionName.Add("");
    }
    /// <summary>
    /// 设为初始状态
    /// </summary>
    void SetDefaultAnimation(int index)
    {
        _MeshAnimatorController._DefaultAnimation = _MeshAnimatorController._MeshAnimationAssets[index];
    }
    /// <summary>
    /// 删除过渡方式
    /// </summary>
    void DeleteTransition(int index)
    {
        _MeshAnimatorController._MeshAnimationTransitions.RemoveAt(index);
        _MeshAnimatorController._HasExitTimes.RemoveAt(index);
        _MeshAnimatorController._MeshAnimationTransitionType.RemoveAt(index);
        _MeshAnimatorController._MeshAnimationTransitionName.RemoveAt(index);
        _SelectedLine = -1;
    }
    /// <summary>
    /// 删除动画片段
    /// </summary>
    void DeleteAnimation(int index)
    {
        //删除相关联的过渡方式
        for (int i = 0; i < _MeshAnimatorController._MeshAnimationTransitions.Count; i++)
        {
            if ((int)_MeshAnimatorController._MeshAnimationTransitions[i].x == index || (int)_MeshAnimatorController._MeshAnimationTransitions[i].y == index)
            {
                DeleteTransition(i);
                i -= 1;
                continue;
            }
            if (_MeshAnimatorController._MeshAnimationTransitions[i].x > index)
                _MeshAnimatorController._MeshAnimationTransitions[i] = new Vector2(_MeshAnimatorController._MeshAnimationTransitions[i].x - 1, _MeshAnimatorController._MeshAnimationTransitions[i].y);
            if (_MeshAnimatorController._MeshAnimationTransitions[i].y > index)
                _MeshAnimatorController._MeshAnimationTransitions[i] = new Vector2(_MeshAnimatorController._MeshAnimationTransitions[i].x, _MeshAnimatorController._MeshAnimationTransitions[i].y - 1);
        }
        //删除动画片段
        _MeshAnimatorController._MeshAnimationAssets.RemoveAt(index);
        _MeshAnimatorController._MeshAnimationAssetPosition.RemoveAt(index);
        _Selected = null;
        if (_MeshAnimatorController._MeshAnimationAssets.Count <= 0)
        {
            _MeshAnimatorController._DefaultAnimation = null;
        }
        else
        {
            if (_MeshAnimatorController._MeshAnimationAssets.IndexOf(_MeshAnimatorController._DefaultAnimation) < 0)
                _MeshAnimatorController._DefaultAnimation = _MeshAnimatorController._MeshAnimationAssets[0];
        }
    }
}

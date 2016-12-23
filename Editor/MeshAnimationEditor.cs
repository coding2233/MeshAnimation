using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(MeshAnimation))]
public class MeshAnimationEditor : Editor
{
    private MeshAnimation _MeshAnimation;
    //子骨骼的名称
    private string _BoneName;
    //是否准备添加子骨骼
    private bool _IsAddSubBone;
    //准备添加子骨骼的父骨骼
    private Bone _IsAddLastBone;

    public void OnEnable()
    {
        _MeshAnimation = (MeshAnimation)target;
        _BoneName = "";
        _IsAddSubBone = false;
    }
    public override void OnInspectorGUI()
    {
        if (_MeshAnimation == null)
            return;

        if (!_MeshAnimation._IsPlay)
        {
            base.OnInspectorGUI();
            
            #region 骨骼与蒙皮
            if (_MeshAnimation._Skeleton == null)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("新建骨架", "flow node 1 on", GUILayout.Height(30), GUILayout.Width(100)))
                {
                    _MeshAnimation.CreateSkeleton();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            if (_MeshAnimation._Skeleton != null)
            {
                ShowNextBone(_MeshAnimation._Skeleton._Bones[0]);
            }
            if (_IsAddSubBone)
            {
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                GUILayout.Label("BoneName");
                _BoneName = GUILayout.TextField(_BoneName, GUILayout.Width(100));
                if (GUILayout.Button("Sure", "minibutton", GUILayout.Width(60)))
                {
                    CreateSure();
                }
                if (GUILayout.Button("Cancel", "minibutton", GUILayout.Width(60)))
                {
                    CreateCancel();
                }
                GUILayout.EndHorizontal();
            }
            if (_MeshAnimation._Skeleton != null && _MeshAnimation._Skeleton._Bones.Count >= 2)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("");
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("保存为原型骨架", "flow node 1 on", GUILayout.Height(30), GUILayout.Width(100)))
                {
                    _MeshAnimation.SaveSkeleton();
                }
                GUILayout.Space(10);
                if (GUILayout.Button("快速蒙皮", "flow node 1 on", GUILayout.Height(30), GUILayout.Width(80)))
                {
                    OpenSkinWindow();
                }
                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();
            }
            #endregion

            //分割线
            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("RL DragHandle");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            #region 动画帧
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("添加动画帧", "LargeButton"))
            {
                _MeshAnimation.AddFrame();
            }
            GUILayout.EndHorizontal();

            if (_MeshAnimation._VerticesAnimationArray.Count > 0)
            {
                for (int i = 0; i < _MeshAnimation._VerticesAnimationArray.Count; i++)
                {
                    if (_MeshAnimation._NowSelectFrame == i)
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("动画帧" + (i + 1), "TL SelectionButton PreDropGlow"))
                        {
                            _MeshAnimation.SelectFrame(i);
                        }
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("Delete", "minibuttonleft"))
                        {
                            _MeshAnimation.DeleteFrame();
                        }
                        if (GUILayout.Button("Apply", "minibuttonright"))
                        {
                            _MeshAnimation.ApplyFrame();
                        }
                        GUILayout.EndHorizontal();
                    }
                    else
                    {
                        GUILayout.BeginHorizontal();
                        if (GUILayout.Button("动画帧" + (i + 1), "PreButton"))
                        {
                            _MeshAnimation.SelectFrame(i);
                        }
                        GUILayout.EndHorizontal();
                    }
                }
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("预览动画", "LargeButtonLeft"))
            {
                _MeshAnimation.PlayAnimation();
            }
            if (GUILayout.Button("导出动画", "LargeButtonRight"))
            {
                _MeshAnimation.ExportAnimation();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("编辑完成", "flow node 1 on", GUILayout.Height(30), GUILayout.Width(100)))
            {
                Finish();
            }
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            #endregion

            //分割线
            GUILayout.BeginHorizontal();
            GUILayout.Label("");
            GUILayout.EndHorizontal();
            GUILayout.BeginHorizontal("RL DragHandle");
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }
    }
    /// <summary>
    /// 画出一块骨骼及他的子骨骼
    /// </summary>
    void ShowNextBone(Bone bone)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Space(bone._Hierarchy * 20);
        //是否允许显示子骨骼
        if (GUILayout.Button("", bone._IsDrawSon ? "Grad Down Swatch" : "Grad Up Swatch"))
        {
            bone._IsDrawSon = !bone._IsDrawSon;
        }
        //骨骼分层级显示
        string style = bone._IsDrawSon ? "PR PrefabLabel" : "PR DisabledPrefabLabel";
        if (_IsAddLastBone == bone) style = "PR BrokenPrefabLabel";
        if (GUILayout.Button(bone._BoneName, style))
        {
            Selection.objects = new UnityEngine.Object[] { bone._BoneObj };
        }
        GUILayout.FlexibleSpace();
        //为此骨骼添加子骨骼
        if (GUILayout.Button("AddSubBone", "toolbarbutton", GUILayout.Width(80)))
        {
            _IsAddLastBone = bone;
            _IsAddSubBone = true;
        }
        //删除骨骼
        if (!bone._IsRoot)
        {
            if (GUILayout.Button("Delete", "toolbarbutton", GUILayout.Width(60)))
            {
                _MeshAnimation.DeleteBone(bone);
            }
        }
        GUILayout.EndHorizontal();
        //是否允许绘制子骨骼
        if (bone._IsDrawSon)
        {
            for (int i = 0; i < bone._NextBone.Count; i++)
            {
                ShowNextBone(bone._NextBone[i]);
            }
        }
    }
    /// <summary>
    /// 添加骨骼确定
    /// </summary>
    void CreateSure()
    {
        if (_IsAddLastBone != null)
        {
            if (_BoneName == "") _BoneName = "Bone";
            _MeshAnimation.CreateBone(_BoneName, _IsAddLastBone);
        }
        CreateCancel();
    }
    /// <summary>
    /// 添加骨骼取消
    /// </summary>
    void CreateCancel()
    {
        _BoneName = "";
        _IsAddSubBone = false;
        _IsAddLastBone = null;
    }
    /// <summary>
    /// 打开快速蒙皮窗口
    /// </summary>
    void OpenSkinWindow()
    {
        SkeletonSkin _SkeletonSkin = EditorWindow.GetWindowWithRect<SkeletonSkin>(new Rect(0, 0, 120, 30),false, "快速蒙皮");
        _SkeletonSkin.titleContent = new GUIContent(EditorGUIUtility.IconContent("AvatarInspector/BodySilhouette"));
        _SkeletonSkin.titleContent.text = "快速蒙皮";
        _SkeletonSkin.autoRepaintOnSceneChange = true;
        _SkeletonSkin.Show();
        _SkeletonSkin._MeshAnimation = _MeshAnimation;
    }
    void Finish()
    {
        DestroyImmediate(_MeshAnimation);
        _MeshAnimation = null;
    }
}

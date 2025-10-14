using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MinigameManager : Singleton<MinigameManager>
{
    [Header("Level & UI")]
    public LevelSO level;
    public RectTransform board;      // 画布区域（Canvas/Board）
    public Transform nodesParent;    // 节点父物体（放 NodeView）
    public GameObject nodeViewPrefab;
    public Canvas canvas; // ← 新增：把场景里的 Canvas 拖进来

    [Header("Drawing")]
    public LineRenderer linePrefab;
    public Transform linesParent;
    public float snapPixelRadius = 40f;     // 吸附半径（屏幕像素）
    public float margin = 100f;             // 网格在 board 内的边距（像素）

    [Header("Colors")]
    public Color nodeColor = new Color(0.2f, 0.8f, 1f, 1f);
    public Color orderedColor = new Color(1f, 0.6f, 0.2f, 1f);
    public Color lineColor = Color.white;

    // ----- Walls Visuals -----
    [Header("Walls Visuals")]
    public Transform wallsParent;        // 建个空物体：Canvas/Board 下的 Walls
    public Color wallColor = new Color(0.15f, 0.2f, 0.3f, 1f);
    public float wallThickness = 8f;     // UI像素宽度


    // 运行时
    private NodeView[,] views;
    private HashSet<EdgeCoord> wallSet;
    private HashSet<NodeCoord> visited = new();
    private Dictionary<NodeCoord, int> orderedIndex = new();
    private List<NodeCoord> path = new();         // 走过的节点序列
    private LineRenderer line;
    private bool dragging = false;
    private int nextOrdered = 0;

    bool _built = false;

    public event System.Action<bool> OnFinished;
    private bool bonusVisited = false;   // 本轮是否踩过 Bonus 节点

    private bool _active = false;   // 面板可交互/运行的开关

    // 线状态
    bool pathCommitted = false;     // 松手后锁定本轮，禁止再开新线

    // 允许一步“回退”
    [SerializeField] bool allowBacktrack = true;


    void Start()
    {
        if (!level || !board || !nodeViewPrefab || !linePrefab || !linesParent || !nodesParent)
        {
            Debug.LogError("MinigameManager: Inspector 引用未绑定完整。");
            enabled = false; return;
        }
        //BuildLevel();

        foreach (Transform t in nodesParent) Destroy(t.gameObject);
        foreach (Transform t in linesParent) Destroy(t.gameObject);
    }

    // 新增：供外部（对话分屏后）调用
    public void InitIfNeeded()
    {
        if (_built)
        {
            Debug.Log("[MinigameManager] 已初始化过，跳过 InitIfNeeded。");
            return;
        }
        Debug.Log("[MinigameManager] InitIfNeeded() 被调用 —— 开始生成关卡。");

        // 让 Board 贴满 MinigamePanel（父物体）
        StretchFull(board);
        BuildLevel();
        _built = true;
        _active = true;
        // 保险：可见&可交互
        board.gameObject.SetActive(true);
        if (canvas != null) canvas.enabled = true;
        Debug.Log("[MinigameManager] BuildLevel() 执行完毕 —— 游戏应该已生成节点。");
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void BuildLevel()
    {

        Debug.Log("[MinigameManager]  BuildLevel() 开始执行。");
        // 清空旧物
        foreach (Transform t in nodesParent) Destroy(t.gameObject);
        foreach (Transform t in linesParent) Destroy(t.gameObject);

        // 墙集合
        wallSet = new HashSet<EdgeCoord>(level.walls);

        // 清理旧墙体
        if (wallsParent != null)
        {
            foreach (Transform t in wallsParent) Destroy(t.gameObject);

            // 逐条墙（被禁用边）生成一条UI矩形
            foreach (var e in level.walls)
            {
                var go = new GameObject("Wall", typeof(RectTransform), typeof(UnityEngine.UI.Image));
                go.transform.SetParent(wallsParent, false);
                var img = go.GetComponent<UnityEngine.UI.Image>();
                img.color = wallColor;

                var rt = go.GetComponent<RectTransform>();
                Vector3 a = NodeToWorld(e.a);
                Vector3 b = NodeToWorld(e.b);
                Vector3 midWorld = (a + b) * 0.5f;

                // 转到 Board 的局部坐标
                Vector2 midLocal = (Vector2)board.InverseTransformPoint(midWorld);
                rt.anchoredPosition = midLocal;

                float len = Vector2.Distance(board.InverseTransformPoint(a), board.InverseTransformPoint(b));
                rt.sizeDelta = new Vector2(len, wallThickness);

                // 旋转到边方向
                Vector2 dir = (board.InverseTransformPoint(b) - board.InverseTransformPoint(a)).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                rt.localRotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // 有序索引表
        orderedIndex.Clear();
        for (int i = 0; i < level.orderedNodes.Count; i++)
            orderedIndex[level.orderedNodes[i]] = i;

        // 生成节点视图
        views = new NodeView[level.rows, level.cols];
        for (int r = 0; r < level.rows; r++)
            for (int c = 0; c < level.cols; c++)
            {
                var go = Instantiate(nodeViewPrefab, nodesParent);
                var v = go.GetComponent<NodeView>();
                bool isOrdered = orderedIndex.TryGetValue(new NodeCoord(r, c), out int idx);
                v.Setup(isOrdered, isOrdered ? idx : -1, isOrdered ? orderedColor : nodeColor);

                // 定位
                var rt = (RectTransform)go.transform;
                rt.anchoredPosition = GridToLocal(new NodeCoord(r, c));

                views[r, c] = v;
            }

        // 新建线
        line = Instantiate(linePrefab, linesParent);
        line.positionCount = 0;
        line.sortingLayerName = "UI";
        line.sortingOrder = 10;
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(lineColor, 0f), new GradientColorKey(lineColor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        line.colorGradient = g;

        // 状态清零
        visited.Clear();
        path.Clear();
        nextOrdered = 0;
        dragging = false;
        bonusVisited = false;
    }

    // 将网格坐标 → board 本地坐标（UI）
    Vector2 GridToLocal(NodeCoord n)
    {
        var rect = board.rect;
        float x0 = rect.xMin + margin;
        float y0 = rect.yMin + margin;
        float x1 = rect.xMax - margin;
        float y1 = rect.yMax - margin;

        // 等距分布：n.c ∈ [0..cols-1]，n.r ∈ [0..rows-1]
        float x = Mathf.Lerp(x0, x1, level.cols == 1 ? 0.5f : n.c / (float)(level.cols - 1));
        float y = Mathf.Lerp(y0, y1, level.rows == 1 ? 0.5f : n.r / (float)(level.rows - 1));
        return new Vector2(x, y);
    }

    // 把节点本地坐标转世界坐标（给 LineRenderer）
    Vector3 NodeToWorld(NodeCoord n)
    {
        Vector3 local = GridToLocal(n);
        return board.TransformPoint(local);
    }

    // 鼠标附近最近节点（像素吸附）
    bool TryGetNearestNode(out NodeCoord nearest)
    {
        nearest = default;
        float best = float.MaxValue;

        // 1) 把鼠标屏幕像素 -> Board 本地坐标
        Camera cam = null;
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                board, Input.mousePosition, cam, out var mouseLocal))
            return false;

        // Canvas Scaler 下：本地单位 ≈ 像素 / scaleFactor
        float snapLocalRadius = snapPixelRadius / (canvas ? canvas.scaleFactor : 1f);

        // 2) 遍历所有节点，比较“本地坐标”距离
        for (int r = 0; r < level.rows; r++)
            for (int c = 0; c < level.cols; c++)
            {
                var n = new NodeCoord(r, c);
                Vector2 nodeLocal = GridToLocal(n);           // 你已有：节点 -> Board本地
                float d = Vector2.Distance(mouseLocal, nodeLocal);
                if (d < best && d <= snapLocalRadius)
                {
                    best = d;
                    nearest = n;
                }
            }
        return best < float.MaxValue;
    }



    // 是否相邻（4 向）
    static bool Adjacent(NodeCoord a, NodeCoord b)
        => (a.r == b.r && Mathf.Abs(a.c - b.c) == 1) || (a.c == b.c && Mathf.Abs(a.r - b.r) == 1);

    // 这条边是否被墙阻断
    bool IsBlocked(NodeCoord a, NodeCoord b) => wallSet.Contains(new EdgeCoord(a, b));


    void StartPath(NodeCoord start)
    {
        if (pathCommitted) return;

        line.useWorldSpace = true; // 关键：世界坐标
        line.material = new Material(Shader.Find("Sprites/Default")); // 保险：有材质

        // 场景里只保留一条线
        if (line != null) Destroy(line.gameObject);
        foreach (Transform t in linesParent) Destroy(t.gameObject);

        // 新线
        line = Instantiate(linePrefab, linesParent);
        line.sortingLayerName = "UI";
        line.sortingOrder = 10;

        // 颜色梯度
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(lineColor, 0f), new GradientColorKey(lineColor, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) }
        );
        line.colorGradient = g;

        line.sortingLayerName = "UI";
        line.sortingOrder = 10;

        path.Clear();
        visited.Clear();
        nextOrdered = 0;

        PushNode(start);

        // if (level.orderedNodes.Count > 0 && orderedIndex.TryGetValue(start, out int sIdx) && nsIdx != 0)
        // {
        //     dragging = false;   // 直接取消，本次不允许开始
        //     return;
        // }
        // 如果起点本身就是有序点，消耗掉它
        if (orderedIndex.TryGetValue(start, out int sIdx))
            nextOrdered = sIdx + 1;
    }


    // —— 输入循环 —— //
    void Update()
    {
        if (!_built) return;
        // 开始
        if (Input.GetMouseButtonDown(0))
        {
            if (TryGetNearestNode(out var start))
            {
                dragging = true;
                StartPath(start);
            }
        }
        if (!dragging) return;

        // 重置：R 键
        if (Input.GetKeyDown(KeyCode.R))
            ResetLevel();

        // 开始拖拽
        if (Input.GetMouseButtonDown(0) && !pathCommitted)
        {
            if (TryGetNearestNode(out var start))
            {
                dragging = true;
                StartPath(start);
            }
        }

        if (!dragging) return;


        // 预览：把末端跟随吸附点
        if (path.Count > 0)
        {
            if (TryGetNearestNode(out var hover))
            {
                var p = NodeToWorld(hover);
                line.positionCount = Mathf.Max(line.positionCount, path.Count + 1);
                line.SetPosition(path.Count, p); // 预览点
            }
            else
            {
                line.positionCount = path.Count;
            }
        }

        // 推进一步 / 回退一步
        if (TryGetNearestNode(out var next))
        {
            var cur = path[path.Count - 1];

            if (next != cur && Adjacent(cur, next) && !IsBlocked(cur, next))
            {
                // 回退一步：点到上一个点
                if (allowBacktrack && path.Count >= 2 && next.Equals(path[path.Count - 2]))
                {
                    PopNode();
                }
                else
                {
                    // 已访问的不允许（除了上面的回退）
                    if (visited.Contains(next)) goto AfterAdvance;

                    // 有序校验
                    if (orderedIndex.TryGetValue(next, out int must) && must != nextOrdered)
                        goto AfterAdvance;

                    PushNode(next);
                    if (orderedIndex.ContainsKey(next)) nextOrdered++;

                    // 胜利判定：全覆盖 + 有序完成 + 终点就是最后一个有序点
                    bool allVisited = visited.Count == level.rows * level.cols;
                    bool allOrdered = nextOrdered == level.orderedNodes.Count;
                    bool endOnLast = level.orderedNodes.Count == 0
                                    || (orderedIndex.TryGetValue(next, out int idx) && idx == level.orderedNodes.Count - 1);

                    if (allVisited && allOrdered && endOnLast)
                    {
                        bonusVisited = true;
                        OnFinished?.Invoke(bonusVisited);
                        Debug.Log("ALL Clear!");
                        pathCommitted = true;            // 松手即提交（如果想“非胜利不提交”，把这一行移到 Win 判定里）
                        CommitPath(); // 提交并锁定
                    }
                }
            }
        }

    AfterAdvance:
        if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
            line.positionCount = path.Count; // 去掉预览点
        }

        if (Input.GetMouseButtonDown(1) && path.Count > 1) PopNode(); // 可选撤销
    }


    void PushNode(NodeCoord n)
    {
        path.Add(n);
        visited.Add(n);
        views[n.r, n.c].SetVisited(true);

        line.positionCount = path.Count;
        line.SetPosition(path.Count - 1, NodeToWorld(n));  // 线用“世界坐标”
    }

    void PopNode()
    {
        if (path.Count <= 1) return; // 不弹起点
        var last = path[path.Count - 1];

        views[last.r, last.c].SetVisited(false);
        visited.Remove(last);
        path.RemoveAt(path.Count - 1);

        line.positionCount = path.Count;
        // 末端位置已在 Push 时设置，这里不必重复
    }

    void CommitPath()
    {
        dragging = false;
        line.positionCount = path.Count;
        pathCommitted = true; // 锁定，禁止再开新线
    }
    // 新增：退场时调用（在 DialogueManager 的 panel 滑出协程结束后调）
    public void HideAndClear()
    {
        // 停止交互循环
        _active = false;
        dragging = false;
        pathCommitted = false;

        // 清掉路径与访问标记
        path.Clear();
        visited.Clear();
        bonusVisited = false;

        // 安全销毁线段
        if (line != null)
        {
            Destroy(line.gameObject);
            line = null;
        }
        foreach (Transform t in linesParent) Destroy(t.gameObject);
    }

    public void ResetLevel()
    {
        dragging = false;
        pathCommitted = false;
        path.Clear();
        visited.Clear();

        // 还原所有节点外观
        for (int r = 0; r < level.rows; r++)
            for (int c = 0; c < level.cols; c++)
                views[r, c].SetVisited(false);

        // 清线
        if (line) Destroy(line.gameObject);
        foreach (Transform t in linesParent) Destroy(t.gameObject);
    }
}

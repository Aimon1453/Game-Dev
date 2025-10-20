using System.Collections.Generic;
using UnityEngine;

public enum NodeType { Start, End, Normal, BonusA, BonusB, BonusC }

// 无向边（相邻两个格子之间的一段线）
public struct UEdge
{
    public NodeCoord a, b;
    public UEdge(NodeCoord x, NodeCoord y)
    {
        // 归一化：小的放前面，确保 (a,b)==(b,a)
        if (x.r < y.r || (x.r == y.r && x.c <= y.c)) { a = x; b = y; }
        else { a = y; b = x; }
    }
    public override int GetHashCode() => a.GetHashCode() * 486187739 ^ b.GetHashCode();
    public override bool Equals(object obj) => obj is UEdge e && e.a.Equals(a) && e.b.Equals(b);
}

public class MinigameManager : Singleton<MinigameManager>
{
    [Header("Level & UI")]
    public LevelSO level;
    public RectTransform board;      // 右侧游戏区域（RectTransform）
    public Transform nodesParent;    // 放 NodeView
    public GameObject nodeViewPrefab;
    public Canvas canvas;

    [Header("Line")]
    // public LineRenderer linePrefab;
    // public Transform   linesParent;
    public float snapPixelRadius = 40f;
    public float margin          = 80f;
    public Color lineColor = Color.white;
    
    // New Line
    [Header("UI Line (instead of LineRenderer)")]
    public Sprite lineSprite;        // 拖入线段美术
    public float  lineThickness = 8; // UI像素厚度
    public Transform linesParent;    // 已有：Board 下的 Lines

    // 运行时(New Line)
    readonly List<RectTransform> _segments = new();  // 当前已画的线段
    readonly Stack<RectTransform> _pool = new();     // 复用池
    RectTransform _preview;                           // 预览线段（鼠标/手指悬停时）    

    // 运行时
    NodeView[,] views;
    List<NodeCoord> path      = new();
    HashSet<NodeCoord> visited = new();
    HashSet<UEdge> usedEdges   = new();    // 用过的无向边 => “不可交叉、不可重复走同一段”
    LineRenderer line;

    bool _built = false;

    // 统计增益
    int countA, countB, countC;

    // 结果回调：把 [A,B,C] 发给外部（对话/剧情）
    public System.Action<int,int,int> OnFinished;

    void Start()
    {
        if (!level || !board || !nodeViewPrefab || !linesParent || !nodesParent)
        {
            Debug.LogError("[Minigame] Inspector 引用未绑定完整。");
            enabled = false; return;
        }
        // 分屏后再 InitIfNeeded；这里不自动生成
        foreach (Transform t in nodesParent)  Destroy(t.gameObject);
        foreach (Transform t in linesParent)  Destroy(t.gameObject);
    }

    // 对外：分屏后调用一次
    public void InitIfNeeded()
    {
        if (_built) return;
        StretchFull(board);
        BuildLevel();
        _built = true;
    }

    void StretchFull(RectTransform rt)
    {
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    void BuildLevel()
    {
        // 清空旧物
        foreach (Transform t in nodesParent) Destroy(t.gameObject);
        foreach (Transform t in linesParent) Destroy(t.gameObject);
        _segments.Clear(); _pool.Clear();

        views = new NodeView[level.rows, level.cols];
        path.Clear(); visited.Clear(); usedEdges.Clear();
        countA = countB = countC = 0;

        // 生成节点
        for (int r = 0; r < level.rows; r++)
        for (int c = 0; c < level.cols; c++)
        {
            var go = Instantiate(nodeViewPrefab, nodesParent);
            var v  = go.GetComponent<NodeView>();
            var n  = new NodeCoord(r, c);

            v.Setup(level.GetNodeType(n));
            ((RectTransform)go.transform).anchoredPosition = GridToLocal(n);
            views[r, c] = v;
        }

        Debug.Log("[Minigame] Build ok (UI lines).");
    }

    // —— 输入 —— //
    void Update()
    {
        if (!_built) return;

        // R 键全重置
        if (Input.GetKeyDown(KeyCode.R)) { ResetLevel(); return; }

        // 鼠标/触控一次“步进”逻辑（支持点击或按住拖过邻格）
        if (Input.GetMouseButton(0) || Input.GetMouseButtonDown(0))
        {
            if (!TryGetNearestNode(out var hit)) return;

            if (path.Count == 0)
            {
                // 只允许从 Start 起步
                if (GetTypeOf(hit) == NodeType.Start) Push(hit);
                return;
            }

            var cur = path[path.Count - 1];

            // 规则：必须相邻；不可回溯；不可访问已访问节点；不可复用边
            if (!IsOrthAdjacent(cur, hit)) return;

            // 不允许回溯：命中上一个节点直接忽略（不 Pop）
            if (path.Count >= 2 && hit.Equals(path[path.Count - 2])) return;

            // 不允许访问已在路径中的节点（避免“回环/交叉于节点”）
            if (visited.Contains(hit)) return;

            // 不允许使用已用过的边（避免“交叉于边” & 重复走同一段）
            var tryEdge = new UEdge(cur, hit);
            if (usedEdges.Contains(tryEdge)) return;

            // 通过：推进一步
            Push(hit);

            // 终点判定：最后一个 == End
            if (GetTypeOf(hit) == NodeType.End)
            {
                FinishAndReport();
            }
        }
    }

    // —— 基础功能 —— //
    Vector2 GridToLocal(NodeCoord n)
    {
        var rect = board.rect;
        float x0 = rect.xMin + margin, y0 = rect.yMin + margin;
        float x1 = rect.xMax - margin, y1 = rect.yMax - margin;

        float x = Mathf.Lerp(x0, x1, level.cols == 1 ? 0.5f : n.c / (float)(level.cols - 1));
        float y = Mathf.Lerp(y0, y1, level.rows == 1 ? 0.5f : n.r / (float)(level.rows - 1));
        return new Vector2(x, y);
    }

    Vector3 NodeToWorld(NodeCoord n) => board.TransformPoint(GridToLocal(n));

    bool TryGetNearestNode(out NodeCoord nearest)
    {
        nearest = default;
        float best = float.MaxValue;

        Camera cam = null;
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            cam = canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(board, Input.mousePosition, cam, out var mouseLocal))
            return false;

        float snapLocalRadius = snapPixelRadius / (canvas ? canvas.scaleFactor : 1f);

        for (int r = 0; r < level.rows; r++)
        for (int c = 0; c < level.cols; c++)
        {
            var n = new NodeCoord(r, c);
            float d = Vector2.Distance(mouseLocal, GridToLocal(n));
            if (d < best && d <= snapLocalRadius) { best = d; nearest = n; }
        }
        return best < float.MaxValue;
    }

    static bool IsOrthAdjacent(NodeCoord a, NodeCoord b)
        => (a.r == b.r && Mathf.Abs(a.c - b.c) == 1) || (a.c == b.c && Mathf.Abs(a.r - b.r) == 1);

    NodeType GetTypeOf(NodeCoord n) => level.GetNodeType(n);

    void Push(NodeCoord n)
    {
        // 记录边（从第二个点开始）
        // if (path.Count > 0)
        // {
        //     var e = new UEdge(path[path.Count - 1], n);
        //     usedEdges.Add(e);
        // }

        // path.Add(n);
        // visited.Add(n);
        // views[n.r, n.c].SetVisited(true);
        // 画一段 from prev -> n
        if (path.Count > 0)
        {
            var e = new UEdge(path[path.Count - 1], n);
            var prev = path[path.Count - 1];
            var seg = GetSeg();
            var a = GridToLocal(prev);
            var b = GridToLocal(n);
            PlaceSegment(seg, a, b);
            _segments.Add(seg);
        }

        path.Add(n);
        visited.Add(n);
        views[n.r, n.c].SetVisited(true);

        // 统计增益（首次进入）
        switch (GetTypeOf(n))
        {
            case NodeType.BonusA: countA++; break;
            case NodeType.BonusB: countB++; break;
            case NodeType.BonusC: countC++; break;
        }
    }

    // 新的线段
    RectTransform GetSeg()
    {
        if (_pool.Count > 0)
        {
            var reused = _pool.Pop();
            reused.gameObject.SetActive(true);
            return reused;
        }

        var go = new GameObject("LineSeg", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(linesParent, false);
        var img = go.GetComponent<UnityEngine.UI.Image>();
        img.sprite = lineSprite;
        img.type   = UnityEngine.UI.Image.Type.Sliced; // 9-slice

        var segRT = go.GetComponent<RectTransform>();
        segRT.pivot = new Vector2(0.5f, 0.5f);
        return segRT;
    }

    void ReleaseSeg(RectTransform rt)
    {
        if (!rt) return;
        rt.gameObject.SetActive(false);
        rt.SetParent(linesParent, false);
        _pool.Push(rt);
    }

    void FinishAndReport()
    {
        Debug.Log($"[Minigame] Finish! Bonus = [{countA},{countB},{countC}]");
        OnFinished?.Invoke(countA, countB, countC);
        // 锁定（可选）：不再响应输入（若需要）
        _built = false;
    }

    void PlaceSegment(RectTransform seg, Vector2 aLocal, Vector2 bLocal)
    {
        // aLocal / bLocal：Board 的本地坐标
        Vector2 mid = (aLocal + bLocal) * 0.5f;
        Vector2 dir = (bLocal - aLocal);
        float len = dir.magnitude;
        float ang = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        seg.SetParent(linesParent, false);
        seg.anchoredPosition = mid;
        seg.sizeDelta = new Vector2(len, lineThickness);
        seg.localRotation = Quaternion.Euler(0,0,ang);
    }

    public void ResetLevel()
    {
        path.Clear(); visited.Clear(); usedEdges.Clear();
        countA = countB = countC = 0;

        // 复位外观
        for (int r = 0; r < level.rows; r++)
            for (int c = 0; c < level.cols; c++)
                if (views[r, c]) views[r, c].SetVisited(false);

        // 清线
        // if (line) Destroy(line.gameObject);
        // foreach (Transform t in linesParent) Destroy(t.gameObject);
        foreach (var s in _segments) ReleaseSeg(s);
        _segments.Clear();

        // 预览线段也隐藏
        if (_preview) ReleaseSeg(_preview);
        _preview = null;

        // line = Instantiate(linePrefab, linesParent);
        // line.positionCount = 0;
        // line.useWorldSpace = true;
        // line.sortingLayerName = "UI";
        // line.sortingOrder     = 10;

        _built = true; // 继续可玩
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Venly;
using Venly.Core;
using Venly.Models.Shared;
using System.Threading;

#region UnityMainThread Helper
public static class UnityMainThread
{
    static SynchronizationContext _ctx;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Capture()
    {
        _ctx = SynchronizationContext.Current; // Unity main thread
    }

    public static void Run(Action action)
    {
        if (action == null) return;

        // If we're already on main thread, run inline
        if (SynchronizationContext.Current == _ctx)
        {
            action();
            return;
        }

        _ctx?.Post(_ => action(), null);
    }
}
#endregion

public class BackTargetArgs<T> where T : Enum
{
    //public BackTargetArgs(){}
    public BackTargetArgs(SampleViewBase<T> target)
    {
        Target = target;
    }

    public SampleViewBase<T> Target { get; set; }
    public Dictionary<string, object> Blackboard { get; private set; }
    public bool RequiresRefresh { get; set; } = true;
    public bool HasBlackboard => Blackboard != null;

    public void SetValue(string key, object value)
    {
        Blackboard ??= new Dictionary<string, object>();
        Blackboard[key] = value;
    }

    public TVal GetValue<TVal>(string key) where TVal : class
    {
        if (HasValue(key))
        {
            return (TVal) Blackboard[key];
        }

        return null;
    }

    public bool HasValue(string key)
    {
        return Blackboard?.ContainsKey(key) ?? false;
    }
}

public class SwitchArgs<T> where T : Enum
{
    public static SwitchArgs<T> Default = new ();

    public SwitchArgs(){}

    public SwitchArgs(SampleViewBase<T> backTarget, bool requiresRefresh = true)
    {
        BackTargetArgs = new BackTargetArgs<T>(backTarget) {RequiresRefresh = requiresRefresh};
    }

    public bool SetBackTarget { get; set; } = true;
    public BackTargetArgs<T> BackTargetArgs { get; set; }
    public bool SkipRefreshData { get; set; } = false;
}

public abstract class SampleViewManager<T> : MonoBehaviour where T : Enum
{
    public List<SampleViewBase<T>> Views = new ();

    public T LandingDevMode = default;
    public T LandingAuth = default;

    public ApiExplorer_LoaderVC Loader;
    public ApiExplorer_ExceptionVC Exception;
    public ApiExplorer_InfoVC Info;

    private bool _firstFrame = true;

    private SampleViewBase<T> _currView = null;
    private T _homeViewId;
    private Dictionary<string, object> _globalBlackboard = new();
    private Dictionary<T, string> _attributeTitles = new ();

    public static SampleViewManager<T> Instance { get; private set; }

    public sealed class LoaderScope : System.IDisposable
    {
        private readonly ApiExplorer_LoaderVC _loader;
        private bool _disposed;
        public LoaderScope(ApiExplorer_LoaderVC loader, string message)
        {
            _loader = loader;
            _loader?.Show(message);
        }
        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _loader?.Hide();
        }
    }

    public LoaderScope BeginLoad(string message)
    {
        return new LoaderScope(Loader, message);
    }

    public void RunOnMainThread(Action a) => UnityMainThread.Run(a);

    // Start is called before the first frame update
    void Start()
    {
        Instance = this;

        if(!VenlyAPI.IsInitialized) 
            VenlyUnity.Initialize();

        InitializeViews();
        Loader.Hide();
        Exception.Hide();
    }

    void Update()
    {
        if (_firstFrame)
        {
            _firstFrame = false;
            OnFirstFrame();
        }
    }

    public abstract string GetTitle(T viewId);

    public VyTask<object> SelectionMode(SampleViewBase<T> targetView, string viewTitle = null)
    {
        return targetView.SelectionMode(viewTitle);
    }

    public VyTask<object> SelectionMode(T targetViewId, string viewTitle = null)
    {
        var targetView = GetView(targetViewId);
        return SelectionMode(targetView, viewTitle);
    }

    public void SwitchView(SampleViewBase<T> targetView, bool setBackTarget = true, bool forceRefresh = false)
    {
        SwitchView(targetView, new SwitchArgs<T>
        {
            SetBackTarget = setBackTarget,
            //SkipRefreshData = !forceRefresh
        });
    }

    public void SwitchView(SampleViewBase<T> targetView, SwitchArgs<T> args)
    {
        if (targetView == null)
        {
            throw new Exception("SwitchView Failed, TargetView is null.");
        }

        if (args.SetBackTarget)
        {
            targetView.BackTargetArgs = args.BackTargetArgs ?? new BackTargetArgs<T>(_currView);
        }

        _currView?.Deactivate();
        _currView = targetView;
        _currView?.Activate(args);
    }

    public void SwitchView(T targetViewId, SwitchArgs<T> args)
    {
        SwitchView(GetView(targetViewId), args);
    }

    public void SwitchView(T targetViewId, T backTargetId, bool backTargetRefresh = true)
    {
        SwitchView(GetView(targetViewId), new SwitchArgs<T>(GetView(backTargetId), backTargetRefresh));
    }

    public void SwitchView(T targetViewId, SampleViewBase<T> backTarget, bool backTargetRefresh = true)
    {
        SwitchView(GetView(targetViewId), new SwitchArgs<T>(backTarget, backTargetRefresh));
    }

    public void SwitchView(T targetViewId, bool setBackTarget = true, bool forceRefresh = false)
    {
        SwitchView(GetView(targetViewId), setBackTarget, forceRefresh);
    }

    public void NavigateHome()
    {
        SwitchView(_homeViewId, false);
    }

    public void NavigateBack()
    {
        if (_currView?.BackTargetArgs != null)
        {
            //Override Blackboard if required
            if (_currView.BackTargetArgs.HasBlackboard)
            {
                _currView.BackTargetArgs.Target.OverrideBlackboard(_currView.BackTargetArgs.Blackboard);
            }

            //Switch to Back Target
            SwitchView(_currView.BackTargetArgs.Target, new SwitchArgs<T>
            {
                SetBackTarget = false,
                SkipRefreshData = !_currView.BackTargetArgs.RequiresRefresh
            });
        }
    }

    [Obsolete("Use typed BlackboardKey APIs (SetGlobalBlackboardData<T>), not string keys", true)]
    public void SetGlobalBlackboardData(string key, object data)
    {
        _globalBlackboard[key] = data;
    }

    [Obsolete("Use typed BlackboardKey APIs (TryGetGlobalBlackboardData<T>), not string keys", true)]
    public object GetGlobalBlackboardDataRaw(string key)
    {
        if (_globalBlackboard.TryGetValue(key, out var obj)) return obj;
        return null;
    }

    [Obsolete("Use typed BlackboardKey APIs (TryGetGlobalBlackboardData<T>), not string keys", true)]
    public T0 GetGlobalBlackBoardData<T0>(string key) where T0 : class
    {
        return GetGlobalBlackboardDataRaw(key) as T0;
    }

    public void ClearGlobalBlackboardData(string key)
    {
        if (HasGlobalBlackboardData(key))
            _globalBlackboard.Remove(key);
    }

    public bool HasGlobalBlackboardData(string key)
    {
        return _globalBlackboard.ContainsKey(key);
    }

    public void ClearViewBlackboardData(T viewId)
    {
        var view = GetView(viewId);
        view.ClearBlackboardData();
    }

    // Typed Blackboard helpers
    public void SetViewBlackboardData<TVal>(T viewId, BlackboardKey<TVal> key, TVal data)
    {
        var view = GetView(viewId);
        view.Set(key, data);
    }

    public void SetGlobalBlackboardData<TVal>(BlackboardKey<TVal> key, TVal data)
    {
        _globalBlackboard[key.Name] = data;
    }

    public bool TryGetGlobalBlackboardData<TVal>(BlackboardKey<TVal> key, out TVal value)
    {
        if (_globalBlackboard.TryGetValue(key.Name, out var obj) && obj is TVal cast)
        {
            value = cast;
            return true;
        }
        value = default;
        return false;
    }

    public void ClearGlobalBlackboardData<TVal>(BlackboardKey<TVal> key)
    {
        if (_globalBlackboard.ContainsKey(key.Name))
            _globalBlackboard.Remove(key.Name);
    }

    public SampleViewBase<T> GetView(T viewId)
    {
        return Views.FirstOrDefault(v => v.ViewId.Equals(viewId));
    }

    public void HandleException(Exception ex)
    {
        Exception.Show(ex);
    }

    void InitializeViews()
    {
        Views.Clear();

        var sampleViewBases = gameObject.transform.GetComponentsInChildren<SampleViewBase<T>>(includeInactive: true);
        Views.AddRange(sampleViewBases);

        Views.ForEach(v =>
        {
            v.gameObject.SetActive(true);
            v.ViewManager = this;
            v.gameObject.SetActive(false);
        });

        // Build attribute-driven titles
        _attributeTitles.Clear();
        foreach (var view in Views)
        {
            var meta = view.GetType().GetCustomAttributes(typeof(SampleViewMetaAttribute), false);
            if (meta != null && meta.Length > 0)
            {
                var m = meta[0] as SampleViewMetaAttribute;
                if (m != null && m.ViewId.Equals(view.ViewId))
                {
                    _attributeTitles[view.ViewId] = m.Title;
                }
            }
        }
    }

    void OnFirstFrame()
    {
        switch (VenlySettings.BackendProvider)
        {
            case eVyBackendProvider.DevMode:
            case eVyBackendProvider.Custom:
                _homeViewId = LandingDevMode;
                break;
            default:
                _homeViewId = LandingAuth;
                break;
        }

        SwitchView(_homeViewId);
    }

    protected string GetTitleAttributeOrNull(T viewId)
    {
        if (_attributeTitles.TryGetValue(viewId, out var title)) return title;
        return null;
    }
}

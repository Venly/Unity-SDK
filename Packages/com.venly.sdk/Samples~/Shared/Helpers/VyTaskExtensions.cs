using Venly.Core;

public static class VyTaskExtensions
{
    public static VyTask<T> WithLoader<T>(
        this VyTask<T> task,
        SampleViewManager<eApiExplorerViewId> vm,
        string message)
    {
        var scope = vm.BeginLoad(message);
        return task.Finally(() => vm.RunOnMainThread(scope.Dispose));
    }
}

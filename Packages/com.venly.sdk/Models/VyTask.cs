using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Proto.Promises;
using Proto.Promises.Async.CompilerServices;

namespace Venly.Models
{
    public class VyTaskResult<T>
    {
        [JsonProperty("success")] public bool Success;
        [JsonProperty("data")] public T Data;
        [JsonProperty("exception")] public VenlyException Exception;

        public VyTaskResult(T data)
        {
            Success = true;
            Data = data;
        }

        public VyTaskResult(VenlyException ex = null)
        {
            Success = false;
            Exception = ex;
        }
    }

    #region Awaiters
    public class VyTaskAwaiter<T> : INotifyCompletion, ICriticalNotifyCompletion
    {
        public bool IsCompleted => _source.IsCompleted;
        private readonly VyTask<T> _source;
        public PromiseAwaiter<VyTaskResult<T>>? _awaiter;

        public VyTaskAwaiter(VyTask<T> source)
        {
            _source = source;
        }

        public VyTaskResult<T> GetResult()
        {
            _source._AwaiterCalled();

            if (_awaiter.HasValue)
            {
                var result = _awaiter?.GetResult();

                if (result.Success) return result;

                throw result.Exception;
            }

            if (_source.HasResult)
            {
                return _source.Result;
            }

            throw new VenlyException("Async result requested but no result available.");
        }
        public void OnCompleted(Action continuation)
        {
            if (_source.IsPromiseValid)
            {
                _awaiter ??= _source.PresevedPromise?.GetAwaiter();
                _awaiter?.UnsafeOnCompleted(continuation);
            }
            else
            {
                continuation();
            }
        }
        public void UnsafeOnCompleted(Action continuation)
        {
            if (_source.IsPromiseValid)
            {
                _awaiter ??= _source.PresevedPromise?.GetAwaiter();
                _awaiter?.UnsafeOnCompleted(continuation);
            }
            else
            {
                continuation();
            }
        }
    }

    public class VyTaskResultAwaiter<T> : INotifyCompletion, ICriticalNotifyCompletion
    {
        public bool IsCompleted => _source.IsCompleted;
        public PromiseAwaiter<VyTaskResult<T>>? _awaiter;
        private readonly VyTask<T> _source;

        public VyTaskResultAwaiter(VyTask<T> source)
        {
            _source = source;
        }

        public T GetResult()
        {
            _source._AwaiterCalled();

            if (_awaiter.HasValue)
            {
                var result = _awaiter?.GetResult();

                if (result.Success) return result.Data;
                throw result.Exception;
            }

            if (_source.HasResult)
            {
                if (_source.Result.Success) return _source.Result.Data;
                throw _source.Result.Exception;
            }

            throw new VenlyException("Async result requested but no result available.");
        }
        public void OnCompleted(Action continuation)
        {
            if (_source.IsPromiseValid)
            {
                if (!_awaiter.HasValue) _awaiter = _source.PresevedPromise?.GetAwaiter();
                _awaiter?.UnsafeOnCompleted(continuation);
            }
            else
            {
                continuation();
            }
        }
        public void UnsafeOnCompleted(Action continuation)
        {
            if (_source.IsPromiseValid)
            {
                if (!_awaiter.HasValue) _awaiter = _source.PresevedPromise?.GetAwaiter();
                _awaiter?.UnsafeOnCompleted(continuation);
            }
            else
            {
                continuation();
            }
        }
    }
    #endregion

    public class VyAwaitResultTask<T>
    {
        private readonly VyTask<T> _baseTask;

        public VyAwaitResultTask(VyTask<T> baseTask)
        {
            _baseTask = baseTask;
        }

        public VyTaskResultAwaiter<T> GetAwaiter()
        {
            return new VyTaskResultAwaiter<T>(_baseTask);
        }
    }

    public struct VyTaskVoid { }

    public class VyTask : VyTask<VyTaskVoid>
    {
        private VyTask(string identifier = "") : base(identifier){}
        private VyTask(VyTaskVoid v, string identifier = "") : base(v, identifier){}
        private VyTask(VenlyException ex, string identifier = "") : base(ex, identifier){}
    }

    public class VyTask<T>
    {
        public enum eVyTaskState
        {
            Pending,
            Success,
            Failed
        }

        public eVyTaskState State => _state;
        private eVyTaskState _state = eVyTaskState.Pending;

        internal bool IsPromiseValid => PromiseCreated && _preservedPromise.Value.IsValid;
        internal bool IsCompleted => _state != eVyTaskState.Pending;
        internal bool HasResult => _result != null;

        internal VyTaskResult<T> Result => _result;
        private VyTaskResult<T> _result = null;

        private bool _resultConsumed = false;
        private bool _exceptionConsumed = false;

        private Action<VyTaskResult<T>> _onCompleteCallback;
        private Action<T> _onSuccesCallback;
        private Action<VenlyException> _onFailCallback;
        private Action _onFinallyCallback;

        private Promise<VyTaskResult<T>>.Deferred? _deferredPromise;
        private Promise<VyTaskResult<T>>? _preservedPromise;
        internal Promise<VyTaskResult<T>>? PresevedPromise => _preservedPromise;

        internal bool PromiseCreated => _deferredPromise.HasValue;
        internal string Identifier { get; private set; }

        internal VyTaskNotifier<T> _notifier;

        ~VyTask()
        {
            if (PromiseCreated)
                _preservedPromise?.Forget();

            if (_state == eVyTaskState.Failed && !_exceptionConsumed)
            {
                //Raise as UnhandledException
                VenlyException.RaiseUnhandled(_result.Exception);
            }
        }

        protected VyTask(string identifier = "")
        {
            Identifier = identifier;

            _deferredPromise = Promise<VyTaskResult<T>>.Deferred.New();
            _preservedPromise = _deferredPromise.Value.Promise.Preserve();

            _preservedPromise.Value
                .WaitAsync(SynchronizationOption.Foreground)
                .Then(HandleCompletion)
                .Catch((VenlyException ex) =>
                {
                    HandleCompletion(new VyTaskResult<T>(ex));
                })
                .Catch((Exception ex) =>
                {
                    var venlyEx = new VenlyException(ex);
                    HandleCompletion(new VyTaskResult<T>(venlyEx));
                })
                .Finally(() =>
                {
                    _onFinallyCallback?.Invoke();
                    _preservedPromise?.Forget();
                }).Forget();

            //Create Notifier
            _notifier = new VyTaskNotifier<T>(this);
        }

        protected VyTask(T data, string identifier = "")
        {
            Identifier = identifier;
            HandleCompletion(new VyTaskResult<T>(data));
        }

        protected VyTask(VenlyException ex, string identifier = "")
        {
            Identifier = identifier;
            HandleCompletion(new VyTaskResult<T>(ex));
        }

        public static VyTaskNotifier<T> Create(string identifier = "")
        {
            var task = new VyTask<T>($"[Promise] {identifier}");
            return task._notifier;
        }

        public VyTaskAwaiter<T> GetAwaiter()
        {
            return new VyTaskAwaiter<T>(this);
        }

        public VyAwaitResultTask<T> AwaitResult()
        {
            return new VyAwaitResultTask<T>(this);
        }

        #region Direct Notifiers
        public static VyTask<T> Succeeded(T data, string identifier = "")
        {
            return new VyTask<T>(data, $"[Direct Success] {identifier}");
        }

        public static VyTask<T> Failed(VenlyException ex, string identifier = "")
        {
            return new VyTask<T>(ex, $"[Direct Fail] {identifier}");
        }

        public static VyTask<T> Failed(Exception ex, string identifier = "")
        {
            return new VyTask<T>(new VenlyException(ex), $"[Direct Fail] {identifier}");
        }
        #endregion

        #region Internal Notifiers
        internal void _NotifySuccess(T data)
        {
            _deferredPromise?.Resolve(new VyTaskResult<T>(data));
        }

        internal void _Notify(VyTaskResult<T> result)
        {
            _deferredPromise?.Resolve(result);
        }

        internal void _NotifyFail(VenlyException ex)
        {
            _deferredPromise?.Resolve(new VyTaskResult<T>(ex));
        }

        internal void _NotifyFail(Exception ex)
        {
            _deferredPromise?.Resolve(new VyTaskResult<T>(new VenlyException(ex)));
        }
        #endregion

        #region State Handlers
        internal void _AwaiterCalled()
        {
            //_resultConsumed = true; //Blocks Invoke Callbacks if await keyword is also used
            _exceptionConsumed = true;
        }

        private void HandleCompletion(VyTaskResult<T> result)
        {
            _result = result;
            _state = result.Success ? eVyTaskState.Success : eVyTaskState.Failed;

            InvokeCallbacks();
        }

        private bool InvokeCallbacks()
        {
            if (_resultConsumed)
                return true;

            if (_state == eVyTaskState.Pending) 
                return false;

            //Call OnSuccess/OnFail
            if (_state == eVyTaskState.Success)
            {
                _onSuccesCallback?.Invoke(_result.Data);
                if (!_resultConsumed) _resultConsumed = _onSuccesCallback != null;
            }
            else
            {
                _onFailCallback?.Invoke(_result.Exception);
                if (!_resultConsumed) _resultConsumed = _onFailCallback != null;
                if (!_exceptionConsumed) _exceptionConsumed = _onFailCallback != null;
            }

            //Call OnComplete
            _onCompleteCallback?.Invoke(_result);
            if (!_resultConsumed) _resultConsumed = _onCompleteCallback != null;
            if (_state == eVyTaskState.Failed && !_exceptionConsumed) _exceptionConsumed = _onCompleteCallback != null;

            //Fail-Safe, todo: check timing...
            if (_state == eVyTaskState.Failed && !_exceptionConsumed)
            {
                _exceptionConsumed = true;
                VenlyException.RaiseUnhandled(_result.Exception);
            }

            return _resultConsumed;
        }
        #endregion

        #region Event Subscribers
        public VyTask<T> OnSucces(Action<T> callback)
        {
            if (_state == eVyTaskState.Success)
            {
                callback?.Invoke(_result.Data);
            }
            else
            {
                _onSuccesCallback = callback;
            }

            return this;
        }

        public VyTask<T> OnFail(Action<VenlyException> callback)
        {
            if (_state == eVyTaskState.Failed)
            {
                callback?.Invoke(_result.Exception);
                if (!_exceptionConsumed) _exceptionConsumed = callback != null;
            }
            else
            {
                _onFailCallback = callback;
            }

            return this;
        }

        public VyTask<T> OnComplete(Action<VyTaskResult<T>> callback)
        {
            if (IsCompleted)
            {
                callback?.Invoke(_result);
                if(!_exceptionConsumed) _exceptionConsumed = callback != null;
            }
            else
            {
                _onCompleteCallback = callback;
            }

            return this;
        }

        public VyTask<T> Finally(Action callback)
        {
            if (IsCompleted)
            {
                callback?.Invoke();
            }
            else
            {
                _onFinallyCallback = callback;
            }

            return this;
        }
        #endregion
    }

    public class VyTaskNotifier<T>
    {
        public bool IsCompleted => _task?.IsCompleted??false;
        public VyTask<T> Task => _task;

        internal VyTask<T> _task;

        internal VyTaskNotifier(VyTask<T> task)
        {
            _task = task;
        }

        public void NotifySuccess(T data)
        {
            _task._NotifySuccess(data);
        }

        public void NotifyFail(VenlyException ex)
        {
            _task._NotifyFail(ex);
        }

        public void NotifyFail(Exception ex)
        {
            _task._NotifyFail(ex);
        }

        public void Notify(VyTaskResult<T> result)
        {
            _task._Notify(result);
        }
    }
}
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Kodama.Core
{
    public class MonoBehaviourUtil : MonoBehaviour
    {
        public static MonoBehaviourUtil Instance { get; private set; }
        
        public event Action OnUpdate;

        private readonly object _mainThreadActionLock = new();

        private void Awake()
        {
            Debug.Log($"[MonoBehaviourUtil] Awake");
            Instance = this;
        }
        
        private void Update()
        {
            OnUpdate?.Invoke();

            lock (_mainThreadActionLock)
            {
                if (_pendingActions.Count > 0)
                {
                    // swap & execute
                    (_pendingActions, _runningActions) = (_runningActions, _pendingActions);
                    _pendingActions.Clear();
                }
            }

            while (_runningActions.TryDequeue(out var contextAction))
            {
                try
                {
                    contextAction.Execute();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    contextAction.ReturnToPool();
                }
            }
        }

        public void RunOnMainThread(Action action)
        {
            var contextAction = ContextAction.Get(action);
            _pendingActions.Enqueue(contextAction);
        }

        public void RunOnMainThread<T>(Action<T> action, T state)
        {
            var contextAction = ContextAction<T>.Get(action, state);
            _pendingActions.Enqueue(contextAction);
        }
        
        #region Context Action
        private ConcurrentQueue<IMainThreadAction> _pendingActions = new();
        private ConcurrentQueue<IMainThreadAction> _runningActions = new();

        private interface IMainThreadAction
        {
            void Execute();
            void ReturnToPool();
        }

        private class ContextAction : IMainThreadAction
        {
            public Action Action;
            
            private static readonly Stack<ContextAction> _pool = new();

            public static ContextAction Get(Action action)
            {
                ContextAction item = null;
                lock (_pool)
                {
                    item = _pool.Count > 0 ? _pool.Pop() : new ContextAction();
                }

                item.Action = action;
                return item;
            }
            
            public void Execute()
            {
                Action?.Invoke();
            }

            public void ReturnToPool()
            {
                Action = null;
                lock (_pool)
                {
                    _pool.Push(this);
                }
            }
        }

        private class ContextAction<T> : IMainThreadAction
        {
            public Action<T> Action;
            public T State;
            
            private static readonly Stack<ContextAction<T>> _pool = new();

            public static ContextAction<T> Get(Action<T> action, T state)
            {
                ContextAction<T> item = null;
                lock (_pool)
                {
                    item = _pool.Count > 0 ? _pool.Pop() : new ContextAction<T>();
                }
                item.Action = action;
                item.State = state;
                return item;
            }
            
            public void Execute()
            {
                Action?.Invoke(State);
            }

            public void ReturnToPool()
            {
                Action = null;
                State = default;
                lock (_pool)
                {
                    _pool.Push(this);
                }
            }
        }
        #endregion
    }
}
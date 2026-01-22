using System;
using System.Collections.Generic;
using UnityEngine;

namespace YuankunHuang.Kodama.Core
{
    public interface IModule
    {
        void Init();
        void Dispose();
    }
    
    public static class ModuleRegistry
    {
        private static readonly Dictionary<Type, IModule> _modules = new();

        public static void Register<T>(T impl) where T : class, IModule
        {
            if (!_modules.TryGetValue(typeof(T), out var existedImpl))
            {
                _modules.Add(typeof(T), impl);
            }
            else
            {
                throw new InvalidOperationException($"Already registered a module of type {typeof(T)} -> {existedImpl.GetType().FullName}");
            }
        }

        public static void Unregister<T>(T impl) where T : class, IModule
        {
            if (_modules.ContainsKey(typeof(T)))
            {
                _modules.Remove(typeof(T));
            }
        }

        public static T Get<T>() where T : class, IModule
        {
            if (_modules.TryGetValue(typeof(T), out var module))
            {
                return module as T;
            }

            return null;
        }

        public static bool TryGet<T>(out T module) where T : class, IModule
        {
            if (_modules.TryGetValue(typeof(T), out var iModule))
            {
                module = iModule as T;
                return true;
            }

            module = null;
            return false;
        }

        public static void Clear()
        {
            _modules.Clear();
        }
    }    
}
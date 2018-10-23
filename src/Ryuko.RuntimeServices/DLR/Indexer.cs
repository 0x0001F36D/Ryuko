﻿// Author: Viyrex(aka Yuyu)
// Contact: mailto:viyrex.aka.yuyu@gmail.com
// Github: https://github.com/0x0001F36D

namespace Ryuko.RuntimeServices.DLR
{
    using CuttingEdge.Conditions;

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public static class NamedIndexExtension
    {
        public static Concat<TAnonymous> Index<TAnonymous>(this TAnonymous anonymous) where TAnonymous : class
        {
            return new Concat<TAnonymous>(anonymous);
        }
        public sealed class Concat<TAnonymous> where TAnonymous : class
        {
            private readonly TAnonymous _anonymous;

            internal Concat(TAnonymous anonymous)
            {
                this._anonymous = anonymous;
            }

            public NamedIndex<TAnonymous, TSelected> MatchAll<TSelected>()
            {
                return NamedIndex<TAnonymous, TSelected>.Create(this._anonymous);
            }
            public NamedIndex<TAnonymous, dynamic> MatchAny()
            {
                return NamedIndex<TAnonymous, dynamic>.Create(this._anonymous);
            }
        }

    }

    public sealed class NamedIndex<TAnonymous, TSelected> : IReadOnlyDictionary<string, TSelected> where TAnonymous : class
    {
        internal static NamedIndex<TAnonymous, TSelected> Create(TAnonymous anonymous)
        {
            Condition.Ensures(anonymous)
                .IsNotNull();

            var fetched = Fetch(anonymous);
            return new NamedIndex<TAnonymous, TSelected>(fetched);
        }



        private const BindingFlags PUBLIC_INSTANCE = (BindingFlags)20;
        private static readonly IReadOnlyDictionary<string, PropertyInfo> s_propertiesCache;
        private readonly ReadOnlyDictionary<string, TSelected> _props;
        

        static NamedIndex()
        {
            var type = typeof(TAnonymous);
            if (type.GetCustomAttribute<CompilerGeneratedAttribute>() is null && !type.IsConstructedGenericType)
                throw new InvalidOperationException($"Invalid type: '{type}'.");

            var props = type.GetProperties(PUBLIC_INSTANCE);
            s_propertiesCache = new ReadOnlyDictionary<string, PropertyInfo>(props.ToDictionary(x => x.Name));
        }

        private static ReadOnlyDictionary<string, TSelected> Fetch(TAnonymous anonymous)
        {
            var props = new Dictionary<string, TSelected>();
            foreach (var p in s_propertiesCache)
            {
                props[p.Key] = p.Value.GetValue(anonymous) is TSelected o ? o : throw new InvalidCastException(p.Key);
            }
            return new ReadOnlyDictionary<string, TSelected>(props);
        }

        private NamedIndex(ReadOnlyDictionary<string, TSelected> d)
        {
            this._props = d;
        }

       

        public bool ContainsKey(string key) => this._props.ContainsKey(key);
        public bool TryGetValue(string key, out TSelected value) => this._props.TryGetValue(key, out value);

        public IEnumerable<string> Keys => ((IReadOnlyDictionary<string, TSelected>)this._props).Keys;

        public IEnumerable<TSelected> Values => ((IReadOnlyDictionary<string, TSelected>)this._props).Values;

        public int Count => this._props.Count;

        public IEnumerator<KeyValuePair<string, TSelected>> GetEnumerator() => this._props.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this._props.GetEnumerator();

        public TSelected this[string key] => this._props[key];
    }
}
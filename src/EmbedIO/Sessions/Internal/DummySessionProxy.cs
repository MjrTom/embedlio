using System;
using System.Collections.Generic;

namespace EmbedIO.Sessions.Internal
{
    internal sealed class DummySessionProxy : ISessionProxy
    {
        private DummySessionProxy()
        {
        }

        public static ISessionProxy Instance { get; } = new DummySessionProxy();

        public bool Exists => false;

        /// <inheritdoc/>
        public string Id => throw DummySessionProxy.NoSessionManager();

        /// <inheritdoc/>
        public TimeSpan Duration => throw DummySessionProxy.NoSessionManager();

        /// <inheritdoc/>
        public DateTime LastActivity => throw DummySessionProxy.NoSessionManager();

        /// <inheritdoc/>
        public int Count => 0;

        /// <inheritdoc/>
        public bool IsEmpty => true;

        /// <inheritdoc/>
        public object this[string key]
        {
            get => throw DummySessionProxy.NoSessionManager();
            set => throw DummySessionProxy.NoSessionManager();
        }

        /// <inheritdoc/>
        public void Delete()
        {
        }

        /// <inheritdoc/>
        public void Regenerate() => throw DummySessionProxy.NoSessionManager();

        /// <inheritdoc/>
        public void Clear()
        {
        }

        /// <inheritdoc/>
        public bool ContainsKey(string key) => throw DummySessionProxy.NoSessionManager();

        /// <inheritdoc/>
        public bool TryGetValue(string key, out object value) => throw DummySessionProxy.NoSessionManager();

        /// <inheritdoc/>
        public bool TryRemove(string key, out object value) => throw DummySessionProxy.NoSessionManager();

        /// <inheritdoc/>
        public IReadOnlyList<KeyValuePair<string, object>> TakeSnapshot() => throw DummySessionProxy.NoSessionManager();

        private static InvalidOperationException NoSessionManager() => new("No session manager registered in the web server.");
    }
}
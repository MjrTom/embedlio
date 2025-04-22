using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace EmbedIO.Authentication
{
    /// <summary>
    /// Simple HTTP basic authentication module that stores credentials
    /// in a <seealso cref="ConcurrentDictionary{TKey,TValue}"/>.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BasicAuthenticationModule"/> class.
    /// </remarks>
    /// <param name="baseRoute">The base route.</param>
    /// <param name="realm">The authentication realm.</param>
    /// <remarks>
    /// <para>If <paramref name="realm"/> is <see langword="null"/> or the empty string,
    /// the <see cref="BasicAuthenticationModuleBase.Realm">Realm</see> property will be set equal to
    /// <see cref="IWebModule.BaseRoute">BaseRoute</see>.</para>
    /// </remarks>
    public class BasicAuthenticationModule(string baseRoute, string? realm = null) : BasicAuthenticationModuleBase(baseRoute, realm)
    {
        /// <summary>
        /// Gets a dictionary of valid user names and passwords.
        /// </summary>
        /// <value>
        /// The accounts.
        /// </value>
        public ConcurrentDictionary<string, string> Accounts { get; } = new ConcurrentDictionary<string, string>(StringComparer.InvariantCulture);

        /// <inheritdoc />
        protected override Task<bool> VerifyCredentialsAsync(string path, string userName, string password, CancellationToken cancellationToken)
            => Task.FromResult(VerifyCredentialsInternal(userName, password));

        private bool VerifyCredentialsInternal(string userName, string password)
            => userName != null
            && Accounts.TryGetValue(userName, out var storedPassword)
            && string.Equals(password, storedPassword, StringComparison.Ordinal);
    }
}
﻿using System;

namespace EmbedIO.Routing
{
    /// <summary>
    /// Decorate methods within controllers with this attribute in order to make them callable from the Web API Module
    /// Method Must match the WebServerModule.
    /// </summary>
    /// <remarks>
    /// Initializes a new instance of the <see cref="BaseRouteAttribute"/> class.
    /// </remarks>
    /// <param name="verb">The verb.</param>
    /// <param name="route">The route.</param>
    /// <exception cref="ArgumentNullException"><paramref name="route"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">
    /// <para><paramref name="route"/> is empty.</para>
    /// <para>- or -</para>
    /// <para><paramref name="route"/> does not start with a slash (<c>/</c>) character.</para>
    /// <para>- or -</para>
    /// <para><paramref name="route"/> does not comply with route syntax.</para>
    /// </exception>
    /// <seealso cref="Routing.Route.IsValid"/>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class BaseRouteAttribute(HttpVerbs verb, string route) : RouteAttribute(verb, route, true)
    {
    }
}
using System;

namespace vProto.RMI
{
    /// <summary>
    /// Represents a proxy with a name.
    /// </summary>
    public interface INamedProxy
    {
        /// <summary>
        /// Gets the name associated with the proxy object.
        /// </summary>
        string Name { get; }
    }

    /// <summary>
    /// Represent a proxy over an object of known type.
    /// </summary>
    /// <typeparam name="TObject">The type proxied by the object.</typeparam>
    public interface ITypedProxy<TObject>
    {
        /// <summary>
        /// Gets the proxied object.
        /// </summary>
        TObject Object { get; }
    }

    /// <summary>
    /// Represents a proxy over an object.
    /// </summary>
    public interface ITypedProxy
    {
        /// <summary>
        /// Gets the proxied object.
        /// </summary>
        object Object { get; }
    }
}

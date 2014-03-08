using System;

namespace vProto.RMI
{
    public interface INamedProxy
    {
        string Name { get; }
    }

    public interface ITypedProxy<TObject>
    {
        TObject Object { get; }
    }

    public interface ITypedProxy
    {
        object Object { get; }
    }
}

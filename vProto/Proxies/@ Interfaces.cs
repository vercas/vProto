using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace vProto.Proxies
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

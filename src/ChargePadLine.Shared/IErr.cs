using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Shared
{
    public interface IErr
    {
        string Code { get; }

        string Message { get; }
    }
}

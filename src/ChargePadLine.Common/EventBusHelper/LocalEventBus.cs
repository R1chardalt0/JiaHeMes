using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChargePadLine.Common.EventBusHelper
{
    public class LocalEventBus<T> where T : class
    {
        public delegate Task LocalEventBusHandler(T t);
        public event LocalEventBusHandler localEventBusHandler;
        public async Task Publish(T t)
        {
            await localEventBusHandler(t);
        }
    }
}

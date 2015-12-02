using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SWMS.Core.Extentions
{
    public static class ActionExtentions
    {
        public static void SafeRise<TArgs>(this Action<Object, TArgs> source, Object sender, TArgs args)
        {
            var handlers = source;
            if (handlers == null)
            {
                return;
            }

            handlers(sender, args);
        }

        public static void SafeRise(this Action<Object> source, Object sender)
        {
            var handlers = source;
            if (handlers == null)
            {
                return;
            }

            handlers(sender);
        }
    }
}

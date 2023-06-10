using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnarPerPortes
{
    public static class ShortUtils
    {
        public static bool IsHardmodeEnabled()
        {
            return HardmodeManager.Singleton.IsHardmodeEnabled;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using WashMachine.Enums;

namespace WashMachine.Protocols.Directives
{
    public abstract class BaseDirective
    {
        public int DirectiveId { get; set; }
        public int TargetDeviceId { get; set; }
        private static int directiveId = 65530;
        public abstract DirectiveTypeEnum DirectiveType { get; }
        public abstract int Priority { get; }
        public TargetDeviceTypeEnum DeviceType { get; set; }

        protected BaseDirective()
        {
            Interlocked.Increment(ref directiveId);
            DirectiveId = directiveId % 0xffff;
        }

        public static void ResetDirectiveId()
        {
            Interlocked.Exchange(ref directiveId, 0);
        }

    }

}

// (C) 2012-2019 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System.Linq;

namespace Flow.Impl
{
    internal class Barrier 
        : Group
        , IBarrier
    {
        internal Barrier()
        {
            //Completed += (tr) => Verbose(10, $"Barrier {this} Completed");
        }

        public override void Post()
        {
            base.Post();

            if (Contents.Any(t => t.Active))
                return;

            if (Additions.Count == 0)
                Complete();
        }
    }
}
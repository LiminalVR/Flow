// (C) 2012-2019 Christian Schladetsch. See https://github.com/cschladetsch/Flow.

using System;
using System.Linq;

namespace Flow.Impl
{
    internal class Node
        : Group
        , INode
    {
        private bool _stepping;
        protected bool _StepOne;

        public void Add(params IGenerator[] gens)
        {
            foreach (var gen in gens)
                DeferAdd(gen);
        }

        public override void Step()
        {
            Pre();

            try
            {
                if (Kernel.DebugLevel > EDebugLevel.Medium)
                    Kernel.Log.Info($"Stepping Node {Name}");

                if (_stepping)
                {
                    Kernel.Log.Error(
                        $"Node {Name} is re-entrant. Nodes cannot directly or indirectly invoke their Step methods when stepping.");
                    throw new ReentrancyException();
                }

                _stepping = true;

                base.Step();

                // TODO: do we really need to copy the contents? Maybe use some double-buffering if required to avoid copying.
                // TODO: that said, it's only creating a new list of references...
                // TODO: the underlying issue is that the contents of the node may be altered while stepping children of the node.
                foreach (var tr in Contents.ToList())
                {
                    if (tr is IGenerator gen)
                    {
                        if (Kernel.Break)
                            goto end;

                        if (!gen.Active)
                        {
                            Remove(gen);
                            continue;
                        }

                        if (gen.Running)
                            gen.Step();
                    }

                    if (_StepOne)
                        break;
                }
            }
            catch (Exception e)
            {
                Error($"Exception: {e.Message} when stepping {Name}. Completing this Node.");
                Complete();
            }
            finally
            {
                _stepping = false;
            }

        end:
            Post();
        }
    }
}
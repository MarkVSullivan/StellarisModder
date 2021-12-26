using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StellarisModder.Actions
{
    public interface IAction
    {
        bool Apply(InstructionSet Instructions, ResourceFile File);
    }
}

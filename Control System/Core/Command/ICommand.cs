using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_System.Core.Command
{
    public interface ICommand<TRespondeType, TInputType>
    {
        TRespondeType Execute(TInputType input);
    }
}

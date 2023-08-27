using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Control_System.Core.Command
{
    // command treat null as not return? I don't know is good idea either.
    public interface ICommand<TRespondeType, TInputType>
    {
        TRespondeType? Execute(TInputType input);
    }

    public interface ICommand<TRespondeType>
    {
        TRespondeType Execute();
    }

    //public interface IProcedureCommand<TInputType>
    //{
    //    void Execute(TInputType input);
    //}
}

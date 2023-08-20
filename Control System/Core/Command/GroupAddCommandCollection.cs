using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// I don't know how to make this work. The idea is a request can make multiple command run, but each command have different type of return, and that is the problem of this class. Exactly, I don't know how to merge return result of commands into one.

namespace Control_System.Core.Command
{
    public class GroupAddCommandCollection<TCommandIdentityType, TCommandRespondType> : BaseCommmandCollection<TCommandIdentityType, TCommandRespondType>
    {
        public GroupAddCommandCollection(IServiceCollection commandCollectionInstance, IDictionary<TCommandIdentityType, Type> dictionaryInstance) : base(commandCollectionInstance, dictionaryInstance) { }
        public virtual void Register(TCommandIdentityType identity, object commandInstance)
        {

        }

        protected class GroupCommand<TRespondType, TInputType> : ICommand<TRespondType, TInputType>
        {
            public IEnumerable<ICommand<TRespondType, TInputType>> Commands { get; private set; }
            private readonly IEnumerable<ICommand<TRespondType, TInputType>> _commandCollection;

            public GroupCommand(IEnumerable<ICommand<TRespondType, TInputType>> commandCollection)
            {
                _commandCollection = commandCollection;
                Commands = _commandCollection;
            }

            public TRespondType Execute(TInputType input)
            {
                foreach (var command in _commandCollection)
                {
                    command.Execute(input);
                }
            }
        }
    }
}

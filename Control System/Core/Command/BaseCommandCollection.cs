using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Control_System.Core.Command
{
    public interface ICommandCollection<TCommandIndentityType>
    {
        public void Register(TCommandIndentityType identity, Delegate instruction);
        public object Resolve(TCommandIndentityType identity);
        public void Remove(TCommandIndentityType identity);
    }

    // the command collection don't care which type of command it containt, but only care about the command identity which command used to distinc with other command
    // in the case command collection take the instruction to create command instance, it cannot provide the parameter which is needed in the executing instruction process, so the instruction have to find somehow provide its own data for create and this can be done without worried because even if the instruction process use something have been disposed by GC, it actually still there to be used, so don't worry about it.
    public abstract partial class BaseCommandCollection<TCommandIdentityType> : ICommandCollection<TCommandIdentityType>
    {
        protected readonly IDictionary<TCommandIdentityType, Delegate> _commands;

        public BaseCommandCollection(IDictionary<TCommandIdentityType, Delegate> commandCollectionInstance)
        {
            _commands = commandCollectionInstance;
        }

        protected bool IsSpecificParameterizedTypeOfGenericTypeDefinition(Type needToCheckType, Type genericTypeDefinition)
        {
            var isThisTypeABoundGenericType = new Func<Type, bool>((type) =>
            {
                return !(type.IsGenericType && type.IsGenericTypeDefinition);
            });

            var isThisTypeAnUnboundGenericType = new Func<Type, bool>((type) =>
            {
                return type.IsGenericTypeDefinition && type.IsGenericType;
            });

            var isThisTypeAClass = new Func<Type, bool>((type) =>
            {
                return type.IsClass;
            });

            var isThisTypeAnInterface = new Func<Type, bool>((type) =>
            {
                return type.IsInterface;
            });

            var confirmProcess = new Func<Type, Type, bool>((thisType, thatType) =>
            {
                if (!thisType.IsGenericType || !thatType.IsGenericType)
                {
                    return false;
                }
                else
                {
                    if (isThisTypeAnUnboundGenericType(thisType) && isThisTypeAnUnboundGenericType(thatType))
                    {
                        return thisType.GetGenericTypeDefinition() == thatType.GetGenericTypeDefinition();
                    }

                    if (isThisTypeAnUnboundGenericType(thisType) && isThisTypeABoundGenericType(thatType))
                    {
                        return false;
                    }

                    if (isThisTypeABoundGenericType(thisType) && isThisTypeAnUnboundGenericType(thatType))
                    {
                        return thisType.GetGenericTypeDefinition().GetGenericTypeDefinition() == thatType.GetGenericTypeDefinition();
                    }

                    if (isThisTypeABoundGenericType(thisType) && isThisTypeABoundGenericType(thatType))
                    {
                        return thisType.GetGenericTypeDefinition().GetGenericTypeDefinition() == thatType.GetGenericTypeDefinition().GetGenericTypeDefinition();
                    }

                    return false;
                }
            });

            return confirmProcess(needToCheckType, genericTypeDefinition);
        }

        protected bool IsInheritedFromGenericTypeDefinition(Type needToCheckType, Type genericTypeDefinition)
        {
            if (!genericTypeDefinition.IsGenericTypeDefinition)
            {
                throw new ArgumentException();
            }
            else
            {
                var isThisTypeAClass = new Func<Type, bool>((type) =>
                {
                    return type.IsClass;
                });

                var isThisTypeAnInterface = new Func<Type, bool>((type) =>
                {
                    return type.IsInterface;
                });

                if (isThisTypeAnInterface(genericTypeDefinition))
                {
                    foreach (var item in needToCheckType.GetInterfaces())
                    {
                        if (IsSpecificParameterizedTypeOfGenericTypeDefinition(item, genericTypeDefinition))
                        {
                            return true;
                        }
                    }

                    if (IsSpecificParameterizedTypeOfGenericTypeDefinition(needToCheckType, genericTypeDefinition))
                    {
                        return true;
                    }
                }

                if (isThisTypeAClass(genericTypeDefinition) && needToCheckType.BaseType != null)
                {
                    return IsSpecificParameterizedTypeOfGenericTypeDefinition(needToCheckType.BaseType, genericTypeDefinition);
                }

                return false;
            }
        }

        public virtual void Register(TCommandIdentityType identity, Delegate instruction)
        {
            if (identity == null)
            {
                throw new NullIdentityException();
            }

            if (_commands.ContainsKey(identity))
            {
                throw new IdentityException("Existed identity.");
            }

            if (instruction == null)
            {
                throw new NullInstructionException();
            }

            if (!IsInheritedFromGenericTypeDefinition(instruction.Method.ReturnType, typeof(ICommand<,>)))
            {
                throw new InstructionReturnTypeException("The instruction does not return instance of ICommand<,> type.");
            }

            if (instruction.Method.GetParameters().Any())
            {
                throw new InstructionParameterException("Cannot provide to instruction any parameter.");
            }

            _commands.Add(identity, instruction);

        }

        public virtual object Resolve(TCommandIdentityType identity)
        {
            return _commands[identity].DynamicInvoke();
        }

        public virtual void Remove(TCommandIdentityType identity)
        {
            _commands.Remove(identity);
        }

    }

    partial class BaseCommandCollection<TCommandIdentityType>
    {
        public class InstructionException : ArgumentException
        {
            public InstructionException(string exceptionInfo = "") : base("Instruction exeption" + (exceptionInfo.Any() ? ": " + exceptionInfo : "")) { }
        }

        public class NullInstructionException : InstructionException
        {
            public NullInstructionException() : base("Instruction is null.") { }
        }

        public class InstructionParameterException : InstructionException
        {
            public InstructionParameterException(string parameterExceptionInfo) : base(parameterExceptionInfo) { }
        }

        public class InstructionReturnTypeException : InstructionException
        {
            public InstructionReturnTypeException(string returnTypeExceptionInfo) : base(returnTypeExceptionInfo) { }
        }

        public class IdentityException : ArgumentException
        {
            public IdentityException(string exceptionInfo = "") : base("Identity exception" + (exceptionInfo.Any() ? ": " + exceptionInfo : "")) { }
        }

        public class NullIdentityException : IdentityException
        {
            public NullIdentityException() : base("Identity is null.") { }
        }
    }
}


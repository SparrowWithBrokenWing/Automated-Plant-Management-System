using Control_System.Core.Command;
using Control_System.Core.Variable;
using System.Reflection;

namespace Control_System.Core.Executer
{
    // each type of executer should handle one specific request type only    
    // but should each executer only execute a type of command?
    // if request itself is where user specify the command they want to run and the variable they want to provide to command, what should be the type of command collection
    public abstract partial class BaseExecuter<TRequestType, TCommandIdentityType, TCommandReturnType, TVariableIdentityType>
    {
        private BaseCommandCollection<TCommandIdentityType> _commands;
        private BaseVariableCollection<TVariableIdentityType> _variables;

        public BaseExecuter(BaseVariableCollection<TVariableIdentityType> variableCollection, BaseCommandCollection<TCommandIdentityType> commandCollection)
        {
            _commands = commandCollection;
            _variables = variableCollection;
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
                return false;
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
                }

                if (isThisTypeAClass(genericTypeDefinition) && needToCheckType.BaseType != null)
                {
                    return IsSpecificParameterizedTypeOfGenericTypeDefinition(needToCheckType.BaseType, genericTypeDefinition);
                }

                return false;
            }
        }

        public virtual void HandleRequest(TRequestType request)
        {
            var specifiedVariable = _variables.Resolve(GetSpecifiedVariableIdentity(request));
            var specifiedCommand = _commands.Resolve(GetSpecifiedCommandIdentity(request));

            Type commandType = specifiedCommand.GetType();

            if (!IsInheritedFromGenericTypeDefinition(commandType, typeof(ICommand<,>)))
            {
                throw new RequestExeption("Specified command isn't a specific parameterized type of ICommand interface.");
            }

            var returnTypeOfExecuteMethod = typeof(TCommandReturnType);
            var parameterTypeOfExecuteMthod = specifiedVariable.GetType();
            
            var executeMethodInfo = commandType.GetMethod(
                "Execute",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.InvokeMethod, 
                new Type[] {parameterTypeOfExecuteMthod});

            if (executeMethodInfo == null)
            {
                throw new RequestExeption("Cannot find match command.");
            }

            // call execute method of command instance
            var commandResult = executeMethodInfo.Invoke(specifiedCommand, new object[] { specifiedVariable });

            HandleReturnResultOfCommand((TCommandReturnType?)commandResult);
        }

        protected abstract TCommandIdentityType GetSpecifiedCommandIdentity(TRequestType request);
        protected abstract TVariableIdentityType GetSpecifiedVariableIdentity(TRequestType request);
        protected abstract void HandleReturnResultOfCommand(TCommandReturnType? returnObject);
    }

    partial class BaseExecuter<TRequestType, TCommandIdentityType, TCommandReturnType, TVariableIdentityType>
    {
        public class RequestExeption : ArgumentException
        {
            public RequestExeption(string? message) : base(message) { }
        }
    }
}

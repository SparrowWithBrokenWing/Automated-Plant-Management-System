using Control_System.Core.Command;
using Control_System.Core.Variable;
using System.Reflection;

namespace Control_System.Core.Executer
{
    // each type of executer should handle one specific request type only    
    // but should each executer only execute a type of command?
    // if request itself is where user specify the command they want to run and the variable they want to provide to command, what should be the type of command collection
    public abstract class BaseExecuter<TRequestType, TCommandIdentityType, TCommandReturnType, TVariableIdentityType>
    {
        private BaseCommandCollection<TCommandIdentityType> _commands;
        private BaseVariableCollection<TVariableIdentityType> _variables;

        public BaseExecuter(BaseVariableCollection<TVariableIdentityType> variableCollection, BaseCommandCollection<TCommandIdentityType> commandCollection)
        {
            _commands = commandCollection;
            _variables = variableCollection;
        }

        protected bool ConfirmThatThisTypeIsSpecificParameterizedTypeOfThatGenericTypeDefinition(Type thisType, Type thatType)
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

            return confirmProcess(thisType, thatType);
        }

        protected bool ConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(Type thisType, Type thatGenericTypeDefinition)
        {
            if (!thatGenericTypeDefinition.IsGenericTypeDefinition)
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

                if (isThisTypeAnInterface(thatGenericTypeDefinition))
                {
                    foreach (var item in thisType.GetInterfaces())
                    {
                        if (ConfirmThatThisTypeIsSpecificParameterizedTypeOfThatGenericTypeDefinition(item, thatGenericTypeDefinition))
                        {
                            return true;
                        }
                    }
                }

                if (isThisTypeAClass(thatGenericTypeDefinition) && thisType.BaseType != null)
                {
                    return ConfirmThatThisTypeIsSpecificParameterizedTypeOfThatGenericTypeDefinition(thisType.BaseType, thatGenericTypeDefinition);
                }

                return false;
            }
        }

        public virtual void HandleRequest(TRequestType request)
        {
            var specifiedCommand = _commands.Resolve(GetSpecificCommandIdentity(request));

            Type commandType = specifiedCommand.GetType();

            if (!ConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(commandType, typeof(ICommand<,>)))
            {
                throw new RequestExeption("Specified command isn't a specific parameterized type of ICommand interface.");
            }

            //var executeMethodInfo = commandType.GetMethod("Execute", );

            //if (executeMethodInfo == null)
            //{
            //    // throw something here
            //}

            //var commandReturnType;
            //var commandParamType;

            // throw exception if executer don't support return type
            //if ()
            //{

            //}

            var specifiedVariable = _variables.Resolve(GetSpecificVariableIdentity(request));

            // throw exception if the specified variable don't match require of command execute method
            //if ()
            //{

            //}

            // call execute method of command instance

        }

        public class RequestExeption : ArgumentException
        {
            public RequestExeption(string? message) : base(message) { }
        }

        protected abstract TCommandIdentityType GetSpecificCommandIdentity(TRequestType request);
        protected abstract TVariableIdentityType GetSpecificVariableIdentity(TRequestType request);
        protected abstract void HandleReturnResultOfCommand(TCommandReturnType returnObject);
    }
}

using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Control_System.Core.Command
{
    // the command collection don't care which type of command it containt, but only care about the command identity which command used to distinc with other command,
    public abstract class BaseCommandCollection<TCommandIdentityType>
    {
        protected readonly IDictionary<TCommandIdentityType, object> _registeredCommand;

        public BaseCommandCollection(IDictionary<TCommandIdentityType, object> commandCollectionInstance)
        {
            _registeredCommand = commandCollectionInstance;
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

        public virtual void Register(TCommandIdentityType identity, object commandInstance)
        {
            if (commandInstance == null)
            {
                throw new ArgumentException("The provided object is null.");
            }

            if (identity == null)
            {
                throw new ArgumentException("The identity is null.");
            }

            if (! ConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(commandInstance.GetType(), typeof(ICommand<,>)))
            {
                throw new ArgumentException("The provided object is not inherited from ICommand."); 
            }

            // i not sure i need this
            //foreach (var command in _registeredCommand)
            //{
            //    if (command.Value.GetType() == commandType)
            //    {
            //        throw new ArgumentException("Existed command type.");
            //    }
            //}

            _registeredCommand.Add(identity, commandInstance);
        }

        public virtual object Resolve(TCommandIdentityType identity)
        {
            return _registeredCommand[identity];
        }

        public virtual void Remove(TCommandIdentityType identity)
        {
            _registeredCommand.Remove(identity);
        }
    }
}


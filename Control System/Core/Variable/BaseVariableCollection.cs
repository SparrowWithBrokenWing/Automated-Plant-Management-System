
namespace Control_System.Core.Variable
{
    public abstract unsafe class BaseVariableCollection<TVariableIdentityType>
    {
        protected readonly IDictionary<TVariableIdentityType, object> _registeredVariable;

        public BaseVariableCollection(IDictionary<TVariableIdentityType, object> variableCollectionInstance)
        {
            _registeredVariable = variableCollectionInstance;
        }

        // i don't know do i need to register with a reference or not.
        // what i want is if you changed state of object and after that you resolve it from collection, you get the variable have state changed.
        public virtual void Register(TVariableIdentityType identity, object variableInstance)
        {
            if (variableInstance == null)
            {
                throw new ArgumentException("The provided object is null.");
            }

            if (identity == null)
            {
                throw new ArgumentException("The identity is null.");
            }

            _registeredVariable.Add(identity, variableInstance);
        }

        public virtual ref object Resolve(TVariableIdentityType identity)
        {
            var result = _registeredVariable[identity];
            return ref result;
        }

        public virtual void Remove(TVariableIdentityType identity)
        {
            _registeredVariable.Remove(identity);
        }
    }
}


namespace Control_System.Core_Components.Variable
{
    public interface IVariableCollection<TVariableIdentityType>
    {
        public void Register(TVariableIdentityType identity, object variableInstance);
        public object Resolve(TVariableIdentityType identity);
        public void Remove(TVariableIdentityType identity);
    }
}

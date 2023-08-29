namespace Control_System.Core_Components.Command
{
    public interface ICommandCollection<TCommandIndentityType>
    {
        public void Register(TCommandIndentityType identity, Delegate instruction);
        public object Resolve(TCommandIndentityType identity);
        public void Remove(TCommandIndentityType identity);
    }
}


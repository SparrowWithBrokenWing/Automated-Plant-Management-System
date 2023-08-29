namespace Control_System.Core_Components.Executer
{
    public interface IExecuter<TRequestType>
    {
        public void HandleRequest(TRequestType request);
    }
}

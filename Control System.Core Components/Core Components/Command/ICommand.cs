namespace Control_System.Core_Components.Command
{
    // command treat null as not return? I don't know is good idea either.
    public interface ICommand<TRespondeType, TInputType>
    {
        TRespondeType? Execute(TInputType input);
    }

    public interface IParameterlessCommand<TRespondeType>
    {
        TRespondeType Execute();
    }

    public interface IProcedureCommand<TInputType>
    {
        void Execute(TInputType input);
    }

    public interface IActionCommand
    {
        void Execute();
    }
}

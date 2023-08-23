using Control_System.Core.Command;
using Control_System.Core.Variable;
using Control_System.Core.Executer;
using Moq;

namespace TestProject1.Executer_Tests
{
    public class BaseExecuterTest
    {
        private class TestCommandCollection : BaseCommandCollection<string>
        {
            public TestCommandCollection() : base(new Dictionary<string, Delegate>()) { }
        }

        private class TestVariableCollection : BaseVariableCollection<string>
        {
            public TestVariableCollection() : base(new Dictionary<string, object>()) { }
        }

        private class TestExecuter : BaseExecuter<string, string, string, string>
        {
            public TestExecuter() : base(
                new TestVariableCollection(),
                new TestCommandCollection())
            { }

            public bool TestConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(Type thisType, Type thatGenericTypeDefinition)
                => ConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(thisType, thatGenericTypeDefinition);

            protected override string GetSpecificCommandIdentity(string request)
            {
                throw new NotImplementedException();
            }

            protected override string GetSpecificVariableIdentity(string request)
            {
                throw new NotImplementedException();
            }

            protected override void HandleReturnResultOfCommand(string returnObject)
            {
                throw new NotImplementedException();
            }
        }

        private class Test { }
        private class Test1 : Test { }
        private interface ITest { }
        private class Test2 : ITest { }
        private abstract class Test<T> { }
        private class Test3<T> : Test<T> { }

        TestExecuter executer = new TestExecuter();

        [Fact]
        public void VerifyConfirmInheritedMethodReturnTrue()
        {
            Assert.True(executer.TestConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(typeof(Test3<string>), typeof(Test<>)));
        }

        [Fact]
        public void VerifyConfirmInheritedMethodReturnFalseWhenSecondParameterIsNotAGenericTypeDefinition()
        {
            Assert.False(executer.TestConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(typeof(Test1), typeof(Test)));
            Assert.False(executer.TestConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(typeof(ITest), typeof(Test2)));
            Assert.False(executer.TestConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(typeof(Test1), typeof(Test2)));
            Assert.False(executer.TestConfirmThatThisTypeIsInheritedFromThatGenericTypeDefinition(typeof(Test2), typeof(System.Collections.IList)));
        }
    }
}

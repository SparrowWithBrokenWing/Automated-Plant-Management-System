using Control_System.Core_Components.Variable;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1.Variable_Tests
{
    public class TestUnitForDefaultImplementationOfIVariableCollection
    {
        private class TestVariableCollection<TTestIdentityType> : DefaultImplementationOfIVariableCollection<TTestIdentityType>
        {
            public TestVariableCollection(IDictionary<TTestIdentityType, object> variableCollectionInstance) : base(variableCollectionInstance)
            {

            }            
        }

        [Fact]
        public void VerifyThrowExceptionWhenTryToRegisterWithNullIdentity()
        {
            var tryToRegisterWithNullIdentity = new Action(() =>
            {
                var testVariableCollection = new TestVariableCollection<string>(new Dictionary<string, object>());
                testVariableCollection.Register(null, "hello");
            });
            Assert.Throws<ArgumentException>(tryToRegisterWithNullIdentity);
        }

        [Fact]
        public void VerifyThrowExceptionWhenTryToRegisterWithNullValue()
        {
            var tryToRegisterWithNullValue = new Action(() =>
            {
                var testVariableCollection = new TestVariableCollection<string>(new Dictionary<string, object>());
                testVariableCollection.Register("hello", null);
            });
            Assert.Throws<ArgumentException>(tryToRegisterWithNullValue);
        }

        [Fact]
        public void VerifyThrowExceptionWhenTryToRegisterWithSameVariableIdentity()
        {
            var tryToRegisterWithSameVariableIdentity = new Action(() =>
            {
                var testVariableCollection = new TestVariableCollection<string>(new Dictionary<string, object>());
                testVariableCollection.Register("hi", "hello world");
                testVariableCollection.Register("hi", "hi world");
            });
            Assert.Throws<ArgumentException>(tryToRegisterWithSameVariableIdentity);
        }

        private class TestClass
        {
            public int Number;
        }

        // can only work with reference type
        [Fact]
        public void VerifyObjectStateChangedAfterResolvedThenChangedAndRegister()
        {
            var registerObject = new TestClass();
            registerObject.Number = 0;
            var testVariableCollection = new TestVariableCollection<string>(new Dictionary<string, object>());
            var identity = "testVariable";

            testVariableCollection.Register(identity, registerObject);

            registerObject.Number = -1;
            var oldState = (TestClass)testVariableCollection.Resolve(identity);
            Assert.Equal(-1, oldState.Number);

            registerObject.Number = 30;
            var newState = (TestClass)testVariableCollection.Resolve(identity);
            Assert.Equal(30, newState.Number);
        }
    }
}

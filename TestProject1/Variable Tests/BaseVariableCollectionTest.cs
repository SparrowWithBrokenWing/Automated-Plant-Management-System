using Control_System.Core.Variable;
using Microsoft.Extensions.DependencyInjection;

namespace TestProject1.Variable_Tests
{
    public class BaseVariableCollectionTest
    {
        private class TestVariableCollection<TTestIdentityType> : BaseVariableCollection<TTestIdentityType>
        {
            public TestVariableCollection(IDictionary<TTestIdentityType, object> variableCollectionInstance) : base(variableCollectionInstance)
            {

            }            
        }

        private class TestStruct
        {
            public int Number;
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

        [Fact]
        public void VerifyObjectStateChangedAfterResolvedThenChangedAndRegister()
        {
            var registerObject = new TestStruct();
            registerObject.Number = 0;
            var testVariableCollection = new TestVariableCollection<string>(new Dictionary<string, object>());
            var identity = "testVariable";

            testVariableCollection.Register(identity, registerObject);

            registerObject.Number = -1;
            var oldState = (TestStruct)testVariableCollection.Resolve(identity);
            Assert.True(oldState.Number == -1);

            registerObject.Number = 30;
            var newState = (TestStruct)testVariableCollection.Resolve(identity);
            Assert.True(newState.Number == 30);
        }
    }
}

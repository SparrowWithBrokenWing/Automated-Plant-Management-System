using Control_System.Core.Command;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace TestProject1.Command_Tests
{
    public class BaseCommandCollectionTest
    {
        private class TestStringCommandCollection : BaseCommandCollection<string>
        {
            public TestStringCommandCollection() : base(new Dictionary<string, object>()) { }
        }

        private class HelloWorldCommand : ICommand<string, string>, IDisposable
        {
            public bool Flag = false;

            public void Dispose()
            {

            }

            public string Execute(string input)
            {
                return "Executed.";
            }
        }

        private class AnotherHelloWorldCommand : ICommand<string, string>
        {
            private readonly TextWriter _writer;

            public AnotherHelloWorldCommand(TextWriter outputWriter)
            {
                _writer = outputWriter;
            }

            public string Execute(string input)
            {
                _writer.WriteLine("This is a message from a hello world command instance: Welcome to our world! (" + input + ")");
                return "Executed.";
            }
        }

        private class HiWorldCommand : ICommand<int, string>, IDisposable
        {
            public bool Flag = false;

            public void Dispose()
            {

            }

            public int Execute(string input)
            {
                Console.WriteLine("Hi from a Hi world command instance");
                Console.WriteLine("Also, there is something need to say too: {0}", input);
                return 0;
            }
        }

        [Fact]
        public void VerifyThrowExeptionWhenTryToRegisterNullObject()
        {
            var instance = new TestStringCommandCollection();

            Action tryToRegisterNullObject = () =>
            {
                instance.Register("Hello", null);
            };
            Assert.Throws<ArgumentException>(tryToRegisterNullObject);
        }

        [Fact]
        public void VerifyThrowExeptionWhenTryToRegisterWithInstanceThatNotDerivedFromICommand()
        {
            var instance = new TestStringCommandCollection();
            Action tryToRegisterWithInstanceThatNotDerivedFromICommand = () =>
            {
                instance.Register("Hello", "string");
            };
            Assert.Throws<ArgumentException>(tryToRegisterWithInstanceThatNotDerivedFromICommand);
        }

        [Fact]
        public void VerifyThrowExeptionWhenTryToRegisterWithExistedIdentity()
        {
            var instance = new TestStringCommandCollection();
            Action tryToRegisterWithExistedIdentity = () =>
            {
                instance.Register("Hello", new HelloWorldCommand());
                instance.Register("Hello", new HiWorldCommand());
            };
            Assert.Throws<ArgumentException>(tryToRegisterWithExistedIdentity);
        }

        // i not sure the basecommandcollection instance need this ability
        //[Fact]
        //public void VerifyThrowExeptionWhenTryToRegisterWithExistedCommandType()
        //{
        //    var instance = new TestStringCommandCollection();
        //    Action tryToRegisterWithExistedCommandType = () =>
        //    {
        //        instance.Register("Hello", (ICommand<string, string>)new HelloWorldCommand());
        //        instance.Register("Another Hello", (ICommand<string, string>)new AnotherHelloWorldCommand());
        //    };
        //    Assert.Throws<ArgumentException>(tryToRegisterWithExistedCommandType);
        //}

        [Fact]
        public void VerifyThrowExceptionWhenTryToRegisterWithNullIdentity()
        {
            var instance = new TestStringCommandCollection();
            Action tryToRegisterWithNullIdentity = () =>
            {
                instance.Register(null, new HelloWorldCommand());
            };
            Assert.Throws<ArgumentException>(tryToRegisterWithNullIdentity);
        }

        [Fact]
        public void VerifyResolveReturnRegisteredInstanceState()
        {
            var instance = new TestStringCommandCollection();

            using (var command = new HelloWorldCommand())
            {
                command.Flag = true;
                instance.Register("Hello", command);
                command.Flag = false;
            }

            var resolvedCommand = instance.Resolve("Hello");

            Assert.Equal(typeof(HelloWorldCommand), resolvedCommand.GetType());
            Assert.False(((HelloWorldCommand)resolvedCommand).Flag);
        }

        [Fact]
        public void VerifyResolvedCommandExecuteWorkAsExpected()
        {
            var instance = new TestStringCommandCollection();
            var strInput = "This is a new holy world!";
            var commandIdentity = "Another hello command";
            var toConsoleOutput = "";
            var expectedToConsoleOutput = "This is a message from a hello world command instance: Welcome to our world! (" + strInput + ")";


            var writerMock = new Mock<TextWriter>();
            writerMock.Setup((writer) => writer.WriteLine(It.IsAny<string>()))
                .Callback<string>((commandInput) =>
                {
                    toConsoleOutput += commandInput.ToString();
                });

            instance.Register(commandIdentity, new AnotherHelloWorldCommand(writerMock.Object));
            var command = (ICommand<string, string>)instance.Resolve(commandIdentity);
            var result = command.Execute(strInput);

            writerMock.Verify((writer) => writer.WriteLine(It.IsAny<string>()), Times.Once);
            Assert.Equal("Executed.", result);
            Assert.Equal(expectedToConsoleOutput, toConsoleOutput);
        }
    }
}
using Control_System.Core.Command;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace TestProject1.Command_Tests
{
    public class BaseCommandCollectionTest
    {
        private class TestCommandCollectionWithStringIdentityType : BaseCommandCollection<string>
        {
            public TestCommandCollectionWithStringIdentityType() : base(new Dictionary<string, Delegate>()) { }
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
        public void VerifyThrowExceptionWhenTryToRegisterWithNullIdentity()
        {
            var commandCollection = new TestCommandCollectionWithStringIdentityType();
            Action tryToRegisterWithNullIdentity = () =>
            {
                commandCollection.Register(null, () => new HelloWorldCommand());
            };
            Assert.Throws<TestCommandCollectionWithStringIdentityType.NullIdentityException>(tryToRegisterWithNullIdentity);
        }

        [Fact]
        public void VerifyThrowExeptionWhenTryToRegisterWithExistedIdentity()
        {
            var commandCollection = new TestCommandCollectionWithStringIdentityType();
            Action tryToRegisterWithExistedIdentity = () =>
            {
                commandCollection.Register("Hello", () => new HelloWorldCommand());
                commandCollection.Register("Hello", () => new HiWorldCommand());
            };
            Assert.Throws<TestCommandCollectionWithStringIdentityType.IdentityException>(tryToRegisterWithExistedIdentity);
        }
        
        [Fact]
        public void VerifyThrowExeptionWhenTryToRegisterNullInstruction()
        {
            var commandCollection = new TestCommandCollectionWithStringIdentityType();

            Action tryToRegisterNullObject = () =>
            {
                commandCollection.Register("Hello", null);
            };
            Assert.Throws<TestCommandCollectionWithStringIdentityType.NullInstructionException>(tryToRegisterNullObject);
        }

        [Fact]
        public void VerifyThrowExeptionWhenTryToRegisterWithAnInstructionNotReturnICommandInstance()
        {
            var commandCollection = new TestCommandCollectionWithStringIdentityType();
            Action tryToRegisterWithAnInstructionNotReturnICommandInstance = () =>
            {
                commandCollection.Register("Try", () => new Object());
            };
            Assert.Throws<TestCommandCollectionWithStringIdentityType.InstructionReturnTypeException>(tryToRegisterWithAnInstructionNotReturnICommandInstance);
        }

        [Fact]
        public void VerifyThrowExeptionWhenTryToRegisterWithAnInstructionThatHasParameter()
        {
            var commandCollection = new TestCommandCollectionWithStringIdentityType();
            Action tryToRegisterWithAnInstructionThatHasParameter = () =>
            {
                Func<string, object, ICommand<string, string>> func = (str, obj) => new HelloWorldCommand();
                commandCollection.Register("Try", func);
            };
            Assert.Throws<TestCommandCollectionWithStringIdentityType.InstructionParameterException>(tryToRegisterWithAnInstructionThatHasParameter);
        }

        [Fact]
        public void VerifyResolveReturnRegisteredInstanceState()
        {
            var commandCollection = new TestCommandCollectionWithStringIdentityType();

            using (var command = new HelloWorldCommand())
            {
                command.Flag = true;
                commandCollection.Register("Hello", () => command);
                command.Flag = false;
            }

            var resolvedCommand = commandCollection.Resolve("Hello");

            Assert.Equal(typeof(HelloWorldCommand), resolvedCommand.GetType());
            Assert.False(((HelloWorldCommand)resolvedCommand).Flag);
        }

        [Fact]
        public void VerifyResolvedCommandExecuteWorkAsExpected()
        {
            var commandCollection = new TestCommandCollectionWithStringIdentityType();
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

            commandCollection.Register(commandIdentity, () => new AnotherHelloWorldCommand(writerMock.Object));
            var command = (ICommand<string, string>)commandCollection.Resolve(commandIdentity);
            var result = command.Execute(strInput);

            writerMock.Verify((writer) => writer.WriteLine(It.IsAny<string>()), Times.Once);
            Assert.Equal("Executed.", result);
            Assert.Equal(expectedToConsoleOutput, toConsoleOutput);
        }
    }
}
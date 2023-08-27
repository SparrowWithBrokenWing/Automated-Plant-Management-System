using Control_System.Core.Command;
using Control_System.Core.Variable;
using Control_System.Core.Executer;
using Moq;
using Xunit.Sdk;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Moq.Protected;
using System.Text.RegularExpressions;

namespace TestProject1.Executer_Tests
{
    public class BaseExecuterTest
    {
        [Fact]
        void VerifyHandleRequestWorkAsExpected()
        {
            var request = "run HelloWorldCommand with hiWorldMessage";
            var consoleOutput = "";
            var consoleOutputExpectation = "Hello world\nHi!\nHelloWorldCommand executed\n";

            // mock the behavior of a Console
            var mockOfTextWriter = new Mock<TextWriter>();
            mockOfTextWriter.Setup((writer) => writer.WriteLine(It.IsAny<string>()))
                .Callback<string>((str) =>
                {
                    consoleOutput += str.ToString() + "\n";
                });

            var commandMock = new Mock<ICommand<string, string>>();
            commandMock
                .Setup((command) => command.Execute(It.IsAny<string>()))
                .Returns<string>((string input) =>
                {
                    mockOfTextWriter.Object.WriteLine("Hello world\n" + input);
                    return "HelloWorldCommand executed";
                });

            //commandMock.Object.Execute("Hi!");

            var mockOfVariableCollection = new Mock<BaseVariableCollection<string>>(new object[] { new Dictionary<string, object>() });
            mockOfVariableCollection
                .Setup((collection) => collection.Register(It.IsAny<string>(), It.IsAny<string>()))
                .CallBase();
            mockOfVariableCollection
                .Setup((collection) => collection.Resolve(It.IsAny<string>()))
                .CallBase();
            var variableIdentity = "hiWorldMessage";
            var variableValue = "Hi!";
            mockOfVariableCollection.Object.Register(variableIdentity, variableValue);

            var mockOfCommandCollection = new Mock<BaseCommandCollection<string>>(new object[] { new Dictionary<string, Delegate>() });
            mockOfCommandCollection
                .Setup((collection) => collection.Register(It.IsAny<string>(), It.IsAny<Delegate>()))
                .CallBase();
            mockOfCommandCollection
                .Setup((collection) => collection.Resolve(It.IsAny<string>()))
                .CallBase();
            var commandIdentity = "HelloWorldCommand";
            var commandConstructionDelegate = () => commandMock.Object;
            mockOfCommandCollection.Object.Register(commandIdentity, commandConstructionDelegate);

            var mockOfBaseExecuter = new Mock<BaseExecuter<string, string, string, string>>(new object[]
            {
                mockOfVariableCollection.Object,
                mockOfCommandCollection.Object
            });
            //
            mockOfBaseExecuter.Protected()
                .Setup<string>("GetSpecifiedCommandIdentity", ItExpr.Is<string>((str) => str == request))
                .Returns<string>((request) =>
                {
                    var requestAnalyzer = new Regex(
                        @"run\s\b(?<commandIdentity>\w+)\b\swith\s\b(?<variableIdentity>\w+)",
                        RegexOptions.Compiled | RegexOptions.ExplicitCapture);
                    var commandIdentity = requestAnalyzer.Match(request).Groups["commandIdentity"].Value;
                    return commandIdentity;
                });
            //
            mockOfBaseExecuter.Protected()
                .Setup<string>("GetSpecifiedVariableIdentity", ItExpr.Is<string>((str) => str == request))
                .Returns<string>((request) =>
                {
                    var requestAnalyzer = new Regex(
                        @"run\s\b(?<commandIdentity>\w+)\b\swith\s\b(?<variableIdentity>\w+)",
                        RegexOptions.Compiled | RegexOptions.ExplicitCapture);
                    var variableIdentity = requestAnalyzer.Match(request).Groups["variableIdentity"].Value;
                    return variableIdentity;
                });
            //
            mockOfBaseExecuter.Protected()
                .Setup("HandleReturnResultOfCommand", ItExpr.Is<string>((str) => str == "HelloWorldCommand executed"))
                .Callback<string?>((str) =>
                {
                    mockOfTextWriter.Object.WriteLine(str);
                });
            //
            mockOfBaseExecuter
                .Setup((executer) => executer.HandleRequest(It.Is<string>((str) => str == request)))
                .CallBase();

            mockOfBaseExecuter.Object.HandleRequest(request);

            Assert.Equal(consoleOutputExpectation, consoleOutput);
        }

    }
}

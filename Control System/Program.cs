using System;
using System.Reflection;

class Program
{
    static void Main()
    {
        Type type = typeof(SomeClass);

        // Create a ParameterModifier with a single true value (for the first parameter)
        ParameterModifier parameterModifier = new ParameterModifier(1);
        parameterModifier[0] = true;

        // Get a specific method by name using the ParameterModifier
        MethodInfo method = type.GetMethod("TestMethod", new Type[] { typeof(string) }, new ParameterModifier[] { parameterModifier });

        // Use the MethodInfo object to invoke the method
        object instance = Activator.CreateInstance(type);
        var test = method.Invoke(instance, new object[] { "Hello, world!" });
    }
}

class SomeClass
{
    public void TestMethod(string message)
    {
        Console.WriteLine(message);
    }
}

using System;

namespace AutofacDemo.BusinessLogic.Implementation
{
    public class HelloWorld : IHelloWorld
    {
        string IHelloWorld.SayHello()
        {
            return $"Hello - time is {DateTime.Now.ToLongTimeString()}";
        }
    }
}

using System;
using Autofac;
using Autofac.Features.ResolveAnything;

namespace AdvancedScenario
{
    class Program
    {
        public interface ICanSpeak
        {
            void Speak();
        }

        public class Person : ICanSpeak
        {
            public void Speak()
            {
                Console.WriteLine("HELLO!");
            }
        }
        
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource());
            
            using var container = builder.Build();
            container.Resolve<Person>().Speak();

        }
    }
}
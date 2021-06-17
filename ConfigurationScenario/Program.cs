using System;
using System.Collections.Generic;
using System.IO;
using Autofac;
using Autofac.Configuration;
using Microsoft.Extensions.Configuration;

namespace ConfigurationScenario
{
    public interface IOperation
    {
        float Calculate(float a, float b);
    }

    public class Addition: IOperation
    {
        public float Calculate(float a, float b)
        {
            return a + b;
        }
    }

    public class Multiplication: IOperation
    {
        public float Calculate(float a, float b)
        {
            return a * b;
        }
    }

    public class CalculationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<Multiplication>().As<IOperation>();
            builder.RegisterType<Addition>().As<IOperation>();
        }
    }
    
    class Program
    {
        static void Main(string[] args)
        {
            var configBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("config.json");
            var configuration = configBuilder.Build();
            var containerBuilder = new ContainerBuilder();
            var configModule = new ConfigurationModule(configuration);
            containerBuilder.RegisterModule(configModule);

            using (var container = containerBuilder.Build())
            {
                float a = 3, b = 4;
                foreach (var op in container.Resolve<IList<IOperation>>())
                {
                    Console.WriteLine($"{op.GetType().Name} of {a} and {b} = {op.Calculate(a, b)}");
                }
            }
        }
    }
}
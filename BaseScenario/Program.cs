using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;

namespace BaseScenario
{
    class Program
    {
        public interface ILog
        {
            void Write(string message);
        }

        public interface IConsole
        {
        }

        public class ConsoleLog : ILog
        {
            public void Write(string message)
            {
                Console.WriteLine(message);
            }
        }

        public class EmailLog : ILog, IConsole
        {
            private const string adminEmail = "someAdmin@some.com";

            public void Write(string message)
            {
                Console.WriteLine($"Email sent to {adminEmail}: {message}");
            }
        }

        public class SMSLog : ILog
        {
            private string phoneNumber;

            public SMSLog(string phoneNumber)
            {
                this.phoneNumber = phoneNumber;
            }

            public void Write(string message)
            {
                Console.WriteLine($"SMS to {phoneNumber} : {message}");
            }
        }

        public class Engine
        {
            private ILog logger;
            private int id;

            public Engine(ILog logger)
            {
                this.logger = logger;
                id = new Random().Next();
            }

            public Engine(ILog logger, int id)
            {
                this.logger = logger;
                this.id = id;
            }

            public void Ahead(int power)
            {
                logger.Write($"Engine [{id}] ahead {power}");
            }
        }

        public class Car
        {
            private ILog logger;
            private Engine engine;

            public Car(Engine engine)
            {
                this.engine = engine;
                this.logger = new EmailLog();
            }

            public Car(Engine engine, ILog logger)
            {
                this.engine = engine;
                this.logger = logger;
            }

            public void Go()
            {
                engine.Ahead(100);
                logger.Write($"Car going forward");
            }
        }

        public class Service
        {
            public string DoSomething(int value)
            {
                return $"I have {value}";
            }
        }

        public class DomainObject
        {
            private Service service;
            private int value;

            public delegate DomainObject Factory(int value);

            public DomainObject(Service service, int value)
            {
                this.service = service;
                this.value = value;
            }

            public override string ToString()
            {
                return service.DoSomething(value);
            }
        }

        internal class Entity
        {
            public delegate Entity Factory();
            private static readonly Random random = new Random();
            private readonly int number;
            public Entity()
            {
                number = random.Next();
            }

            public override string ToString()
            {
                return $"Random: {number}";
            }
        }

        internal class ViewModel
        {
            private readonly Entity.Factory entityFactory;

            public ViewModel(Entity.Factory entityFactory)
            {
                this.entityFactory = entityFactory;
            }

            public void SomeMethod()
            {
                var entity = entityFactory.Invoke();
                Console.WriteLine(entity);
            }
        }

        
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Entity>().InstancePerDependency();
            builder.RegisterType<ViewModel>();

            var container = builder.Build();
            var viewModel = container.Resolve<ViewModel>();
            viewModel.SomeMethod();
            viewModel.SomeMethod();

        }
    }
}
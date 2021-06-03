using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Features.OwnedInstances;
using Module = Autofac.Module;

namespace BaseScenario
{
    class Program
    {
        public interface ILog : IDisposable
        {
            void Write(string message);
        }

        public interface IConsole
        {
        }

        public class ConsoleLog : ILog
        {
            public ConsoleLog()
            {
                Console.WriteLine($"Console logger created at {DateTime.Now.Ticks.ToString()}");
            }
            public void Write(string message)
            {
                Console.WriteLine(message);
            }

            public void Dispose()
            {
                Console.WriteLine("Console logger no longer required");
            }
        }

        public class EmailLog : ILog, IConsole
        {
            private const string adminEmail = "someAdmin@some.com";

            public EmailLog()
            {
                Console.WriteLine($"Email logger created at {DateTime.Now.Ticks.ToString()}");
            }
            
            public void Write(string message)
            {
                Console.WriteLine($"Email sent to {adminEmail}: {message}");
            }
            
            public void Dispose()
            {
                Console.WriteLine("Email logger no longer required");
            }
        }

        public class SMSLog : ILog
        {
            private string phoneNumber;

            public SMSLog(string phoneNumber)
            {
                this.phoneNumber = phoneNumber;
                Console.WriteLine($"SMS logger created at {DateTime.Now.Ticks.ToString()}");
            }

            public void Write(string message)
            {
                Console.WriteLine($"SMS to {phoneNumber} : {message}");
            }
            
            public void Dispose()
            {
                Console.WriteLine("SMS logger no longer required");
            }
        }

        public class Reporting
        {
            private Owned<ConsoleLog> logger;

            public Reporting(Owned<ConsoleLog> logger)
            {
                this.logger = logger;
                Console.WriteLine("Reporting initialized");
            }

            public void ReportOnce()
            {
                logger.Value.Write("Logger started");
                logger.Dispose();
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
        
        class Parent
        {
            public override string ToString()
            {
                return "I am your father";
            }
        }

        class Child
        {
            public string Name { get; set; }
            public Parent Parent { get; set; }

            public void SetParent(Parent parent)
            {
                Parent = parent;
            }
            
        }

        public class ParentChildModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                builder.RegisterType<Parent>();
                builder.Register(c => new Child {Parent = c.Resolve<Parent>()});
            }
        }
        
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleLog>();
            builder.RegisterType<Reporting>();

            using var container = builder.Build();
            container.Resolve<Reporting>().ReportOnce();
            Console.WriteLine("Done reporting");
        }
    }
}
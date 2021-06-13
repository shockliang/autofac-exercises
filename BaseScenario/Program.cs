using System;
using System.Collections.Generic;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Features.Indexed;
using Autofac.Features.Metadata;
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

        public class ConsoleLog : ILog, IDisposable
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

        public class Settings
        {
            public string LogMode { get; set; }
        }
        
        public class Reporting
        {
            private IIndex<string, ILog> loggers;

            public Reporting(IIndex<string, ILog> loggers)
            {
                this.loggers = loggers;
            }

            public void Report()
            {
                loggers["sms"].Write($"Starting sms log..");
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
        
        internal class Parent
        {
            public override string ToString()
            {
                return "I am your father";
            }
        }

        internal class Child
        {
            public string Name { get; set; }
            public Parent Parent { get; set; }

            public Child()
            {
                Console.WriteLine("Child begin created");
            }

            public void SetParent(Parent parent)
            {
                Parent = parent;
            }

            public override string ToString()
            {
                return "Hi there";
            }
        }
        
        internal class BadChild : Child
        {
            public override string ToString()
            {
                return "I hate you";
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

        public interface IResource
        {
            
        }
        
        public class ResourceManager
        {
            public IEnumerable<IResource> Resources { get; set; }

            public ResourceManager(IEnumerable<IResource> resources)
            {
                Resources = resources;
            }
        }

        public class SingletonResource : IResource
        {
            
        }
        
        public class InstancePerDependencyResource: IResource, IDisposable
        {
            public InstancePerDependencyResource()
            {
                Console.WriteLine("Instance per dependency resource created");
            }

            public void Dispose()
            {
                Console.WriteLine("Instance per dependency resource destroyd");
            }
        }
        
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<Parent>();
            builder.RegisterType<Child>()
                .OnActivating(a =>
                {
                    Console.WriteLine("Child activating");
                    // a.Instance.Parent = a.Context.Resolve<Parent>();
                    a.ReplaceInstance(new BadChild());
                })
                .OnActivated(a =>
                {
                    Console.WriteLine("Child activated");
                })
                .OnRelease(a =>
                {
                    Console.WriteLine("Child about to be removed");
                });

            // builder.RegisterType<ConsoleLog>()
            //     .As<ILog>()
            //     .OnActivating(a =>
            //     {
            //         a.ReplaceInstance(new SMSLog("+34213412"));
            //     });
            builder.RegisterType<ConsoleLog>().AsSelf();
            builder.Register<ILog>(c => c.Resolve<ConsoleLog>())
                .OnActivating(a => a.ReplaceInstance(new SMSLog("+142213213")));

            using (var scope = builder.Build().BeginLifetimeScope())
            {
                var child = scope.Resolve<Child>();
                var parent = child.Parent;
                Console.WriteLine(parent);
                Console.WriteLine(child.ToString());

                var logger = scope.Resolve<ILog>();
                logger.Write("Some exception");
            }
        }
    }
}
using System;
using Autofac;

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
        
        public class ConsoleLog: ILog
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

        public class Engine
        {
            private ILog logger;
            private int id;

            public Engine(ILog logger)
            {
                this.logger = logger;
                id = new Random().Next();
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
        
        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            // Both resolved for ILog and IConsole.
            // builder.RegisterType<EmailLog>()
            //     .As<ILog>()
            //     .As<IConsole>()
            //     .AsSelf();
            // builder.RegisterType<ConsoleLog>().As<ILog>().AsSelf().PreserveExistingDefaults();
            var logger = new ConsoleLog();
            builder.RegisterInstance(logger).As<ILog>();
            builder.RegisterType<Engine>();
            builder
                .RegisterType<Car>()
                .UsingConstructor(typeof(Engine));

            var container = builder.Build();
            var car = container.Resolve<Car>();
            car.Go();
        }
    }
}

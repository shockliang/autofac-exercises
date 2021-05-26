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

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();
            builder.RegisterType<ConsoleLog>().As<ILog>();
            // builder.RegisterType<Engine>();
            builder.Register(c => new Engine(c.Resolve<ILog>(), 123));
            builder.RegisterType<Car>();

            var container = builder.Build();
            var car = container.Resolve<Car>();
            car.Go();
        }
    }
}
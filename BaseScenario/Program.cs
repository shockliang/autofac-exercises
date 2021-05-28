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

        static void Main(string[] args)
        {
            var builder = new ContainerBuilder();

            // named parameter
            // builder
            //     .RegisterType<SMSLog>().As<ILog>()
            //     .WithParameter("phoneNumber", "+123456789");

            // typed parameter
            // builder
            //     .RegisterType<SMSLog>().As<ILog>()
            //     .WithParameter(new TypedParameter(typeof(string), "+123456789"));

            // resolved parameter
            // builder
            //     .RegisterType<SMSLog>().As<ILog>()
            //     .WithParameter(new ResolvedParameter(
            //         (pi, ctx) => pi.ParameterType == typeof(string) && pi.Name == "phoneNumber",
            //         (pi, ctx) => "+123456789"));

            var random = new Random();
            builder.Register((c, p)
                => new SMSLog(p.Named<string>("phoneNumber"))).As<ILog>();

            var container = builder.Build();
            var logger = container.Resolve<ILog>(
                new NamedParameter("phoneNumber", random.Next().ToString()));
            logger.Write("Random phone number message");
        }
    }
}
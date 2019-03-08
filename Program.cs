using Nancy.Hosting.Self;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Venereissu_backend
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var host = new NancyHost(new Uri("http://localhost:8080")))
            {
                host.Start();

                Console.WriteLine("NancyFX Stand alone test application.");
                Console.WriteLine("Press enter to exit the application");
                Console.ReadLine();
            }
        }
    }
}

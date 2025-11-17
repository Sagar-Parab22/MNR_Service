using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace EmailService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
#if DEBUG
            // --- This code runs ONLY in Debug mode ---
            EmailSendingService myService = new EmailSendingService();
            try
            {
                // Call the public helper method to start the service's logic
                myService.StartService();

                // Keep the console window open to let the service run
                Console.WriteLine("Service is running in debug mode. Press any key to stop...");
                Console.ReadKey();

                // Call the public helper method to stop the service's logic
                myService.StopService();
            }
            catch (Exception ex)
            {
                // This will catch any error that was crashing your app!
                Console.WriteLine("A fatal error occurred: " + ex.ToString());
                Console.WriteLine("Press any key to exit.");
                Console.ReadKey();
            }
#else
    // --- This is the original code for the installed service ---
    ServiceBase[] ServicesToRun;
    ServicesToRun = new ServiceBase[]
    {
        new EmailSendingService()
    };
    ServiceBase.Run(ServicesToRun);
#endif
        }
    }
}

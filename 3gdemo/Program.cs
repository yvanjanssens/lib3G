using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using _3g;
namespace _3gdemo
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string pn;
                Console.WriteLine("lib3g-demo 1");
                Console.WriteLine("~~~~~~~~~~~~");
                Console.WriteLine("Written by Yvan Janssens");
                Console.WriteLine("\n");
                Console.Write("Enter port name (e.g. COM1): ");

                pn = Console.ReadLine();
                if (pn.Length < 4)
                    epicfail("Invalid port name.");

                Modem.Init(pn);

                Console.Write("Checking pin... ");
                PinStatus pins = Modem.GetPinStatus();
                if (pins == PinStatus.PIN_REQUIRED)
                {
                    Console.WriteLine("Not entered. Asking PIN:");
                    Console.Write("PIN NUMBER: ");
                    string pin = Console.ReadLine();
                    try
                    {
                        PinEntryStatus pes = Modem.EnterPin(pin);
                        if (pes == PinEntryStatus.INVALID_PIN)
                        {
                            epicfail("PIN code incorrect. The application should be restarted.");
                        }
                        else if (pes == PinEntryStatus.OK)
                        {
                            Console.WriteLine("PIN accepted!");
                        }
                    }
                    catch (Exception e)
                    {
                        epicfail(e.Message);
                    }
                }
                else if (pins == PinStatus.OK)
                {
                    Console.Write(" OK");
                }
                else if (pins == PinStatus.PUK_REQUIRED)
                {
                    epicfail("PUK routines not implemented yet.");
                }
                else
                {
                    epicfail("An unknown error has occured while reading out the PIN status.");
                }

                APNSettings apn = Modem.GetAPNSettings();
                if (apn == null)
                {
                    Console.Write("No APN stored in device (yet).\nPlease enter your APN:");
                    string APN = Console.ReadLine();
                    Modem.SetAPN(APNType.IPv4, APN);
                }
                else
                {
                    Console.WriteLine("Current APN: " + apn.APNname);
                    string changeAPN = "";
                    while (changeAPN.ToLower() != "n" || changeAPN.ToLower() != "y")
                    {
                        Console.Write("Send change request? (y/n/Y/N)");
                        changeAPN = Console.ReadLine();

                    }
                    if (changeAPN.ToLower() == "y")
                    {
                        Console.Write("APN name: ");
                        string correctAPN = Console.ReadLine();
                        Modem.SetAPN(APNType.IPv4, correctAPN);
                        Console.WriteLine("APN set.");
                    }

                }
                for (int x = 0; x < 25; x++)
                    Console.WriteLine(" ");

                Console.WriteLine("All settings are set now. Please add a Dial Up connection (if not done already) with the number '*99#'.");
                Console.WriteLine("Then dial the connection created, and enter your ISP's login data.");
                Console.WriteLine(" ");
                Console.WriteLine("You must run this util every time the modem has been unplugged, to set APN/PIN settings.");
                Console.WriteLine("\nPress any key to exit.");
                Console.ReadKey();
            }
            catch (Exception e)
            {
                epicfail(e.Message);
            }
        }

        static void epicfail(string msg)
        {
            fail(msg);
            Console.WriteLine("Due to the above error, you must quit the app. Please close the console window.");
            while (true)
                Console.ReadKey();

            
        }

        static void fail(string msg)
        {

            for (int x = 0; x < 25; x++)
                Console.WriteLine(" ");
            Console.WriteLine();
            Console.WriteLine("Error: " + msg);
        }
    }
}

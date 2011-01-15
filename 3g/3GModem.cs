#define DEBUG

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;



namespace _3g
{
    public static class Modem
    {
        private static SerialPort port;
        public static void Init(string portname)
        {
            port = new SerialPort(portname, 115200, Parity.None, 8, 1);
            port.Open();
            port.WriteLine("ATZ");
            log("ATZ Command send");
            string response = port.ReadExisting();
            if (!response.Contains("OK"))
            {
                log("Response: " + response);
                throw new Exception("ATZ (reset) failed.");
            }
            log("ATI command send");
            port.WriteLine("ATI");
            response = port.ReadExisting();
            string[] arrLines = response.Split("\n");
            log("Response:");
            foreach(string l in arrLines)
                log(l);

            /*
             * Huawei check - currently only tested with Huawei Modem.
             */
            if (!response.ToLower().Contains("huawei"))
            {
#if ONLYHUAWEI
                throw new Exception("Only HUAWEI modems are suported. Recompile wih ONLYHUAWEI commented.");
#endif
                log("Not a HUAWEI device - be careful!");
            }
        }

        public static string DeviceInfo()
        {
            checkOpen();
            return command("ATI");
        }

        public static bool DeviceReset() {
            checkOpen();
            return command("ATZ").Contains("OK");
        }

        public static PinStatus GetPinStatus()
        {
            checkOpen();
            string response = command("AT+CPIN?");
            if (response.Contains("READY"))
                return PinStatus.OK;
            if (response.Contains("SIM PIN"))
                return PinStatus.PIN_REQUIRED;
            if (response.Contains("SIM PUK"))
                return PinStatus.PUK_REQUIRED;
            return PinStatus.ERROR;
        }

        public static void PrepareForDial()
        {
            port.Close();
        }

        public static PinEntryStatus EnterPin(string pin)
        {
            checkOpen();
            if (int.Parse(pin) > 0)
            {
                string response = command("AT+CPIN=\"" + pin + "\"");
                if (response.Contains("OK"))
                {
                    return PinEntryStatus.OK;
                }
                else
                    return PinEntryStatus.INVALID_PIN;
            }
            else
                return PinEntryStatus.ERROR;
            
        }

        public static APNSettings GetAPNSettings(){
            APNSettings apns = new APNSettings();
            string response = command("AT+CGDCONT?");
            response = response.Replace("+CGDCONT:");
            string[] responseComps = response.Split(",");
            if (responseComps.Length < 2)
                return null;
            apns.APNname = responseComps[2].Replace("\"","");
            switch (responseComps[1].Trim().Replace("\"", ""))
            {
                case "PPP":
                    apns.ApnType = APNType.PPP;
                    break;
                case "IPV6":
                    apns.ApnType = APNType.IPv6;
                    break;
                case "IP":
                    apns.ApnType = APNType.IPv4;
                    break;
            }
        }

        private static string command(string cmd)
        {
            checkOpen();
            port.WriteLine(cmd);
            log(cmd + " send.");


            string response = port.ReadLine();

            string[] arrLines = response.Split("\n");
            log("Response:");
            foreach (string l in arrLines)
                log(l);
            return response;
        }

        private static void checkOpen()
        {
            if (port.IsOpen == false)
                throw new Exception("You must (re)Init the Modem first.\nThis should be done every time you connect.");
        }

        private static void log(string msg)
        {
#if DEBUG
            Console.WriteLine("{3G DEBUG} " + msg);
#endif
        }
    }
}

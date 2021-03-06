﻿#define DEBUG
//#define ONLYHUAWEI

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.Threading;


namespace _3g
{
    public static class Modem
    {
        private static SerialPort port;
        public static void Init(string portname)
        {
            port = new SerialPort(portname, 115200, Parity.None, 8, StopBits.One);
            
            port.Open();
            port.WriteLine("ATZ");
            log("ATZ Command send");
            string response = port.ReadExisting();
            if (!response.Contains("ATZ"))
            {
                log("Response: " + response);
                throw new Exception("ATZ (reset) failed.");
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
            if (response.Contains("AT+CPIN?"))
                return PinStatus.OK;
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
            response = response.Replace("+CGDCONT:", "");
            string[] responseComps = response.Split(',');
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
            return apns;
        }

        public static void SetAPN(APNType type ,string apn)
        {
            // Safety measure
            // We don't want our H/W to be exposed to idiots :-S
            apn = apn.Replace("\"", "");
            
            // Defaulting to IP...
            string types = "IP";
            switch (type)
            {
                case APNType.IPv4:
                    types = "IP";
                    break;
                case APNType.IPv6:
                    types = "IPV6";
                    break;
                case APNType.PPP:
                    types = "PPP";
                    break;

            }


            string response = command("AT+CGDCONT=1,\""+ types + "\",\"" + apn + "\"");
            if (!response.Contains("OK"))
                throw new Exception("Could not set APN.");
        }

        private static string command(string cmd)
        {
            checkOpen();
            port.ReadExisting();
            port.WriteLine(cmd);
            log(cmd + " send.");

            Thread.Sleep(250);
            string response = port.ReadExisting();
            log("Response: " + response);

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

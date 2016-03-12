using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Kis
{
    /*
     * Nazwa: DefaultIpAdresses
     * Opis: Pozwala uzyskać domyślne (automatyczne) adresy ip i broadcast.
     * Opis: IPAddress.Any i IPAdress.Broadcast zawsze wskazują na te same dane i nie odzwierciedlają informacji gdzie faktycznie informacja dotrze.
     * Opis: Rozwiązanie znalezione na StackOverflow.
     * Link: http://stackoverflow.com/questions/18551686/how-do-you-get-hosts-broadcast-address-of-the-default-network-adapter-c-sharp
     * Autor: Carlo Arnaboldi, drobna refaktoryzacja: Adrian Pędziwiatr
     */

    public class DefaultIpAdresses
    {
        public static IPAddress GetBroadcastIp()
        {
            IPAddress maskIp = GetHostMask();
            IPAddress hostIp = GetHostIp();

            if (maskIp == null || hostIp == null)
                return null;

            byte[] hostIpBytes = hostIp.GetAddressBytes();
            byte[] maskIpBytes = maskIp.GetAddressBytes();

            byte[] complementedMaskBytes = new byte[hostIpBytes.Length];
            byte[] broadcastIpBytes = new byte[hostIpBytes.Length];

            for (int i = 0; i < hostIpBytes.Length; i++)
            {
                complementedMaskBytes[i] = (byte) ~(maskIpBytes[i]);
                broadcastIpBytes[i] = (byte) (hostIpBytes[i] | complementedMaskBytes[i]);
            }

            return new IPAddress(broadcastIpBytes);
        }

        public static IPAddress GetHostMask()
        {
            NetworkInterface[] allNetworkInterfaces = NetworkInterface.GetAllNetworkInterfaces();

            return (from Interface in allNetworkInterfaces
                let hostIp = GetHostIp()
                let unicastIpInfoCol = Interface.GetIPProperties().UnicastAddresses
                from unicastIpAddressInformation in
                    unicastIpInfoCol.Where(
                        unicastIpAddressInformation =>
                            unicastIpAddressInformation.Address.ToString() == hostIp.ToString())
                select unicastIpAddressInformation.IPv4Mask).FirstOrDefault();
        }

        public static IPAddress GetHostIp()
        {
            return
                (Dns.GetHostEntry(Dns.GetHostName())).AddressList.FirstOrDefault(
                    ip => ip.AddressFamily == AddressFamily.InterNetwork);
        }
    }
}
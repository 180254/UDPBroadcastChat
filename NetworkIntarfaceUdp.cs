using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Kis
{
    /*
     * Nazwa: NetworkInterfaceUdp
     * Opis: Klasa jest wrapperem na typ NetworkInterface.
     * Opis: Kontruktor dodatkowo rozszerza informacje o adres ip i broadcast przypisane do danego interfejsu sieciowego.
     * Opis: Ponadto jest w formie identyfikującej się czytelny sposób w kontrolce select.
     * Autor: Adrian Pędziwiatr
     */

    public class NetworkInterfaceUdp
    {
        public readonly IPAddress IpAddress;
        public readonly IPAddress IpAddressBroadcast;
        public readonly NetworkInterface NetworkInterface;
        public readonly string SecondName;
        public string Name;
        /*
         * Nazwa: NetworkInterfaceUdp (konstruktor).
         * Opis: Konstruuje obiekt na podstawie instancji bazowej klasy NetworkInterface.
         * Opis: Dopisuje jego nazwę, a także znajduje adres ip i broadcast tego interfejsu.
         * Argumenty: networkInterface - instancja interfejsu bazowego
         * Zwraca: nie dotyczy
         * Używa: brak
         * Modyfikuje: NetworkInterface, IpAddress, IpAddressBroadcast, Name
         * Autor: Adrian Pędziwiatr
         */

        public NetworkInterfaceUdp(NetworkInterface networkInterface)
        {
            NetworkInterface = networkInterface;
            IpAddress = GetIpForNetworkInterface(networkInterface);
            Name = networkInterface.Name;

            if (IpAddress != null)
            {
                IPAddress ipv4Mask = GetIPv4MaskForNetworkInterface(networkInterface);
                byte[] ipAddressBytes = IpAddress.GetAddressBytes();
                byte[] ipv4MaskBytes = ipv4Mask.GetAddressBytes();
                byte[] broadcastBytes = new byte[ipv4MaskBytes.Length];
                for (int i = 0; i < ipv4MaskBytes.Length; i++)
                    broadcastBytes[i] = (byte) (ipAddressBytes[i] | ~ipv4MaskBytes[i]);
                IpAddressBroadcast = new IPAddress(broadcastBytes);
            }
        }

        /*
         * Nazwa: NetworkInterfaceUdp (konstruktor).
         * Opis: Konstruuje domyślny obiekt wskazujący na wszystkie dostępne interfejsy sieciowe.
         * Argumenty: brak
         * Zwraca: brak
         * Używa: brak
         * Modyfikuje: IpAddress, 
         * Autor: Adrian Pędziwiatr, IpAddressBroadcast, Name, SecondName
         */

        private NetworkInterfaceUdp()
        {
            IpAddress = IPAddress.Any;
            IpAddressBroadcast = IPAddress.Broadcast;
            Name = "ALL NETWORKS";
            SecondName = "default (auto)";
        }

        /*
         * Nazwa: GetDefaultInstance
         * Opis: Zwraca domyślną instancję interfejsu sieciowego
         * Opis: Konstruuje ją za pomocą prywatnego konstruktora.
         * Argumenty: brak
         * Zwraca: NetworkInterfaceUdp - domyślna instancja interfejsu sieciowego, wskazująca na wszystkie interfejsy sieciowe.
         * Używa: brak
         * Modyfikuje: nie dotyczy - static
         * Autor: Adrian Pędziwiatr
         */

        public static NetworkInterfaceUdp GetDefaultInstance()
        {
            return new NetworkInterfaceUdp();
        }

        /*
         * Nazwa: ToString
         * Opis: Przeciążenie funkcji ToString, które ma zapewnić czytelną nazwę klienta na liście interfejsów sieciowych.
         * Opis: Opisem intefejsu jest jego nazwa.
         * Argumenty: brak
         * Zwraca: Name - czytelny identyfikator interfejsu sieciowego
         * Używa: brak
         * Modyfikuje: brak 
         * Autor: Adrian Pędziwiatr
         */

        public override string ToString()
        {
            return Name;
        }

        /*
         * Nazwa: GetIpForNetworkInterface
         * Opis: Znajduje adres ip dla interfejsu sieciowego.
         * Argumenty: networkInterface - interfejs sieciowy dla którego ma być znaleziony adres ip
         * Zwraca: IPAddress - adres ip
         * Używa: brak
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private static IPAddress GetIpForNetworkInterface(NetworkInterface networkInterface)
        {
            return (
                from ipAddressInformation in networkInterface.GetIPProperties().UnicastAddresses
                where ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork
                select ipAddressInformation.Address)
                .FirstOrDefault();
        }

        /*
         * Nazwa: GetIPv4MaskForNetworkInterface
         * Opis: Znajduje maskę ip dla interfejsu sieciowego.
         * Argumenty: networkInterface - interfejs sieciowy dla którego ma być znaleziona maska ip
         * Zwraca: IPAddress - maska ip
         * Używa: brak
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private static IPAddress GetIPv4MaskForNetworkInterface(NetworkInterface networkInterface)
        {
            return (
                from ipAddressInformation in networkInterface.GetIPProperties().UnicastAddresses
                where ipAddressInformation.Address.AddressFamily == AddressFamily.InterNetwork
                select ipAddressInformation.IPv4Mask)
                .FirstOrDefault();
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;

namespace Kis
{
    /*
     * Nazwa: UdpBroadcast
     * Opis: Klasa reprezentująca broadcasting po UDP. Realizuje połączenia sieciowe, i przekazuje stan do okna MainWindow.
     * Autor: Adrian Pędziwiatr
     */

    public class UdpBroadcast
    {
        private const int ReadBufferSize = 4096;
        private readonly byte[] readBuffer = new byte[ReadBufferSize];
        private readonly MainWindow window;
        private EndPoint listenEndPoint;
        private Socket listenSocket;
        private EndPoint sendEndPoint;
        private Socket sendSocket;
        /*
           * Nazwa: UdpBroadcast (konstruktor)
           * Opis: Konstruktor ustawia referencję do głównego okna, i wstępnie inicjalizuje socket.
           * Argumenty: window - referencja do głównego okna
           * Zwraca: nie dotyczy
           * Używa: brak
           * Modyfikuje: window, (listen|send)socket, (listen|send)endPoint
           * Autor: Adrian Pędziwiatr
           */

        public UdpBroadcast(MainWindow window)
        {
            this.window = window;
            InitializeSockets();
        }

        /*
         * Nazwa: InitializeSocket
         * Opis: Inicjalizuje obiekty typu Socket i EndPoint wstawiająć nową referencję.
         * Argumenty: brak
         * Zwraca: void
         * Używa: klasy Socket
         * Modyfikuje: (listen|send)socket, (listen|send)endPoint
         * Autor: Adrian Pędziwiatr
         */

        private void InitializeSockets()
        {
            listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            listenSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            sendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
            sendSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            listenEndPoint = new IPEndPoint(0, 0);
            sendEndPoint = new IPEndPoint(0, 0);
        }

        /*
         * Nazwa: GetAllListenInterfaces
         * Opis: Funkcja przygotowująca listę interfejsów sieciowych możliwych do nasłuchu w formie możliwiej do wylisowania na kontrolce select.
         * Argumenty: brak
         * Zwraca: NetworkInterfaceUdp[] - tablica interfejsów
         * Używa: klasy NetworkInterface i jej metody statycznej GetAllNetworkInterfaces.
         * Używa: Zwrócone interfejsy zostają opakowane własną klasą NetworkInterfaceUdp
         * Modyfikuje nie dotyczy (static) 
         * Autor: Adrian Pędziwiatr
         */

        public static NetworkInterfaceUdp[] GetAllListenInterfaces()
        {
            List<NetworkInterfaceUdp> networkInterfacesUdpsList = new List<NetworkInterfaceUdp>
            {
                NetworkInterfaceUdp.GetDefaultInstance()
            };

            networkInterfacesUdpsList.AddRange(from networkInterface in NetworkInterface.GetAllNetworkInterfaces()
                where networkInterface.OperationalStatus == OperationalStatus.Up
                select new NetworkInterfaceUdp(networkInterface));

            networkInterfacesUdpsList.RemoveAll(i => i.IpAddress == null);

            return networkInterfacesUdpsList.ToArray();
        }

        /*
         * Nazwa: GetAllListenInterfaces
         * Opis: Funkcja przygotowująca listę interfejsów sieciowych możliwych,
         * Opis: które są poprawne jako interfejsy wysyłające, dla danego interfejsu broadcast,
         * Opis: w  formie możliwiej do wylisowania na kontrolce select.
         * Argumenty: networkInterfaceUdp - interfejs wybrany przez użytkownika
         * Zwraca: NetworkInterfaceUdp[] - tablica interfejsów
         * Używa: klasy NetworkInterface i jej metody statycznej GetAllNetworkInterfaces.
         * Używa: Zwrócone interfejsy zostają opakowane własną klasą NetworkInterfaceUdp
         * Modyfikuje nie dotyczy (static) 
         * Autor: Adrian Pędziwiatr
         */

        public static NetworkInterfaceUdp[] GetAllSendInterfaces(NetworkInterfaceUdp listenInterfaceUdp)
        {
            List<NetworkInterfaceUdp> networkInterfacesUdpsList = new List<NetworkInterfaceUdp>();

            if (listenInterfaceUdp.NetworkInterface == null)
            {
                networkInterfacesUdpsList.AddRange(GetAllListenInterfaces());
                networkInterfacesUdpsList[0].Name = networkInterfacesUdpsList[0].SecondName;
            }
            else
            {
                networkInterfacesUdpsList.Add(listenInterfaceUdp);
            }

            return networkInterfacesUdpsList.ToArray();
        }

        /*
         * Nazwa: UpdateSendSocket
         * Opis: Aktualizuje EndPoint wskazujący na interfejs sieciowy do wysyłania wiadomości broadcast.
         * Argumenty: networkInterfaceUdp - interfejs wybrany przez użytkownika
         * Zwraca: brak
         * Używa: brak
         * Modyfikuje: sendEndPoint
         * Autor: Adrian Pędziwiatr
         */

        public void UpdateSendSocket(NetworkInterfaceUdp networkInterfaceUdp)
        {
            if (networkInterfaceUdp != null)
            {
                sendEndPoint = new IPEndPoint(networkInterfaceUdp.IpAddressBroadcast, ((IPEndPoint) listenEndPoint).Port);
            }
        }

        /*
          * Nazwa: AsyncConnect
          * Opis: Funkcja inicjalizująca asynchroniczne ropoczęcie transmisji broadcast.
          * Argumenty: newtorkInterfaceUdp - interfejs na którym powininna nastąpić transmisja
          * Argumenty: port - nr portu transmisji broadcast
          * Zwraca: void
          * Używa: (listen|send)socket, (listen|send)endPoint
          * Modyfikuje: listenEndPoint
          * Autor: Adrian Pędziwiatr
          */

        public void AsyncConnect(NetworkInterfaceUdp newtorkInterfaceUdp, int port)
        {
            lock (listenSocket)
            {
                lock (sendSocket)
                {
                    lock (listenEndPoint)
                    {
                        lock (sendEndPoint)
                        {
                            InitializeSockets();

                            if (newtorkInterfaceUdp.NetworkInterface == null ||
                                newtorkInterfaceUdp.NetworkInterface.OperationalStatus == OperationalStatus.Up &&
                                newtorkInterfaceUdp.IpAddress != null)
                            {
                                try
                                {
                                    listenEndPoint = new IPEndPoint(newtorkInterfaceUdp.IpAddress, port);

                                    listenSocket.Bind(listenEndPoint);
                                    listenSocket.BeginReceiveFrom(readBuffer, 0, ReadBufferSize, 0, ref listenEndPoint,
                                        AsyncReceiveCallback, null);

                                    UpdateSendSocket(newtorkInterfaceUdp);

                                    window.MsgBindSuccess(newtorkInterfaceUdp.IpAddressBroadcast.ToString(), port);
                                }
                                catch (SocketException ex)
                                {
                                    window.MsgBindError(ex.Message);
                                    Console.WriteLine(ex.SocketErrorCode);
                                }
                            }
                            else
                            {
                                window.MsgBindErrorNetworkIsDown();
                            }
                        }
                    }
                }
            }
        }

        /*
         * Nazwa: AsyncReceiveCallback
         * Opis: Callback wykonywany, gdy na nasłuchiwanym intefejsie pojawi się wiadomość broadcast.
         * Opis: Informuje okno główne o otrzymaniu wiadomości.
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego. 
         * Zwraca: void
         * Używa: listenSocket, listenEndPoint, readBuffer
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncReceiveCallback(IAsyncResult ar)
        {
            lock (listenSocket)
            {
                lock (listenEndPoint)
                {
                    int read;
                    EndPoint sender = new IPEndPoint(0, 0);

                    try
                    {
                        read = listenSocket.EndReceiveFrom(ar, ref (sender));
                    }
                    catch (ObjectDisposedException)
                    {
                        read = 0;
                    }


                    if (read > 0)
                    {
                        string rcvedText = Encoding.UTF8.GetString(readBuffer, 0, read);
                        listenSocket.BeginReceiveFrom(readBuffer, 0, ReadBufferSize, 0, ref listenEndPoint,
                            AsyncReceiveCallback, null);
                        window.MsgReceived(((IPEndPoint) sender).Address.ToString(), rcvedText);
                    }
                }
            }
        }

        /*
         * Nazwa: AsyncSendData
         * Opis: Funkcja rozpoczyna wysyłanie wiadomości broadcast w formie tekstowej.
         * Argumenty: data - string do wysłania.
         * Zwraca: void
         * Używa: sendSocket, sendEndPoint
         * Modyfikuje: nie
         * Autor: Adrian Pędziwiatr
         */

        public void AsyncSendData(string msg)
        {
            if (msg.Length == 0)
            {
                return;
            }


            lock (sendSocket)
            {
                lock (sendEndPoint)
                {
                    byte[] dataBytes = Encoding.UTF8.GetBytes(msg);
                    sendSocket.BeginSendTo(dataBytes, 0, dataBytes.Length, 0, sendEndPoint, AsyncSendDataCallback, null);
                }
            }
        }

        /*
         * Nazwa: AsyncSendDataCallback
         * Opis: Callback dla funkcji AsyncSendData. Wywołana zostanie po zrealizowaniu wysyłania.
         * Opis: Kończy proces wysyłania.
         * Argumenty: IAsyncResult ar - stan wysyłania asynchronicznego. 
         * Zwraca: void
         * Używa: sendSocket
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        private void AsyncSendDataCallback(IAsyncResult ar)
        {
            lock (sendSocket)
            {
                sendSocket.EndSendTo(ar);
            }
        }

        /*
         * Nazwa: AsyncUnbind
         * Opis: Funkcja kończąca nasłuch broadcast.
         * Argumenty: brak
         * Zwraca: void
         * Używa: (listen|send)socket
         * Modyfikuje: brak
         * Autor: Adrian Pędziwiatr
         */

        public void AsyncUnbind()
        {
            lock (listenSocket)
            {
                lock (sendSocket)
                {
                    lock (listenEndPoint)
                    {
                        lock (sendEndPoint)
                        {
                            listenSocket.Close();
                            sendSocket.Close();
                            window.MsgUnbind();
                        }
                    }
                }
            }
        }
    }
}
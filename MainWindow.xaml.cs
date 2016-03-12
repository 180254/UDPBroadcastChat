using System;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Kis
{
    /*
     * Nazwa: MainWindow
     * Opis: Klasa reprezentująca okno główne programu, odpowiedzi na zdarzenia z niego pochodzące.
     * Autor: Adrian Pędziwiatr
     */

    public partial class MainWindow
    {
        private readonly ObservableCollection<NetworkInterfaceUdp> listenInterfaceBoxElements =
            new ObservableCollection<NetworkInterfaceUdp>();

        private readonly ObservableCollection<NetworkInterfaceUdp> sendInterfaceBoxElements =
            new ObservableCollection<NetworkInterfaceUdp>();

        private readonly UdpBroadcast udpBroadcast;
        /*
         * Nazwa: MainWindow (konstruktor).
         * Opis: Konstruktor inicjalizuje obiekt UdpBroadcast, do którego będzie zlecał zadania sieciowe,
         * Opis: ustawia stan okna na "rozłączony", oraz inicjalizuje wybór interfejsów sieciowych.
         * Argumenty: nie dotyczy
         * Zwraca: nie dotyczy
         * Używa: nic
         * Modyfikuje: udpBroadcast 
         * Autor: Adrian Pędziwiatr
         */

        public MainWindow()
        {
            udpBroadcast = new UdpBroadcast(this);
            InitializeComponent();
            SetWindowStateAsStopped();
            InitInterfacesBox();
        }

        /*
         * Nazwa: SetWindowStateAsStopped
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada braku komunikacji.
         * Argumenty: brak
         * Zwraca: void
         * Używa: nic
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Modyfikuje: Listę interfejsów sieciowych, które mogą być użyte do nasłuchu broadcast.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowStateAsStopped()
        {
            UpdateListenInterfaceBox();

            StartButton.IsEnabled = true;
            StopButton.IsEnabled = false;
            SenderNameBox.IsEnabled = false;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            ListenInterfaceBox.IsEnabled = true;
            SendInterfaceBox.IsEnabled = false;
            PortBox.IsEnabled = true;
            ListenInterfaceBox.Focus();
        }

        /*
         * Nazwa: SetWindowWstateAsAwaiting
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada oczekiwaniu rozpoczęście komunikacji.
         * Argumenty: brak
         * Zwraca: void
         * Używa: nic
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowWstateAsAwaiting()
        {
            StartButton.IsEnabled = false;
            StopButton.IsEnabled = false;
            SenderNameBox.IsEnabled = false;
            MessageBox.IsEnabled = false;
            SendButton.IsEnabled = false;
            ListenInterfaceBox.IsEnabled = false;
            SendInterfaceBox.IsEnabled = false;
            PortBox.IsEnabled = false;
        }

        /*
         * Nazwa: SetWindowStateAsStarted
         * Opis: Ustawia stan wszystkich kontrolek na oknie, na taki, który odpowiada działającej komunikacji.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek - włączone, bądź nie.
         * Modyfikuje: Listę interfejsów sieciowych, które mogą być użyte do wysłania wiadomości broadcast.
         * Autor: Adrian Pędziwiatr
         */

        private void SetWindowStateAsStarted()
        {
            UpdateSendInterfaceBox();

            StartButton.IsEnabled = false;
            StopButton.IsEnabled = true;
            SenderNameBox.IsEnabled = true;
            MessageBox.IsEnabled = true;
            SendButton.IsEnabled = true;
            ListenInterfaceBox.IsEnabled = false;
            SendInterfaceBox.IsEnabled = true;
            PortBox.IsEnabled = false;
            SenderNameBox.Focus();
        }

        /*
         * Nazwa: InitInterfacesBox
         * Opis: Inicjalizuje listy interfejsów sieciowych. Ustawia odpowiednie źrodło elementów.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: źródła SendInterfaceBox i ListenInterfaceBox
         * Autor: Adrian Pędziwiatr
         */

        private void InitInterfacesBox()
        {
            SendInterfaceBox.Items.Clear();
            SendInterfaceBox.ItemsSource = sendInterfaceBoxElements;
            ListenInterfaceBox.Items.Clear();
            ListenInterfaceBox.ItemsSource = listenInterfaceBoxElements;
        }

        /*
         * Nazwa: UpdateListenInterfaceBox
         * Opis: Odświeża listę intefejsów sieciowych możliwych do nasłuchu.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Zasób listowy, i stan kontrolki ListenInterfaceBox. 
         * Autor: Adrian Pędziwiatr
         */

        private void UpdateListenInterfaceBox()
        {
            listenInterfaceBoxElements.Clear();
            foreach (NetworkInterfaceUdp network in UdpBroadcast.GetAllListenInterfaces())
            {
                listenInterfaceBoxElements.Add(network);
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Loaded,
                new Action(() => { ListenInterfaceBox.SelectedIndex = 0; }));
        }

        /*
         * Nazwa: UpdateListenInterfaceBox
         * Opis: Odświeża listę intefejsów sieciowych możliwych do wysłania wiadomości.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Zasób listowy, i stan kontrolki SendInterfaceBox. 
         * Autor: Adrian Pędziwiatr
         */

        public void UpdateSendInterfaceBox()
        {
            sendInterfaceBoxElements.Clear();
            foreach (
                NetworkInterfaceUdp network in
                    UdpBroadcast.GetAllSendInterfaces((NetworkInterfaceUdp) ListenInterfaceBox.SelectedItem))
            {
                sendInterfaceBoxElements.Add(network);
            }
            SendInterfaceBox.SelectedIndex = 0;
        }

        /*
         * Nazwa: LimitTextTo2048
         * Opis: Funkcja pomocnicza, który obcina tekst do 1024 znaków.
         * Opis: Zakładamy, że nie istnieje potrzeba wyświetlać naraz więcej informacji,
         * Opis: a doświadczanie zauważyłem, że zbyt duża ilośc informacji naraz przychodzącej,
         * Opis: znacząco przywiesza okno.
         * Argumenty: text - tekst do przycięcia
         * Zwraca: string - przycięty tekst
         * Używa: nie dotyczy (static)
         * Modyfikuje: nie dotyczy (static) 
         * Autor: Adrian Pędziwiatr
         */

        public static string LimitTextTo2048(string text)
        {
            const int lenLimit = 2048;
            return (text.Length <= lenLimit) ? text : text.Substring(text.Length - lenLimit, lenLimit);
        }

        /*
         * Nazwa: GetCurrentTimeAsString
         * Opis: Funkcja pomocnicza zwracająca aktualną datę w formie stringa.
         * Argumenty: brak
         * Zwraca: string - aktualna data, wraz z czasem
         * Używa: nie dotyczy (static) 
         * Modyfikuje: nie dotyczy (static) 
         * Autor: Adrian Pędziwiatr
         */

        public static string GetCurrentTimeAsString()
        {
            return "(" + DateTime.Now.ToString("H':'mm':'ss'.'ffffff") + ")";
        }

        /*
          * Nazwa: AppendMsg
          * Opis: Funkcja dodająca otrzymaną wiadomość do kontrolki z logiem.
          * Opis: Informacja zostaje dodana na końcu.
          * Argumenty: ip - adres ip źródła
          * Argumenty: msg - dodawana wiadomość
          * Zwraca: void
          * Używa: brak
          * Modyfikuje: Zawartość kontrolki LogBox.
          * Autor: Adrian Pędziwiatr
          */

        private void AppendMsg(string ip, string msg)
        {
            string newText = LimitTextTo2048(LogBox.Text
                                             + GetCurrentTimeAsString()
                                             + " /" + ip + "/ " + msg + "\n");
            LogBox.Text = newText;
            LogBox.ScrollToEnd();
        }

        /*
         * Nazwa: AppendServerState
         * Opis: Funkcja dododanie do kontrolki z logiem serwera informację o stanie nasłuchu.
         * Opis: Informacja zostaje dodana na końcu.
         * Argumenty: msg - dodawana treść stanu
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Zawartość kontrolki LogBox.
         * Autor: Adrian Pędziwiatr
         */

        private void AppendServerState(string msg)
        {
            string newText = LimitTextTo2048(LogBox.Text + GetCurrentTimeAsString() + " Log: " + msg + "\n");
            LogBox.Text = newText;
            LogBox.ScrollToEnd();
        }

        /*
         * Nazwa: StartButton_OnClick
         * Opis: Funkcja wywoływana po kliknięciu przyciku "Start".
         * Opis: Funkcja rozpoczyna próbę nasłuchu broadcast. Zleca to zadanie przy użyciu obiektu UdpBroadcast.
         * Opis: Jeżeli wpisane dane nie pozwalają na rozpoczęcie połączenia natychmniastowo loguje taką informację.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: Zawartości kontrolek z wyborem interfejsu sieciowego i portu nasłuchu. 
         * Modyfikuje: Zawartość kontrolki z LogBox.
         * Autor: Adrian Pędziwiatr
         */

        public void StartButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowWstateAsAwaiting();

            try
            {
                NetworkInterfaceUdp network = ((NetworkInterfaceUdp) ListenInterfaceBox.SelectedItem);
                int port = Int32.Parse(PortBox.Text);
                udpBroadcast.AsyncConnect(network, port);
            }
            catch (FormatException)
            {
                AppendServerState("Connection Error. Written port is not proper whole number.");
                SetWindowStateAsStopped();
            }
        }

        /*
         * Nazwa: PortBox_OnKeyDown
         * Opis: Funkcja wywoływana po każdorazowym kliknięciu klawisza w polu z numerem portu.
         * Opis: W praktyce wykonuje jakiekolwiek zadania jedynie jeżeli klawiszem tym był enter.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "Start".
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void PortBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                StartButton_OnClick(null, null);
            }
        }

        /*
         * Nazwa: StopButton_OnClick
         * Opis: Funkcja wywoływana po kliknięciu przycisku "Stop" realizującego zakończenie nasłuchu broadcast.
         * Opis: Ustawia stan okna na oczekujący. Zleca rozłączenie z serwerem obiektowi UdpBroadcast
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: obiekt UdpBroadcast
         * Modyfikuje: Stan kontrolek w oknie.
         * Autor: Adrian Pędziwiatr
         */

        private void StopButton_OnClick(object sender, RoutedEventArgs e)
        {
            SetWindowWstateAsAwaiting();
            udpBroadcast.AsyncUnbind();
        }

        /*
         * Nazwa: MainWindow_OnClosed
         * Opis: Funkcja wywoływana przy zamykaniu okna.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "Stop".
         * Opis: Jeżeli zostało zamknięte okno najpierw poprawnie wyłączany jest nasłuch.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: nie dotyczy - "symuluje inne zdarzenie"
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            StopButton_OnClick(null, null);
        }

        /*
         * Nazwa: SendButton_OnClick
         * Opis: Funkcja wywoływania po kliknięciu klawisza "Send" realizującego żadanie wysłania tekstu na adres broadcast.
         * Opis: zlecenia wysłanie tekstu obiektowi UdpBroadcast
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: obiekt UdpBroadcast
         * Modyfikuje: Stan kontrolek w oknie - kasuje pole do wpisywania tekstu.
         * Autor: Adrian Pędziwiatr
         */

        private void SendButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SenderNameBox.Text != "" && MessageBox.Text != "")
            {
                udpBroadcast.AsyncSendData(SenderNameBox.Text + ": " + MessageBox.Text);
                MessageBox.Text = "";
            }
        }

        /*
         * Nazwa: MessageBox_OnKeyDown
         * Opis: Funkcja wywoływana po każdorazowym kliknięciu klawisza w polu z wysyłanym tekstem.
         * Opis: W praktyce wykonuje jakiekolwiek zadania jedynie jeżeli klawiszem tym był enter.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "Send".
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void MessageBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {
                SendButton_OnClick(null, null);
            }
        }

        /*
         * Nazwa: SenderNameBox_OnKeyDown
         * Opis: Funkcja wywoływana po każdorazowym kliknięciu klawisza w polu z nazwą wysyłającego.
         * Opis: W praktyce wykonuje jakiekolwiek zadania jedynie jeżeli klawiszem tym był enter.
         * Opis: Kliknięcie klawisza enter powoduje wykonanie zadania wskazanego przez klawisz "Send".
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: nie dotyczy - "symuluje inne zdarzenie"
         * Modyfikuje: nie dotyczy - "symuluje inne zdarzenie"
         * Autor: Adrian Pędziwiatr
         */

        private void SenderNameBox_OnKeyDown(object sender, KeyEventArgs e)
        {
            MessageBox_OnKeyDown(null, e);
        }

        /*
         * Nazwa: SendInterfaceBox_OnSelectionChanged
         * Opis: Funkcja wywoływana w momencie zdarzenia zmiany wyboru interfejsu sieciowego,
         * Opis: przy użyciu którego ma nastąpić wysłanie wiadomości broadcast.
         * Opis: Jest to informacja istotna dla obiektu UdpBroadcast, zostaje wykonane odpowiednie powiadomienie.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: kontrolki SendInterfaceBox - sprawdza jaki wybrany został interfejs sieciowy
         * Modyfikuje: braks
         * Autor: Adrian Pędziwiatr
         */

        private void SendInterfaceBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NetworkInterfaceUdp network = SendInterfaceBox.SelectedItem as NetworkInterfaceUdp;
            if (network != null)
            {
                udpBroadcast.UpdateSendSocket(network);
                MsgDeliveryBroadcastChanged(network.IpAddress.ToString(), network.IpAddressBroadcast.ToString());
            }
        }

        /* 
         * Nazwa: MsgBindSuccess
         * Opis: Funkcja, która realiuje zadania odpowiednie dla poprawnego ustanowienia nasłuchu.
         * Opis: Dopisuje informacje do loga, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt UdpBroadcast jako informacja o stanie serwera.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna. 
         * Autor: Adrian Pędziwiatr
        */

        public void MsgBindSuccess(string host, int port)
        {
            Dispatcher.Invoke(() =>
            {
                AppendServerState("Msgs will be received from " + host + ":" + port + ".");
                SetWindowStateAsStarted();
            });
        }

        /* 
         * Nazwa: MsgDeliveryBroadcastChanged
         * Opis: Zapisuje w logu informację o zmianie interfejsu sieciowego, na który będą wysyłane wiadomości broadcast.
         * Argumenty ip - adres ip wybranego interfejsu sieciowego, broadcast - adres broadcast wybranego interfejsu sieciowego
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna. 
         * Autor: Adrian Pędziwiatr
        */

        public void MsgDeliveryBroadcastChanged(string ip, string broadcast)
        {
            if (ip == IPAddress.Any.ToString())
            {
                ip = DefaultIpAdresses.GetHostIp().ToString();
                broadcast = DefaultIpAdresses.GetBroadcastIp().ToString();
            }

            Dispatcher.Invoke(
                () =>
                    AppendServerState("Msgs will be broadcasted to " + broadcast + ":" + PortBox.Text + " as " + ip +
                                      "."));
        }

        /*
         * Nazwa: MsgBindError
         * Opis: Funkcja, która realiuje zadania odpowiednie dla braku realizacji nasłuchu.
         * Opis: Dopisuje informacje do loga - powód błedu, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgBindError(string msg)
        {
            Dispatcher.Invoke(() =>
            {
                AppendServerState("Connection error. " + msg);
                SetWindowStateAsStopped();
            });
        }

        /*
         * Nazwa: MsgBindErrorNetworkIsDown
         * Opis: Funkcja, która realiuje zadania odpowiednie dla braku realizacji nasłuchu.
         * Opis: Jest ona przeznaczona dla przypadku specyficznego - wybrany interfejs sieciowy nie jest podłączony.
         * Opis: Dopisuje informacje do loga - powód błedu, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt UdpBroadcast jako informacja o stanie serwera.
         * Argumenty: sender - obiekt wywołujący zdarzenie, e - informacje o okoliczności zdarzenia
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgBindErrorNetworkIsDown()
        {
            Dispatcher.Invoke(() =>
            {
                AppendServerState("Connection error. Selected network interface is down.");
                SetWindowStateAsStopped();
            });
        }

        /*
         * Nazwa: MsgReceived
         * Opis: Funkcja, która realiuje zadania odpowiednie dla odbioru danych na nasłuchiwanym adresie broadcast.
         * Opis: Dopisuje odebrany tekst do stosownej kontrolki, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt UdpBroadcast jako informacja o stanie nasłuchu.
         * Argumenty: data - odebrany tekst
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgReceived(string ip, string data)
        {
            Dispatcher.Invoke(() => AppendMsg(ip, data));
        }

        /*
         * Nazwa: MsgUnbind
         * Opis: Funkcja, która realiuje zadania odpowiednie dla zakończenia nasłuchu.
         * Opis: Dopisuje informacje do loga, ustawia odpowiedni stan okna.
         * Opis: Funkcja jest wywołyana przez obiekt TcpServer jako informacja o stanie serwera.
         * Argumenty: brak
         * Zwraca: void
         * Używa: brak
         * Modyfikuje: Stan kontrolek okna.
         * Autor: Adrian Pędziwiatr
         */

        public void MsgUnbind()
        {
            Dispatcher.Invoke(() =>
            {
                AppendServerState("Successfully stopped.");
                SetWindowStateAsStopped();
            });
        }
    }
}
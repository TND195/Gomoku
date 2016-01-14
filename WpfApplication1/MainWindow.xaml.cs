using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Quobject.SocketIoClientDotNet.Client;
using System.Configuration;
namespace WpfApplication1
{
    public partial class MainWindow : Window
    {
        BanCo banco;
        public int UI=0;
        public static int rowngaunhien, colngaunhien;
        Node node = new Node();
        public static TextBox txt = new TextBox();
        public static TextBox txtMay = new TextBox();
        public static TextBox txtNguoiMay = new TextBox();
        public static string message;
        public static int dem = 0;
        public static int dem1 = 0;
        public static string name;
        public static bool newgame=false;
        public static bool newgame1 = false;
        public static Socket socket ;
        public MainWindow()
        {
            
            this.InitializeComponent();
            banco=new BanCo(this,grdBanCo);
            BanCo.Option.WhoPlayWith = Player.MayOnline;
			banco.DrawGomokuBoard();
            grdBanCo.MouseDown += new System.Windows.Input.MouseButtonEventHandler(banco.grdBanCo_MouseDown);
            BanCo.WinEvent += new BanCo.WinEventHander(banco_WinEvent);
            cbCheDo.Items.Add("Human vs Human");
            cbCheDo.Items.Add("Human vs AI");
            cbCheDo.Items.Add("Human vs Human Online");
            cbCheDo.Items.Add("Human vs AI online");
            cbCheDo.SelectedIndex = 0;
            txt.TextChanged += new TextChangedEventHandler(txt_change);
            txtMay.TextChanged += new TextChangedEventHandler(txtMay_Change);
            txtNguoiMay.TextChanged += new TextChangedEventHandler(txtNguoiMay_Change);
            btnstart.IsEnabled = false;
            btnSend.IsEnabled = false;
        }
      
        private void txtNguoiMay_Change(object sender, TextChangedEventArgs e)
        {

            BanCo.Option.WhoPlayWith = Player.MayOnline;
          


            if (BanCo.Option.WhoPlayWith == Player.MayOnline)
            {
                if (BanCo.currPlayer == Player.Com )//Nếu lượt đi là máy và trận đấu chưa kết thúc
                {

                    //Tìm đường đi cho máy

                    BanCo.DiNgauNhien();
                    connect.rw1 = rowngaunhien;
                    connect.cl1 = colngaunhien;
                    connect.guitoado(socket, rowngaunhien,colngaunhien);
                    BanCo.currPlayer = Player.Human;
                   
                }
            }
        }

        private void txtMay_Change(object sender, TextChangedEventArgs e)
        {
            if (BanCo.Option.WhoPlayWith == Player.MayOnline)
            {

                
                if (BanCo.currPlayer == Player.Human && BanCo.end == Player.None)
                {

                    BanCo.board[BanCo.rows, BanCo.columns] = BanCo.currPlayer;//Lưu loại cờ vừa đánh vào mảng
                    BanCo.DrawDataBoard(BanCo.rows, BanCo.columns, true, true);
                    BanCo.end = BanCo.CheckEnd(BanCo.rows, BanCo.columns);//Kiểm tra xem trận đấu kết thúc chưa

                    if (BanCo.end == Player.Human)//Nếu người chơi 2 thắng
                    {
                        BanCo.OnWin();//Khai báo sư kiện Win
                        BanCo.OnWinOrLose();//Hiển thị 5 ô Win.
                 
                        newgame = true;
                    }
                    else
                    {
                        BanCo.currPlayer = Player.Com;//Thiết lập lại lượt chơi
                        BanCo.OnComDanhXong();// Khai báo sự kiện người chơi 2 đánh xong
                    }
                }
                if (BanCo.currPlayer == Player.Com && BanCo.end == Player.None)//Nếu lượt đi là máy và trận đấu chưa kết thúc
                {

                    //Tìm đường đi cho máy

                    BanCo.eBoard.ResetBoard();
                    BanCo.LuongGia(Player.Com);//Lượng giá bàn cờ cho máy
                    node = BanCo.eBoard.GetMaxNode();//lưu vị trí máy sẽ đánh
                    int r, c;
                    r = node.Row; c = node.Column;
                    connect.rw1 = r;
                    connect.cl1 = c;
                    BanCo.board[r, c] = BanCo.currPlayer; //Lưu loại cờ vừa đánh vào mảng
                    BanCo.DrawDataBoard(r, c, true, true); //Vẽ con cờ theo lượt chơi
                    connect.guitoado(socket, r, c);
                    BanCo.end = BanCo.CheckEnd(r, c);//Kiểm tra xem trận đấu kết thúc chưa

                    if (BanCo.end == Player.Com)//Nếu máy thắng
                    {
                        BanCo.OnLose();//Khai báo sư kiện Lose
                        BanCo.OnWinOrLose();//Hiển thị 5 ô Lose.
                    
                        newgame = true;
                    }
                    else if (BanCo.end == Player.None)
                    {
                        BanCo.currPlayer = Player.Human;//Thiết lập lại lượt chơi
                        BanCo.OnComDanhXong();// Khai báo sự kiện người đánh xong
                    }
                }
            }
        }
         private void txt_change(object sender, TextChangedEventArgs e)
        {
            if(BanCo.currPlayer==Player.Online && BanCo.end==Player.None)
            {
                BanCo.board[BanCo.rows, BanCo.columns] = BanCo.currPlayer;//Lưu loại cờ vừa đánh vào mảng
                BanCo.DrawDataBoard(BanCo.rows, BanCo.columns, true, true);
                BanCo.end = BanCo.CheckEnd(BanCo.rows, BanCo.columns);//Kiểm tra xem trận đấu kết thúc chưa

                if (BanCo.end == Player.Online)//Nếu người chơi 2 thắng
                {
                    BanCo.OnWin();//Khai báo sư kiện Win
                    BanCo.OnWinOrLose();//Hiển thị 5 ô Win.
                    ChatMessage chatMessage = new ChatMessage("Server", DateTime.Now.ToString("hh:mm:ss tt"), name+ " là người thắng");
                    chatBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                    chatBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                    chatBox.Items.Add(chatMessage);
                    btnstart.Content = "New game";
                    newgame1 = true;
                }
                else
                {
                    BanCo.currPlayer = Player.Human;//Thiết lập lại lượt chơi
                    BanCo.OnComDanhXong();// Khai báo sự kiện người chơi 2 đánh xong
                }
                    
               

            }
        }

        private void banco_WinEvent()
        {

            if (banco.End == Player.Human && BanCo.Option.WhoPlayWith == Player.Online)
            {
                string temp2 = txtYourName.Text + " won the game";

                ChatMessage chatMessage = new ChatMessage("Server", DateTime.Now.ToString("hh:mm:ss tt"), temp2);
                chatBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                chatBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                chatBox.Items.Add(chatMessage);
                dem = 1;
                dem1 = 1;
                UI = 0;
            }
            if (banco.End == Player.Online && BanCo.Option.WhoPlayWith == Player.Online)
            {
                string temp2 = MainWindow.name +" won the game" ;

                ChatMessage chatMessage = new ChatMessage("Server", DateTime.Now.ToString("hh:mm:ss tt"), temp2);
                chatBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                chatBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                chatBox.Items.Add(chatMessage);
                dem = 1;
                dem1 = 1;
                UI = 0;
            }
            if (banco.End == Player.Human && BanCo.Option.WhoPlayWith == Player.MayOnline)
            {
                string temp2 = MainWindow.name + " won the game";
                ChatMessage chatMessage = new ChatMessage("Server", DateTime.Now.ToString("hh:mm:ss tt"), temp2);
                chatBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                chatBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                chatBox.Items.Add(chatMessage);
                dem = 1;
                dem1 = 1;
                UI = 0;
            }
            newgame = true;
            newgame1 = true;
        }
        public int test;
        private void btnPlayerAgain_Click(object sender, RoutedEventArgs e)
        {
            banco.PlayAgain();
        }
        private void txtYourName_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtYourName.Text == "")
            {
                txtYourName.Text = "Guest";
            }
        }
        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
  
                connect.sendmessage(socket, txtYourName.Text, txtMessage.Text);
         
        }
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            connect.changname(socket, txtYourName.Text);
          
        }
        private void Message_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (MainWindow.name != null)
            {
                ChatMessage chatMessage = new ChatMessage(MainWindow.name, DateTime.Now.ToString("hh:mm:ss tt"), message);
                chatBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                chatBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                chatBox.Items.Add(chatMessage);
            }
            else
            {
                ChatMessage chatMessage = new ChatMessage("Server", DateTime.Now.ToString("hh:mm:ss tt"), message);
                chatBox.VerticalAlignment = System.Windows.VerticalAlignment.Top;
                chatBox.HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch;
                chatBox.Items.Add(chatMessage);
            }
        }

        private void Chonchedo_Click(object sender, RoutedEventArgs e)
        {
            if (cbCheDo.Text == "Human vs Human")
            {
                btnstart.IsEnabled = false;
                btnSend.IsEnabled = false;
                banco.NewGame();
                BanCo.Option.WhoPlayWith = Player.Human;
                test = 1;
            
            }
            if (cbCheDo.Text == "Human vs AI")
            {
                btnstart.IsEnabled = false;
                btnSend.IsEnabled = false;
                banco.NewGame();
                BanCo.Option.WhoPlayWith = Player.Com;
                test = 0;
                
            }
            if(cbCheDo.Text == "Human vs Human Online")
            {
                btnstart.IsEnabled = true;
                btnSend.IsEnabled = true;
                UI = 1;
                if (dem == 0)
                {
                    socket = IO.Socket(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
                
                   
                    BanCo.Option.WhoPlayWith = Player.Online;

                    BanCo.currPlayer = Player.Online;
                    connect.connected(socket, txtYourName.Text.ToString());
                    dem++;
                }
              
                if (newgame1 == true)
                {

                    banco.PlayAgain();
                    socket = IO.Socket(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
                    BanCo.Option.WhoPlayWith = Player.Online;
                    BanCo.currPlayer = Player.Online;
                    connect.connected(socket, txtYourName.Text.ToString());
                    connect.rw = -1;
                    connect.cl = -1;
                    newgame1 = false;
                }
            }
            if (cbCheDo.Text == "Human vs AI online")
            {
                btnstart.IsEnabled = true;
                btnSend.IsEnabled = true;
                UI = 1;
                if (dem1 == 0)
                {
                    socket = IO.Socket(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
    
 
                    BanCo.Option.WhoPlayWith = Player.MayOnline;
                    BanCo.currPlayer = Player.Human;
                    connect.connected(socket, txtYourName.Text.ToString());
                    dem1++;
                }
                else
                {

                    connect.changname(socket, txtYourName.Text);

                }
                if (newgame == true)
                {
                    banco.PlayAgain();
                    socket = IO.Socket(ConfigurationManager.ConnectionStrings["Conn"].ConnectionString);
            
 
                    BanCo.Option.WhoPlayWith = Player.MayOnline;
                    BanCo.currPlayer = Player.Human;
                    connect.connected(socket, txtYourName.Text.ToString());
                    connect.rw1 = -1;
                    connect.cl1 = -1;
                    newgame = false;
                }
            }
        }
        }
    }

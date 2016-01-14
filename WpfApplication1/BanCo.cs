using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Threading;
using System.IO;
using System.Windows.Media.Imaging;
using System.ComponentModel;
using System.Threading;

namespace WpfApplication1
{
    enum Player
    {
        None = 0,
        Human = 1,
        Com = 2,
        Online = 3,
        MayOnline=4,
    }
    struct Node
    {
        public int Row;
        public int Column;
        public Node(int rw, int cl)
        {
            this.Row = rw;
            this.Column = cl;
        }
    }

    class BanCo
    {
        private readonly BackgroundWorker worker = new BackgroundWorker();
        //Các biến chính
        public static int row, column; //Số hàng, cột
        private const int length = 45;//Độ dài mỗi ô
        public static Player currPlayer; //lượt đi
        public static Player[,] board; //mảng lưu vị trí các con cờ
        public static Player end; //biến kiểm tra trò chơi kết thúc
        private MainWindow frmParent; //Form thực hiện
        public static Grid grdBanCo; // Nơi vẽ bàn cờ
        public static LuongGia eBoard; //Bảng lượng giá bàn cờ
        public static cls5OWin OWin; // Kiểm tra 5 ô win
        public static TuyChon Option; // Tùy chọn trò chơi
        //Các biến phụ

        // Điểm lượng giá
        public static int[] PhongThu = new int[5] { 0, 1, 9, 85, 769 };
        public static int[] TanCong = new int[5] { 0, 2, 28, 256, 2308 };

        //Properties
        public Player End
        {
            get { return end; }
            set { end = value; }
        }
        public int Row
        {
            get { return row; }
        }
        public int Column
        {
            get { return column; }
        }
        //Contructors
        public BanCo(MainWindow frm, Grid grd)
        {
            Option = new TuyChon();
            OWin = new cls5OWin();
            row = column = 12;
            currPlayer = Player.None;
            end = Player.None;
            frmParent = frm;
            grdBanCo = grd;
            board = new Player[row, column];
            ResetBoard();
            eBoard = new LuongGia(this);

          
            grdBanCo.MouseDown += new System.Windows.Input.MouseButtonEventHandler(grdBanCo_MouseDown);
            worker.DoWork += wait;
            worker.RunWorkerCompleted +=maydanh;
        }
        private void wait(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(1000);
        }

  

        // Thiết lập các giá trị lưu vị trí bàn cờ.
        public void ResetBoard()
        {
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    board[i, j] = Player.None;
                }
            }
        }
        //Bắt đầu trò chơi mới
        public void NewGame()
        {
            currPlayer = Option.LuotChoi;//Lấy thông tin lượt chơi
            if (Option.WhoPlayWith == Player.Com)//Nếu chọn kiểu chơi với máy
            {
                if (currPlayer == Player.Com)//Nếu lược đi là máy
                {
                    DiNgauNhien();
                }
            }
        }

        //Bắt đầu lại trò chơi mới
        public void PlayAgain()
        {
            OWin = new cls5OWin();

            grdBanCo.Children.Clear();
     
            ResetBoard();
            this.DrawGomokuBoard();
            if (Option.WhoPlayWith == Player.Com)
            {
                if (end == Player.None)
                {
                    currPlayer = Player.Com;

                }
                if (currPlayer == Player.Com && Option.WhoPlayWith == Player.Com)
                {
                    DiNgauNhien();
                }
            }
            else
            {
                if (end == Player.None)
                {
                    if (currPlayer == Player.Human)
                    {
                        currPlayer = Player.Com;

                    }
                    else if (currPlayer == Player.Com)
                    {
                        currPlayer = Player.Human;

                    }
                }
            }
            end = Player.None;
        }

        public static void DiNgauNhien()
        {
            if (currPlayer == Player.Com)
            {
                board[row / 2, column / 2] = currPlayer;
                DrawDataBoard(row / 2, column / 2, true, true);
                MainWindow.rowngaunhien = row / 2;
                MainWindow.colngaunhien = column / 2;
                currPlayer = Player.Human;
                OnComDanhXong();//Khai báo sự kiện khi máy đánh xong
            }
        }
        public static int rows, columns;
        public static int test;
        Node node = new Node();
        public void maydanh(object sender, RunWorkerCompletedEventArgs e)
        {
            if (currPlayer == Player.Com && end == Player.None)//Nếu lượt đi là máy và trận đấu chưa kết thúc
            {
                //Tìm đường đi cho máy
                eBoard.ResetBoard();
                LuongGia(Player.Com);//Lượng giá bàn cờ cho máy
                node = eBoard.GetMaxNode();//lưu vị trí máy sẽ đánh
                int r, c;
                r = node.Row; c = node.Column;
                board[r, c] = currPlayer; //Lưu loại cờ vừa đánh vào mảng
                DrawDataBoard(r, c, true, true); //Vẽ con cờ theo lượt chơi
                end = CheckEnd(r, c);//Kiểm tra xem trận đấu kết thúc chưa

                if (end == Player.Com)//Nếu máy thắng
                {
                    OnLose();//Khai báo sư kiện Lose
                    OnWinOrLose();//Hiển thị 5 ô Lose.
                }
                else if (end == Player.None)
                {
                    currPlayer = Player.Human;//Thiết lập lại lượt chơi
                    OnComDanhXong();// Khai báo sự kiện người đánh xong
                }
            }

        }
        //Hàm đánh cờ
        public void grdBanCo_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            System.GC.Collect();//Thu gôm rác
            if (Option.WhoPlayWith == Player.Com)//Nếu chọn kiểu chơi đánh với máy
            {
                Point toado = e.GetPosition(grdBanCo); //Lấy tọa độ chuột
                //Xử lý tọa độ
                int cl = ((int)toado.X / length);
                int rw = ((int)toado.Y / length);

               
                if (board[rw, cl] == Player.None) //Nếu ô bấm chưa có cờ
                {
                    if (currPlayer == Player.Human && end == Player.None)//Nếu lượt đi là người và trận đấu chưa kết thúc
                    {
                        board[rw, cl] = currPlayer;//Lưu loại cờ vừa đánh vào mảng
                        DrawDataBoard(rw, cl, true, true);//Vẽ con cờ theo lượt chơi
                        end = CheckEnd(rw, cl);//Kiểm tra xem trận đấu kết thúc chưa
                        if (end == Player.Human)//Nếu người thắng cuộc là người
                        {
                            OnWin();//Khai báo sự kiện Win
                            OnWinOrLose();//Hiển thị 5 ô Win.
                        }
                        else if (end == Player.None) //Nếu trận đấu chưa kết thúc
                        {
                            currPlayer = Player.Com;//Thiết lập lại lượt chơi
                            OnHumanDanhXong(); // Khai báo sự kiện người đánh xong
                        }
                    }
                        try
                        {
                            worker.RunWorkerAsync();
                        }
                        catch
                        {

                        }
                   
                }
            }
            else if (Option.WhoPlayWith == Player.Human) //Nếu chọn kiểu chơi 2 người đánh với nhau
            {
                //Player.Com sẽ đóng vai trò người chơi thứ 2
                Point toado = e.GetPosition(grdBanCo);//Lấy thông tin tọa độ chuột
                //Xử lý tọa độ
                int cl = ((int)toado.X / length);
                int rw = ((int)toado.Y / length);
                if (board[rw, cl] == Player.None)//Nếu ô bấm chưa có cờ
                {
                    if (currPlayer == Player.Human && end == Player.None)//Nếu lượt đi là người và trận đấu chưa kết thúc
                    {
                        board[rw, cl] = currPlayer;//Lưu loại cờ vừa đánh vào mảng
                        DrawDataBoard(rw, cl, true, true);//Vẽ con cờ theo lượt chơi
                        end = CheckEnd(rw, cl);//Kiểm tra xem trận đấu kết thúc chưa
                        if (end == Player.Human)//Nếu người chơi 1 thắng
                        {
                            currPlayer = Player.Human; //Thiết lập lại lượt chơi
                            OnWin();//Khai báo sư kiện Win
                            OnWinOrLose();//Hiển thị 5 ô Win.
                        }
                        else
                        {
                            currPlayer = Player.Com;//Thiết lập lại lượt chơi
                            OnHumanDanhXong();// Khai báo sự kiện người chơi 1 đánh xong
                        }
                    }
                    else if (currPlayer == Player.Com && end == Player.None)
                    {
                        board[rw, cl] = currPlayer;//Lưu loại cờ vừa đánh vào mảng
                        DrawDataBoard(rw, cl, true, true);//Vẽ con cờ theo lượt chơi
                        end = CheckEnd(rw, cl);//Kiểm tra xem trận đấu kết thúc chưa
                        if (end == Player.Com)//Nếu người chơi 2 thắng
                        {
                            OnWin();//Khai báo sư kiện Win
                            OnWinOrLose();//Hiển thị 5 ô Win.
                        }
                        else
                        {
                            currPlayer = Player.Human;//Thiết lập lại lượt chơi
                            OnComDanhXong();// Khai báo sự kiện người chơi 2 đánh xong
                        }
                    }
                }
            }
            else if (Option.WhoPlayWith == Player.Online) // chọn người chơi online
            {
                Point toado = e.GetPosition(grdBanCo); //Lấy tọa độ chuột
                //Xử lý tọa độ
                int cl = ((int)toado.X / length);
                int rw = ((int)toado.Y / length);
               
                if (board[rw, cl] == Player.None) //Nếu ô bấm chưa có cờ
                {
                    if (currPlayer == Player.Human && end == Player.None)// /Nếu lượt đi là mình và trận đấu chưa kết thúc
                    {
                        connect.rw = rw;
                        connect.cl = cl;
                        connect.guitoado(MainWindow.socket, rw, cl);
                        board[rw, cl] = currPlayer;//Lưu loại cờ vừa đánh vào mảng
                        DrawDataBoard(rw, cl, true, true);//Vẽ con cờ theo lượt chơi
                        end = CheckEnd(rw, cl);//Kiểm tra xem trận đấu kết thúc chưa
                        if (end == Player.Human)//Nếu người thắng cuộc là mình
                        {
                            OnWin();//Khai báo sự kiện Win
                            OnWinOrLose();//Hiển thị 5 ô Win.
                            MainWindow.newgame1 = true;
                        }
                        else if (end == Player.None) //Nếu trận đấu chưa kết thúc
                        {
                            currPlayer = Player.Online;//Thiết lập lại lượt chơi
                            OnHumanDanhXong(); // Khai báo sự kiện người đánh xong
                        }


                    }
                }
            }
        }
        //delegate sự kiện Win
        public delegate void WinEventHander();
        public static event WinEventHander WinEvent;
        public static void OnWin()
        {
            if (WinEvent != null)
            {
                WinEvent();
            }
        }
        //delegate sự kiện Lose
        public delegate void LoseEventHander();
        public static event LoseEventHander LoseEvent;
        public static void OnLose()
        {
            if (LoseEvent != null)
            {
                LoseEvent();
            }
        }

        //delegate sự kiện máy đánh xong
        public delegate void ComDanhXongEventHandler();
        public static event ComDanhXongEventHandler ComDanhXongEvent;
        public static void OnComDanhXong()
        {
            if (ComDanhXongEvent != null)
            {
                ComDanhXongEvent();
            }
        }
        //delegate sự kiện người đánh xong
        public delegate void HumanDanhXongEventHandler();
        public event HumanDanhXongEventHandler HumanDanhXongEvent;
        private void OnHumanDanhXong()
        {
            if (HumanDanhXongEvent != null)
            {
                HumanDanhXongEvent();
            }
        }

        //Hàm lượng giá thế cờ
        public static void LuongGia(Player player)
        {
            int cntHuman = 0, cntCom = 0;//Biến đếm Human,Com
            #region Luong gia cho hang
            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column - 4; j++)
                {
                    //Khởi tạo biến đếm
                    cntHuman = cntCom = 0;
                    //Đếm số lượng con cờ trên 5 ô kế tiếp của 1 hàng
                    for (int k = 0; k < 5; k++)
                    {
                        if (board[i, j + k] == Player.Human) cntHuman++;
                        if (board[i, j + k] == Player.Com) cntCom++;
                    }
                    //Lượng giá
                    //Nếu 5 ô kế tiếp chỉ có 1 loại cờ (hoặc là Human,hoặc la Com)
                    if (cntHuman * cntCom == 0 && cntHuman != cntCom)
                    {
                        //Gán giá trị cho 5 ô kế tiếp của 1 hàng
                        for (int k = 0; k < 5; k++)
                        {
                            //Nếu ô đó chưa có quân đi
                            if (board[i, j + k] == Player.None)
                            {
                                //Nếu trong 5 ô đó chỉ tồn tại cờ của Human
                                if (cntCom == 0)
                                {
                                    //Nếu đối tượng lượng giá là Com
                                    if (player == Player.Com)
                                    {
                                        //Vì đối tượng người chơi là Com mà trong 5 ô này chỉ có Human
                                        //nên ta sẽ cộng thêm điểm phòng thủ cho Com
                                        eBoard.GiaTri[i, j + k] += PhongThu[cntHuman];
                                    }
                                    //Ngược lại cộng điểm phòng thủ cho Human
                                    else
                                    {
                                        eBoard.GiaTri[i, j + k] += TanCong[cntHuman];
                                    }
                                    //Nếu chơi theo luật Việt Nam
                                

                                }
                                //Tương tự như trên
                                if (cntHuman == 0) //Nếu chỉ tồn tại Com
                                {
                                    if (player == Player.Human) //Nếu người chơi là Người
                                    {
                                        eBoard.GiaTri[i, j + k] += PhongThu[cntCom];
                                    }
                                    else
                                    {
                                        eBoard.GiaTri[i, j + k] += TanCong[cntCom];
                                    }
                                    //Trường hợp chặn 2 đầu
                                

                                }
                                if ((j + k - 1 > 0) && (j + k + 1 <= column - 1) && (cntHuman == 4 || cntCom == 4)
                                   && (board[i, j + k - 1] == Player.None || board[i, j + k + 1] == Player.None))
                                {
                                    eBoard.GiaTri[i, j + k] *= 3;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //Tương tự như lượng giá cho hàng
            #region Luong gia cho cot
            for (int i = 0; i < row - 4; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    cntHuman = cntCom = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        if (board[i + k, j] == Player.Human) cntHuman++;
                        if (board[i + k, j] == Player.Com) cntCom++;
                    }
                    if (cntHuman * cntCom == 0 && cntCom != cntHuman)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (board[i + k, j] == Player.None)
                            {
                                if (cntCom == 0)
                                {
                                    if (player == Player.Com) eBoard.GiaTri[i + k, j] += PhongThu[cntHuman];
                                    else eBoard.GiaTri[i + k, j] += TanCong[cntHuman];
                                    // Truong hop bi chan 2 dau.
                                    if ((i - 1) >= 0 && (i + 5) <= row - 1 && board[i - 1, j] == Player.Com && board[i + 5, j] == Player.Com)
                                    {
                                        eBoard.GiaTri[i + k, j] = 0;
                                    }
                                }
                                if (cntHuman == 0)
                                {
                                    if (player == Player.Human) eBoard.GiaTri[i + k, j] += PhongThu[cntCom];
                                    else eBoard.GiaTri[i + k, j] += TanCong[cntCom];
                                    // Truong hop bi chan 2 dau.
                                   
                                }
                                if ((i + k - 1) >= 0 && (i + k + 1) <= row - 1 && (cntHuman == 4 || cntCom == 4)
                                    && (board[i + k - 1, j] == Player.None || board[i + k + 1, j] == Player.None))
                                {
                                    eBoard.GiaTri[i + k, j] *= 3;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //Tương tự như lượng giá cho hàng
            #region  Luong gia tren duong cheo chinh (\)
            for (int i = 0; i < row - 4; i++)
            {
                for (int j = 0; j < column - 4; j++)
                {
                    cntHuman = cntCom = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        if (board[i + k, j + k] == Player.Human) cntHuman++;
                        if (board[i + k, j + k] == Player.Com) cntCom++;
                    }
                    if (cntHuman * cntCom == 0 && cntCom != cntHuman)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (board[i + k, j + k] == Player.None)
                            {
                                if (cntCom == 0)
                                {
                                    if (player == Player.Com) eBoard.GiaTri[i + k, j + k] += PhongThu[cntHuman];
                                    else eBoard.GiaTri[i + k, j + k] += TanCong[cntHuman];
                                    // Truong hop bi chan 2 dau.
                                    
                                }
                                if (cntHuman == 0)
                                {
                                    if (player == Player.Human) eBoard.GiaTri[i + k, j + k] += PhongThu[cntCom];
                                    else eBoard.GiaTri[i + k, j + k] += TanCong[cntCom];
                               
                                }
                                if ((i + k - 1) >= 0 && (j + k - 1) >= 0 && (i + k + 1) <= row - 1 && (j + k + 1) <= column - 1 && (cntHuman == 4 || cntCom == 4)
                                    && (board[i + k - 1, j + k - 1] == Player.None || board[i + k + 1, j + k + 1] == Player.None))
                                {
                                    eBoard.GiaTri[i + k, j + k] *= 3;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
            //Tương tự như lượng giá cho hàng
            #region Luong gia tren duong cheo phu (/)
            for (int i = 4; i < row - 4; i++)
            {
                for (int j = 0; j < column - 4; j++)
                {
                    cntCom = 0; cntHuman = 0;
                    for (int k = 0; k < 5; k++)
                    {
                        if (board[i - k, j + k] == Player.Human) cntHuman++;
                        if (board[i - k, j + k] == Player.Com) cntCom++;
                    }
                    if (cntHuman * cntCom == 0 && cntHuman != cntCom)
                    {
                        for (int k = 0; k < 5; k++)
                        {
                            if (board[i - k, j + k] == Player.None)
                            {
                                if (cntCom == 0)
                                {
                                    if (player == Player.Com) eBoard.GiaTri[i - k, j + k] += PhongThu[cntHuman];
                                    else eBoard.GiaTri[i - k, j + k] += TanCong[cntHuman];
                                    // Truong hop bi chan 2 dau.
                                    if (i + 1 <= row - 1 && j - 1 >= 0 && i - 5 >= 0 && j + 5 <= column - 1 && board[i + 1, j - 1] == Player.Com && board[i - 5, j + 5] == Player.Com)
                                    {
                                        eBoard.GiaTri[i - k, j + k] = 0;
                                    }
                                }
                                if (cntHuman == 0)
                                {
                                    if (player == Player.Human) eBoard.GiaTri[i - k, j + k] += PhongThu[cntCom];
                                    else eBoard.GiaTri[i - k, j + k] += TanCong[cntCom];
                                    // Truong hop bi chan 2 dau.
                                  
                                }
                                if ((i - k + 1) <= row - 1 && (j + k - 1) >= 0
                                    && (i - k - 1) >= 0 && (j + k + 1) <= column - 1
                                    && (cntHuman == 4 || cntCom == 4)
                                    && (board[i - k + 1, j + k - 1] == Player.None || board[i - k - 1, j + k + 1] == Player.None))
                                {
                                    eBoard.GiaTri[i - k, j + k] *= 3;
                                }
                            }
                        }
                    }
                }
            }
            #endregion
        }
        //Hàm lấy đối thủ của người chơi hiện tại
        public static Player DoiNgich(Player cur)
        {
            if (cur == Player.Com) return Player.Human;
            if (cur == Player.Human) return Player.Com;
            return Player.None;
        }
        //Hàm kiểm tra trận đấu kết thúc chưa
        public static Player CheckEnd(int rw, int cl)
        {
            int rowTemp = rw;
            int colTemp = cl;
            int count1, count2, count3, count4;
            count1 = count2 = count3 = count4 = 1;
            Player cur = board[rw, cl];
            OWin.Reset();
            OWin.Add(new Node(rowTemp, colTemp));
            #region Kiem Tra Hang Ngang
            while (colTemp - 1 >= 0 && board[rowTemp, colTemp - 1] == cur)
            {
                OWin.Add(new Node(rowTemp, colTemp - 1));
                count1++;
                colTemp--;
            }
            colTemp = cl;
            while (colTemp + 1 <= column - 1 && board[rowTemp, colTemp + 1] == cur)
            {
                OWin.Add(new Node(rowTemp, colTemp + 1));
                count1++;
                colTemp++;
            }
            if (count1 == 5)
            {
               return cur;
            }
            #endregion
            #region Kiem Tra Hang Doc
            OWin.Reset();
            colTemp = cl;
            OWin.Add(new Node(rowTemp, colTemp));

            while (rowTemp - 1 >= 0 && board[rowTemp - 1, colTemp] == cur)
            {
                OWin.Add(new Node(rowTemp - 1, colTemp));
                count2++;
                rowTemp--;
            }
            rowTemp = rw;
            while (rowTemp + 1 <= row - 1 && board[rowTemp + 1, colTemp] == cur)
            {
                OWin.Add(new Node(rowTemp + 1, colTemp));
                count2++;
                rowTemp++;
            }
            if (count2 == 5)
            {
                 return cur;
            }
            #endregion
            #region Kiem Tra Duong Cheo Chinh (\)
            colTemp = cl;
            rowTemp = rw;
            OWin.Reset();
            OWin.Add(new Node(rowTemp, colTemp));
            while (rowTemp - 1 >= 0 && colTemp - 1 >= 0 && board[rowTemp - 1, colTemp - 1] == cur)
            {
                OWin.Add(new Node(rowTemp - 1, colTemp - 1));
                count3++;
                rowTemp--;
                colTemp--;
            }
            rowTemp = rw;
            colTemp = cl;
            while (rowTemp + 1 <= row - 1 && colTemp + 1 <= column - 1 && board[rowTemp + 1, colTemp + 1] == cur)
            {
                OWin.Add(new Node(rowTemp + 1, colTemp + 1));
                count3++;
                rowTemp++;
                colTemp++;
            }
            if (count3 == 5)
            {
                return cur;
            }
            #endregion
            #region Kiem Tra Duong Cheo Phu
            rowTemp = rw;
            colTemp = cl;
            OWin.Reset();
            OWin.Add(new Node(rowTemp, colTemp));
            while (rowTemp + 1 <= row - 1 && colTemp - 1 >= 0 && board[rowTemp + 1, colTemp - 1] == cur)
            {
                OWin.Add(new Node(rowTemp + 1, colTemp - 1));
                count4++;
                rowTemp++;
                colTemp--;
            }
            rowTemp = rw;
            colTemp = cl;
            while (rowTemp - 1 >= 0 && colTemp + 1 <= column - 1 && board[rowTemp - 1, colTemp + 1] == cur)
            {
                OWin.Add(new Node(rowTemp - 1, colTemp + 1));
                count4++;
                rowTemp--;
                colTemp++;
            }
            if (count4 == 5)
            {
               return cur;
            }
            #endregion
            return Player.None;
        }
        //Hàm lấy thông tin 5 ô Win hoặc Lose
        public static void OnWinOrLose()
        {
     
          
        }

        public static void DrawDataBoard(int rw, int cl, bool record, bool type)
        {
            if (type == true)
            {
                if (currPlayer == Player.Human)
                {
                    Image Chess1 = new Image();
                    Chess1.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Chess/Den.png"));
                    Chess1.Width = Chess1.Height = length;
                    Chess1.HorizontalAlignment = 0;
                    Chess1.VerticalAlignment = 0;
                    Chess1.Margin = new Thickness(cl * length, rw * length, 0, 0);
                    Chess1.Opacity = 100;
                    grdBanCo.Children.Add(Chess1);
                    //Ghi lại cờ vừa đánh

                }
                else if (currPlayer == Player.Com || currPlayer == Player.Online)
                {
                    Image Chess2 = new Image();
                    Chess2.Source = new BitmapImage(new Uri("pack://application:,,,/Image/Chess/Trang.png"));
                    Chess2.Width = Chess2.Height = length;
                    Chess2.HorizontalAlignment = 0;
                    Chess2.VerticalAlignment = 0;
                    Chess2.Margin = new Thickness(cl * length, rw * length, 0, 0);
                    Chess2.Opacity = 100;
                    grdBanCo.Children.Add(Chess2);


                }
                
            }
        }

        //Hàm vẽ bàn cờ
        public void DrawGomokuBoard()
        {
            for (int i = 0; i < row + 1; i++)
            {
                Line line = new Line();

                line.Stroke = Brushes.Black;
                line.X1 = 0;
                line.Y1 = i * length;
                line.X2 = length * row;
                line.Y2 = i * length;
                line.HorizontalAlignment = HorizontalAlignment.Left;
                line.VerticalAlignment = VerticalAlignment.Top;
                grdBanCo.Children.Add(line);
            }
            for (int i = 0; i < column + 1; i++)
            {
                Line line = new Line();
                line.Stroke = Brushes.Black;
                line.X1 = i * length;
                line.Y1 = 0;
                line.X2 = i * length;
                line.Y2 = length * column;
                line.HorizontalAlignment = HorizontalAlignment.Left;
                line.VerticalAlignment = VerticalAlignment.Top;
                grdBanCo.Children.Add(line);
            }

        }
    }
}

using Newtonsoft.Json.Linq;
using Quobject.SocketIoClientDotNet.Client;
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



namespace WpfApplication1
{
    public class connect 
    {
        public static bool bl=false;
        public static string copy;
        public static int rw=-1,cl=-1;
        public static int rw1 = -1, cl1 = -1;
        public static void connected(Quobject.SocketIoClientDotNet.Client.Socket socket, string name)
        {
     
            socket.On(Socket.EVENT_CONNECT, () =>
            {
                MessageBox.Show("connected", "Thông báo ");

            });
            socket.On(Socket.EVENT_MESSAGE, (data) =>
            {
                MessageBox.Show(data.ToString());
                
          
            });
            socket.On(Socket.EVENT_CONNECT_ERROR, (data) =>
            {
                MessageBox.Show(data.ToString());
            });
            socket.On("ChatMessage", (data) =>
            {
               string k = "You are the first player!";
               bl = data.ToString().Contains(k);
               var o = JObject.Parse(data.ToString());
               if (connect.bl == true)
               {
                   if (BanCo.Option.WhoPlayWith == Player.Online)
                   {
                       BanCo.currPlayer = Player.Human;
                   }
                   if (BanCo.Option.WhoPlayWith == Player.MayOnline)
                   {
                       Application.Current.Dispatcher.Invoke((Action)(() =>
                       {
                           BanCo.currPlayer = Player.Com;

                           MainWindow.txtNguoiMay.Text = data.ToString();
                       }));
                   }
                   
               }

               
               MainWindow.message = (string)o["message"];
               var jobject = data as JToken;
               Caidat.Ketnoi.Message += jobject.Value<String>("message") + "  " + DateTime.Now.ToString() + "\n\n";
                if (((Newtonsoft.Json.Linq.JObject)data)["message"].ToString() == "Welcome!")
                {
                    socket.Emit("MyNameIs", name);
                    socket.Emit("ConnectToOtherPlayer");
                    copy = name;   
                } 
                if((string)o["from"]!=null)
                {
                    MainWindow.name = (string)o["from"];

                }
                
                
            }); 
          
            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                MessageBox.Show(data.ToString());
            });
           
            socket.On("NextStepIs", (data) =>
            {
                var o = JObject.Parse(data.ToString());
                if (BanCo.Option.WhoPlayWith == Player.Online)
                {
                    if (((int)o["row"] != connect.rw || (int)o["col"] != connect.cl))
                    {
                        BanCo.rows = (int)o["row"];
                        BanCo.columns = (int)o["col"];
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            MainWindow.txt.Text = data.ToString();
                        }));
                    }
                }
                if (BanCo.Option.WhoPlayWith == Player.MayOnline)
                {
                    if (((int)o["row"] != connect.rw1 || (int)o["col"] != connect.cl1))
                    {
                        BanCo.rows = (int)o["row"];
                        BanCo.columns = (int)o["col"];
                        Application.Current.Dispatcher.Invoke((Action)(() =>
                        {
                            MainWindow.txtMay.Text = data.ToString();
                        }));
                    }
                }

            });
            
        }
        public static void guitoado(Quobject.SocketIoClientDotNet.Client.Socket socket, int row, int col)
        {

            socket.On(Socket.EVENT_ERROR, (data) =>
            {
                MessageBox.Show(data.ToString());
            });

            socket.Emit("MyStepIs", JObject.FromObject(new { row = row, col = col }));
        }
        public static void changname(Quobject.SocketIoClientDotNet.Client.Socket socket, string name)
        {
                  socket.Emit("MyNameIs", name);
                  socket.Emit("message:", copy + "is now called" + name);
                  copy = name;
        }
        public static void sendmessage (Quobject.SocketIoClientDotNet.Client.Socket socket, string name, string txt)
        {
            
                socket.Emit("ChatMessage", txt);
                socket.Emit("message:"+txt, "from:" +name);
           
        }
        
    }
}

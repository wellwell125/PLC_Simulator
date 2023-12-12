using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PLC_Simulator
{
    internal class PLC_Comm
    {
        public string maker;
        public System.Net.Sockets.TcpClient tcp;
        public System.Net.Sockets.NetworkStream ns;

        public bool connection(string ip, int port)
        {
            try
            {
                //tcp = new System.Net.Sockets.TcpClient(ip, port);

                tcp = new System.Net.Sockets.TcpClient();
                var task = tcp.ConnectAsync(ip, port);
                if (!task.Wait(1000))  //ここで画面がフリーズしてしまう。
                {
                    return false;
                }

                ns = tcp.GetStream();
                ns.ReadTimeout = 5000;
                ns.WriteTimeout = 5000;
                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool disconnection()
        {
            try
            {
                ns.Close();
                tcp.Close();

                ns.Dispose();
                tcp.Dispose();

                return true;
            }
            catch
            {
                return false;
            }

        }

        public bool write_plc_in(bool[] plc_in, string io)
        {
            if (ns == null)
            {
                return false;
            }

            int size = 0;
            string send_msg = "";
            string recieve = "";


            size = plc_in.Length / 16 + plc_in.Length % 16;
            string send_bit = "";
            int send_dec = 0;

            for (int i = 0; i < 16 * size; i++)
            {
                if (plc_in[i])
                {
                    send_bit = "1" + send_bit;
                }
                else
                {
                    send_bit = "0" + send_bit;
                }
            }
            send_dec = Convert.ToInt16(send_bit, 2);

            send_msg = "WRS" + " " + io + ".U" + " " + size + " " + send_dec + "\r";
            recieve = send(send_msg);

            if (recieve == "")
            {
                return false;
            }

            return true;
        }

        public bool read_plc_out(ref bool[] plc_out, string io)
        {
            if (ns == null)
            {
                return false;
            }

            int size = 0;
            string send_msg = "";
            string recieve = "";



            size = plc_out.Length / 16 + plc_out.Length % 16;
            send_msg = "RDS" + " " + io + ".U" + " " + size + "\r";
            recieve = send(send_msg);

            if (recieve == "")
            {
                return false;
            }

            recieve = recieve.Substring(0, recieve.IndexOf("\r\n"));
            string recieve_bit = Convert.ToString(int.Parse(recieve), 2).PadLeft(16, '0');

            for (int i = 0; i < 16 * size; i++)
            {
                string bit = recieve_bit.Substring(recieve_bit.Length - i - 1, 1);
                if (bit == "1")
                {
                    plc_out[i] = true;
                }
                else
                {
                    plc_out[i] = false;
                }
            }

            return true;
        }

        public bool read_plc_data(ref int[] plc_out, string io)
        {
            if (ns == null)
            {
                return false;
            }

            int size = 0;
            string send_msg = "";
            string recieve = "";


            size = plc_out.Length;
            send_msg = "RDS" + " " + io + ".U" + " " + size + "\r";
            recieve = send(send_msg, 100);

            if (recieve == "")
            {
                return false;
            }

            recieve = recieve.Substring(0, recieve.IndexOf("\r\n"));
            string[] recieve_split = recieve.Split(" ");

            for (int i = 0; i < recieve_split.Length; i++)
            {
                plc_out[i] = int.Parse(recieve_split[i]);
            }

            return true;
        }

        public bool write_plc_data(ref int[] plc_out, string io)
        {
            if (ns == null)
            {
                return false;
            }

            int size = 0;
            string send_msg = "";
            string recieve = "";
            string data = "";


            size = plc_out.Length;

            for (int i = 0; i < size; i++)
            {
                data += " " + plc_out[i];
            }


            send_msg = "WRS" + " " + io + ".U" + " " + size + data + "\r";
            recieve = send(send_msg, 100);

            if (recieve == "")
            {
                return false;
            }

            return true;
        }


        public string send(string sendMsg, int byteValue = 50)
        {

            if (tcp == null)
            {
                return "";
            }

            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);


            string res = "";
            try
            {
                char[] chArray2 = sendMsg.ToCharArray();
                byte[] bArray_send = new byte[chArray2.Length];

                for (int i = 0; i < chArray2.Length; i++)
                {
                    bArray_send[i] = Convert.ToByte(chArray2[i] & 0xFFFF);
                }

                ns.Write(bArray_send, 0, bArray_send.Length);


                byte[] bArray_res = new byte[byteValue];

                ns.Read(bArray_res, 0, bArray_res.Length);
                res = Encoding.GetEncoding(932).GetString(bArray_res);
            }
            catch
            {
                res = "";
            }


            return res;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Windows.Forms;

using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO.Ports;

namespace WindowsFormsApplication1
{
    public partial class serial : Form
    {
        SerialPort sp = null; //声明一个串口类
        bool isOpen = false;
        bool isSetProperty = false; //属性设置标志位
        bool isHex = false;

        public serial()
        {
            InitializeComponent();
        }

        private void serial_Load(object sender, EventArgs e)
        {
            this.MaximumSize = this.Size;
            this.MinimumSize = this.Size;
            this.MaximizeBox = false;
            for (int i = 0; i < 10; i++) //最大支持到串口数
            {
                cbxCOMPort.Items.Add("COM"+(i+1).ToString());
            }
            cbxCOMPort.SelectedIndex = 0;//设当前指点选项的索引值为0

            //列出常用波特率
            cbxBaudRate.Items.Add("1200");
            cbxBaudRate.Items.Add("2400");
            cbxBaudRate.Items.Add("4800");
            cbxBaudRate.Items.Add("9600");
            cbxBaudRate.Items.Add("19200");
            cbxBaudRate.Items.Add("38400");
            cbxBaudRate.Items.Add("43000");
            cbxBaudRate.Items.Add("56000");
            cbxBaudRate.Items.Add("57600");
            cbxBaudRate.Items.Add("115200");
            cbxBaudRate.SelectedIndex = 3;

            //列出停止位
            cbxStopBits.Items.Add("1");
            cbxStopBits.Items.Add("1.5");
            cbxStopBits.Items.Add("2");
            cbxStopBits.SelectedIndex = 0;

            //列出奇偶校验位
            cbxParity.Items.Add("无");
            cbxParity.Items.Add("奇校验");
            cbxParity.Items.Add("偶校验");
            cbxParity.SelectedIndex = 0;

            //列出数据位
            cbxDataBits.Items.Add("8");
            cbxDataBits.Items.Add("7");
            cbxDataBits.Items.Add("6");
            cbxDataBits.Items.Add("5");
            cbxDataBits.SelectedIndex = 0;

            //默认为char显示
            rbnChar.Checked = true;
        }

        private void btnCheckCOM_Click(object sender, EventArgs e)
        {
            bool comExistence = false;//有可用串口标志位
            cbxCOMPort.Items.Clear();//清楚当前串口号中的所有串口名称
            for (int i = 1; i <= 10; i++)
            {
                try
                {
                    sp = new SerialPort("COM" + i);
                    sp.Open();
                    sp.Close();
                    cbxCOMPort.Items.Add("COM" + i);
                    comExistence = true;
                }
                catch
                {
                    continue;
                }

            }
            if (comExistence)
            {
                cbxCOMPort.SelectedIndex = 0; //显示第一个添加的索引
            }
            else
            {
                MessageBox.Show("没有找到可用串口！", "错误提示");//“错误提示”显示在标题栏，“没找到串口”为显示文本
            }
        }

        //检查串口是否设置
        private bool CheckPortSetting()
        {
            if (cbxCOMPort.Text.Trim() == "") return false;
            if (cbxBaudRate.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            if (cbxParity.Text.Trim() == "") return false;
            if (cbxDataBits.Text.Trim() == "") return false;
            else return true;
        }

        private bool CheckSendData()
        {
            if (tbxSendData.Text.Trim() == "") return false;
            else return true;
        }

        //设置串口的属性
        private void SetPortProperty()
        {
            sp.PortName = cbxCOMPort.Text.Trim();  //设置串口名

            sp.BaudRate = Convert.ToInt32(cbxBaudRate.Text.Trim());//设置串口的波特率

            float f = Convert.ToSingle(cbxStopBits.Text.Trim());//将数字的制定字符串表示形式转换为等效的单精度浮点数
            if (f == 1.0)
            {
                sp.StopBits = StopBits.One;
            }
            else if (f == 1.5)
            {
                sp.StopBits = StopBits.OnePointFive;
            }
            else if (f == 2.0)
            {
                sp.StopBits = StopBits.Two;

            }
            else
            {
                sp.StopBits = StopBits.One;
            }

            sp.DataBits = Convert.ToInt16(cbxDataBits.Text.Trim());//设置数据位

            string s = cbxParity.Text.Trim(); //设置奇偶校验位
            if (s.CompareTo("无") == 0) //与字符串是否在同一位置，在前面，还是在后面
            {
                sp.Parity = Parity.None;
            }
            else if (s.CompareTo("奇校验") == 0)
            {
                sp.Parity = Parity.Odd;
            }
            else if (s.CompareTo("偶校验") == 0)
            {
                sp.Parity = Parity.Even;
            }
            else
            {
                sp.Parity = Parity.None;
            }

            sp.ReadTimeout = -1;//设置超时读取时间

            sp.RtsEnable = true;//该值指示在串行通信中是否启用请求发送（RTS）信号

            sp.DataReceived += new SerialDataReceivedEventHandler(sp_DataReceived);
            if (rbnHex.Checked)
            {
                isHex = true;
            }
            else
            {
                isHex = false;
            }
        }

        public void sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            System.Threading.Thread.Sleep(10);
            //this.Invoke就是跨线程访问ui的方法
            this.Invoke((EventHandler)(delegate
            {
                if (isHex == false)
                {
                    tbxRecvData.Text += sp.ReadLine();
                }
                else
                {
                    Byte[] ReceiveData = new Byte[sp.BytesToRead];//创建字节数组
                    sp.Read(ReceiveData, 0, ReceiveData.Length); //读取所接收到的数据
                    string RecvDataText = null;
                    for (int i = 0; i < ReceiveData.Length; i++)
                    {
                        RecvDataText += ("0x" + ReceiveData[i].ToString("X2") + " ");//X为十六进制 2为每次都是两位数 这样看着整齐

                    }
                    tbxRecvData.Text += RecvDataText;

                }
                sp.DiscardInBuffer();//丢弃接收缓冲区的数据

            }));
        }

        private void btnOpenCOM_Click_1(object sender, EventArgs e)
        {
            if (isOpen == false)
            {
                if (!CheckPortSetting())
                {
                    MessageBox.Show("串口未设置！", "错误提示");
                    return;
                }
                if (!isSetProperty)//串口未设置则设置串口
                {
                    SetPortProperty();
                    isSetProperty = true;
                }
                try//打开串口
                {
                    sp.Open();
                    isOpen = true;
                    btnOpenCOM.Text = "关闭串口";
                    //串口打开后则相关的串口设置按钮便不可再用
                    cbxCOMPort.Enabled = false;
                    cbxBaudRate.Enabled = false;
                    cbxDataBits.Enabled = false;
                    cbxParity.Enabled = false;
                    cbxStopBits.Enabled = false;
                    rbnChar.Enabled = false;
                    rbnHex.Enabled = false;
                }
                catch
                {
                    //打开串口失败后，相应标志位取消
                    isSetProperty = false;
                    isOpen = false;
                    MessageBox.Show("串口无效或已被占用！", "错误提示");
                }
            }

            else
            {
                try //打开串口
                {
                    sp.Close();
                    isOpen = false;
                    isSetProperty = false;
                    btnOpenCOM.Text = "打开串口";
                    //关闭串口后，串口设置选项便可以继续使用
                    cbxCOMPort.Enabled = true;
                    cbxBaudRate.Enabled = true;
                    cbxDataBits.Enabled = true;
                    cbxParity.Enabled = true;
                    cbxStopBits.Enabled = true;
                    rbnChar.Enabled = true;
                    rbnHex.Enabled = true;
                }
                catch
                {
                    MessageBox.Show("关闭串口时发生错误！", "错误提示");
                }
            }
        }

        private void btnCleanData_Click_1(object sender, EventArgs e)
        {
            //tbxRecvData.Text = "";
            tbxSendData.Text = "";
            tbxSendData.Focus();
        }
        /*
        public string CRCCalc(string data)  //CRC检验算法  
        {
            string[] datas = data.Split(' ');
            List<byte> bytedata = new List<byte>();

            foreach (string str in datas)
            {
                bytedata.Add(byte.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier));
            }
          
            byte[] crcbuf = bytedata.ToArray();
            //计算并填写CRC校验码  
            byte crc = 0;
            int len = crcbuf.Length;

            for (byte n = 0; n < len; n++)
            {
                byte value = crcbuf[n];
                value = Convert.ToByte(data, n);
                crc = (byte)(crc ^ value);
                for (byte i = 0; i < 8; i++)
                {
                    byte k = (byte)(crc & 0x01);
                    if (0x01 == k) 
                    {
                        crc = (byte)((crc >> 1) ^ 0x8c);
                    }
                    else
                        crc >>= 1;
                }
            }
            string[] redata = new string[len+1];
     
            redata[len] = Convert.ToString((byte)(crc));

            return redata[0] + " " + redata[1];
        }  
         */
        public string CRCCalc(string data)  //CRC检验算法  
        {
            string[] datas = data.Split(' ');
            List<byte> bytedata = new List<byte>();

            foreach (string str in datas)
            {
                bytedata.Add(byte.Parse(str, System.Globalization.NumberStyles.AllowHexSpecifier));
            }

            byte[] crcbuf = bytedata.ToArray();
            //计算并填写CRC校验码  
            byte crc = 0;
            int len = crcbuf.Length;

            for (byte n = 0; n < len; n++)
            {
                byte value = crcbuf[n];
                if (0 == n) continue;
                crc = (byte)(crc ^ value);
                for (byte i = 0; i < 8; i++)
                {
                    byte k = (byte)(crc & 0x01);
                    if (0x01 == k)
                    {
                        crc = (byte)((crc >> 1) ^ 0x8c);
                    }
                    else
                        crc >>= 1;
                }
            }
            return crc.ToString("X");
        }  
        private void btnSend_Click_1(object sender, EventArgs e)
        {
            //写串口数据
            if (isOpen)
            {
                try
                {
                    //sp.WriteLine(tbxSendData.Text);
                    string expression = tbxSendData.Text.Trim();
                    this.tbxSendData.Text += " "+this.CRCCalc(expression).ToString();
                    sp.WriteLine(tbxSendData.Text);
                }
                catch
                {
                    MessageBox.Show("发送数据时发生错误！", "错误提示");
                    return;
                }
            }

            else
            {
                MessageBox.Show("串口未打开！", "错误提示");
                return;
            }

            //检测要发送的数据
            if (!CheckSendData())
            {
                MessageBox.Show("请输入要发送的数据！", "错误提示");
                return;
            }
        }

        private void btnReceive_Click(object sender, EventArgs e)
        {
            tbxRecvData.Text = "";
        }


    }
}

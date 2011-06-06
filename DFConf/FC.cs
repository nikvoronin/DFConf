using System;
using System.Collections.Generic;
using System.Text;
using System.IO.Ports;

namespace DFConf
{
    public class FC
    {
        public static byte STX				= 0x02;
		public static int OP_RESP_WORD		= 0x10;
		public static int OP_RESP_DWORD		= 0x20;
		public static int OP_READ = 0x10;
		public static int OP_READ_TEXT		= 0xF0;
		public static int OP_WRITE_WORD		= 0xE0;
		public static int OP_WRITE_DWORD	= 0xD0;

        private static FC _instance = new FC();
        private FC() {}
        public static FC I { get { return _instance; } }

        private SerialPort _port;

        public static string[] PortNames
        {
            get
            {
                
                return SerialPort.GetPortNames();
            }
        }

        public static byte BCC(byte[] buffer)
        {
            byte bcc = 0;

            for (int i = 0; i < buffer.Length - 1; i++)
            {
                bcc ^= buffer[i];
            }

            return bcc;
        }
        
        public void Open(string portName)
        {
            if (_port != null)
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }
            }

            _port = new SerialPort(portName, 9600, Parity.Even, 8);
            _port.WriteTimeout = _port.ReadTimeout = 100;
            _port.ReadBufferSize = _port.WriteBufferSize = 16;
            _port.Open();
        }

		public string GetStringValue(int address, byte index, byte inverterNo)
		{
			// send request
			byte[] wbuffer = new byte[16];
			wbuffer[0] = STX;
			wbuffer[1] = 14;			// telegram length
			wbuffer[2] = inverterNo;	// inverter on bus address

			int addrLo = address & 0xff;
			int addrHi = (address & 0x0f00) >> 8;
			wbuffer[3] = (byte)(OP_READ_TEXT | addrHi);
			wbuffer[4] = (byte)addrLo;

			wbuffer[5] = 0;		// INDhi
			wbuffer[6] = index;	// INDlo

			wbuffer[7] = 0;		// PEWhi
			wbuffer[8] = 0;		// PWEhi

			wbuffer[9] = 0;		// PEWlo
			wbuffer[10] = 0;	// PWElo

			wbuffer[11] = 0;	// PCD1
			wbuffer[12] = 0;	// PCD1

			wbuffer[13] = 0;	// PCD2
			wbuffer[14] = 0;	// PCD2

			wbuffer[15] = BCC(wbuffer);		// BCC

			_port.Write(wbuffer, 0, wbuffer.Length);

			// read response
			byte[] rbuffer = new byte[255];
			int len = rbuffer.Length;
			for (int i = 0; i < len; i++)
			{
				rbuffer[i] = (byte)_port.ReadByte();
				if (i == 1)
				{
					len = rbuffer[i] + 2;
				}
			}

			string result = string.Empty;

			int cnt = len - 12;
			for (int i = 0; i < cnt; i++)
			{
				result += ((char)rbuffer[i + 7]).ToString();
			}

			return result;
		}

		public bool SetParameterValue(int address, int value, byte inverterNo)
		{
			return SetParameterValue(address, 0, value, inverterNo);
		}

		public bool SetParameterValue(int address, byte index, int value, byte inverterNo)
		{
			// send request
			byte[] wbuffer = new byte[16];
			wbuffer[0] = STX;
			wbuffer[1] = 14;	// telegram length
			wbuffer[2] = inverterNo;		// inverter on bus address

			wbuffer[5] = 0;			// IND
			wbuffer[6] = index;		// IND

			int operation = OP_WRITE_WORD;

			uint valLlo = (uint)(value & 0xff);
			uint valLhi = (uint)((value & 0xff00) >> 8);
			uint valHlo = 0;
			uint valHhi = 0;
			if (value > 0xffff)
			{
				operation = OP_WRITE_DWORD;
				valHlo = (uint)((value & 0xff0000) >> 16);
				valHhi = (uint)((value & 0xff000000) >> 24);
			}
			wbuffer[7] = (byte)valHhi;		// PEWhi
			wbuffer[8] = (byte)valHlo;		// PWEhi
			wbuffer[9] = (byte)valLhi;		// PEWlo
			wbuffer[10] = (byte)valLlo;		// PWElo

			int addrLo = address & 0xff;
			int addrHi = (address & 0x0f00) >> 8;
			wbuffer[3] = (byte)(operation | addrHi);	// PKE
			wbuffer[4] = (byte)addrLo;					// PKE 0x654 = 1620 = [16-20] Motor Angle

			wbuffer[11] = 0;	// PCD1
			wbuffer[12] = 0;	// PCD1

			wbuffer[13] = 0;	// PCD2
			wbuffer[14] = 0;	// PCD2

			wbuffer[15] = BCC(wbuffer);		// BCC

			_port.Write(wbuffer, 0, wbuffer.Length);

			// read response
			byte[] rbuffer = new byte[16];
			for (int i = 0; i < rbuffer.Length; i++)
			{
				rbuffer[i] = (byte)_port.ReadByte();
			}
			int result = rbuffer[9] * 0xff + rbuffer[10];

			return false;
		}

		public int GetParameterValue(int address, byte inverterNo)
		{
			return GetParameterValue(address, 0, inverterNo);
		}

		public int GetParameterValue(int address, byte index, byte inverterNo)
        {
            // send request
            byte[] wbuffer = new byte[16];
            wbuffer[0] = STX;
            wbuffer[1] = 14;			// telegram length
			wbuffer[2] = inverterNo;	// inverter on bus address

            int addrLo = address & 0xff;
            int addrHi = (address & 0x0f00) >> 8;
            wbuffer[3] = (byte)(OP_READ | addrHi);	// PKE 1 - read
            wbuffer[4] = (byte)addrLo;				// PKE 0x654 = 1620 = [16-20] Motor Angle

            wbuffer[5] = 0;			// IND
            wbuffer[6] = index;		// IND

            wbuffer[7] = 0;		// PEWhi
            wbuffer[8] = 0;		// PWEhi

            wbuffer[9] = 0;		// PEWlo
            wbuffer[10] = 0;	// PWElo

            wbuffer[11] = 0;	// PCD1
            wbuffer[12] = 0;	// PCD1

            wbuffer[13] = 0;	// PCD2
            wbuffer[14] = 0;	// PCD2

            wbuffer[15] = BCC(wbuffer);		// BCC

            _port.Write(wbuffer, 0, wbuffer.Length);

            // read response
            byte[] rbuffer = new byte[16];
            for (int i = 0; i < rbuffer.Length; i++)
            {
                rbuffer[i] = (byte)_port.ReadByte();
            }			
			
			int hih = rbuffer[7];
			int loh = rbuffer[8];
			int hil = rbuffer[9];
			int lol = rbuffer[10];
			int result = 0;
			int isWord = rbuffer[3] & OP_READ;
			int isDWord = rbuffer[3] & OP_WRITE_WORD;
			if (isWord == OP_RESP_WORD)
			{
				result = (hil << 8) + lol;
			}
			else
			{
				if (isDWord == OP_RESP_DWORD)
				{
					result = (hih << 24) + (loh << 16) + (hil << 8) + lol;
				}
			}
			
			return result;
        }

		public void Start(int speed, byte inverterNo)
		{
			speed = (int)(16384.0 / 100.0 * (double)speed);

			// send request
			byte[] wbuffer = new byte[8];
			wbuffer[0] = STX;
			wbuffer[1] = 6;				// telegram length
			wbuffer[2] = inverterNo;	// inverter on bus address

			////////////////////////// fedcba9876543210
			int cmd = Convert.ToInt32("0000010001111100", 2);
			byte hi = (byte)((cmd & 0xff00) >> 8);
			byte lo = (byte)(cmd & 0xff);
			wbuffer[3] = hi;		// PCD1hi
			wbuffer[4] = lo;		// PCD1lo

			hi = (byte)((speed & 0xff00) >> 8);
			lo = (byte)(speed & 0xff);
			wbuffer[5] = hi;		// PCD2hi
			wbuffer[6] = lo;		// PCD2lo

			wbuffer[7] = BCC(wbuffer);		// BCC

			_port.Write(wbuffer, 0, wbuffer.Length);

			// read response
			byte[] rbuffer = new byte[8];
			for (int i = 0; i < rbuffer.Length; i++)
			{
				rbuffer[i] = (byte)_port.ReadByte();
			}

			//hi = rbuffer[3];
			//lo = rbuffer[4];
		}

		public void Stop(byte inverterNo)
		{
			// send request
			byte[] wbuffer = new byte[8];
			wbuffer[0] = STX;
			wbuffer[1] = 6;				// telegram length
			wbuffer[2] = inverterNo;	// inverter on bus address

			////////////////////////// fedcba9876543210
			int cmd = Convert.ToInt32("0000010000000000", 2);
			byte hi = (byte)((cmd & 0xff00) >> 8);
			byte lo = (byte)(cmd & 0xff);
			wbuffer[3] = hi;		// PCD1hi
			wbuffer[4] = lo;		// PCD1lo

			wbuffer[5] = 0;		// PCD2hi
			wbuffer[6] = 0;		// PCD2lo

			wbuffer[7] = BCC(wbuffer);		// BCC

			_port.Write(wbuffer, 0, wbuffer.Length);

			// read response
			byte[] rbuffer = new byte[8];
			for (int i = 0; i < rbuffer.Length; i++)
			{
				rbuffer[i] = (byte)_port.ReadByte();
			}

			//int hi2 = rbuffer[5];
			//int lo2 = rbuffer[6];
		}
		
		public void Close()
        {
            if (_port != null)
            {
                if (_port.IsOpen)
                {
                    _port.Close();
                }
            }
        }
    }
}

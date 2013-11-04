﻿using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace BizHawk.Common
{
	public static class Colors
	{
		public static int ARGB(byte red, byte green, byte blue)
		{
			return (int)((uint)((red << 0x10) | (green << 8) | blue | (0xFF << 0x18)));
		}

		public static int ARGB(byte red, byte green, byte blue, byte alpha)
		{
			return (int)((uint)((red << 0x10) | (green << 8) | blue | (alpha << 0x18)));
		}

		public static int Luminosity(byte lum)
		{
			return (int)((uint)((lum << 0x10) | (lum << 8) | lum | (0xFF << 0x18)));
		}
	}

	public unsafe static class Util
	{
		static readonly char[] HexConvArr = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };
		static System.Runtime.InteropServices.GCHandle HexConvHandle;
		public static char* HexConvPtr;
		static unsafe Util()
		{
			HexConvHandle = System.Runtime.InteropServices.GCHandle.Alloc(HexConvArr, System.Runtime.InteropServices.GCHandleType.Pinned);
			HexConvPtr = (char*)HexConvHandle.AddrOfPinnedObject().ToPointer();
		}

		public static string Hash_MD5(byte[] data, int offset, int len)
		{
			using (var md5 = System.Security.Cryptography.MD5.Create())
			{
				md5.TransformFinalBlock(data, offset, len);
				return Util.BytesToHexString(md5.Hash);
			}
		}

		public static string Hash_SHA1(byte[] data, int offset, int len)
		{
			using (var sha1 = System.Security.Cryptography.SHA1.Create())
			{
				sha1.TransformFinalBlock(data, offset, len);
				return Util.BytesToHexString(sha1.Hash);
			}
		}

		public static bool IsPowerOfTwo(int x)
		{
			if (x == 0) return true;
			if (x == 1) return true;
			return (x & (x - 1)) == 0;
		}

		public static int SaveRamBytesUsed(byte[] SaveRAM)
		{
			for (int j = SaveRAM.Length - 1; j >= 0; j--)
				if (SaveRAM[j] != 0)
					return j + 1;
			return 0;
		}

		// Read bytes from a BinaryReader and translate them into the UTF-8 string they represent.
		public static string ReadStringFixedAscii(this BinaryReader r, int bytes)
		{
			byte[] read = new byte[bytes];
			for (int b = 0; b < bytes; b++)
				read[b] = r.ReadByte();
			return System.Text.Encoding.UTF8.GetString(read);
		}

		public static string ReadStringAsciiZ(this BinaryReader r)
		{
			StringBuilder sb = new StringBuilder();
			for(;;)
			{
				int b = r.ReadByte();
				if(b <= 0) break;
				sb.Append((char)b);
			}
			return sb.ToString();
		}

		/// <summary>
		/// conerts bytes to an uppercase string of hex numbers in upper case without any spacing or anything
		/// //could be extension method
		/// </summary>
		public static string BytesToHexString(byte[] bytes)
		{
			StringBuilder sb = new StringBuilder();
			foreach (byte b in bytes)
				sb.AppendFormat("{0:X2}", b);
			return sb.ToString();
		}

		//could be extension method
		public static byte[] HexStringToBytes(string str)
		{
			MemoryStream ms = new MemoryStream();
			if (str.Length % 2 != 0) throw new ArgumentException();
			int len = str.Length / 2;
			for (int i = 0; i < len; i++)
			{
				int d = 0;
				for (int j = 0; j < 2; j++)
				{
					char c = char.ToLower(str[i * 2 + j]);
					if (c >= '0' && c <= '9')
						d += (c - '0');
					else if (c >= 'a' && c <= 'f')
						d += (c - 'a') + 10;
					else throw new ArgumentException();
					if (j == 0) d <<= 4;
				}
				ms.WriteByte((byte)d);
			}
			return ms.ToArray();
		}

		//could be extension method
		public static void WriteByteBuffer(BinaryWriter bw, byte[] data)
		{
			if (data == null) bw.Write(0);
			else
			{
				bw.Write(data.Length);
				bw.Write(data);
			}
		}

		public static short[] ByteBufferToShortBuffer(byte[] buf)
		{
			int num = buf.Length / 2;
			short[] ret = new short[num];
			for (int i = 0; i < num; i++)
			{
				ret[i] = (short)(buf[i * 2] | (buf[i * 2 + 1] << 8));
			}
			return ret;
		}

		public static byte[] ShortBufferToByteBuffer(short[] buf)
		{
			int num = buf.Length;
			byte[] ret = new byte[num * 2];
			for (int i = 0; i < num; i++)
			{
				ret[i * 2 + 0] = (byte)(buf[i] & 0xFF);
				ret[i * 2 + 1] = (byte)((buf[i] >> 8) & 0xFF);
			}
			return ret;
		}

		public static uint[] ByteBufferToUintBuffer(byte[] buf)
		{
			int num = buf.Length / 4;
			uint[] ret = new uint[num];
			for (int i = 0; i < num; i++)
			{
				ret[i] = (uint)(buf[i * 4] | (buf[i * 4 + 1] << 8) | (buf[i * 4 + 2] << 16) | (buf[i * 4 + 3] << 24));
			}
			return ret;
		}

		public static byte[] UintBufferToByteBuffer(uint[] buf)
		{
			int num = buf.Length;
			byte[] ret = new byte[num * 4];
			for (int i = 0; i < num; i++)
			{
				ret[i * 4 + 0] = (byte)(buf[i] & 0xFF);
				ret[i * 4 + 1] = (byte)((buf[i] >> 8) & 0xFF);
				ret[i * 4 + 2] = (byte)((buf[i] >> 16) & 0xFF);
				ret[i * 4 + 3] = (byte)((buf[i] >> 24) & 0xFF);
			}
			return ret;
		}

		public static int[] ByteBufferToIntBuffer(byte[] buf)
		{
			int num = buf.Length / 4;
			int[] ret = new int[num];
			for (int i = 0; i < num; i++)
			{
				ret[i] = buf[(i * 4) + 3];
				ret[i] <<= 8;
				ret[i] |= buf[(i * 4) + 2];
				ret[i] <<= 8;
				ret[i] |= buf[(i * 4) + 1];
				ret[i] <<= 8;
				ret[i] |= buf[(i * 4)];
			}
			return ret;
		}

		public static byte[] IntBufferToByteBuffer(int[] buf)
		{
			int num = buf.Length;
			byte[] ret = new byte[num * 4];
			for (int i = 0; i < num; i++)
			{
				ret[i * 4 + 0] = (byte)(buf[i] & 0xFF);
				ret[i * 4 + 1] = (byte)((buf[i] >> 8) & 0xFF);
				ret[i * 4 + 2] = (byte)((buf[i] >> 16) & 0xFF);
				ret[i * 4 + 3] = (byte)((buf[i] >> 24) & 0xFF);
			}
			return ret;
		}

		public static byte[] ReadByteBuffer(BinaryReader br, bool return_null)
		{
			int len = br.ReadInt32();
			if (len == 0 && return_null) return null;
			byte[] ret = new byte[len];
			int ofs = 0;
			while (len > 0)
			{
				int done = br.Read(ret, ofs, len);
				ofs += done;
				len -= done;
			}
			return ret;
		}

		public static unsafe int memcmp(void* a, string b, int len)
		{
			fixed (byte* bp = Encoding.ASCII.GetBytes(b))
				return memcmp(a, bp, len);
		}

		public static unsafe int memcmp(void* a, void* b, int len)
		{
			byte* ba = (byte*)a;
			byte* bb = (byte*)b;
			for (int i = 0; i < len; i++)
			{
				byte _a = ba[i];
				byte _b = bb[i];
				int c = _a - _b;
				if (c != 0) return c;
			}
			return 0;
		}

		public static unsafe void memset(void* ptr, int val, int len)
		{
			byte* bptr = (byte*)ptr;
			for (int i = 0; i < len; i++)
				bptr[i] = (byte)val;
		}

		public static unsafe void memset32(void* ptr, int val, int len)
		{
			System.Diagnostics.Debug.Assert(len % 4 == 0);
			int dwords = len / 4;
			int* dwptr = (int*)ptr;
			for (int i = 0; i < dwords; i++)
				dwptr[i] = val;
		}

		public static byte[] ReadAllBytes(Stream stream)
		{
			const int BUFF_SIZE = 4096;
			byte[] buffer = new byte[BUFF_SIZE];

			int bytesRead = 0;
			var inStream = new BufferedStream(stream);
			var outStream = new MemoryStream();

			while ((bytesRead = inStream.Read(buffer, 0, BUFF_SIZE)) > 0)
			{
				outStream.Write(buffer, 0, bytesRead);
			}

			return outStream.ToArray();
		}

        public static byte BinToBCD(this byte v)
        {
            return (byte) (((v / 10) * 16) + (v % 10));
        }

        public static byte BCDtoBin(this byte v)
        {
            return (byte) (((v / 16) * 10) + (v % 16));
        }

		public static string FormatFileSize(long filesize)
		{
			Decimal size = (Decimal)filesize;

			Decimal OneKiloByte = 1024M;
			Decimal OneMegaByte = OneKiloByte * 1024M;
			Decimal OneGigaByte = OneMegaByte * 1024M;

			string suffix;
			if (size > 1024 * 1024 * 1024)
			{
				size /= 1024 * 1024 * 1024;
				suffix = "GB";
			}
			else if (size > 1024 * 1024)
			{
				size /= 1024 * 1024;
				suffix = "MB";
			}
			else if (size > 1024)
			{
				size /= 1024;
				suffix = "KB";
			}
			else
			{
				suffix = " B";
			}

			string precision = "2";
			return String.Format("{0:N" + precision + "}{1}", size, suffix);
		}
	}

	public unsafe class Serializer
	{
		BinaryReader br;
		BinaryWriter bw;
		TextReader tr;
		TextWriter tw;
		public BinaryReader BinaryReader { get { return br; } }
		public BinaryWriter BinaryWriter { get { return bw; } }
		public TextReader TextReader { get { return tr; } }
		public TextWriter TextWriter { get { return tw; } }
		public Serializer() { }
		public Serializer(BinaryWriter _bw) { StartWrite(_bw); }
		public Serializer(BinaryReader _br) { StartRead(_br); }
		public Serializer(TextWriter _tw) { StartWrite(_tw); }
		public Serializer(TextReader _tr) { StartRead(_tr); }
		public static Serializer CreateBinaryWriter(BinaryWriter _bw) { return new Serializer(_bw); }
		public static Serializer CreateBinaryReader(BinaryReader _br) { return new Serializer(_br); }
		public static Serializer CreateTextWriter(TextWriter _tw) { return new Serializer(_tw); }
		public static Serializer CreateTextReader(TextReader _tr) { return new Serializer(_tr); }
		public void StartWrite(BinaryWriter _bw) { bw = _bw; isReader = false; }
		public void StartRead(BinaryReader _br) { br = _br; isReader = true; }
		public void StartWrite(TextWriter _tw) { tw = _tw; isReader = false; isText = true; }
		public void StartRead(TextReader _tr) {
			tr = _tr;
			isReader = true; 
			isText = true;
			BeginTextBlock();
		}

		public bool IsReader { get { return isReader; } }
		public bool IsWriter { get { return !IsReader; } }
		public bool IsText { get { return isText; } }
		bool isText;
		bool isReader;

		readonly Stack<string> sections = new Stack<string>();

		class Section : Dictionary<string, Section>
		{
			public string Name;
			public readonly Dictionary<string, string> Items = new Dictionary<string, string>();
		}

		Section ReaderSection, CurrSection;
		Stack<Section> SectionStack = new Stack<Section>();

		void BeginTextBlock()
		{
			if (!IsText) return;
			if (IsWriter) return;

			ReaderSection = new Section();
			ReaderSection.Name = "";
			Stack<Section> ss = new Stack<Section>();
			ss.Push(ReaderSection);
			Section curs = ReaderSection;

			var rxEnd = new System.Text.RegularExpressions.Regex(@"\[/(.*?)\]",System.Text.RegularExpressions.RegexOptions.Compiled);
			var rxBegin = new System.Text.RegularExpressions.Regex(@"\[(.*?)\]",System.Text.RegularExpressions.RegexOptions.Compiled);

			//read the entire file into a data structure for flexi-parsing
			string str;
			while ((str = tr.ReadLine()) != null)
			{
				var end = rxEnd.Match(str);
				var begin = rxBegin.Match(str);
				if (end.Success)
				{
					string name = end.Groups[1].Value;
					if (name != curs.Name) throw new InvalidOperationException("Mis-formed savestate blob");
					curs = ss.Pop();
					// consume no data past the end of the last proper section
					if (curs == ReaderSection)
					{
						CurrSection = curs;
						return;
					}
				}
				else if (begin.Success)
				{
					string name = begin.Groups[1].Value;
					ss.Push(curs);
					var news = new Section();
					news.Name = name;
					if (!curs.ContainsKey(name))
						curs[name] = news;
					else
						throw new Exception(string.Format("Duplicate key \"{0}\" in serializer savestate!", name));
					curs = news;
				}
				else
				{
					//add to current section
					if (str.Trim().Length == 0) continue;
					var parts = str.Split(' ');
					var key = parts[0];
					//UGLY: adds whole string instead of splitting the key. later, split the key, and have the individual Sync methods give up that responsibility
					if (!curs.Items.ContainsKey(key))
						curs.Items[key] = parts[1];
					else
						throw new Exception(string.Format("Duplicate key \"{0}\" in serializer savestate!", key));
				}
			}

			CurrSection = ReaderSection;
		}

		public void BeginSection(string name)
		{
			sections.Push(name);
			if (IsText) 
				if (IsWriter) { tw.WriteLine("[{0}]", name); }
				else 
				{
					SectionStack.Push(CurrSection);
					CurrSection = CurrSection[name];
				}
		}

		public void EndSection()
		{
			string name = sections.Pop();
			if (IsText)
				if (IsWriter) tw.WriteLine("[/{0}]", name);
				else
				{
					CurrSection = SectionStack.Pop();
				}
		}

		string Item(string key)
		{
			return CurrSection.Items[key];
		}

		bool Present(string key)
		{
			return CurrSection.Items.ContainsKey(key);
		}

		public void SyncEnum<T>(string name, ref T val) where T : struct
		{
			if (typeof(T).BaseType != typeof(System.Enum))
				throw new InvalidOperationException();
			if (isText) SyncEnumText<T>(name, ref val);
			else if (IsReader) val = (T)Enum.ToObject(typeof(T), br.ReadInt32());
			else bw.Write(Convert.ToInt32(val));
		}

		public void SyncEnumText<T>(string name, ref T val) where T : struct
		{
			if (IsReader) { if (Present(name)) val = (T)Enum.Parse(typeof(T), Item(name)); }
			else tw.WriteLine("{0} {1}", name, val.ToString());
		}

		void SyncBuffer(string name, int elemsize, int len, void* ptr)
		{
			if (IsReader)
			{
				byte[] temp = null;
				Sync(name, ref temp, false);
				int todo = Math.Min(temp.Length, len * elemsize);
				System.Runtime.InteropServices.Marshal.Copy(temp, 0, new IntPtr(ptr), todo);
			}
			else
			{
				int todo = len * elemsize;
				byte[] temp = new byte[todo];
				System.Runtime.InteropServices.Marshal.Copy(new IntPtr(ptr), temp, 0, todo);
				Sync(name, ref temp, false);
			}
		}

		public void Sync(string name, ref ByteBuffer byteBuf)
		{
			SyncBuffer(name, 1, byteBuf.len, byteBuf.ptr);
		}

		public void Sync(string name, ref IntBuffer byteBuf)
		{
			SyncBuffer(name, 4, byteBuf.len, byteBuf.ptr);
		}

		public void Sync(string name, ref byte[] val, bool use_null)
		{
			if (IsText) SyncText(name, ref val, use_null);
			else if (IsReader) val = Util.ReadByteBuffer(br, use_null);
			else Util.WriteByteBuffer(bw, val);
		}
		public void SyncText(string name, ref byte[] val, bool use_null)
		{
			if (IsReader)
			{
				if(Present(name)) val = Util.HexStringToBytes(Item(name));
				if (val != null && val.Length == 0 && use_null) val = null;
			}
			else
			{
				byte[] temp = val;
				if (temp == null) temp = new byte[0];
				tw.WriteLine("{0} {1}", name, Util.BytesToHexString(temp));
			}
		}

		public void Sync(string name, ref short[] val, bool use_null)
		{
			if (IsText) SyncText(name, ref val, use_null);
			else if (IsReader)
			{
				val = Util.ByteBufferToShortBuffer(Util.ReadByteBuffer(br, false));
				if (val == null && !use_null) val = new short[0];
			}
			else Util.WriteByteBuffer(bw, Util.ShortBufferToByteBuffer(val));
		}
		public void SyncText(string name, ref short[] val, bool use_null)
		{
			if (IsReader)
			{
				if (Present(name))
				{
					byte[] bytes = Util.HexStringToBytes(Item(name));
					val = Util.ByteBufferToShortBuffer(bytes);
				}
				if (val != null && val.Length == 0 && use_null) val = null;
			}
			else
			{
				short[] temp = val;
				if (temp == null) temp = new short[0];
				tw.WriteLine("{0} {1}", name, Util.BytesToHexString(Util.ShortBufferToByteBuffer(temp)));
			}
		}

		public void Sync(string name, ref int[] val, bool use_null)
		{
			if (IsText) SyncText(name, ref val, use_null);
			else if (IsReader)
			{
				val = Util.ByteBufferToIntBuffer(Util.ReadByteBuffer(br, false));
				if (val == null && !use_null) val = new int[0];
			}
			else Util.WriteByteBuffer(bw, Util.IntBufferToByteBuffer(val));
		}
		public void SyncText(string name, ref int[] val, bool use_null)
		{
			if (IsReader)
			{
				if (Present(name))
				{
					byte[] bytes = Util.HexStringToBytes(Item(name));
					val = Util.ByteBufferToIntBuffer(bytes);
				}
				if (val != null && val.Length == 0 && use_null) val = null;
			}
			else
			{
				int[] temp = val;
				if (temp == null) temp = new int[0];
				tw.WriteLine("{0} {1}", name, Util.BytesToHexString(Util.IntBufferToByteBuffer(temp)));
			}
		}

		public void Sync(string name, ref uint[] val, bool use_null)
		{
			if (IsText) SyncText(name, ref val, use_null);
			else if (IsReader)
			{
				val = Util.ByteBufferToUintBuffer(Util.ReadByteBuffer(br, false));
				if (val == null && !use_null) val = new uint[0];
			}
			else Util.WriteByteBuffer(bw, Util.UintBufferToByteBuffer(val));
		}
		public void SyncText(string name, ref uint[] val, bool use_null)
		{
			if (IsReader)
			{
				if(Present(name))
				{
					byte[] bytes = Util.HexStringToBytes(Item(name));
					val = Util.ByteBufferToUintBuffer(bytes);
				}
				if (val != null && val.Length == 0 && use_null) val = null;
			}
			else
			{
				uint[] temp = val;
				if (temp == null) temp = new uint[0];
				tw.WriteLine("{0} {1}", name, Util.BytesToHexString(Util.UintBufferToByteBuffer(temp)));
			}
		}

		public void Sync(string name, ref Bit val)
		{
			if (IsText) SyncText(name, ref val);
			else if (IsReader) Read(ref val);
			else Write(ref val);
		}
		public void SyncText(string name, ref Bit val)
		{
			if (IsReader) ReadText(name, ref val);
			else WriteText(name, ref val);
		}
		public void Sync(string name, ref byte val)
		{
			if (IsText) SyncText(name, ref val);
			else if (IsReader) Read(ref val);
			else Write(ref val);
		}
		void SyncText(string name, ref byte val)
		{
			if (IsReader) ReadText(name, ref val);
			else WriteText(name, ref val);
		}
		public void Sync(string name, ref ushort val)
		{
			if (IsText) SyncText(name, ref val);
			else if (IsReader) Read(ref val);
			else Write(ref val);
		}
		void SyncText(string name, ref ushort val)
		{
			if (IsReader) ReadText(name, ref val);
			else WriteText(name, ref val);
		}
		public void Sync(string name, ref uint val)
		{
			if (IsText) SyncText(name, ref val);
			else if (IsReader) Read(ref val);
			else Write(ref val);
		}
		void SyncText(string name, ref uint val)
		{
			if (IsReader) ReadText(name, ref val);
			else WriteText(name, ref val);
		}
		public void Sync(string name, ref sbyte val)
		{
			if (IsText) SyncText(name, ref val);
			else if (IsReader) Read(ref val);
			else Write(ref val);
		}
		void SyncText(string name, ref sbyte val)
		{
			if (IsReader) ReadText(name, ref val);
			else WriteText(name, ref val);
		}
		public void Sync(string name, ref short val)
		{
			if (IsText) SyncText(name, ref val);
			else if (IsReader) Read(ref val);
			else Write(ref val);
		}
		void SyncText(string name, ref short val)
		{
			if (IsReader) ReadText(name, ref val);
			else WriteText(name, ref val);
		}
		public void Sync(string name, ref int val)
		{
			if (IsText) SyncText(name, ref val);
			else if (IsReader) Read(ref val);
			else Write(ref val);
		}
		void SyncText(string name, ref int val)
		{
			if (IsReader) ReadText(name, ref val);
			else WriteText(name, ref val);
		}
		public void Sync(string name, ref bool val)
		{
			if (IsText) SyncText(name, ref val);
			else if (IsReader) Read(ref val);
			else Write(ref val);
		}
		void SyncText(string name, ref bool val)
		{
			if (IsReader) ReadText(name, ref val);
			else WriteText(name, ref val);
		}
		public void SyncFixedString(string name, ref string val, int length)
		{
			//TODO - this could be made more efficient perhaps just by writing values right out of the string..

			if (IsReader)
			{
				char[] buf = new char[length];
				if (isText)
				{
					tr.Read(buf, 0, length);
				}
				else
				{
					br.Read(buf, 0, length);
				}
				int len = 0;
				for (; len < length; len++)
				{
					if (buf[len] == 0) break;
				}
				val = new string(buf, 0, len);
			}
			else
			{
				if (name.Length > length) throw new InvalidOperationException("SyncFixedString too long");
				char[] buf = val.ToCharArray();
				char[] remainder = new char[length - buf.Length];
				if (IsText)
				{
					tw.Write(buf);
					tw.Write(remainder);
				}
				else
				{
					bw.Write(buf);
					bw.Write(remainder);
				}
			}
		}

		void Read(ref Bit val) { val = br.ReadBit(); }
		void Write(ref Bit val) { bw.WriteBit(val); }
		void ReadText(string name, ref Bit val) { if(Present(name)) val = (Bit)int.Parse(Item(name)); }
		void WriteText(string name, ref Bit val) { tw.WriteLine("{0} {1}", name, (int)val); }

		void Read(ref byte val) { val = br.ReadByte(); }
		void Write(ref byte val) { bw.Write(val); }
		void ReadText(string name, ref byte val) { if (Present(name)) val = byte.Parse(Item(name).Replace("0x", ""), NumberStyles.HexNumber); }
		void WriteText(string name, ref byte val) { tw.WriteLine("{0} 0x{1:X2}", name, val); }

		void Read(ref ushort val) { val = br.ReadUInt16(); }
		void Write(ref ushort val) { bw.Write(val); }
		void ReadText(string name, ref ushort val) { if (Present(name)) val = ushort.Parse(Item(name).Replace("0x", ""), NumberStyles.HexNumber); }
		void WriteText(string name, ref ushort val) { tw.WriteLine("{0} 0x{1:X4}", name, val); }

		void Read(ref uint val) { val = br.ReadUInt32(); }
		void Write(ref uint val) { bw.Write(val); }
		void ReadText(string name, ref uint val) { if (Present(name)) val = uint.Parse(Item(name).Replace("0x", ""), NumberStyles.HexNumber); }
		void WriteText(string name, ref uint val) { tw.WriteLine("{0} 0x{1:X8}", name, val); }

		void Read(ref sbyte val) { val = br.ReadSByte(); }
		void Write(ref sbyte val) { bw.Write(val); }
		void ReadText(string name, ref sbyte val) { if (Present(name)) val = sbyte.Parse(Item(name).Replace("0x", ""), NumberStyles.HexNumber); }
		void WriteText(string name, ref sbyte val) { tw.WriteLine("{0} 0x{1:X2}", name, val); }

		void Read(ref short val) { val = br.ReadInt16(); }
		void Write(ref short val) { bw.Write(val); }
		void ReadText(string name, ref short val) { if (Present(name)) val = short.Parse(Item(name).Replace("0x", ""), NumberStyles.HexNumber); }
		void WriteText(string name, ref short val) { tw.WriteLine("{0} 0x{1:X4}", name, val); }

		void Read(ref int val) { val = br.ReadInt32(); }
		void Write(ref int val) { bw.Write(val); }
		void ReadText(string name, ref int val) { if (Present(name)) val = int.Parse(Item(name).Replace("0x", ""), NumberStyles.HexNumber); }
		void WriteText(string name, ref int val) { tw.WriteLine("{0} 0x{1:X8}", name, val); }

		void Read(ref bool val) { val = br.ReadBoolean(); }
		void Write(ref bool val) { bw.Write(val); }
		void ReadText(string name, ref bool val) { if (Present(name)) val = bool.Parse(Item(name)); }
		void WriteText(string name, ref bool val) { tw.WriteLine("{0} {1}", name, val); }
	}


	public static class BITREV
	{
		public static byte[] byte_8;
		static BITREV()
		{
			make_byte_8();
		}
		static void make_byte_8()
		{
			int bits = 8;
			int n = 1 << 8;
			byte_8 = new byte[n];

			int m = 1;
			int a = n >> 1;
			int j = 2;

			byte_8[0] = 0;
			byte_8[1] = (byte)a;

			while ((--bits) != 0)
			{
				m <<= 1;
				a >>= 1;
				for (int i = 0; i < m; i++)
					byte_8[j++] = (byte)(byte_8[i] + a);
			}
		}

		public static uint reverse_32(uint v)
		{
			return (uint)((byte_8[v & 0xff] << 24) |
					(byte_8[(v >> 8) & 0xff] << 16) |
					(byte_8[(v >> 16) & 0xff] << 8) |
					(byte_8[(v >> 24) & 0xff]));
		}
	}

	/// <summary>
	/// a Dictionary-of-lists with key K and values List&lt;V&gt;
	/// </summary>
	[Serializable]
	public class Bag<K, V> : BagBase<K, V, Dictionary<K, List<V>>, List<V>> { }

	/// <summary>
	/// a Dictionary-of-lists with key K and values List&lt;V&gt;
	/// </summary>
	[Serializable]
	public class SortedBag<K, V> : BagBase<K, V, SortedDictionary<K, List<V>>, List<V>> { }

	/// <summary>
	/// A dictionary that creates new values on the fly as necessary so that any key you need will be defined. 
	/// </summary>
	/// <typeparam name="K">dictionary keys</typeparam>
	/// <typeparam name="V">dictionary values</typeparam>
	public class WorkingDictionary<K, V> : Dictionary<K, V> where V : new()
	{
		public new V this[K key]
		{
			get
			{
				V temp;
				if (!TryGetValue(key, out temp))
					temp = this[key] = new V();
				return temp;
			}
			set { base[key] = value; }
		}
	}

	/// <summary>
	/// base class for Bag and SortedBag
	/// </summary>
	/// <typeparam name="K">dictionary keys</typeparam>
	/// <typeparam name="V">list values</typeparam>
	/// <typeparam name="D">dictionary type</typeparam>
	/// <typeparam name="L">list type</typeparam>
	[Serializable]
	public class BagBase<K, V, D, L> : IEnumerable<V>
		where D : IDictionary<K, L>, new()
		where L : IList<V>, IEnumerable<V>, new()
	{
		D dictionary = new D();
		public void Add(K key, V val)
		{
			this[key].Add(val);
		}

		public bool ContainsKey(K key) { return dictionary.ContainsKey(key); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
		public IEnumerator<V> GetEnumerator()
		{
			foreach (L lv in dictionary.Values)
				foreach (V v in lv)
					yield return v;
		}

		public IEnumerable KeyValuePairEnumerator { get { return dictionary; } }

		/// <summary>
		/// the list of keys contained herein
		/// </summary>
		public IList<K> Keys { get { return new List<K>(dictionary.Keys); } }

		public L this[K key]
		{
			get
			{
				L slot;
				if (!dictionary.TryGetValue(key, out slot))
					dictionary[key] = slot = new L();
				return slot;
			}
			set
			{
				dictionary[key] = value;
			}
		}
	}

    public class NotTestedException : Exception
    {
    }
}

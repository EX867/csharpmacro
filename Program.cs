using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
 
namespace Hello_World
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("program started");
            try{
                using (StreamWriter writer_keyboard = new(filename_keyboard))
                using (StreamWriter writer_mouse = new(filename_mouse))
                {
                    send(writer_keyboard, 'a');
                    send(writer_keyboard, 's');
                    send(writer_keyboard, 'd');
                    send(writer_keyboard, 'f');
                }
            }catch(IOException e)
            {
                Console.WriteLine("Can't write to file! " + e.ToString());
            }
        }

        const String filename_keyboard = "/dev/hidg0";
        const String filename_mouse = "/dev/hidg1";
        static byte[] packet = new byte[8];
        const int Keyboard_Packet_Size = 8;
        const int Mouse_Packet_Size = 3;

        static void send(StreamWriter writer_keyboard, KeyboardVal val, KeyboardMod mod = KeyboardMod.Null, byte offset = 0)
        {
            packet[0] = (byte)(packet[0] | (byte)mod);
            packet[2 + offset] = (byte)val;
            writer_keyboard.BaseStream.Write(packet, 0, Keyboard_Packet_Size);
        }

        static void send(StreamWriter writer_keyboard, char c, KeyboardMod mod = KeyboardMod.Null, byte offset = 0)
        {
            packet[0] = (byte)(packet[0] | (byte)mod);
            packet[2 + offset] = (byte)(c - ('a' - 0x04));
            writer_keyboard.BaseStream.Write(packet, 0, Keyboard_Packet_Size);
        }

        static void send(StreamWriter writer_mouse, MouseVal val)
        {
            packet[0] = (byte)(packet[0] | (byte)val);
            writer_mouse.BaseStream.Write(packet, 0, Mouse_Packet_Size);
        }
        static void send(StreamWriter writer_mouse, byte scroll, byte offset = 0)
        {
            packet[1 + offset] = scroll;
            writer_mouse.BaseStream.Write(packet, 0, Mouse_Packet_Size);
        }

        static void clear_packet()
        {
            Array.Fill(packet, (byte)0);
        }
    }

    enum KeyboardMod : byte
    {
        Null = 0x00,
        Ctrl_L = 0x01,
        Ctrl_R = 0x10,
        Shift_L = 0x02,
        Shift_R = 0x20,
        Alt_L = 0x04,
        Alt_R = 0x40,
        Meta_L = 0x08,
        Meta_R = 0x80,
    }

    enum KeyboardVal : byte
    {
        Null = 0x00,
        Enter = 0x28,
        Esc = 0x29,
        Backspace = 0x2a,
        Tab = 0x2b,
        Space = 0x2c,
        Capslock = 0x39,
        F1 = 0x3a,
        F2 = 0x3b,
        F3 = 0x3c,
        F4 = 0x3d,
        F5 = 0x3e,
        F6 = 0x3f,
        F7 = 0x40,
        F8 = 0x41,
        F9 = 0x42,
        F10 = 0x43,
        F11 = 0x44,
        F12 = 0x45,
        Insert = 0x49,
        Home = 0x4a,
        Pageup = 0x4b,
        Delete = 0x4c,
        End = 0x4d,
        Pagedown = 0x4e,
        Right = 0x4f,
        Left = 0x50,
        Down = 0x51,
        Enter_Keypad = 0x58,
        Up = 0x52,
        Numlock = 0x53,
    }

    enum MouseVal : byte
    {
        Left = 0x01,
        Scroll = 0x02,
        Right = 0x04,
    }
}

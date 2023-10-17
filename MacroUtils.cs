using System;
using System.Collections;
using System.Collections.Generic;
namespace Macro{

    class MacroUtils{

        // 변경
        public uint ScreenWidth = 1920;
        public uint ScreenHeight = 1080;

        public MacroUtils(){
            // empty
        }

        public MacroUtils(uint ScreenWidth_, uint ScreenHeight_){
            ScreenWidth = ScreenWidth_;
            ScreenHeight = ScreenHeight_;
        }

        byte[] packet_keyboard = new byte[9];
        byte[] packet_mouse = new byte[9];
        const int packet_size_keyboard = 8;
        const int packet_size_mouse_relative = 4;
        const int packet_size_mouse_absolute = 6;

        List<int> pressed_keys = new List<int>();

        public class Data{

            public MacroUtils utils;
            public StreamWriter writer_keyboard;
            public StreamWriter writer_mouse;

            public int eventCode = -1;
            public int eventValue = -1;

            public Data(MacroUtils utils_, StreamWriter writer_keyboard_, StreamWriter writer_mouse_){
                utils = utils_;
                writer_keyboard = writer_keyboard_;
                writer_mouse = writer_mouse_;
            }
        }

        public void Run(Action<Data> action){
            Console.WriteLine("[MacroUtils] Macro start");
            
            try{
                using (StreamWriter writer_keyboard = new("/dev/hidg0"))
                using (StreamWriter writer_mouse = new("/dev/hidg1")){
                    Data data = new Data(this, writer_keyboard, writer_mouse);
                    action(data);
                }
            }catch(IOException e){
                Console.WriteLine("Can't write to file! " + e.ToString());
            }
            return;

            try{
                using (FileStream reader = new FileStream("/dev/input/event0", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                using (StreamWriter writer_keyboard = new("/dev/hidg0"))
                using (StreamWriter writer_mouse = new("/dev/hidg1")){
                    byte[] buffer = new byte[24];
                    Data data = new Data(this, writer_keyboard, writer_mouse);

                    while (true){
                        reader.Read(buffer, 0, buffer.Length);

                        int eventType = BitConverter.ToInt16(buffer, 16); // timeval 8?
                        data.eventCode = BitConverter.ToInt16(buffer, 18);
                        data.eventValue = BitConverter.ToInt32(buffer, 20);

                        if(eventType != (int)EventType.Keyboard){
                            continue;
                        }

                        if(data.eventValue == (int)EventValue.KeyPressed){
                            pressed_keys.Add(data.eventCode);
                        }else if(data.eventValue == (int)EventValue.KeyReleased){
                            pressed_keys.Remove(data.eventCode);
                        }
                        Console.WriteLine($"[MacroUtils] type: {eventType} key: {((KeyCodeInput)data.eventCode).ToString()} value: {data.eventValue}");
                        action(data);
                    }
                }
            }catch(IOException e){
                Console.WriteLine("Can't write to file! " + e.ToString());
            }
        }

        public bool IsKeyPressed(KeyCode key){
            return pressed_keys.Contains((int)key);
        }

        public void WaitMs(int time_ms){
            Thread.Sleep(time_ms);
        }
        
        public void WaitS(int time_s){
            Thread.Sleep(time_s * 1000);
        }
        
        public void WaitM(int time_m){
            Thread.Sleep(time_m * 60 * 1000);
        }
        
        /*public void WaitH(int time_h){
            TimeSpan ts = TimeSpan.FromMinutes(time_h);
            // 있어야됨??
        }*/

        int keyboard_offset = 0;

        public void SendKeyDown(StreamWriter writer_keyboard, KeyCode val, Action action = null, int delay = 64, bool hold = false){
            // Console.WriteLine($"[SendKeyDown] key : {(int)val}");

            packet_keyboard[0] = 1;
            packet_keyboard[3 + keyboard_offset] = (byte)val;
            keyboard_offset++;

            writer_keyboard.BaseStream.Write(packet_keyboard, 0, packet_size_keyboard);
            writer_keyboard.Flush();
            Thread.Sleep(delay);
            
            action?.Invoke();
            if(!hold){
                SendKeyUp(writer_keyboard, delay);
            }
        }

        public void SendKeyDown(StreamWriter writer_keyboard, KeyMod mod, Action action = null, int delay = 64){
            packet_keyboard[0] = 1;
            packet_keyboard[1] = (byte)(packet_keyboard[1] | (byte)mod);

            writer_keyboard.BaseStream.Write(packet_keyboard, 0, packet_size_keyboard);
            writer_keyboard.Flush();
            Thread.Sleep(delay);
            
            action?.Invoke();
            
            packet_keyboard[1] = (byte)(packet_keyboard[1] & ~(byte)mod);
            
            writer_keyboard.BaseStream.Write(packet_keyboard, 0, packet_size_keyboard);
            writer_keyboard.Flush();
            Thread.Sleep(delay);
        }

        public void SendKeyUp(StreamWriter writer_keyboard, int delay = 64){
            keyboard_offset--;
            packet_keyboard[3 + keyboard_offset] = 0;

            writer_keyboard.BaseStream.Write(packet_keyboard, 0, packet_size_keyboard);
            writer_keyboard.Flush();
            Thread.Sleep(delay);
        }

        public void SendMouseDown(StreamWriter writer_mouse, MouseCode val, int delay = 64){
            Array.Fill(packet_mouse, (byte)0);

            packet_mouse[0] = 2;
            packet_mouse[1] = (byte)(packet_mouse[1] | (byte)val);

            writer_mouse.BaseStream.Write(packet_mouse, 0, packet_size_mouse_relative);
            writer_mouse.Flush();
            Thread.Sleep(delay);
            
            packet_mouse[1] = 0;

            writer_mouse.BaseStream.Write(packet_mouse, 0, packet_size_mouse_relative);
            writer_mouse.Flush();
            Thread.Sleep(delay);
        }

        public void SendMouseMoveRelative(StreamWriter writer_mouse, int x, int y, int delay = 64){
            Array.Fill(packet_mouse, (byte)0);

            packet_mouse[0] = 2;
            packet_mouse[2] = (byte)x; // TODO : 해상도 기준으로 변경 필요
            packet_mouse[3] = (byte)y;

            writer_mouse.BaseStream.Write(packet_mouse, 0, packet_size_mouse_relative);
            writer_mouse.Flush();
            Thread.Sleep(delay);
        }

        public void SendMouseMoveAbsolute(StreamWriter writer_mouse, uint x, uint y, int delay = 64){
            Array.Fill(packet_mouse, (byte)0);

            uint x_ = x * 32767 / ScreenWidth;
            uint y_ = y * 32767 / ScreenHeight;
            packet_mouse[0] = 3;
            // big endian
            packet_mouse[1] = (byte)(x_ % 256);
            packet_mouse[2] = (byte)(x_ / 256);
            packet_mouse[3] = (byte)(y_ % 256);
            packet_mouse[4] = (byte)(y_ / 256);

            writer_mouse.BaseStream.Write(packet_mouse, 0, packet_size_mouse_absolute);
            writer_mouse.Flush();
            Thread.Sleep(delay);
        }

        public void SendMouseWheel(StreamWriter writer_mouse, int wheel_signed_byte, int delay = 64){
            Array.Fill(packet_mouse, (byte)0);

            packet_mouse[0] = 3;
            packet_mouse[5] = (byte)wheel_signed_byte; // postive : down

            writer_mouse.BaseStream.Write(packet_mouse, 0, packet_size_mouse_absolute);
            writer_mouse.Flush();
            Thread.Sleep(delay);
        }
    }    
        
    public enum EventType{
        Separator = 0,
        Keyboard = 1,
        Relative = 2, // mouse
        Absolute = 3, // mouse
        Error = 4
    }

    public enum EventValue{
        KeyReleased = 0,
        KeyPressed = 1,
        KeyHold = 2
    }

    public enum KeyMod : byte{
        Null = 0x00,
        CtrlL = 0x01,
        CtrlR = 0x10,
        ShiftL = 0x02,
        ShiftR = 0x20,
        AltL = 0x04,
        AltR = 0x40,
        MetaL = 0x08,
        MetaR = 0x80,
    }

    public enum KeyCode : byte{
        // https://gist.github.com/MightyPork/6da26e382a7ad91b5496ee55fdc73db2

        Null = 0x00,
        ErrorOVF = 0x01,
        // PostFail = 0x02,
        // ErrorUndef = 0x02,

        KeyA = 0x04,
        KeyB = 0x05,
        KeyC = 0x06,
        KeyD = 0x07,
        KeyE = 0x08,
        KeyF = 0x09,
        KeyG = 0x0a,
        KeyH = 0x0b,
        KeyI = 0x0c,
        KeyJ = 0x0d,
        KeyK = 0x0e,
        KeyL = 0x0f,
        KeyM = 0x10,
        KeyN = 0x11,
        KeyO = 0x12,
        KeyP = 0x13,
        KeyQ = 0x14,
        KeyR = 0x15,
        KeyS = 0x16,
        KeyT = 0x17,
        KeyU = 0x18,
        KeyV = 0x19,
        KeyW = 0x1a,
        KeyX = 0x1b,
        KeyY = 0x1c,
        KeyZ = 0x1d,

        Key1 = 0x1e,
        Key2 = 0x1f,
        Key3 = 0x20,
        Key4 = 0x21,
        Key5 = 0x22,
        Key6 = 0x23,
        Key7 = 0x24,
        Key8 = 0x25,
        Key9 = 0x26,
        Key0 = 0x27,

        Enter = 0x28,
        Esc = 0x29,
        Backspace = 0x2a,
        Tab = 0x2b,
        Space = 0x2c,
        Minus = 0x2d,
        Equal = 0x2e,
        LBrace = 0x2f,
        RBrace = 0x30,
        Backslash = 0x31,
        HashTilde = 0x32,
        Semicolon = 0x33,
        Apostrophe = 0x34,
        Grave = 0x35,
        Comma = 0x36,
        Dot = 0x37,
        Slash = 0x38,
        CapsLock = 0x39,

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
        
        PrintScr = 0x46,
        ScrollLock = 0x47,
        Pause = 0x48,
        Insert = 0x49,
        Home = 0x4a,
        PageUp = 0x4b,
        Delete = 0x4c,
        End = 0x4d,
        PageDown = 0x4e,
        Right = 0x4f,
        Left = 0x50,
        Down = 0x51,
        Up = 0x52,
        
        KpNumLock = 0x53,
        KpSlash = 0x54,
        KpAsterisk = 0x55,
        KpMinus = 0x56,
        KpPlus = 0x57,
        KpEnter = 0x58,

        KpKey1 = 0x59,
        KpKey2 = 0x5a,
        KpKey3 = 0x5b,
        KpKey4 = 0x5c,
        KpKey5 = 0x5d,
        KpKey6 = 0x5e,
        KpKey7 = 0x5f,
        KpKey8 = 0x60,
        KpKey9 = 0x61,
        KpKey0 = 0x62,
        KpDot = 0x63,

        Kp102ND = 0x64,
        KpCompose = 0x65,
        KpPower = 0x66,
        KpEqual = 0x67
    }

    public enum MouseCode : byte{
        Left = 0x01,
        Middle = 0x02,
        Right = 0x04,
    }
    
    public enum KeyCodeInput{
        Key1 = 2,
        Key2,
        Key3,
        Key4,
        Key5,
        Key6,
        Key7,
        Key8,
        Key9,
        Key0,
        Minus,
        Equal,
        Backspace,
        Tab,
        KeyQ,
        KeyW,
        KeyE,
        KeyR,
        KeyT,
        KeyY,
        KeyU,
        KeyI,
        KeyO,
        KeyP,
        BraceL,
        BraceR,
        Enter,
        CtrlL,
        KeyA,
        KeyS,
        KeyD,
        KeyF,
        KeyG,
        KeyH,
        KeyJ,
        KeyK,
        KeyL,
        Semicolon,
        Apostrophe,
        Grave,
        ShiftL,
        Backslash,
        KeyZ,
        KeyX,
        KeyC,
        KeyV,
        KeyB,
        KeyN,
        KeyM,
        Comma,
        Dot,
        Slash,
        ShiftR,
        KpAsterisk,
        AltL,
        Space,
        CapsLock,
        KeyF1,
        KeyF2,
        KeyF3,
        KeyF4,
        KeyF5,
        KeyF6,
        KeyF7,
        KeyF8,
        KeyF9,
        KeyF10,
        KpNumLock,
        KpScrollLock,
        KpKey7,
        KpKey8,
        KpKey9,
        KpMinus,
        KpKey4,
        KpKey5,
        KpKey6,
        KpPlus,
        KpKey1,
        KpKey2,
        KpKey3,
        KpKey0,
        KpDot
    }
}

/*
struct keyboard_report_t [size = 9]
{
    uint8_t report_id = 1;
    uint8_t options;
    uint8_t padding;
    uint8_t keys[6];
};

struct mouse_rel_report_t [size = 4]
{
    uint8_t report_id = 2;
    uint8_t buttons;
    int8_t x;
    int8_t y;
};

struct mouse_abs_report_t [size = 6]
{
    uint8_t report_id = 3;
    int16_t x;
    int16_t y;
    int8_t wheel;
};

*/

// https://www.sysnet.pe.kr/2/0/11355
// https://github.com/jonatanklosko/gerbil/blob/main/bin/enable_mouse_hid_gadget.sh

/*
05, 01      USAGE_PAGE (Generic Desktop)
09, 06      USAGE (Keyboard)
a1, 01      COLLECTION (Application)
85, 01          REPORT_ID (1)
05, 07          USAGE_PAGE (Keyboard)
19, e0          USAGE_MINIMUM (Keyboard LeftControl)
29, e7          USAGE_MAXIMUM (Keyboard Right GUI)
15, 00          LOGICAL_MINIMUM (0)
25, 01          LOGICAL_MAXIMUM (1)
75, 01          REPORT_SIZE (1)
95, 08          REPORT_COUNT (8)
81, 02          INPUT (Data, Var, Abs)
95, 01          REPORT_COUNT (1)
75, 08          REPORT_SIZE (8)
81, 03          INPUT (Cnst, Var, Abs)
95, 05          REPORT_COUNT (5)
75, 01          REPORT_SIZE (1)
05, 08          USAGE_PAGE (LEDs)
19, 01          USAGE_MINIMUM (Num Lock)
29, 05          USAGE_MAXIMUM (Kana)
91, 02          OUTPUT (Data, Var, Abs)
95, 01          REPORT_COUNT (1)
75, 03          REPORT_SIZE (3)
91, 03          OUTPUT (Cnst, Var, Abs)
95, 06          REPORT_COUNT (6)
75, 08          REPORT_SIZE (8)
15, 00          LOGICAL_MINIMUM (0)
25, 65          LOGICAL_MAXIMUM (101)
05, 07          USAGE_PAGE (Keyboard)
19, 00          USAGE_MINIMUM (Reserved (no event indicated))
29, 65          USAGE_MAXIMUM (Keyboard Application)
81, 00          INPUT (Data, Ary, Abs)
C0          END_COLLECTION

05, 01  USAGE_PAGE (Generic Desktop)
09, 02  USAGE (Mouse)
a1, 01  COLLECTION (Application)
09, 01      USAGE (Pointer)
a1, 00      COLLECTION (Physical)
85, 02          REPORT_ID (2)
05, 09          USAGE_PAGE (Button)
19, 01          USAGE_MINIMUM (Button 1)
29, 03          USAGE_MAXIMUM (Button 3)
15, 00          LOGICAL_MINIMUM (0)
25, 01          LOGICAL_MAXIMUM (1)
95, 03          REPORT_COUNT (3)
75, 01          REPORT_SIZE (1)
81, 02          INPUT (Data, Var, Abs)
95, 01          REPORT_COUNT (1)
75, 05          REPORT_SIZE (5)
81, 03          INPUT (Cnst, Var, Abs)
05, 01          USAGE_PAGE (Generic Desktop)
09, 30          USAGE (X)
09, 31          USAGE (Y)
15, 81          LOGICAL_MINIMUM (-127)
25, 7f          LOGICAL_MAXIMUM (127)
75, 08          REPORT_SIZE (8)
95, 02          REPORT_COUNT (2)
81, 06          INPUT (Data, Var, Rel)
C0          END_COLLECTION
C0      END_COLLECTION

05, 01  USAGE_PAGE (Generic Desktop)
09, 02  USAGE (Mouse)
a1, 01  COLLECTION (Application)
09, 01      USAGE (Pointer)
a1, 00      COLLECTION (Physical)
85, 03          REPORT_ID (3)
05, 01          USAGE_PAGE (Generic Desktop)
09, 30          USAGE (X)
09, 31          USAGE (Y)
15, 00,         LOGICAL_MINIMUM(0)
26, ff, 7f      LOGICAL_MAXIMUM(32767)
75, 10          REPORT_SIZE (16)
95, 02          REPORT_COUNT (2)
81, 02          INPUT (Data, Var, Abs)
09, 38          USAGE (Wheel)
15, 81          LOGICAL_MINIMUM (-127)
25, 7f          LOGICAL_MAXIMUM (127)
75, 08          REPORT_SIZE (8)
95, 01          REPORT_COUNT (1)
81, 06          INPUT (Data, Var, Rel)
C0          END_COLLECTION
C0      END_COLLECTION
*/
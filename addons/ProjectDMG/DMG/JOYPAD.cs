using static ProjectDMG.Utils.BitOps;
using Godot;
using System.Collections.Generic;
using System;

namespace ProjectDMG {
    public partial class JOYPAD : Node
    {

        private const int JOYPAD_INTERRUPT = 4;
        private const byte PAD_MASK = 0x10;
        private const byte BUTTON_MASK = 0x20;
        private byte pad = 0xF;
        private byte buttons = 0xF;

        Key previousKey = Key.None;
        bool keyPassed;

        static Dictionary<Key, uint> keyMappings = new Dictionary<Key, uint>()
            {
                {Key.A, 27},
                {Key.B, 28},
                {Key.C, 29},
                {Key.D, 30},
                {Key.E, 31},
                {Key.F, 32},
                {Key.G, 33},
                {Key.H, 34},
                {Key.I, 35},
                {Key.J, 36},
                {Key.K, 37},
                {Key.L, 38},
                {Key.M, 39},
                {Key.N, 40},
                {Key.O, 41},
                {Key.P, 42},
                {Key.Q, 43},
                {Key.R, 44},
                {Key.S, 45},
                {Key.T, 46},
                {Key.U, 47},
                {Key.V, 48},
                {Key.W, 49},
                {Key.X, 50},
                {Key.Y, 51},
                {Key.Z, 52},

                {Key.Key0, 53},
                {Key.Key1, 54},
                {Key.Key2, 55},
                {Key.Key3, 56},
                {Key.Key4, 57},
                {Key.Key5, 58},
                {Key.Key6, 59},
                {Key.Key7, 60},
                {Key.Key8, 61},
                {Key.Key9, 62},

                {Key.Period, 63},
                {Key.Comma, 64},

                {Key.Apostrophe, 66},

                {Key.Minus, 79},
                {Key.Slash, 80},

                {Key.Semicolon, 82},

                {Key.Equal, 84},

                {Key.Bracketleft, 86},
                {Key.Bracketright, 87},
                {Key.Backslash, 88},

                {Key.Space, 95},

                {Key.Up, 251},
                {Key.Right, 252},
                {Key.Down, 253},
                {Key.Left, 254},
                {Key.Backspace, 255}
            };

            static Dictionary<Key, uint> shiftKeyMappings = new Dictionary<Key, uint>()
            {
                {Key.A, 1},
                {Key.B, 2},
                {Key.C, 3},
                {Key.D, 4},
                {Key.E, 5},
                {Key.F, 6},
                {Key.G, 7},
                {Key.H, 8},
                {Key.I, 9},
                {Key.J, 10},
                {Key.K, 11},
                {Key.L, 12},
                {Key.M, 13},
                {Key.N, 14},
                {Key.O, 15},
                {Key.P, 16},
                {Key.Q, 17},
                {Key.R, 18},
                {Key.S, 19},
                {Key.T, 20},
                {Key.U, 21},
                {Key.V, 22},
                {Key.W, 23},
                {Key.X, 24},
                {Key.Y, 25},
                {Key.Z, 26},

                {Key.Key0, 77},
                {Key.Key1, 68},
                {Key.Key2, 69},
                {Key.Key3, 72},
                {Key.Key4, 73},
                {Key.Key5, 74},
                {Key.Key6, 89},
                {Key.Key7, 75},
                {Key.Key8, 71},
                {Key.Key9, 76},

                {Key.Period, 85},
                {Key.Comma, 83},

                {Key.Apostrophe, 65},

                {Key.Minus, 70},
                {Key.Slash, 67},

                {Key.Semicolon, 81},

                {Key.Equal, 78},

                {Key.Bracketleft, 91},
                {Key.Bracketright, 92},
                {Key.Backslash, 93},

                {Key.Space, 95},

                {Key.Up, 251},
                {Key.Right, 252},
                {Key.Down, 253},
                {Key.Left, 254},
                {Key.Backspace, 255}
            };

        public void HandleInput()
        {
            if(Input.IsActionPressed("Arcade Up")){handleKeyDown(0x14);}
            if(!Input.IsActionPressed("Arcade Up")){handleKeyUp(0x14);}
            if(Input.IsActionPressed("Arcade Down")){handleKeyDown(0x18);}
            if(!Input.IsActionPressed("Arcade Down")){handleKeyUp(0x18);}
            if(Input.IsActionPressed("Arcade Left")){handleKeyDown(0x12);}
            if(!Input.IsActionPressed("Arcade Left")){handleKeyUp(0x12);}
            if(Input.IsActionPressed("Arcade Right")){handleKeyDown(0x11);}
            if(!Input.IsActionPressed("Arcade Right")){handleKeyUp(0x11);}
            if(Input.IsActionPressed("Arcade A")){handleKeyDown(0x21);}
            if(!Input.IsActionPressed("Arcade A")){handleKeyUp(0x21);}
            if(Input.IsActionPressed("Arcade B")){handleKeyDown(0x22);}
            if(!Input.IsActionPressed("Arcade B")){handleKeyUp(0x22);}
            if(Input.IsActionPressed("Arcade Start")){handleKeyDown(0x28);}
            if(!Input.IsActionPressed("Arcade Start")){handleKeyUp(0x28);}
            if(Input.IsActionPressed("Arcade Select")){handleKeyDown(0x24);}
            if(!Input.IsActionPressed("Arcade Select")){handleKeyUp(0x24);}
        }

        public void HandleKeyboardInput()
        {     
            //Standard Keys
            if (Input.IsKeyPressed(Key.Shift))
            {
                SetKeyType(shiftKeyMappings);
            }
            else
            {
                SetKeyType(keyMappings);
            }
            if(keyPassed)  
            {                            
                buttons = 0xF;
                pad = 0xF;    
            }  
        }

        void SetKeyType(Dictionary<Key, uint> dict)
        {
            foreach (Key key in dict.Keys)
            {
                if (Input.IsKeyPressed(key))
                {
                    if(previousKey == key)
                    {   
                        if(keyPassed)  
                        {
                            buttons = 0xF;
                            pad = 0xF;    
                        }  
                        return;
                    }
                    SetByte(dict[key]);
                    previousKey = key;
                    keyPassed = false;
                    return;
                }
                if(key == previousKey && !Input.IsKeyPressed(key))
                {
                    previousKey = Key.None;
                }
            }
        }

        public void SetByte(uint num)
        {
            if(num == 0)
            {
                return;
            }

            pad = (byte)((num & 0xF) ^ 0xF);
            buttons = (byte)(((num >> 4) & 0xF) ^ 0xF);            
        }

        internal void handleKeyDown(byte b) {
            if ((b & PAD_MASK) == PAD_MASK) {
                pad = (byte)(pad & ~(b & 0xF));
            } else if((b & BUTTON_MASK) == BUTTON_MASK) {
                buttons = (byte)(buttons & ~(b & 0xF));
            }
        }

        internal void handleKeyUp(byte b) {
            if ((b & PAD_MASK) == PAD_MASK) {
                pad = (byte)(pad | (b & 0xF));
            } else if ((b & BUTTON_MASK) == BUTTON_MASK) {
                buttons = (byte)(buttons | (b & 0xF));
            }
        }

        public void update(MMU mmu) {
                byte JOYP = mmu.JOYP;
                if(!isBit(4, JOYP)) {
                    mmu.JOYP = (byte)((JOYP & 0xF0) | pad);
                    if(pad != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
                    keyPassed = true;
                }
                if (!isBit(5, JOYP)) {
                    mmu.JOYP = (byte)((JOYP & 0xF0) | buttons);
                    if (buttons != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
                    keyPassed = true;
                }
                if ((JOYP & 0b00110000) == 0b00110000) mmu.JOYP = 0xFF;
        }

    }
}

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

        static Dictionary<Key, uint> keyMappings = new Dictionary<Key, uint>()
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
                {Key.Z, 26}
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
            buttons = 0xF;
            pad = 0xF;
            //Standard Keys
            foreach (Key key in keyMappings.Keys)
            {
                if (Input.IsKeyPressed(key))
                {
                    SetByte(keyMappings[key]);
                    return;
                }
            }
        }

        public void SetByte(uint num)
        {
            if(num == 0)
            {
                return;
            }
            GD.Print("B " + Convert.ToString(num,2).PadZeros(8));

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
                }
                if (!isBit(5, JOYP)) {
                    mmu.JOYP = (byte)((JOYP & 0xF0) | buttons);
                    if (buttons != 0xF) mmu.requestInterrupt(JOYPAD_INTERRUPT);
                }
                if ((JOYP & 0b00110000) == 0b00110000) mmu.JOYP = 0xFF;


        }

    }
}

using static ProjectDMG.Utils.BitOps;
using Godot;

namespace ProjectDMG {
    public partial class JOYPAD : Node
    {

        private const int JOYPAD_INTERRUPT = 4;
        private const byte PAD_MASK = 0x10;
        private const byte BUTTON_MASK = 0x20;
        private byte pad = 0xF;
        private byte buttons = 0xF;


        public void HandleInput()
        {
            if(Input.IsActionJustPressed("Arcade Up")){handleKeyDown(0x14);}
            if(!Input.IsActionJustPressed("Arcade Up")){handleKeyUp(0x14);}
            if(Input.IsActionJustPressed("Arcade Down")){handleKeyDown(0x18);}
            if(!Input.IsActionJustPressed("Arcade Down")){handleKeyUp(0x18);}
            if(Input.IsActionJustPressed("Arcade Left")){handleKeyDown(0x12);}
            if(!Input.IsActionJustPressed("Arcade Left")){handleKeyUp(0x12);}
            if(Input.IsActionJustPressed("Arcade Right")){handleKeyDown(0x11);}
            if(!Input.IsActionJustPressed("Arcade Right")){handleKeyUp(0x11);}
            if(Input.IsActionJustPressed("Arcade A")){handleKeyDown(0x21);}
            if(!Input.IsActionJustPressed("Arcade A")){handleKeyUp(0x21);}
            if(Input.IsActionJustPressed("Arcade B")){handleKeyDown(0x22);}
            if(!Input.IsActionJustPressed("Arcade B")){handleKeyUp(0x22);}
            if(Input.IsActionJustPressed("Arcade Start")){handleKeyDown(0x28);}
            if(!Input.IsActionJustPressed("Arcade Start")){handleKeyUp(0x28);}
            if(Input.IsActionJustPressed("Arcade Select")){handleKeyDown(0x24);}
            if(!Input.IsActionJustPressed("Arcade Select")){handleKeyUp(0x24);}
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

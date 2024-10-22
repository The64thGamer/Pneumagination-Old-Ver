﻿namespace ProjectDMG.DMG.gameRom {
    interface IgameRom {
        byte ReadLoROM(ushort addr);
        byte ReadHiROM(ushort addr);
        void WriteROM(ushort addr, byte value);
        byte ReadERAM(ushort addr);
        void WriteERAM(ushort addr, byte value);
        void Init(byte[] ROM);
    }
}

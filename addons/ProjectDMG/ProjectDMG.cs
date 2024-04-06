using ProjectDMG.Utils;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Godot;

namespace ProjectDMG {
    public partial class ProjectDMG : TextureRect {

        [Export] public  Color customPaletteA;
        [Export] public  Color customPaletteB;
        [Export] public  Color customPaletteC;
        [Export] public Color customPaletteD;
        [Export] public string path;
        [Export] public bool useKeyboardInput;

        private CPU cpu;
        private MMU mmu;
        private PPU ppu;
        private TIMER timer;
        public JOYPAD joypad;

        public bool power_switch;

        long start = nanoTime();
            long elapsed = 0;
            int cpuCycles = 0;
            int cyclesThisUpdate = 0;

            Stopwatch timerCounter = new Stopwatch();

        public override void _Process(double delta)
        {
            if (power_switch)
            {
                if(useKeyboardInput)
                {
                    joypad.HandleKeyboardInput();
                }
                else
                {
                    joypad.HandleInput();
                }
            }

            EXECUTE();
        }

        public override void _Ready()
        {            
            POWER_ON(path);
            timerCounter.Start();
        }

        public void POWER_ON(string cartName) {
            mmu = new MMU();
            cpu = new CPU(mmu);
            ppu = new PPU(this);
            timer = new TIMER();
            joypad = new JOYPAD();
            Texture = ppu.finalScreen;
            mmu.loadgameRom(cartName);

            power_switch = true;
        }

        public void POWER_OFF() {
            power_switch = false;
        }

        int fpsCounter;

        public void EXECUTE() {
            // Main Loop Work in progress

            if (power_switch) {

                if (timerCounter.ElapsedMilliseconds > 1000) {
                    timerCounter.Restart();
                    fpsCounter = 0;
                }

                if ((elapsed - start) >= 16740000) { //nanoseconds per frame
                   start += 16740000;
                    while (cyclesThisUpdate < Constants.CYCLES_PER_UPDATE) {
                        cpuCycles = cpu.Exe();
                        cyclesThisUpdate += cpuCycles;

                        timer.update(cpuCycles, mmu);
                        ppu.update(cpuCycles, mmu);
                        joypad.update(mmu);
                        handleInterrupts();
                    }
                    fpsCounter++;
                    cyclesThisUpdate -= Constants.CYCLES_PER_UPDATE;
                }

                elapsed = nanoTime();
            }
            
        }

        private void handleInterrupts() {
            byte IE = mmu.IE;
            byte IF = mmu.IF;
            for (int i = 0; i < 5; i++) {
                if ((((IE & IF) >> i) & 0x1) == 1) {
                    cpu.ExecuteInterrupt(i);
                }
            }

            cpu.UpdateIME();
        }

        private static long nanoTime() {
            long nano = 10000L * Stopwatch.GetTimestamp();
            nano /= TimeSpan.TicksPerMillisecond;
            nano *= 100L;
            return nano;
        }

    }
}

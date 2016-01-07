using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
	struct StatsCounters
	{
		public int InstructionsExecuted;
		public int LoadWaits;
		public int StoreWaits;
		public int FetchWaits;
		public int InterruptWaits;
		public int ICacheHits;
		public int ICacheMisses;
	}

    class Program
    {
        static CPU m_cpu;
        static Bios m_bios;
        static Display m_display;
        static PlatformControlHub m_PCH;
		static VMKeyboard m_keyboard;
		static RAM m_RAM;
		static BlockDevice m_SSD;
        
        static InterconnectTerminal m_CPU_PCH_Interconnect = new InterconnectTerminal(1, 10);
        static InterconnectTerminal m_PCH_CPU_Interconnect = new InterconnectTerminal(1, 10);

        static InterconnectTerminal m_PCH_BIOS_Interconnect = new InterconnectTerminal(32, 10);
        static InterconnectTerminal m_BIOS_PCH_Interconnect = new InterconnectTerminal(32, 10);

        static InterconnectTerminal m_PCH_Display_Interconnect = new InterconnectTerminal(32, 10);
        static InterconnectTerminal m_Display_PCH_Interconnect = new InterconnectTerminal(32, 10);

		static InterconnectTerminal m_PCH_Keyboard_Interconnect = new InterconnectTerminal(32, 10);
		static InterconnectTerminal m_Keyboard_PCH_Interconnect = new InterconnectTerminal(32, 10);

		static InterconnectTerminal m_CPU_RAM_Interconnect = new InterconnectTerminal(1, 10);
		static InterconnectTerminal m_RAM_CPU_Interconnect = new InterconnectTerminal(1, 10);

		static InterconnectTerminal m_PCH_SSD_Interconnect = new InterconnectTerminal(32, 10);
		static InterconnectTerminal m_SSD_PCH_Interconnect = new InterconnectTerminal(32, 10);

		public const uint PICAddress = 32;
		public const uint PCHStartAddress = 512;
        public const uint biosStartAddress = PCHStartAddress;
        public const uint displayStartAddress = biosStartAddress + 1024;
		public const uint displayCommandAddress = displayStartAddress + 2048;
		public const uint displayFgColourAddress = displayCommandAddress + 1;
		public const uint displayBkgColourAddress = displayCommandAddress + 2;
		public const uint keyboardStartAddress = displayCommandAddress + 4;
		
		public const uint RAMStartAddress = keyboardStartAddress + 1; // Keep this at the top of the memory space for organisational convenience.
		public const uint RAMSize = 128 * 1024 * 1024; // 0.5GB

		public const uint SSDSeekAddress = RAMStartAddress + RAMSize;
		public const uint SSDFIFOAddress = SSDSeekAddress + 1;
		public const uint SSDInterruptAcknowledgeAddress = SSDFIFOAddress + 1;

		static List<InterconnectTerminal> m_interconnects = new List<InterconnectTerminal>();

        static uint tickCount;

		public static StatsCounters Counters;

        static void Main(string[] args)
        {
			m_interconnects.Add(m_CPU_PCH_Interconnect);
			m_interconnects.Add(m_PCH_CPU_Interconnect);
			m_interconnects.Add(m_PCH_BIOS_Interconnect);
			m_interconnects.Add(m_BIOS_PCH_Interconnect);
			m_interconnects.Add(m_PCH_Display_Interconnect);
			m_interconnects.Add(m_Display_PCH_Interconnect);
			m_interconnects.Add(m_PCH_Keyboard_Interconnect);
			m_interconnects.Add(m_Keyboard_PCH_Interconnect);
			m_interconnects.Add(m_RAM_CPU_Interconnect);
			m_interconnects.Add(m_CPU_RAM_Interconnect);
			m_interconnects.Add(m_SSD_PCH_Interconnect);
			m_interconnects.Add(m_PCH_SSD_Interconnect);

            m_CPU_PCH_Interconnect.SetOtherEnd(m_PCH_CPU_Interconnect);
            m_PCH_BIOS_Interconnect.SetOtherEnd(m_BIOS_PCH_Interconnect);
            m_PCH_Display_Interconnect.SetOtherEnd(m_Display_PCH_Interconnect);
			m_PCH_Keyboard_Interconnect.SetOtherEnd(m_Keyboard_PCH_Interconnect);
			m_RAM_CPU_Interconnect.SetOtherEnd(m_CPU_RAM_Interconnect);
			m_PCH_SSD_Interconnect.SetOtherEnd(m_SSD_PCH_Interconnect);

            m_cpu = new CPU(m_CPU_PCH_Interconnect, m_CPU_RAM_Interconnect);

			m_RAM = new RAM(m_RAM_CPU_Interconnect, (int)RAMSize, RAMStartAddress);

            m_bios = new Bios(biosStartAddress, m_BIOS_PCH_Interconnect);
            m_display = new Display(displayStartAddress, m_Display_PCH_Interconnect);
			m_keyboard = new VMKeyboard(m_Keyboard_PCH_Interconnect);
			m_SSD = new BlockDevice((int)SSDSeekAddress, m_SSD_PCH_Interconnect, @"MainDrive", 1024 * 1024, 200000, 1); // 4GB, 0.1ms seek time

            m_PCH = new PlatformControlHub(m_PCH_CPU_Interconnect, PCHStartAddress);

            m_PCH.AddDevice(m_PCH_BIOS_Interconnect, biosStartAddress);
            m_PCH.AddDevice(m_PCH_Display_Interconnect, displayStartAddress);
			m_PCH.AddDevice(m_PCH_Keyboard_Interconnect, keyboardStartAddress);
			m_PCH.AddDevice(m_PCH_SSD_Interconnect, SSDSeekAddress);

            while (true)
            {
                tickCount++;

                m_cpu.Tick();

				m_RAM.Tick();

                m_PCH.Tick();

                m_bios.Tick();
				m_display.Tick();
				m_SSD.Tick();

				if(tickCount % 100000 == 0)
				{
					m_keyboard.Tick();
				}

                foreach(InterconnectTerminal interconnect in m_interconnects)
				{
					interconnect.Tick();
				}
            }
        }
    }
}

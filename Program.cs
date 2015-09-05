using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class Program
    {
        static CPU m_cpu;
        static Bios m_bios;
        static Display m_display;
        static PlatformControlHub m_PCH;
		static VMKeyboard m_keyboard;
        
        static InterconnectTerminal m_CPU_PCH_Interconnect = new InterconnectTerminal(1, 10);
        static InterconnectTerminal m_PCH_CPU_Interconnect = new InterconnectTerminal(1, 10);

        static InterconnectTerminal m_PCH_BIOS_Interconnect = new InterconnectTerminal(32, 10);
        static InterconnectTerminal m_BIOS_PCH_Interconnect = new InterconnectTerminal(32, 10);

        static InterconnectTerminal m_PCH_Display_Interconnect = new InterconnectTerminal(32, 10);
        static InterconnectTerminal m_Display_PCH_Interconnect = new InterconnectTerminal(32, 10);

		static InterconnectTerminal m_PCH_Keyboard_Interconnect = new InterconnectTerminal(32, 10);
		static InterconnectTerminal m_Keyboard_PCH_Interconnect = new InterconnectTerminal(32, 10);

        public const uint PCHStartAddress = 128;
        public const uint biosStartAddress = 128;
        public const uint displayStartAddress = biosStartAddress + 1024;
		public const uint keyboardStartAddress = displayStartAddress + 4;

		static List<InterconnectTerminal> m_interconnects = new List<InterconnectTerminal>();

        static uint tickCount;

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

            m_CPU_PCH_Interconnect.SetOtherEnd(m_PCH_CPU_Interconnect);
            m_PCH_BIOS_Interconnect.SetOtherEnd(m_BIOS_PCH_Interconnect);
            m_PCH_Display_Interconnect.SetOtherEnd(m_Display_PCH_Interconnect);
			m_PCH_Keyboard_Interconnect.SetOtherEnd(m_Keyboard_PCH_Interconnect);

            m_cpu = new CPU(m_CPU_PCH_Interconnect);

            m_bios = new Bios(biosStartAddress, m_BIOS_PCH_Interconnect);
            m_display = new Display(displayStartAddress, m_Display_PCH_Interconnect);
			m_keyboard = new VMKeyboard(m_cpu.LocalPIC, m_Keyboard_PCH_Interconnect);

            m_PCH = new PlatformControlHub(m_PCH_CPU_Interconnect, PCHStartAddress);

            m_PCH.AddDevice(m_PCH_BIOS_Interconnect, 1024);
            m_PCH.AddDevice(m_PCH_Display_Interconnect, 4);
			m_PCH.AddDevice(m_PCH_Keyboard_Interconnect, 1);

            while (true)
            {
                tickCount++;

                m_cpu.Tick();

                m_PCH.Tick();
                m_bios.Tick();

				if(tickCount % 5000 == 0)
				{
					m_display.Tick();
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

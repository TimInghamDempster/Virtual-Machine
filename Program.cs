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
        
        static InterconnectTerminal m_CPU_PCH_Interconnect = new InterconnectTerminal(1, 10);
        static InterconnectTerminal m_PCH_CPU_Interconnect = new InterconnectTerminal(1, 10);

        static InterconnectTerminal m_PCH_BIOS_Interconnect = new InterconnectTerminal(32, 10);
        static InterconnectTerminal m_BIOS_PCH_Interconnect = new InterconnectTerminal(32, 10);

        static InterconnectTerminal m_PCH_Display_Interconnect = new InterconnectTerminal(32, 10);
        static InterconnectTerminal m_Display_PCH_Interconnect = new InterconnectTerminal(32, 10);

        public const uint PCHStartAddress = 128;
        public const uint biosStartAddress = 128;
        public const uint displayStartAddress = biosStartAddress + 14;

        static uint tickCount;

        static void Main(string[] args)
        {
            m_CPU_PCH_Interconnect.SetOtherEnd(m_PCH_CPU_Interconnect);
            m_PCH_BIOS_Interconnect.SetOtherEnd(m_BIOS_PCH_Interconnect);
            m_PCH_Display_Interconnect.SetOtherEnd(m_Display_PCH_Interconnect);

            m_cpu = new CPU(m_CPU_PCH_Interconnect);

            m_bios = new Bios(biosStartAddress, m_BIOS_PCH_Interconnect);
            m_display = new Display(displayStartAddress, m_Display_PCH_Interconnect);

            m_PCH = new PlatformControlHub(m_PCH_CPU_Interconnect, PCHStartAddress);

            m_PCH.AddDevice(m_PCH_BIOS_Interconnect, m_bios.Size);
            m_PCH.AddDevice(m_PCH_Display_Interconnect, 2);



            while (true)
            {
                tickCount++;

                m_cpu.Tick();

                m_PCH.Tick();
                m_bios.Tick();
                m_display.Tick();

                m_CPU_PCH_Interconnect.Tick();
                m_PCH_CPU_Interconnect.Tick();
                m_PCH_BIOS_Interconnect.Tick();
                m_BIOS_PCH_Interconnect.Tick();
                m_PCH_Display_Interconnect.Tick();
                m_Display_PCH_Interconnect.Tick();
            }
        }
    }
}

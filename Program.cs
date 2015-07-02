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

        public const uint displayStartAddress = 0;
        public const uint southbridgeStartAddress = 128;
        public const uint biosStartAddress = 128;

        static void Main(string[] args)
        {
            m_bios = new Bios(biosStartAddress);
            m_display = new Display(displayStartAddress);

            m_cpu = new CPU(m_bios, m_display);

            while (true)
            {
                m_cpu.Tick();
                m_display.Tick();
            }
        }
    }
}

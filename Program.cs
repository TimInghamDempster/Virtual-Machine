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
        static Bus m_northbridge;
        static Bus m_southbridge;
        static Bios m_bios;

        public const uint southbridgeStartAddress = 0;
        public const uint biosStartAddress = 0;

        static void Main(string[] args)
        {

            m_northbridge = new Bus();
            m_southbridge = new Bus();
            m_bios = new Bios(biosStartAddress);

            m_northbridge.Add(m_southbridge, southbridgeStartAddress);
            m_southbridge.Add(m_bios, biosStartAddress);

            m_cpu = new CPU(m_northbridge);

            while (true)
            {
                m_cpu.Tick();
            }
        }
    }
}

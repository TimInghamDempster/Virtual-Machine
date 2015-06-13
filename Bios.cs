using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class Bios : IIODevice
    {
        uint m_startAddress;
        int[] m_biosData;

        public Bios(uint startAddress)
        {
            m_startAddress = startAddress;
            m_biosData = new int[] { (int)Operations.MoveToRegister, 0,  0x48 };
        }

        public int Read(uint address)
        {
            uint localAddress = (address - m_startAddress) / 4;

            if (localAddress < m_biosData.Count())
            {
                return m_biosData[localAddress];
            }
            else
            {
                return 0;
            }
        }

        public void Write(int value, uint address)
        {
        }
    }
}

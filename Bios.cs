using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class Bios
    {
        uint m_startAddress;
        int[] m_biosData;

        public Bios(uint startAddress)
        {
            m_startAddress = startAddress;
            m_biosData = new int[] {    ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.SetLiteral << 8) | 0,                       (int)Program.displayStartAddress,    // Put display address into a register
                                        ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.AddLiteral << 8) | 0,                       1,                                   // Move past control byte of display
                                        ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.SetLiteral << 8) | 1,                       0x68656c6c,                          // Add the four characters "hell" to register
                                        ((int)ExecutionUnitCodes.Store << 16) | ((int)StoreOperations.StoreToRegisterLocation << 8) | 1,            0,                                   // Move characters from register to display (address stored in another register)
                                        ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.AddLiteral << 8) | 0,                       4,                                   // Move display address past four characters just added
                                        ((int)ExecutionUnitCodes.SimpleALU << 16) | ((int)ALUOperations.SetLiteral << 8) | 1,                       0x6f000000,                          // Add the character "o" to a register
                                        ((int)ExecutionUnitCodes.Store << 16) |  ((int)StoreOperations.StoreToRegisterLocation << 8) | 1,           0};                                  // Move character to display
        }

        public int Read(uint address)
        {
            uint localAddress = address - m_startAddress;

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    class CPUCore
    {
        uint m_instructionPointer;
        Operations m_currentOperation;
        int[] m_currentOperationData;
        CPU m_cpu;
        int[] m_registers;

        public CPUCore(CPU cpu)
        {
            m_cpu = cpu;
            m_instructionPointer = Program.biosStartAddress;
            m_currentOperationData = new int[3];
            m_registers = new int[10];
        }

        void GetInstruction()
        {
            m_currentOperation = (Operations)m_cpu.m_northbridge.Read(m_instructionPointer);
            m_currentOperationData[0] = m_cpu.m_northbridge.Read(m_instructionPointer + 4);
            m_currentOperationData[1] = m_cpu.m_northbridge.Read(m_instructionPointer + 8);
            m_currentOperationData[2] = (int)CPU.CycleCountsPerInstruction[(int)m_currentOperation];
        }

        void DoInstruction()
        {
            switch (m_currentOperation)
            {
                case Operations.MoveToRegister:
                    {
                        m_registers[m_currentOperationData[0]] = m_currentOperationData[1];
                    }break;
            }
        }

        public void Tick()
        {
            if (m_currentOperation == Operations.None)
            {
                GetInstruction();
            }
            else
            {
                if (m_currentOperationData[2] > 0)
                {
                    m_currentOperationData[2]--;
                }
                else
                {
                    DoInstruction();
                    m_instructionPointer += CPU.InstructionSize;
                    GetInstruction();
                }
            }
        }
    }
}

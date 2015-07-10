using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum StoreOperations
    {
        StoreToRegisterLocation,
        StoreToLiteralLocation
    }

    class StoreUnit
    {
        CPUCore m_CPUCore;
        MemeoryController m_memoryController;
        InterconnectTerminal m_ioInterconnect;

        int[] m_currentInstruction;
        bool m_hasInstruction;

        public StoreUnit(CPUCore cPUCore, InterconnectTerminal IOInterconenct)
        {
            m_CPUCore = cPUCore;
            m_ioInterconnect = IOInterconenct;
        }

        public void Tick()
        {
            if (m_CPUCore.m_currentStage == PipelineStages.Execution && m_hasInstruction == true)
            {
                StoreOperations operation = (StoreOperations)((m_currentInstruction[0] >> 8) & 0xff);
                int registerWithValueToStore = m_currentInstruction[0] & 0xff;
                switch(operation)
                {
                    case StoreOperations.StoreToRegisterLocation:
                        {
                            uint address = (uint)m_CPUCore.m_registers[m_currentInstruction[1]];
                            int value = m_CPUCore.m_registers[registerWithValueToStore];

                            int[] packet = new int[2];
                            packet[0] = (int)address;
                            packet[1] = value;
                            bool stored = m_ioInterconnect.SendPacket(packet, packet.Count());
                            
                            if (stored)
                            {
                                m_CPUCore.m_nextStage = PipelineStages.BranchPredict;
                            }
                        }
                        break;
                    case StoreOperations.StoreToLiteralLocation:
                        {
                            uint address = (uint)m_currentInstruction[1];
                            int value = m_CPUCore.m_registers[registerWithValueToStore];

                            int[] packet = new int[2];
                            packet[0] = (int)address;
                            packet[1] = value;
                            bool stored = m_ioInterconnect.SendPacket(packet, packet.Count());
                            
                            if (stored)
                            {
                                m_CPUCore.m_nextStage = PipelineStages.BranchPredict;
                            }
                        }
                        break;
                }
            }
        }

        public void SetInstruction(int[] instruction)
        {
            m_hasInstruction = true;
            m_currentInstruction = instruction;
        }
    }
}

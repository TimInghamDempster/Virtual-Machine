using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum ALUOperations
    {
        AddLiteral,
        SetLiteral = 1 << 16,
        CopyRegister = 2 << 16
    }

    class ArithmeticLogicUnit
    {
        bool m_complex;
        CPUCore m_CPUCore;
        Dictionary<ALUOperations, uint> m_cycleCountsPerInstruction;

        int[] m_currentInstruction;
        bool m_hasInstruction;
        uint m_currentInstructionTimeRemaining;

        public ArithmeticLogicUnit(bool complex, CPUCore cPUCore)
        {
            m_complex = complex;
            m_CPUCore = cPUCore;
            SetupCycleCounts();
        }

        private void SetupCycleCounts()
        {
            m_cycleCountsPerInstruction = new Dictionary<ALUOperations, uint>();

            m_cycleCountsPerInstruction.Add(ALUOperations.AddLiteral, 1);
			m_cycleCountsPerInstruction.Add(ALUOperations.SetLiteral, 1);
			m_cycleCountsPerInstruction.Add(ALUOperations.CopyRegister, 1);
        }

        public void Tick()
        {
            if (m_CPUCore.m_currentStage == PipelineStages.Execution && m_hasInstruction == true)
            {
                if (m_currentInstructionTimeRemaining > 0)
                {
                    m_currentInstructionTimeRemaining--;
                }
                else
                {
                    ALUOperations instructionCode = (ALUOperations)(m_currentInstruction[0] & 0x00ff0000);
                    int targetRegister = (m_currentInstruction[0] & 0x0000ff00) >> 8;
					int sourceRegister = m_currentInstruction[0] & 0x000000ff;
                    switch (instructionCode)
                    {
                        case ALUOperations.AddLiteral:
							m_CPUCore.m_registers[targetRegister] = m_CPUCore.m_registers[sourceRegister] + m_currentInstruction[1];
                            break;
                        case ALUOperations.SetLiteral:
							m_CPUCore.m_registers[targetRegister] = m_currentInstruction[1];
                            break;
                        case ALUOperations.CopyRegister:
							m_CPUCore.m_registers[targetRegister] = m_CPUCore.m_registers[m_currentInstruction[1]];
                            break;
                    }
                    m_hasInstruction = false;
                    m_CPUCore.m_nextStage = PipelineStages.BranchPredict;
                }
            }
        }

        public void SetInstruction(int[] instruction)
        {
            m_hasInstruction = true;
            m_currentInstruction = instruction;
            m_currentInstructionTimeRemaining = m_cycleCountsPerInstruction[(ALUOperations)(instruction[0] & 0x00ff0000)] - 1;
        }
    }
}

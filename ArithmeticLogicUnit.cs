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
        SetLiteral
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
                    ALUOperations instructionCode = (ALUOperations)((m_currentInstruction[0] >> 8) & 0xff);
                    int registerToOperateOn = m_currentInstruction[0] & 0xff;
                    switch (instructionCode)
                    {
                        case ALUOperations.AddLiteral:
                            m_CPUCore.m_registers[registerToOperateOn] += m_currentInstruction[1];
                            break;
                        case ALUOperations.SetLiteral:
                            m_CPUCore.m_registers[registerToOperateOn] = m_currentInstruction[1];
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
            m_currentInstructionTimeRemaining = m_cycleCountsPerInstruction[(ALUOperations)(instruction[0] & 0xff)] - 1;
        }
    }
}

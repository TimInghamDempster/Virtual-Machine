using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Virutal_Machine
{
    enum PipelineStages
    {
        BranchPredict,
        InstructionFetch,
        InstructionDispatch,
        Execution,
        Retirement
    }

    enum ExecutionUnitCodes
    {
        Nop,
        ALU	= 1 << 16,
        Load		= 2 << 16,
        Store		= 3 << 16,
		Branch		= 4 << 16,
		Fetch		= 5 << 16
    }

    class CPUCore
    {
        public uint m_instructionPointer;
        
        public int[] m_registers;

        public PipelineStages m_currentStage;
        public PipelineStages m_nextStage;

        InterconnectTerminal m_IOInterconnect;
        BranchUnit m_branchUnit;
        InstructionFetchUnit m_fetchUnit;
        InstructionDispatchUnit m_dispatchUnit;
        ArithmeticLogicUnit m_simpleALU;
        ArithmeticLogicUnit m_complexALU;
        LoadUnit m_loadUnit;
        StoreUnit m_storeUnit;
        RetireUnit m_retireUnit;



        public CPUCore(InterconnectTerminal IOInterconnect)
        {
            m_instructionPointer = Program.biosStartAddress;
            m_registers = new int[10];
            m_currentStage = PipelineStages.InstructionFetch;
            m_nextStage = PipelineStages.InstructionFetch;
            m_IOInterconnect = IOInterconnect;
            m_retireUnit = new RetireUnit(this);
            m_simpleALU = new ArithmeticLogicUnit(false, this);
            m_complexALU = new ArithmeticLogicUnit(true, this);
            m_loadUnit = new LoadUnit(this, m_IOInterconnect);
            m_storeUnit = new StoreUnit(this, IOInterconnect);
            m_branchUnit = new BranchUnit(this);
            m_dispatchUnit = new InstructionDispatchUnit(this, m_branchUnit, m_simpleALU, m_complexALU, m_loadUnit, m_storeUnit);
            m_fetchUnit = new InstructionFetchUnit(this, IOInterconnect, m_dispatchUnit);
        }

        public void Tick()
        {
            m_branchUnit.Tick();
            m_fetchUnit.Tick();
            m_dispatchUnit.Tick();
            m_simpleALU.Tick();
            m_complexALU.Tick();
            m_loadUnit.Tick();
            m_storeUnit.Tick();
            m_retireUnit.Tick();

            m_currentStage = m_nextStage;
        }
    }
}

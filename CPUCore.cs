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
        SimpleALU,
        ComplexALU,
        Load,
        Store
    }

    class CPUCore
    {
        public uint m_instructionPointer;
        
        public int[] m_registers;

        public PipelineStages m_currentStage;
        public PipelineStages m_nextStage;

        MemeoryController m_memoryController;
        BranchUnit m_branchUnit;
        InstructionFetchUnit m_fetchUnit;
        InstructionDispatchUnit m_dispatchUnit;
        ArithmeticLogicUnit m_simpleALU;
        ArithmeticLogicUnit m_complexALU;
        LoadUnit m_loadUnit;
        StoreUnit m_storeUnit;
        RetireUnit m_retireUnit;



        public CPUCore(MemeoryController memoryController)
        {
            m_instructionPointer = Program.biosStartAddress;
            m_registers = new int[8];
            m_currentStage = PipelineStages.InstructionFetch;
            m_nextStage = PipelineStages.InstructionFetch;

            m_memoryController = memoryController;
            m_retireUnit = new RetireUnit(this);
            m_simpleALU = new ArithmeticLogicUnit(false, this);
            m_complexALU = new ArithmeticLogicUnit(true, this);
            m_loadUnit = new LoadUnit(this);
            m_storeUnit = new StoreUnit(this, m_memoryController);
            m_dispatchUnit = new InstructionDispatchUnit(this, m_simpleALU, m_complexALU, m_loadUnit, m_storeUnit);
            m_fetchUnit = new InstructionFetchUnit(this, m_memoryController, m_dispatchUnit);
            m_branchUnit = new BranchUnit(this);
        }

        public void Tick()
        {
            m_memoryController.Tick();

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

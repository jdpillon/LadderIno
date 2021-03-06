﻿
namespace Core.Components
{
    /// <summary>
    /// Component: MUL
    /// Description: Integer 16 bits multiply function block
    /// Function: put on destination variable value A multiplied by value B
    /// </summary>
    public class MUL : MathComponent
    {
        #region Functions
        protected override void RunLogicalTest()
        {
            if (LeftLide.LogicLevel)
            {
                RetrieveData();
                if (DataTable != null) DataTable.SetValue(Destination, (short)(ValueA * ValueB));
            }
            InternalState = LeftLide.LogicLevel;
        }
        #endregion Functions

        #region Constructors
        public MUL()
        {
            
        }

        public MUL(Node Left, Node Right)
            : base(Left, Right)
        {
            
        }
        #endregion Constructors
    }
}

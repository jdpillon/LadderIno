﻿
namespace Core.Components
{
    /// <summary>
    /// Component: One Shot Raising
    /// Description: Border detector from Low to High
    /// Function: Detects one raising border on input
    /// </summary>
    public class OSR : ComponentBase
    {
        #region Functions
        protected override void RunLogicalTest()
        {
            InternalState = !LastInput && LeftLide.LogicLevel;
            LastInput = LeftLide.LogicLevel;
        }
        #endregion Functions

        #region Constructors
        public OSR()
        {
            Class = ComponentClass.Input;
        }

        public OSR(Node Left, Node Right)
            : base(Left, Right)
        {
            Class = ComponentClass.Input;
        }
        #endregion Constructors

        #region Internal Data
        bool LastInput;
        #endregion Internal Data
    }
}

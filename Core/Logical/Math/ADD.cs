﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Components.Logical
{
    /// <summary>
    /// Component: ADD
    /// Description: Integer 16 bits sum function block
    /// Function: put on destination variable the sum of value A and value B
    /// </summary>
    public class ADD : MathComponent
    {
        #region Functions
        protected override void RunLogicalTest()
        {
            RetrieveData();
            if (LeftLide.LogicLevel) Data.LDIVariableTable.SetValue(Destination, (short)(ValueA + ValueB));
            InternalState = LeftLide.LogicLevel;
        }
        #endregion Functions

        #region Constructors
        public ADD()
        {
            
        }

        public ADD(Node Left, Node Right)
            : base(Left, Right)
        {
            
        }
        #endregion Constructors
    }
}
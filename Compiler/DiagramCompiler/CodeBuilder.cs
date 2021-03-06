﻿using System.Text;

namespace Compiler
{
    public static partial class DiagramCompiler
    {
        /// <summary>
        /// Assemble the code parts into a finished compiled code
        /// </summary>
        /// <param name="codeBuffer"></param>
        /// <returns></returns>
        internal static string CodeBuilder(CompilerBuffer codeBuffer)
        {
            StringBuilder codeBuilder = new StringBuilder();

            codeBuilder.AppendLine("// Code generated by Ladderino V " + COMPILER_VERSION);
            codeBuilder.AppendLine("// Developed by: Allan Leon");
            codeBuilder.AppendLine();

            #region Defines
            if (codeBuffer.Defines.Count > 0)
            {
                codeBuilder.AppendLine("/* ================================[ Defines ]=============================== */");
                foreach (string item in codeBuffer.Defines) codeBuilder.AppendLine(item);
                codeBuilder.AppendLine("/* ========================================================================== */");
                codeBuilder.AppendLine();
            }
            #endregion Defines

            #region Includes
            if (codeBuffer.Includes.Count > 0)
            {
                codeBuilder.AppendLine("/* ===============================[ Includes ]=============================== */");
                foreach (string item in codeBuffer.Includes) codeBuilder.AppendLine(item);
                codeBuilder.AppendLine("/* ========================================================================== */");
                codeBuilder.AppendLine();
            }
            #endregion Includes

            #region Global Variables
            codeBuilder.AppendLine("/* ================================[ Globals ]=============================== */");
            foreach (string item in codeBuffer.Globals) codeBuilder.AppendLine(item);
            if (codeBuffer.OSFCount > 0) codeBuilder.AppendLine("byte " + OSF_DATA + "[" + ((int)(codeBuffer.OSFCount / 8) + 1) + "];");
            if (codeBuffer.OSRCount > 0) codeBuilder.AppendLine("byte " + OSR_DATA + "[" + ((int)(codeBuffer.OSRCount / 8) + 1) + "];");
            codeBuilder.AppendLine("/* ========================================================================== */");
            codeBuilder.AppendLine();
            #endregion Global Variables

            #region Ladder Functions
            codeBuilder.AppendLine("/* ===========================[ Ladder Functions ]=========================== */");
            LadderFunctionBuilder(codeBuffer, codeBuilder);
            codeBuilder.AppendLine("/* ========================================================================== */");
            codeBuilder.AppendLine();
            #endregion Ladder Functions

            #region User Functions
            if (codeBuffer.Functions.Count > 0)
            {
                codeBuilder.AppendLine("/* ===============================[ Functions ]============================== */");
                foreach (var item in codeBuffer.Functions)
                {
                    foreach(string line in item) codeBuilder.AppendLine(line);
                    codeBuilder.AppendLine();
                }
                codeBuilder.AppendLine("/* ========================================================================== */");
                codeBuilder.AppendLine();
            }
            #endregion User Functions

            #region Main
            codeBuilder.AppendLine("/* =================================[ Main ]================================= */");

            #region Setup
            codeBuilder.AppendLine("void setup() {");
            foreach (string item in codeBuffer.SetupContent) codeBuilder.AppendLine(INDENT + item);
            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine();
            #endregion

            #region Loop
            codeBuilder.AppendLine("void loop() {");
            if (codeBuffer.BoolTempCount > 0) codeBuilder.AppendLine(INDENT + "boolean " + RATD + "[" + codeBuffer.BoolTempCount + "]; //Rung Activation Temporary Data");
            if (codeBuffer.InputRefreshContent.Count > 0) codeBuilder.AppendLine(INDENT + REFRESH_INPUT_FN + "();");
            codeBuilder.AppendLine();

            for (int c = 0; c < codeBuffer.Rungs.Count; c++)
            {
                codeBuilder.AppendLine(INDENT + "//Rung " + c + " Code:");
                foreach (string item in codeBuffer.Rungs[c]) codeBuilder.AppendLine(INDENT + item);
                codeBuilder.AppendLine();
            }

            if (codeBuffer.OutputRefreshContent.Count > 0) codeBuilder.AppendLine(INDENT + REFRESH_OUTPUT_FN + "();");

            codeBuilder.AppendLine("}");
            codeBuilder.AppendLine();
            #endregion

            codeBuilder.AppendLine("/* ========================================================================== */");
            codeBuilder.AppendLine();
            #endregion Main

            return codeBuilder.ToString();
        }


        /// <summary>
        /// Generate the automatic ladder functions
        /// </summary>
        /// <param name="codeBuffer"></param>
        /// <param name="codeBuilder"></param>
        private static void LadderFunctionBuilder(CompilerBuffer codeBuffer, StringBuilder codeBuilder)
        {
            #region OSF
            if (codeBuffer.OSFCount > 0)
            {
                codeBuilder.AppendLine("boolean " + OSF_FN + "(int n, boolean input){");
                codeBuilder.AppendLine(INDENT + "byte *osf = &" + OSF_DATA + "[n / 8];");
                codeBuilder.AppendLine(INDENT + "int j = (n % 8);");
                codeBuilder.AppendLine(INDENT + "boolean prev = bitRead(*osf, j);");
                codeBuilder.AppendLine(INDENT + "bitWrite(*osf, j, input);");
                codeBuilder.AppendLine(INDENT + "return prev && !input;");
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine();
            }
            #endregion

            #region OSR
            if (codeBuffer.OSRCount + codeBuffer.CTDCount + codeBuffer.CTUCount > 0)
            {
                codeBuilder.AppendLine("boolean " + OSR_FN + "(int n, boolean input){");
                codeBuilder.AppendLine(INDENT + "byte *osr = &" + OSR_DATA + "[n / 8];");
                codeBuilder.AppendLine(INDENT + "int j = (n % 8);");
                codeBuilder.AppendLine(INDENT + "boolean prev = bitRead(*osr, j);");
                codeBuilder.AppendLine(INDENT + "bitWrite(*osr, j, input);");
                codeBuilder.AppendLine(INDENT + "return !prev && input;");
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine();
            }
            #endregion

            #region CTD
            if (codeBuffer.CTDCount > 0)
            {
                codeBuilder.AppendLine("boolean " + CTD_FN + "(int limit, int *dest, int osr, boolean input){");
                codeBuilder.AppendLine(INDENT + "if (" + OSR_FN + "(osr, input)) *dest -= 1;");
                codeBuilder.AppendLine(INDENT + "return *dest >= limit;");
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine();
            }
            #endregion

            #region CTU
            if (codeBuffer.CTUCount > 0)
            {
                codeBuilder.AppendLine("boolean " + CTU_FN + "(int limit, int *dest, int osr, boolean input){");
                codeBuilder.AppendLine(INDENT + "if (" + OSR_FN + "(osr, input)) *dest += 1;");
                codeBuilder.AppendLine(INDENT + "return *dest >= limit;");
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine();
            }
            #endregion

            #region Refresh Inputs
            if (codeBuffer.InputRefreshContent.Count > 0)
            {
                codeBuilder.AppendLine("void " + REFRESH_INPUT_FN + "(){");
                foreach (string item in codeBuffer.InputRefreshContent) codeBuilder.AppendLine(INDENT + item);
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine();
            }
            #endregion

            #region Refresh Outputs
            if (codeBuffer.OutputRefreshContent.Count > 0)
            {
                codeBuilder.AppendLine("void " + REFRESH_OUTPUT_FN + "(){");
                foreach (string item in codeBuffer.OutputRefreshContent) codeBuilder.AppendLine(INDENT + item);
                codeBuilder.AppendLine("}");
                codeBuilder.AppendLine();
            }
            #endregion
        }
    }
}

using System;
using System.Collections.Generic;
using System.Resources;

namespace VSMerlin32
{
    public enum Merlin32Opcodes
    {
        ADC, ADCL, AND, ANDL, ASL, 
        BCC, BLT, BCS, BGE, BEQ, BIT, BMI, BNE, BPL, BRA, BRK, BRL, BVC, BVS, 
        CLC, CLD, CLI, CLV, CMP, CMPL, COP, CPX, CPY, 
        DEC, DEX, DEY, 
        EOR, EORL, 
        INC, INX, INY, 
        JMP, JML, JMPL, JSR, JSL, 
        LDA, LDAL, LDX, LDY, LSR, 
        MVN, MVP, 
        NOP, 
        ORA, ORAL, 
        PEA, PEI, PER, PHA, PHB, PHD, PHK, PHP, PHX, PHY, PLA, PLB, PLD, PLP, PLX, PLY, 
        REP, ROL, ROR, RTI, RTL, RTS, 
        SBC, SBCL, SEC, SED, SEI, SEP, STA, STAL, STP, STX, STY, STZ, 
        TAX, TAY, TCD, TCS, TDC, TRB, TSB, TSC, TSX, TXA, TXS, TXY, TYA, TYX, 
        WAI, WDM, 
        XBA, XCE
    }

    public enum Merlin32Directives
    {
        EQU,
        ANOP, ORG, PUT, PUTBIN,          /* PUTBIN n'existe pas dans Merlin 16+ */
        START, END, 
        DUM, DEND, 
        MX, XC, LONGA, LONGI, 
        USE, USING, 
        REL, DSK, LNK, SAV, 
        TYP, 
        IF, DO, ELSE, FIN, 
        LUP, ELUP, // --^, ignored for now (invalid enum)
        ERR, DAT, 
        AST, CYC, EXP, LST, LSTDO, PAG, TTL, SKP, TR, KBD, PAU, SW, USR
    }

    public enum Merlin32DataDefines
    {
        DA, DW, DDB, DFB, DB, ADR, ADRL, HEX, DS, 
        DC, DE,   /* ? */
        ASC, DCI, INV, FLS, REV, STR, STRL, 
        CHK
    }

    class Merlin32KeywordsHelper
    {
        /* The regex for opcodes is now defined in Merlin32CodeHelper.cs
        public const string strMerlin32OpcodesRegex =
@"\b(ADC(L?)|AND(L?)|ASL|BCC|BCS|BEQ|BIT|BMI|BNE|BPL|BRA|BRK|BRL|BVC|BVS|CLC|CLD|CLI|CLV|CMP(L?)|COP|CPX|CPY|DEC|DEX|DEY|EOR(L?)|INC|INX|INY|JMP(L?)|JML|JSR|JSL|LDA(L?)|LDX|LDY|LSR|MVN|MVP|NOP|ORA(L?)|ORG|PEA|PEI|PER|PHA|PHB|PHD|PHK|PHP|PHX|PHY|PLA|PLB|PLD|PLP|PLX|PLY|REP|ROL|ROR|RTI|RTL|RTS|SBC(L?)|SEC|SED|SEI|SEP|STA(L?)|STP|STX|STY|STZ|TAX|TAY|TCD|TCS|TDC|TRB|TSB|TSC|TSX|TXA|TXS|TXY|TYA|TYX|WAI|WDM|XBA|XCE)\b";
        */

        public IDictionary<string, string> _Merlin32KeywordsQuickInfo;

        public Merlin32KeywordsHelper()
        {
            // Read the resources for opcodes, all of them...
            ResourceSet rsOpcodes = VSMerlin32.Resources.opcodes.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
            // Read the resources for directives too, all of them...
            ResourceSet rsDirectives = VSMerlin32.Resources.directives.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);
            // Read the resources for datadefines too, all of them...
            ResourceSet rsData = VSMerlin32.Resources.data.ResourceManager.GetResourceSet(System.Globalization.CultureInfo.CurrentCulture, true, true);

            _Merlin32KeywordsQuickInfo = new Dictionary<string, string>();
            foreach (Merlin32Opcodes token in Enum.GetValues(typeof(Merlin32Opcodes)))
            {
                // _Merlin32OpcodesQuickInfo[token.ToString()] = token.ToString();
                _Merlin32KeywordsQuickInfo[token.ToString()] = rsOpcodes.GetString(token.ToString());
            }
            foreach (Merlin32Directives token in Enum.GetValues(typeof(Merlin32Directives)))
            {
                _Merlin32KeywordsQuickInfo[token.ToString()] = rsDirectives.GetString(token.ToString());
            }
            foreach (Merlin32DataDefines token in Enum.GetValues(typeof(Merlin32DataDefines)))
            {
                _Merlin32KeywordsQuickInfo[token.ToString()] = rsData.GetString(token.ToString());
            }
            /*
            _Merlin32OpcodesQuickInfo[Merlin32Opcodes.ORG.ToString()] = VSMerlin32.strings.ORG;
            */
        }
    }

    internal sealed class Merlin32TokenHelper
    {
        public const string Merlin32Opcode = "Merlin32Opcode";
        public const string Merlin32Directive = "Merlin32Directive";
        public const string Merlin32DataDefine = "Merlin32DataDefine";
        public const string Merlin32Text = "Merlin32Text";
        public const string Merlin32Comment = "Merlin32Comment";
    }

    public enum Merlin32TokenTypes
    {
        Merlin32Opcode, Merlin32Directive, Merlin32DataDefine, Merlin32Text, Merlin32Comment
    }
}

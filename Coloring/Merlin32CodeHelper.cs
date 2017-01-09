using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.Text;
using VSMerlin32.Coloring.Data;

namespace VSMerlin32.Coloring
{
    internal class Merlin32CodeHelper
    {
        const string COMMENT_REG = @"((\u003B)|(\u002A))(.*)"; // ;
        const string TEXT_REG = @"(""|')[^']*(""|')";
        // OPCODE_REG is initialized dynamically below.
        static string OPCODE_REG = "";
        static string DIRECTIVE_REG = "";
        static string DATADEFINE_REG = "";
        
        public static IEnumerable<SnapshotHelper> GetTokens(SnapshotSpan span)
        {
            string strTempRegex; // temp var string
            ITextSnapshotLine containingLine = span.Start.GetContainingLine();
            int curLoc = containingLine.Start.Position;
            string formattedLine = containingLine.GetText();

            int commentMatch = int.MaxValue;
            Regex reg = new Regex(COMMENT_REG);
            foreach (Match match in reg.Matches(formattedLine))
            {
                commentMatch = match.Index < commentMatch ? match.Index : commentMatch;
                yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, match.Index + curLoc), match.Length), Merlin32TokenTypes.Merlin32Comment);
            }

            reg = new Regex(TEXT_REG);
            foreach (Match match in reg.Matches(formattedLine))
            {
                if (match.Index < commentMatch)
                    yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, match.Index + curLoc), match.Length), Merlin32TokenTypes.Merlin32Text);
            }

            // OG NEW
            // OPCODES
            strTempRegex = "";
            foreach (Merlin32Opcodes token in Enum.GetValues(typeof(Merlin32Opcodes)))
            {
                strTempRegex += (token.ToString() + ("|"));
            }
            // we remove the last "|" added
            strTempRegex = strTempRegex.Remove(strTempRegex.LastIndexOf("|"));
            OPCODE_REG = string.Format(@"\b({0})\b", strTempRegex);

            reg = new Regex(OPCODE_REG,RegexOptions.IgnoreCase);
            foreach (Match match in reg.Matches(formattedLine))
            {
                if (match.Index < commentMatch)
                    yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, match.Index + curLoc), match.Length), Merlin32TokenTypes.Merlin32Opcode);
            }

            // OG NEW
            // DIRECTIVES
            strTempRegex = "";
            foreach (Merlin32Directives token in Enum.GetValues(typeof(Merlin32Directives)))
            {
                if (token.ToString() != Resources.directives.ELUP)
                    strTempRegex += (token.ToString() + ("|"));
            }
            DIRECTIVE_REG = string.Format(@"\b({0})\b|{1}", strTempRegex, Resources.directives.ELUPRegex);

            reg = new Regex(DIRECTIVE_REG, RegexOptions.IgnoreCase);
            foreach (Match match in reg.Matches(formattedLine))
            {
                if (match.Index < commentMatch)
                    yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, match.Index + curLoc), match.Length), Merlin32TokenTypes.Merlin32Directive);
            }

            // OG NEW
            // DATADEFINES
            strTempRegex = "";
            foreach (Merlin32DataDefines token in Enum.GetValues(typeof(Merlin32DataDefines)))
            {
                strTempRegex += (token.ToString() + ("|"));
            }
            // we remove the last "|" added
            strTempRegex = strTempRegex.Remove(strTempRegex.LastIndexOf("|"));
            DATADEFINE_REG = string.Format(@"\b({0})\b", strTempRegex);

            reg = new Regex(DATADEFINE_REG, RegexOptions.IgnoreCase);
            foreach (Match match in reg.Matches(formattedLine))
            {
                if (match.Index < commentMatch)
                    yield return new SnapshotHelper(new SnapshotSpan(new SnapshotPoint(span.Snapshot, match.Index + curLoc), match.Length), Merlin32TokenTypes.Merlin32DataDefine);
            }
        }
    }
}

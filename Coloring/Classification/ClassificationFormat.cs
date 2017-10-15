using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace VSMerlin32.Coloring.Classification
{
    #region Format definition

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32Comment)]
    [Name("Merlin32CommentFormat")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class CommentFormat : ClassificationFormatDefinition
    {
        public CommentFormat()
        {
            this.DisplayName = "Merlin32 Comments"; //human readable version of the name
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32Opcode)]
    [Name("Merlin32OpcodeFormat")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class OpcodeFormat : ClassificationFormatDefinition
    {
        public OpcodeFormat()
        {
            this.DisplayName = "Merlin32 Opcodes"; //human readable version of the name
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32Directive)]
    [Name("Merlin32DirectiveFormat")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class DirectiveFormat : ClassificationFormatDefinition
    {
        public DirectiveFormat()
        {
            this.DisplayName = "Merlin32 Directives"; //human readable version of the name
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32DataDefine)]
    [Name("Merlin32DataDefineFormat")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class DataDefineFormat : ClassificationFormatDefinition
    {
        public DataDefineFormat()
        {
            this.DisplayName = "Merlin32 Data Definitions"; //human readable version of the name
        }
    }

    [Export(typeof(EditorFormatDefinition))]
    [ClassificationType(ClassificationTypeNames = Merlin32TokenHelper.Merlin32Text)]
    [Name("Merlin32TextFormat")]
    [UserVisible(true)]
    [Order(Before = Priority.Default)]
    internal sealed class TextFormat : ClassificationFormatDefinition
    {
        public TextFormat()
        {
            this.DisplayName = "Merlin32 Strings"; //human readable version of the name
        }
    }
    #endregion //Format definition
}

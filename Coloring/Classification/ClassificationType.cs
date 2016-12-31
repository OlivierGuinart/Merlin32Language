using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Utilities;

namespace VSMerlin32.Coloring.Classification
{
    internal static class OrdinaryClassificationDefinition
    {
        #region Type definition

        /// <summary>
        /// Defines the "opcode" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32Opcode)]
        internal static ClassificationTypeDefinition opcode = null;

        /// <summary>
        /// Defines the "directive" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32Directive)]
        internal static ClassificationTypeDefinition directive = null;

        /// <summary>
        /// Defines the "datadefine" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32DataDefine)]
        internal static ClassificationTypeDefinition datadefine = null;

        /// <summary>
        /// Defines the "text" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32Text)]
        internal static ClassificationTypeDefinition text = null;

        /// <summary>
        /// Defines the "comment" classification type.
        /// </summary>
        [Export(typeof(ClassificationTypeDefinition))]
        [Name(Merlin32TokenHelper.Merlin32Comment)]
        internal static ClassificationTypeDefinition comment = null;
        
        #endregion
    }
}

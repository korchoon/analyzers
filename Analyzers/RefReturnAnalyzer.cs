using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Analyzers {
    [DiagnosticAnalyzer (LanguageNames.CSharp)]
    public sealed class RefReturnAnalyzer : DiagnosticAnalyzer {
        static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor (
            id: "REF0001",
            title: "Ref return",
            messageFormat: (LocalizableString)"Method return value should be used by reference",
            category: string.Empty,
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create (Rule);

        public override void Initialize (AnalysisContext context) {
            context.EnableConcurrentExecution ();
            context.ConfigureGeneratedCodeAnalysis (GeneratedCodeAnalysisFlags.None);
            context.RegisterSyntaxNodeAction (Analyze, SyntaxKind.InvocationExpression);
        }

        void Analyze (SyntaxNodeAnalysisContext context) {
            if (TryGetSyntaxNode (context, out InvocationExpressionSyntax syntaxNode)) {
                if (context.SemanticModel.GetSymbolInfo (syntaxNode).Symbol is IMethodSymbol methodSymbol) {
                    if (!methodSymbol.ReturnsByRef) {
                        return;
                    }

                    if (syntaxNode.Parent is RefExpressionSyntax) {
                        return;
                    }

                    if (syntaxNode.Parent is EqualsValueClauseSyntax) {
                        context.ReportDiagnostic (Diagnostic.Create (Rule, syntaxNode.GetLocation ()));
                    }
                }
            }
        }

        static bool TryGetSyntaxNode<T> (SyntaxNodeAnalysisContext context, out T syntaxNode) where T : SyntaxNode {
            if (context.Node is T node) {
                if (node.GetDiagnostics ().Any (x => x.Severity == DiagnosticSeverity.Error)) {
                    syntaxNode = null;
                    return false;
                }

                syntaxNode = node;
                return true;
            }

            syntaxNode = default;
            return false;
        }
    }
}
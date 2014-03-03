using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SlnViz {
    public class SimonFeatureRewriter : SyntaxRewriter {
        private ISemanticModel _semanticModel;
        private HashSet<KeyValuePair<string, ITypeSymbol>> _delegatingFields;
        private Dictionary<ITypeSymbol, List<ISymbol>> _implementedSymbols;

        public SimonFeatureRewriter(ISemanticModel semanticModel) {
            _semanticModel = semanticModel;
        }

        public override SyntaxNode VisitClassDeclaration(ClassDeclarationSyntax node) {
            _delegatingFields = new HashSet<KeyValuePair<string, ITypeSymbol>>();
            _implementedSymbols = new Dictionary<ITypeSymbol, List<ISymbol>>();
            if (node.BaseList == null)
                return node;
            var sb = new StringBuilder();
            ITypeSymbol typeSymbol = null;
            var baseTypes = new List<TypeSyntax>();
            foreach (var type in node.BaseList.Types) {
                var typeSymbolLoop = _semanticModel.GetSymbolInfo(type).Symbol as ITypeSymbol;
                if (typeSymbolLoop == null) {
                    if (typeSymbol == null || typeSymbol.TypeKind != CommonTypeKind.Interface)
                        throw new InvalidOperationException();
                    var field = node.Members.OfType<FieldDeclarationSyntax>().SelectMany(f => f.Declaration.Variables.Select(v => v.Identifier.ValueText)).FirstOrDefault(v => v == type.ToString());
                    if (field == null)
                        throw new InvalidOperationException();
                    _delegatingFields.Add(new KeyValuePair<string, ITypeSymbol>(field, typeSymbol));
                    foreach (var t in typeSymbol.AllInterfaces)
                        _delegatingFields.Add(new KeyValuePair<string, ITypeSymbol>(field, t));
                } else
                    baseTypes.Add(Syntax.ParseTypeName(type.ToString())); // remove descendant trivia
                if (typeSymbolLoop == null)
                    typeSymbol = null;
                else
                    typeSymbol = typeSymbolLoop.TypeKind == CommonTypeKind.Interface ? typeSymbolLoop : null;
            }

            var members = node.Members.Select(m => (MemberDeclarationSyntax)Visit(m)).ToList();
            foreach (var interfaces in _delegatingFields) {
                var throwIfNull = ThrowIfNull(interfaces.Key);
                List<ISymbol> interfaceAddedMembers;
                _implementedSymbols.TryGetValue(interfaces.Value, out interfaceAddedMembers);
                var field = Syntax.ParenthesizedExpression(Syntax.CastExpression(ParseTypeNameWithGlobal(interfaces.Value), Syntax.IdentifierName(interfaces.Key)));
                foreach (var member in interfaces.Value.GetMembers()) {
                    if (interfaceAddedMembers != null && interfaceAddedMembers.Contains(member))
                        continue;
                    var method = member as MethodSymbol;
                    PropertySymbol property;
                    EventSymbol @event;
                    if (method != null) {
                        switch (method.MethodKind) {
                            case MethodKind.EventAdd:
                            case MethodKind.EventRemove:
                            case MethodKind.PropertyGet:
                            case MethodKind.PropertySet:
                                break;
                            default:
                                var a = method.Parameters.Select(p => Syntax.Argument(Syntax.IdentifierName(p.Name)));
                                var b = method.Parameters.AsEnumerable().Skip(1).Select(
                                    _ => Syntax.Token(SyntaxKind.CommaToken));
                                var call = Syntax.InvocationExpression(Syntax.MemberAccessExpression(SyntaxKind.MemberAccessExpression, field, Syntax.IdentifierName(method.Name)))
                                    .WithArgumentList(
                                    Syntax.ArgumentList(
                                    Syntax.SeparatedList(a, b)));
                                StatementSyntax exp;
                                if (method.ReturnsVoid)
                                    exp = Syntax.ExpressionStatement(call);
                                else
                                    exp = Syntax.ReturnStatement(call);
                                members.Add(Syntax.MethodDeclaration(method.ReturnsVoid ? Syntax.PredefinedType(Syntax.Token(SyntaxKind.VoidKeyword)) : ParseTypeNameWithGlobal(method.ReturnType), method.Name).WithExplicitInterfaceSpecifier(Syntax.ExplicitInterfaceSpecifier(Syntax.IdentifierName("global::" + interfaces.Value.ToString()))).WithParameterList(Syntax.ParameterList(Syntax.SeparatedList(method.Parameters.Select(p => Syntax.Parameter(Syntax.Identifier(p.Name)).WithType(ParseTypeNameWithGlobal(p.Type))), method.Parameters.AsEnumerable().Skip(1).Select(_ => Syntax.Token(SyntaxKind.CommaToken))))).WithBody(Syntax.Block(throwIfNull, exp)));
                                break;
                        }
                    } else if ((property = member as PropertySymbol) != null) {
                        var accessors = new List<AccessorDeclarationSyntax>();
                        Action<ExpressionSyntax> defineAccessors = ma => {
                            if (property.GetMethod != null)
                                accessors.Add(Syntax.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, Syntax.Block(throwIfNull, Syntax.ReturnStatement(ma))));
                            if (property.SetMethod != null)
                                accessors.Add(Syntax.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, Syntax.Block(throwIfNull, Syntax.ExpressionStatement(Syntax.BinaryExpression(SyntaxKind.AssignExpression, ma, Syntax.IdentifierName("value"))))));
                        };
                        ExpressionSyntax memberAccess;
                        if (property.IsIndexer) {
                            memberAccess = Syntax.ElementAccessExpression(field, Syntax.BracketedArgumentList(Syntax.SeparatedList(property.Parameters.Select(p => Syntax.Argument(Syntax.IdentifierName(p.Name))), property.Parameters.AsEnumerable().Skip(1).Select(_ => Syntax.Token(SyntaxKind.CommaToken)))));
                            defineAccessors(memberAccess);
                            members.Add(Syntax.IndexerDeclaration(ParseTypeNameWithGlobal(property.Type)).WithExplicitInterfaceSpecifier(Syntax.ExplicitInterfaceSpecifier(Syntax.IdentifierName("global::" + interfaces.Value.ToString()))).WithAccessorList(Syntax.AccessorList(Syntax.List<AccessorDeclarationSyntax>(accessors))).WithParameterList(Syntax.BracketedParameterList(Syntax.SeparatedList(property.Parameters.Select(p => Syntax.Parameter(Syntax.Identifier(p.Name)).WithType(ParseTypeNameWithGlobal(p.Type))), property.Parameters.AsEnumerable().Skip(1).Select(_ => Syntax.Token(SyntaxKind.CommaToken))))));
                        } else {
                            defineAccessors(Syntax.MemberAccessExpression(SyntaxKind.MemberAccessExpression, field, Syntax.IdentifierName(property.Name)));
                            members.Add(Syntax.PropertyDeclaration(ParseTypeNameWithGlobal(property.Type), Syntax.Identifier(property.Name)).WithExplicitInterfaceSpecifier(Syntax.ExplicitInterfaceSpecifier(Syntax.IdentifierName("global::" + interfaces.Value.ToString()))).WithAccessorList(Syntax.AccessorList(Syntax.List<AccessorDeclarationSyntax>(accessors))));
                        }
                    } else if ((@event = member as EventSymbol) != null) {
                        var memberAccess = Syntax.MemberAccessExpression(SyntaxKind.MemberAccessExpression, field, Syntax.IdentifierName(@event.Name));
                        members.Add(Syntax.EventDeclaration(ParseTypeNameWithGlobal(@event.Type), Syntax.Identifier(@event.Name)).WithExplicitInterfaceSpecifier(Syntax.ExplicitInterfaceSpecifier(Syntax.IdentifierName("global::" + interfaces.Value.ToString()))).WithAccessorList(Syntax.AccessorList(Syntax.List(Syntax.AccessorDeclaration(SyntaxKind.AddAccessorDeclaration, Syntax.Block(throwIfNull, Syntax.ExpressionStatement(Syntax.BinaryExpression(SyntaxKind.AddAssignExpression, memberAccess, Syntax.IdentifierName("value"))))), Syntax.AccessorDeclaration(SyntaxKind.RemoveAccessorDeclaration, Syntax.Block(throwIfNull, Syntax.ExpressionStatement(Syntax.BinaryExpression(SyntaxKind.SubtractAssignExpression, memberAccess, Syntax.IdentifierName("value")))))))));
                    } else
                        throw new InvalidOperationException();
                }
            }
            var value = Syntax.ClassDeclaration(node.AttributeLists, node.Modifiers, node.Identifier, node.TypeParameterList, Syntax.BaseList(Syntax.SeparatedList(baseTypes, baseTypes.Skip(1).Select(_ => Syntax.Token(SyntaxKind.CommaToken)))), node.ConstraintClauses, Syntax.List<MemberDeclarationSyntax>(members));
            return value;
        }

        private static IfStatementSyntax ThrowIfNull(string field) {
            return Syntax.IfStatement(Syntax.BinaryExpression(SyntaxKind.EqualsExpression, Syntax.IdentifierName(field), Syntax.LiteralExpression(SyntaxKind.NullLiteralExpression)), Syntax.ThrowStatement(Syntax.ObjectCreationExpression(Syntax.ParseTypeName("global::SimonFeature.System.ImplementingMemberNullException")).WithArgumentList(Syntax.ArgumentList())));
        }

        public override SyntaxNode VisitMethodDeclaration(MethodDeclarationSyntax node) {
            if (node.ExplicitInterfaceSpecifier == null && !node.Modifiers.Any(m => m.Kind == SyntaxKind.PublicKeyword))
                return node;
            var sourceSymbol = (MethodSymbol)_semanticModel.GetDeclaredSymbol(node);
            string _;
            ITypeSymbol __;
            RegisterMethod(sourceSymbol, node.ExplicitInterfaceSpecifier, (interfaceType, m) => new List<ISymbol> { m }, out _, out __);
            return node;
        }

        public override SyntaxNode VisitAccessorList(AccessorListSyntax node) {
            AccessorDeclarationSyntax get, set;
            bool getNull, setNull;
            if (node.Accessors.Count == 2 && ((getNull = (get = node.Accessors.First(a => a.Kind == SyntaxKind.GetAccessorDeclaration)).Body == null) ^ (setNull = (set = node.Accessors.First(a => a.Kind == SyntaxKind.SetAccessorDeclaration)).Body == null))) {
                var propertySourceSymbol = (PropertySymbol)_semanticModel.GetDeclaredSymbol(node.Parent);
                var sourceSymbol = getNull ? propertySourceSymbol.GetMethod : propertySourceSymbol.SetMethod;
                var property = node.Parent as PropertyDeclarationSyntax;
                IndexerDeclarationSyntax indexer = null;
                ExplicitInterfaceSpecifierSyntax explicitInterface;
                SyntaxTokenList modifiers;
                string propertyName;
                if (property != null) {
                    explicitInterface = property.ExplicitInterfaceSpecifier;
                    propertyName = property.Identifier.ValueText;
                    modifiers = property.Modifiers;
                } else if ((indexer = node.Parent as IndexerDeclarationSyntax) != null) {
                    explicitInterface = indexer.ExplicitInterfaceSpecifier;
                    propertyName = "this[]";
                    modifiers = indexer.Modifiers;
                } else
                    throw new InvalidOperationException();
                string field;
                ITypeSymbol interfaceTypeSymbol;
                var symbol = RegisterMethod(sourceSymbol, explicitInterface, (interfaceType, _) => explicitInterface == null && !modifiers.Any(m => m.Kind == SyntaxKind.PublicKeyword) ? new List<ISymbol>() : new List<ISymbol> { interfaceType.GetMembers().First(m => m.Name == propertyName) }, out field, out interfaceTypeSymbol, true);
                if (symbol == null)
                    throw new InvalidOperationException();
                ExpressionSyntax memberAccess = Syntax.ParenthesizedExpression(Syntax.CastExpression(ParseTypeNameWithGlobal(interfaceTypeSymbol), Syntax.IdentifierName(field)));
                if (property == null)
                    memberAccess = Syntax.ElementAccessExpression(memberAccess, Syntax.BracketedArgumentList(Syntax.SeparatedList(indexer.ParameterList.Parameters.Select(p => Syntax.Argument(Syntax.IdentifierName(p.Identifier.ValueText))), indexer.ParameterList.Parameters.AsEnumerable().Skip(1).Select(_ => Syntax.Token(SyntaxKind.CommaToken)))));
                else
                    memberAccess = Syntax.MemberAccessExpression(SyntaxKind.MemberAccessExpression, memberAccess, Syntax.IdentifierName(symbol.Name));
                var thowIfNull = ThrowIfNull(field);
                return Syntax.AccessorList(Syntax.List(getNull ? Syntax.AccessorDeclaration(SyntaxKind.GetAccessorDeclaration, Syntax.Block(thowIfNull, Syntax.ReturnStatement(memberAccess))) : get, setNull ? Syntax.AccessorDeclaration(SyntaxKind.SetAccessorDeclaration, Syntax.Block(thowIfNull, Syntax.ExpressionStatement(Syntax.BinaryExpression(SyntaxKind.AssignExpression, memberAccess, Syntax.IdentifierName("value"))))) : set));
            }
            return node;
        }

        private ISymbol RegisterMethod(MethodSymbol sourceSymbol, ExplicitInterfaceSpecifierSyntax explicitInterface, Func<ITypeSymbol, ISymbol, List<ISymbol>> declarationMembersToAdd, out string field, out ITypeSymbol interfaceTypeSymbol, bool throwIfNotFound = false) {
            string sourceSymbolName = sourceSymbol.Name;
            interfaceTypeSymbol = null;
            ISymbol symbol = null;
            List<KeyValuePair<string, ITypeSymbol>> interfacesFields;
            Func<ITypeSymbol, ISymbol> getEqualsMethod = type => type.GetMembers().FirstOrDefault(m => {
                if (m.Name != sourceSymbolName)
                    return false;
                var method = m as MethodSymbol;
                if (method == null)
                    return true;
                if (method.Parameters.Count != sourceSymbol.Parameters.Count)
                    return false;
                for (int i = 0; i < method.Parameters.Count; i++)
                    if (!(method.Parameters[i].Type.Equals(sourceSymbol.Parameters[i].Type)))
                        return false;
                return true;
            });
            if (explicitInterface == null) {
                interfacesFields = _delegatingFields.Where(f => {
                    ISymbol symbolTmp;
                    if ((symbolTmp = getEqualsMethod(f.Value)) != null) {
                        symbol = symbolTmp;
                        return true;
                    }
                    return false;
                }).ToList();
            } else {
                var interfaceTypeSymbolLocal = interfaceTypeSymbol = (ITypeSymbol)_semanticModel.GetSymbolInfo(explicitInterface.Name).Symbol;
                interfacesFields = _delegatingFields.Where(f => f.Value.Equals(interfaceTypeSymbolLocal)).ToList();
                sourceSymbolName = sourceSymbolName.Substring(interfaceTypeSymbol.ToString().Length + 1);
                symbol = getEqualsMethod(interfaceTypeSymbol);
            }
            switch (interfacesFields.Count) {
                case 0:
                    if (throwIfNotFound)
                        goto default;
                    field = null;
                    return null;
                case 1:
                    break;
                default:
                    throw new InvalidOperationException();
            }
            field = interfacesFields[0].Key;
            if (interfaceTypeSymbol == null)
                interfaceTypeSymbol = interfacesFields[0].Value;
            List<ISymbol> symbols;
            if (!_implementedSymbols.TryGetValue(interfaceTypeSymbol, out symbols))
                _implementedSymbols.Add(interfaceTypeSymbol, symbols = new List<ISymbol>());
            symbols.AddRange(declarationMembersToAdd(interfaceTypeSymbol, symbol));
            return symbol;
        }

        private static TypeSyntax ParseTypeNameWithGlobal(ITypeSymbol type) {
            string typeName = type.ToString();
            if (type.ContainingNamespace != null && typeName.StartsWith(type.ContainingNamespace.ToString()))
                typeName = "global::" + typeName;
            return Syntax.ParseTypeName(typeName);
        }
    }
}

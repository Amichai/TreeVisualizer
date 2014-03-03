using Roslyn.Compilers;
using Roslyn.Compilers.Common;
using Roslyn.Compilers.CSharp;
using Roslyn.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;

namespace SlnViz {
    public class SyntaxNodeWrapper : INotifyPropertyChanged {
        public CommonSyntaxNode Node { get; set; }
        public SyntaxNodeWrapper(CommonSyntaxNode n, IDocument doc) {
            this.Node = n;
            this.Selected = false;
            this.document = doc;
            this.documentRootNode = doc.GetSyntaxRoot() as CompilationUnitSyntax;
            this.TextEditorText = this.FullText;
        }

        public string TextEditorText { get; set; }

        public IDocument document;
        public CompilationUnitSyntax documentRootNode;

        public enum NodeType { File, Method, Property, Namespace, Class, PropertyMethod, Other };
        public static NodeType TypeToShow;

        public List<string> GetNamespaces() {
            List<string> result = new List<string>();
            if (this.Node is NamespaceDeclarationSyntax) {
                result.Add((this.Node as NamespaceDeclarationSyntax).Name.ToFullString());
            } else if (this.ChildrenType == NodeType.Namespace) {
                foreach(var r in this.Children.Select(i => i.GetNamespaces())){
                    result.AddRange(r);
                }
            }
            return result;
        }

        private bool _Selected;
        public bool Selected {
            get { return _Selected; }
            set {
                if (_Selected != value) {
                    _Selected = value;
                    OnPropertyChanged("Selected");
                }
            }
        }

        public FileLinePositionSpan LineSpan {
            get {
                try {
                    return this.Node.GetLocation().GetLineSpan(true);
                } catch {
                    return new FileLinePositionSpan();
                }
            }
        }

        public string Filepath {
            get {
                return this.LineSpan.Path;
            }
        }

        public NodeType GetNodeType() {
            if (this.Node is CompilationUnitSyntax) {
                return NodeType.File;
            } else if (this.Node is NamespaceDeclarationSyntax) {
                return NodeType.Namespace;
            } else if (this.Node is ClassDeclarationSyntax) {
                return NodeType.Class;
            } else if(this.Node is MethodDeclarationSyntax){
                return NodeType.Method;
            } else if(this.Node is PropertyDeclarationSyntax){
                return NodeType.Property;
            }
            return NodeType.Other;
        }

        public int CountSelfAndChildren(NodeType type, out int lineCount) {
            var count = 0;
            lineCount = 0;
            if (this.GetNodeType() == type) {
                count++;
                lineCount = this.Node.GetText().Lines.Count();
            }
            foreach (var c in this.Children) {
                int subCount;
                count += c.CountSelfAndChildren(type, out subCount);
                lineCount += subCount;
            }
            return count;
        }

        public int CountSelfAndChildren(NodeType type) {
            var count = 0;
            if (this.GetNodeType() == type) {
                count++;
            }
            foreach (var c in this.Children) {
                count += c.CountSelfAndChildren(type);
            }
            return count;
        }

        private NodeType ChildrenType {
            get {
                if (this.Node is CompilationUnitSyntax) {
                    return NodeType.Namespace;
                } else if (this.Node is NamespaceDeclarationSyntax) {
                    return NodeType.Class;
                } else if (this.Node is ClassDeclarationSyntax) {
                    return NodeType.PropertyMethod;
                }
                return NodeType.Method;
            }
        }

        public List<SyntaxNodeWrapper> Children {
            get {
                switch (this.ChildrenType) {
                    case NodeType.File:
                        return this.Node.DescendantNodes().OfType<CompilationUnitSyntax>().Select(i => new SyntaxNodeWrapper(i, document)).ToList();
                    case NodeType.Class:
                        return this.Node.DescendantNodes().OfType<ClassDeclarationSyntax>().Select(i => new SyntaxNodeWrapper(i, document)).ToList();
                    case NodeType.Method:
                        var children = this.Node.DescendantNodes().OfType<MethodDeclarationSyntax>().Select(i => new SyntaxNodeWrapper(i, document)).ToList();
                        return children;
                    case NodeType.Namespace:
                        return this.Node.DescendantNodes().OfType<NamespaceDeclarationSyntax>().Select(i => new SyntaxNodeWrapper(i, document)).ToList();
                    case NodeType.Property:
                        return this.Node.DescendantNodes().OfType<PropertyDeclarationSyntax>().Select(i => new SyntaxNodeWrapper(i, document)).ToList();
                    case NodeType.PropertyMethod:
                        var l1 = this.Node.DescendantNodes().OfType<PropertyDeclarationSyntax>().Select(i => new SyntaxNodeWrapper(i, document)).ToList();
                        var l2 = this.Node.DescendantNodes().OfType<MethodDeclarationSyntax>().Select(i => new SyntaxNodeWrapper(i, document));
                        var l3 = this.Node.DescendantNodes().OfType<ConstructorDeclarationSyntax>().Select(i => new SyntaxNodeWrapper(i, document));
                        l1.AddRange(l2);
                        l1.AddRange(l3);
                        return l1;


                    default:
                        throw new Exception();
                }
            }
        }

        public static Subject<ProjectUpdated> ProjectUpdated = new Subject<ProjectUpdated>();

        public string Description {
            get {
                string toReturn = "";
                if (this.Node is NamespaceDeclarationSyntax) {
                    toReturn += "Namespace: " + (this.Node as NamespaceDeclarationSyntax).Name;
                } else if (this.Node is MethodDeclarationSyntax) {
                    var cast = this.Node as MethodDeclarationSyntax;
                    toReturn += "Method: " + string.Concat(cast.Modifiers.Select(i => i.ToString() + " ")) +
                        cast.ReturnType.ToFullString() + cast.Identifier.ToFullString() + cast.ParameterList.ToFullString();
                } else if (this.Node is CompilationUnitSyntax) {
                    var cast = this.Node as CompilationUnitSyntax;
                    //toReturn += "Compilation unit: " + cast.;
                } else if (this.Node is ClassDeclarationSyntax) {
                    var cast = this.Node as ClassDeclarationSyntax;
                    toReturn += "Class: " +
                        string.Concat(cast.Modifiers.Select(i => i.ToString() + " ")) +
                        cast.Identifier;
                } else if (this.Node is PropertyDeclarationSyntax) {
                    var cast = this.Node as PropertyDeclarationSyntax;
                    toReturn += "Property: " +
                        string.Concat(cast.Modifiers.Select(i => i.ToString() + " ")) +
                        cast.Type.ToFullString() + cast.Identifier;
                } else if (this.Node is ConstructorDeclarationSyntax) {
                    var cast = this.Node as ConstructorDeclarationSyntax;
                    toReturn += "Constructor: " +
                        string.Concat(cast.Modifiers.Select(i => i.ToString() + " ")) +
                        cast.ParameterList.ToFullString();
                }
                
                return toReturn;
            }
        }

        public string FullText {
            get {
                return this.Node.GetText().ToString();
            }
        }

        public string AsString {
            get {
                return this.Node.GetType().Name;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name) {
            var eh = PropertyChanged;
            if (eh != null) {
                eh(this, new PropertyChangedEventArgs(name));
            }
        }
    }

    public class ProjectUpdated {
        public IProject Project { get; set; }
        public SyntaxTree Tree { get; set; }
    }
}

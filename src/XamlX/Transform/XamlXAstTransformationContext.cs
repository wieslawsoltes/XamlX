using System;
using System.Collections.Generic;
using XamlX.Ast;
using XamlX.TypeSystem;

namespace XamlX.Transform
{
    public class XamlXAstTransformationContext
    {
        private Dictionary<Type, object> _items = new Dictionary<Type, object>();
        private List<IXamlXAstNode> _parentNodes = new List<IXamlXAstNode>();
        public Dictionary<string, string> NamespaceAliases { get; set; } = new Dictionary<string, string>();      
        public XamlXTransformerConfiguration Configuration { get; }
        public IXamlXAstValueNode RootObject { get; set; }
        public bool StrictMode { get; }

        public IXamlXAstNode Error(IXamlXAstNode node, Exception e)
        {
            if (StrictMode)
                throw e;
            return node;
        }

        public IXamlXAstNode ParseError(string message, IXamlXAstNode node) =>
            Error(node, new XamlXParseException(message, node));
        
        public IXamlXAstNode ParseError(string message, IXamlXAstNode offender, IXamlXAstNode ret) =>
            Error(ret, new XamlXParseException(message, offender));

        public XamlXAstTransformationContext(XamlXTransformerConfiguration configuration,
            Dictionary<string, string> namespaceAliases, bool strictMode = true)
        {
            Configuration = configuration;
            NamespaceAliases = namespaceAliases;
            StrictMode = strictMode;
        }

        public T GetItem<T>() => (T) _items[typeof(T)];
        public void SetItem<T>(T item) => _items[typeof(T)] = item;

        class Visitor : IXamlXAstVisitor
        {
            private readonly XamlXAstTransformationContext _context;
            private readonly IXamlXAstTransformer _transformer;

            public Visitor(XamlXAstTransformationContext context, IXamlXAstTransformer transformer)
            {
                _context = context;
                _transformer = transformer;
            }
            
            public IXamlXAstNode Visit(IXamlXAstNode node) => _transformer.Transform(_context, node);

            public void Push(IXamlXAstNode node) => _context._parentNodes.Add(node);

            public void Pop() => _context._parentNodes.RemoveAt(_context._parentNodes.Count - 1);
        }
        
        public IXamlXAstNode Visit(IXamlXAstNode root, IXamlXAstTransformer transformer)
        {
            root = root.Visit(new Visitor(this, transformer));
            return root;
        }
    }
}

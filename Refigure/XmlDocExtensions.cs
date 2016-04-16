using System;
using System.Xml;

namespace Refigure
{
    public static class XmlDocExtensions
    {
        /// <summary>
        /// Source : http://stackoverflow.com/questions/2915294/iterating-through-all-nodes-in-xml-file
        /// </summary>
        /// <param name="doc"></param>
        /// <param name="elementVisitor"></param>
        public static void IterateThroughAllNodes(
            this XmlDocument doc,
            Action<XmlNode> elementVisitor)
        {
            if (doc != null && elementVisitor != null)
            {
                foreach (XmlNode node in doc.ChildNodes)
                {
                    doIterateNode(node, elementVisitor);
                }
            }
        }

        /// <summary>
        /// Source : http://stackoverflow.com/questions/2915294/iterating-through-all-nodes-in-xml-file
        /// </summary>
        /// <param name="node"></param>
        /// <param name="elementVisitor"></param>
        private static void doIterateNode(
            XmlNode node,
            Action<XmlNode> elementVisitor)
        {
            elementVisitor(node);

            foreach (XmlNode childNode in node.ChildNodes)
            {
                doIterateNode(childNode, elementVisitor);
            }
        }
    }
}

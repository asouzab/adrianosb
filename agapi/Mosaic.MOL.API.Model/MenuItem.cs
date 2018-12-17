using System;
using System.Collections.Generic;

namespace Mosaic.MOL.API.Model
{
    public class MenuItem
    {
        public int Id { get; set; }
        public string label { get; set; }
        public string icon { get; set; }
        public string url { get; set; }
        public List<MenuItem> items { get; set; }

        public MenuItem()
        {
            items = new List<MenuItem>();
        }

        public static IEnumerable<MenuItem> DepthFirstSearch(MenuItem root, Func<MenuItem, bool> predicate)
        {
            var stack = new Stack<MenuItem>();
            stack.Push(root);
            while (stack.Count > 0)
            {
                var current = stack.Pop();
                if (predicate(current))
                {
                    yield return current;
                }

                foreach (var node in current.items)
                {
                    stack.Push(node);
                }
            }
        }
    }
}

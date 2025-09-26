using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;

namespace Ui.Helpers
{
    public static class VisualTreeHelpers
    {
        public static IEnumerable<DependencyObject> GetVisualDescendants(this DependencyObject root)
        {
            if (root == null)
                yield break;

            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                yield return child;

                foreach (var descendant in GetVisualDescendants(child))
                {
                    yield return descendant;
                }
            }
        }
    }
}
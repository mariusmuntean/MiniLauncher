using System;
using Xamarin.Forms;

namespace MiniLauncher
{
    public static class ViewExtensions
    {
        public static View GetParentView(this View view)
        {
            View parentView = null;
            var parent = view.Parent;
            while (parent != null)
            {
                parentView = parent as View;
                if (parentView != null)
                {
                    break;
                }

                parent = parent.Parent;
            }

            return parentView;
        }

        public static double? GetDistanceToCenter(this View view)
        {
            // Determine parent view
            var parentView = view.GetParentView();
            if (parentView == null)
            {
                return null;
            }

            // Compute cartesian distance from view's center to the parent#s center
            var xDistance = view.X + (view.Width / 2.0d) + view.TranslationX
                            - (parentView.Width / 2.0d);

            var yDistance = view.Y + (view.Height / 2.0d) + view.TranslationY
                            - (parentView.Height / 2.0d);

            return Math.Sqrt(Math.Pow(xDistance, 2) + Math.Pow(yDistance, 2));
        }
    }
}
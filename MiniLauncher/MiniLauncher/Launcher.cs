using System;
using System.Linq;
using Xamarin.Forms;
using Color = System.Drawing.Color;

namespace MiniLauncher
{
    public partial class Launcher : ContentView
    {
        private readonly RelativeLayout _content;
        private readonly RingCompute _ringCompute;

        private const double DegToRandFactor = 0.0174533d;

        public Launcher()
        {
            _ringCompute = new RingCompute();

            _content = new RelativeLayout();
            Content = _content;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ItemsProperty.PropertyName)
            {
                Test();
            }
        }

        private void Test()
        {
            var edgeLength = 30.0d;

            _content.Children.Add(new BoxView() {BackgroundColor = Xamarin.Forms.Color.Goldenrod},
                Constraint.RelativeToParent(parent =>
                {
                    var halfWidth = parent.Width / 2.0d;
                    // var edgeLength = sizeFactor * Math.Min(parent.Width, parent.Height);

                    return halfWidth // move to center 
                           + 0.0d * Math.Cos(0.0d)
                           - edgeLength / 2.0d;
                }),
                Constraint.RelativeToParent(parent =>
                {
                    var halfHeight = parent.Height / 2.0d;
                    // var edgeLength = sizeFactor * Math.Min(parent.Width, parent.Height);

                    return halfHeight // move to center 
                           + 0.0d * Math.Sin(0.0d)
                           - edgeLength / 2.0d;
                }),
                Constraint.RelativeToParent(parent => edgeLength),
                Constraint.RelativeToParent(parent => edgeLength)
            );

            var startAngle = 0.0d;
            var angleStep = 60.0d;
            var sizeFactor = 0.1d;
            var previousTheta = 0.0d;


            for (var i = 1; i < Items.Count(); i++)
            {
                var ringIndex = _ringCompute.ComputeRingIndex(i);
                var rho = 1.5 * ringIndex * edgeLength;

                var currentOffset = angleStep / ringIndex;
                var theta = (startAngle + previousTheta + currentOffset);
                previousTheta = theta;

                _content.Children.Add(new BoxView() {BackgroundColor = Color.Gold},
                    Constraint.RelativeToParent(parent =>
                    {
                        var halfWidth = parent.Width / 2.0d;
                        // var edgeLength = sizeFactor * Math.Min(parent.Width, parent.Height);

                        return halfWidth // move to center 
                               + rho * Math.Cos(theta * DegToRandFactor) // radians
                               - edgeLength / 2.0d;
                    }),
                    Constraint.RelativeToParent(parent =>
                    {
                        var halfHeight = parent.Height / 2.0d;
                        // var edgeLength = sizeFactor * Math.Min(parent.Width, parent.Height);

                        return halfHeight // move to center 
                               + rho * Math.Sin(theta * DegToRandFactor) // radians
                               - edgeLength / 2.0d;
                    }),
                    Constraint.RelativeToParent(parent => edgeLength),
                    Constraint.RelativeToParent(parent => edgeLength)
                );
            }
        }
    }
}
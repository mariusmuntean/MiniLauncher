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
            
            var space = new HexSpace(Items.Select(i => new object()));
            var hex2pix = new Hex2Pix();
            foreach (var hexPayloadPair in space.Elements())
            {
                var hex = hexPayloadPair.Key;
                var payload = hexPayloadPair.Value;

                var (x, y) = hex2pix.ToPix(hex, edgeLength);
                _content.Children.Add(new BoxView() {BackgroundColor = Color.Gold},
                    Constraint.RelativeToParent(parent =>
                    {
                        var halfWidth = parent.Width / 2.0d;
                        return halfWidth // move to center 
                               + x
                               - edgeLength / 2.0d;
                    }),
                    Constraint.RelativeToParent(parent =>
                    {
                        var halfHeight = parent.Height / 2.0d;

                        return halfHeight // move to center 
                               + y
                               - edgeLength / 2.0d;
                    }),
                    Constraint.RelativeToParent(parent => edgeLength),
                    Constraint.RelativeToParent(parent => edgeLength)
                );
            }
        }
    }
}
using System;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;
using Color = System.Drawing.Color;

namespace MiniLauncher
{
    public partial class Launcher<T> : ContentView where T : IItem
    {
        private const double DegToRandFactor = 0.0174533d;
        private const double Sqrt3 = 1.73205080757d;
        private const double Size = 25.0d;

        private readonly RelativeLayout _content;
        private readonly RingCompute _ringCompute;

        private HexSpace<T> _space = new HexSpace<T>();
        private readonly Hex2Pix _hex2Pix = new Hex2Pix();

        public Launcher()
        {
            _ringCompute = new RingCompute();

            _content = new RelativeLayout();
            Content = _content;
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == ItemTemplateProperty.PropertyName)
            {
                RenderChildren();
            }

            if (propertyName == ItemsProperty.PropertyName)
            {
                _space = new HexSpace<T>(Items);

                if (Items is INotifyCollectionChanged observableCollection)
                {
                    observableCollection.CollectionChanged -= ItemsChanged;
                    observableCollection.CollectionChanged += ItemsChanged;
                }

                RenderChildren();
            }
        }

        private void ItemsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:

                    foreach (var eNewItem in e.NewItems)
                    {
                        var newPayload = eNewItem as T;
                        var newHex = _space.Add(newPayload);
                        RenderChild(newHex, newPayload);
                    }

                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Remove:

                    foreach (var removedPayload in e.OldItems)
                    {
                        var hexPayloadToRemove = _space.Elements().FirstOrDefault(pair => pair.Value == removedPayload);
                        _space.Remove(hexPayloadToRemove.Key);

                        var viewToRemove = _content.Children.FirstOrDefault(v => v.BindingContext == removedPayload);
                        _content.Children.Remove(viewToRemove);
                    }


                    break;
                case NotifyCollectionChangedAction.Replace:
                    break;
                case NotifyCollectionChangedAction.Reset:

                    _space.Clear();
                    _content.Children.Clear();

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void RenderChildren()
        {
            _content.Children.Clear();

            foreach (var hexPayloadPair in _space.Elements())
            {
                var hex = hexPayloadPair.Key;
                var payload = hexPayloadPair.Value;

                RenderChild(hex, payload);
            }
        }

        private void RenderChild(Hex hex, T payload)
        {
            var (x, y) = _hex2Pix.ToPix(hex, Size);
            var w = Sqrt3 * Size;
            var h = 2 * Size;

            var view = ItemTemplate != null ? ItemTemplate.CreateContent() as View : new BoxView() {BackgroundColor = Color.Gold};
            view.BindingContext = payload;
            _content.Children.Add(view,
                Constraint.RelativeToParent(parent =>
                {
                    var halfWidth = parent.Width / 2.0d;
                    return halfWidth // move to center 
                           + x
                           - w / 2.0d;
                }),
                Constraint.RelativeToParent(parent =>
                {
                    var halfHeight = parent.Height / 2.0d;

                    return halfHeight // move to center 
                           + y
                           - h / 2.0d;
                }),
                Constraint.RelativeToParent(parent => w),
                Constraint.RelativeToParent(parent => h)
            );
        }
    }
}
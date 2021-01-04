using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Shapes;

namespace MiniLauncher
{
    public partial class Launcher<T> : ContentView where T : IItem
    {
        private const double DegToRandFactor = 0.0174533d;
        private const double Sqrt3 = 1.73205080757d;
        private const double Sqrt3_by_3 = 0.57735026919d;
        private const double Size = 25.0d;
        private const double ChildNeutralScale = 0.8d;
        private const string ChildReleasedAnimationName = "ChildReleasedAnimation";
        private const string ChildPressedAnimationName = "ChildPressedAnimation";

        private readonly RelativeLayout _content;
        private readonly RingCompute _ringCompute;

        private double _xTranslationAtGestureStart = 0.0d;
        private double _yTranslationAtGestureStart = 0.0d;
        private double _xTranslation = 0.0d;
        private double _yTranslation = 0.0d;

        private HexSpace<T> _space = new HexSpace<T>();
        private readonly Hex2Pix _hex2Pix = new Hex2Pix();

        public Launcher()
        {
            _ringCompute = new RingCompute();

            _content = new RelativeLayout();
            _content.IsClippedToBounds = true;
            Content = _content;

            var panGestureRecognizer = new PanGestureRecognizer();
            panGestureRecognizer.PanUpdated += OnPanUpdated;
            this.GestureRecognizers.Add(panGestureRecognizer);

            var pinchGestureRecognizer = new PinchGestureRecognizer();
            pinchGestureRecognizer.PinchUpdated += OnPinchUpdated;
            this.GestureRecognizers.Add(pinchGestureRecognizer);
        }

        private void OnPinchUpdated(object sender, PinchGestureUpdatedEventArgs e)
        {
            Console.WriteLine("pinch: " + e.Status);
        }

        private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
        {
            switch (e.StatusType)
            {
                case GestureStatus.Started:
                    _xTranslationAtGestureStart = _xTranslation;
                    _yTranslationAtGestureStart = _yTranslation;
                    break;
                case GestureStatus.Running:
                    _xTranslation = _xTranslationAtGestureStart + e.TotalX;
                    _yTranslation = _yTranslationAtGestureStart + e.TotalY;
                    break;
                case GestureStatus.Completed:
                    SnapToNearestHex();
                    break;
                case GestureStatus.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            TranslateChildren();
        }

        private void SnapToNearestHex()
        {
            // If the space has no hexes, then just leave
            if (_space.Count == 0)
            {
                return;
            }

            // First, work out which Hex is now in the center. It might be an occupied Hex or a free one.
            var fractionalQ = (Sqrt3_by_3 * -_xTranslation - 0.3333 * -_yTranslation) / Size;
            var fractionalR = 0.6666 * -_yTranslation / Size;

            var q = (int) Math.Truncate(fractionalQ);
            var r = (int) Math.Truncate(fractionalR);
            var centerHex = new Hex(q, r);

            // If occupied Hex: just center it.
            if (_space.Contains(centerHex))
            {
                SnapToHex(centerHex);
            }
            else // If free Hex: find the nearest occupied Hex and move it to the center
            {
                var nearestHex = _space.GetNearestHexes(centerHex).First();
                SnapToHex(nearestHex);
            }

            void SnapToHex(Hex hex)
            {
                var centerPayload = _space.GetPayload(hex);
                var centerChild = _content.Children.First(c => c.BindingContext == centerPayload);
                SnapChildren(
                    _content.Width / 2.0d - (centerChild.X + centerChild.Width / 2.0d + _xTranslation),
                    _content.Height / 2.0d - (centerChild.Y + centerChild.Height / 2.0d + _yTranslation)
                );
            }
        }

        private void SnapChildren(double horizontalDistance, double verticalDistance)
        {
            var horizontalSnapAnimation = new Animation(d => { _xTranslation = d; }, _xTranslation, _xTranslation + horizontalDistance, easing: Easing.CubicOut);
            var verticalSnapAnimation = new Animation(d => { _yTranslation = d; }, _yTranslation, _yTranslation + verticalDistance, easing: Easing.CubicOut);

            var parentAnimation = new Animation(d => TranslateChildren());
            parentAnimation.Add(0, 1, horizontalSnapAnimation);
            parentAnimation.Add(0, 1, verticalSnapAnimation);
            parentAnimation.Commit(this, "SnapChildrenAnimation");
        }

        private void TranslateChildren()
        {
            foreach (var child in _content.Children)
            {
                child.TranslationX = _xTranslation;
                child.TranslationY = _yTranslation;
            }
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

            var view = ItemTemplate switch
            {
                _ when !(ItemTemplate is null) => ItemTemplate.CreateContent(),
                _ when ItemTemplate is null => GetDefaultItemTemplate(payload, w, h).CreateContent()
            } as View;
            view.TranslationX = _xTranslation;
            view.TranslationY = _yTranslation;
            view.BindingContext = payload;
            _content.Children.Add(view,
                Constraint.RelativeToParent(parent =>
                {
                    var halfWidth = parent.Width / 2.0d;
                    return halfWidth // move to center 
                           + x // move to own position
                           - w / 2.0d; // center
                }),
                Constraint.RelativeToParent(parent =>
                {
                    var halfHeight = parent.Height / 2.0d;

                    return halfHeight // move to center 
                           + y // move to own position
                           - h / 2.0d; // center
                }),
                Constraint.RelativeToParent(parent => w),
                Constraint.RelativeToParent(parent => h)
            );
        }

        private DataTemplate GetDefaultItemTemplate(T payload, double width, double height)
        {
            return new DataTemplate(() =>
            {
                var imageButton = new ImageButton {Aspect = Aspect.AspectFill};
                imageButton.SetBinding(ImageButton.SourceProperty, nameof(IItem.Icon));
                imageButton.Clip = new EllipseGeometry
                {
                    Center = new Point(width / 2.0d, height / 2.0d),
                    RadiusX = width / 2.0d,
                    RadiusY = width / 2.0d
                };
                imageButton.Scale = ChildNeutralScale;
                //imageButton.InputTransparent = true;

                imageButton.SetBinding(ImageButton.CommandProperty, nameof(IItem.Command));

                imageButton.Pressed += (sender, args) => { OnChildPressed(payload); };
                imageButton.Released += (sender, args) => { OnChildReleased(payload); };

                return imageButton;
            });
        }

        private void OnChildPressed(T payload)
        {
            var childrenToAnimate = FindChildrenToAnimate(payload);

            var startScale = childrenToAnimate.First().Scale;
            var endScale = 0.9d * startScale;
            var childrenAnimation = new Animation(d =>
            {
                foreach (var child in childrenToAnimate)
                {
                    child.Scale = d;
                }
            }, startScale, endScale, Easing.CubicInOut);

            this.AbortAnimation(ChildReleasedAnimationName);
            childrenAnimation.Commit(this, ChildPressedAnimationName);
        }

        private void OnChildReleased(T payload)
        {
            var childrenToAnimate = FindChildrenToAnimate(payload);

            var startScale = childrenToAnimate.First().Scale;
            var endScale = ChildNeutralScale;
            var childrenAnimation = new Animation(d =>
            {
                foreach (var child in childrenToAnimate)
                {
                    child.Scale = d;
                }
            }, startScale, endScale, Easing.CubicInOut);

            this.AbortAnimation(ChildPressedAnimationName);
            childrenAnimation.Commit(this, ChildReleasedAnimationName);
        }

        private View[] FindChildrenToAnimate(T payload)
        {
            var matchingHexPayloadPair = _space.Elements().FirstOrDefault(hexPayloadPair => hexPayloadPair.Value == payload);
            if (matchingHexPayloadPair.Equals(default(KeyValuePair<Hex, T>)))
            {
                return new View[] { };
            }

            var neighborHexes = _space.GetNeighborHexes(matchingHexPayloadPair.Key);
            if (!neighborHexes.Any())
            {
                return new View[] { };
            }

            var childPayloads = new HashSet<T>(neighborHexes.Select(h => _space.GetPayload(h)));
            childPayloads.Add(payload);

            var childrenToAnimate = _content.Children.Where(child => childPayloads.Contains(child.BindingContext)).ToArray();
            return childrenToAnimate;
        }
    }
}
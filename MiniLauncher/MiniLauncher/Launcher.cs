using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.PlatformConfiguration;
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
        private const string KineticScrollAnimationName = "KineticScrollAnimation";
        private const string SnapChildrenAnimationName = "SnapChildrenAnimation";

        private readonly RelativeLayout _content;
        private readonly RingCompute _ringCompute;

        private double _xTranslationAtGestureStart = 0.0d;
        private double _yTranslationAtGestureStart = 0.0d;
        private double _xTranslation = 0.0d;
        private double _yTranslation = 0.0d;

        private double _previousXTranslationTotal = 0.0d;
        private double _previousYTranslationTotal = 0.0d;
        private double _xTranslationDelta = 0.0d;
        private double _yTranslationDelta = 0.0d;

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

                    this.AbortAnimation(KineticScrollAnimationName);
                    break;
                case GestureStatus.Running:
                    _xTranslation = _xTranslationAtGestureStart + e.TotalX;
                    _yTranslation = _yTranslationAtGestureStart + e.TotalY;

                    _xTranslationDelta = e.TotalX - _previousXTranslationTotal;
                    _yTranslationDelta = e.TotalY - _previousYTranslationTotal;
                    _previousXTranslationTotal = e.TotalX;
                    _previousYTranslationTotal = e.TotalY;
                    break;
                case GestureStatus.Completed:
                    KineticTranslateChildren(_xTranslationDelta, _yTranslationDelta);
                    break;
                case GestureStatus.Canceled:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            ScaleTranslateChildren();
        }

        private void KineticTranslateChildren(double xTranslationDelta, double yTranslationDelta)
        {
            // Determine the distance of the last finger movement.
            var distance = Math.Sqrt(Math.Pow(xTranslationDelta, 2) + Math.Pow(yTranslationDelta, 2));

            // Determine the proportion of the x and y components.
            var xProportion = Math.Abs(distance / xTranslationDelta);
            var yProportion = Math.Abs(distance / yTranslationDelta);

            // Determine the max translation distance. The idea is to stop the kinetic scrolling when there are no more visible Hexes.
            var farthestHexes = _space.GetFarthestHexes(new Hex(0, 0));
            var farthestHex = farthestHexes.Any() ? farthestHexes.First() : new Hex(0, 0);
            var (farX, farY) = _hex2Pix.ToPix(farthestHex, Size);
            var farthestHexDistanceToCenter = Math.Max(_content.Width / 2.0d, _content.Height / 2.0d) + Math.Sqrt(Math.Pow(farX, 2) + Math.Pow(farY, 2));

            // Continue moving in the same direction
            this.AnimateKinetic(KineticScrollAnimationName,
                (sign, velocity) =>
                {
                    // Direction is kept by accounting for the proportion of the x/y component.
                    _xTranslation += Math.Sign(xTranslationDelta) * velocity / xProportion;
                    _yTranslation += Math.Sign(yTranslationDelta) * velocity / yProportion;
                    ScaleTranslateChildren();

                    // If the kinetic scrolling translated past the max distance then stop.
                    var totalTranslation = Math.Sqrt(Math.Pow(_xTranslation, 2) + Math.Pow(_yTranslation, 2));
                    return totalTranslation <= farthestHexDistanceToCenter;
                },
                distance,
                0.05d,
                () =>
                {
                    // When the kinetic animation is finished, just snap to the nearest Hex
                    SnapToNearestHex();
                });
        }

        private void TransformChild(View child)
        {
            // Translation
            child.TranslationX = _xTranslation;
            child.TranslationY = _yTranslation;

            var (hScaling, vScaling) = ComputeScaling(child);

            // Scaling
            child.Scale = Math.Min(hScaling, vScaling);
        }

        private (double hScaling, double vScaling) ComputeScaling(View child)
        {
            var minScale = 0.0d;
            var normalScale = 0.80d;
            var maxScale = 1.0d;

            var hScaling = normalScale;
            var vScaling = normalScale;
            var regionWidth = 0.15d * Math.Min(_content.Width, _content.Height);
            var centerRegionRadius = 0.1d * Math.Min(_content.Width, _content.Height);

            var childCenterX = child.X + (child.Width / 2.0d) + child.TranslationX;
            var childCenterY = child.Y + (child.Height / 2.0d) + child.TranslationY;

            // near the left edge?
            if (childCenterX <= regionWidth)
            {
                hScaling = minScale + (childCenterX / regionWidth).Clamp(0.0d, 1.0d) * normalScale;
            }
            else
            {
                var rightEdgeStart = _content.Width - regionWidth;
                if (childCenterX >= rightEdgeStart) // near the right edge?
                {
                    var pastRightEdgeProportion = 1.0d - ((childCenterX - rightEdgeStart) / regionWidth).Clamp(0.0d, 1.0d);
                    hScaling = minScale + pastRightEdgeProportion * normalScale;
                }
            }

            // near the top edge?
            if (childCenterY <= regionWidth)
            {
                vScaling = minScale + (childCenterY / regionWidth).Clamp(0.0d, 1.0d) * normalScale;
            }
            else
            {
                var bottomEdgeStart = _content.Height - regionWidth;
                if (childCenterY >= bottomEdgeStart) // near the bottom edge?
                {
                    var pastBottomEdgeProportion = (1.0d - ((childCenterY - bottomEdgeStart) / regionWidth).Clamp(0.0d, 1.0d));
                    vScaling = minScale + pastBottomEdgeProportion * normalScale;
                }
            }

            // near the center?
            var (distance, hDistance, vDistance) = ComputeDistanceFromCenter(child);
            if (distance <= centerRegionRadius)
            {
                hScaling = normalScale + (1.0d - distance / centerRegionRadius) * (maxScale - normalScale);
                vScaling = hScaling;
            }

            return (hScaling, vScaling);
        }

        private (double distance, double hDistance, double vDistance) ComputeDistanceFromCenter(View child)
        {
            var horizontalDistance = _content.Width / 2.0d - (child.X + child.Width / 2.0d + child.TranslationX);
            var verticalDistance = _content.Height / 2.0d - (child.Y + child.Height / 2.0d + child.TranslationY);
            var childDistanceFromCenter = Math.Sqrt(
                Math.Pow(horizontalDistance, 2) +
                Math.Pow(verticalDistance, 2)
            );
            return (childDistanceFromCenter, horizontalDistance, verticalDistance);
        }

        private void SnapToNearestHex()
        {
            // If the space has no hexes, then just leave
            if (_space.Count == 0)
            {
                return;
            }

            // First, work out which Hex is now in the center. It might be an occupied Hex or a free one.
            var centerHex = _hex2Pix.ToHex(-_xTranslation, -_yTranslation, Size);

            // If occupied Hex: just center it.
            if (_space.Contains(centerHex))
            {
                SnapToHex(centerHex);
            }
            else // If free Hex: find the nearest occupied Hex and move it to the center
            {
                var nearestHexes = _space.GetNearestHexes(centerHex);

                // From the equidistant Hexes, determine the one where the View is closest to the center and snap to that Hex
                var nearestHex = nearestHexes.OrderBy(hex => GetViewForHex(hex).GetDistanceToCenter()).FirstOrDefault();
                SnapToHex(nearestHex);
            }

            void SnapToHex(Hex hex)
            {
                var centerChild = GetViewForHex(hex);
                SnapChildren(
                    _content.Width / 2.0d - (centerChild.X + centerChild.Width / 2.0d + _xTranslation),
                    _content.Height / 2.0d - (centerChild.Y + centerChild.Height / 2.0d + _yTranslation)
                );
            }
        }

        private View GetViewForHex(Hex hex)
        {
            return _content.Children.FirstOrDefault(view => view.BindingContext == _space.GetPayload(hex));
        }

        private void SnapChildren(double horizontalDistance, double verticalDistance)
        {
            var horizontalSnapAnimation = new Animation(d => { _xTranslation = d; }, _xTranslation, _xTranslation + horizontalDistance, easing: Easing.CubicOut);
            var verticalSnapAnimation = new Animation(d => { _yTranslation = d; }, _yTranslation, _yTranslation + verticalDistance, easing: Easing.CubicOut);

            var parentAnimation = new Animation(d => ScaleTranslateChildren());
            parentAnimation.Add(0, 1, horizontalSnapAnimation);
            parentAnimation.Add(0, 1, verticalSnapAnimation);
            parentAnimation.Commit(this, SnapChildrenAnimationName, length: 500);
        }

        private void ScaleTranslateChildren()
        {
            foreach (var child in _content.Children)
            {
                TransformChild(child);
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
                        var hexToRemove = hexPayloadToRemove.Key;
                        _space.Remove(hexToRemove);

                        var viewToRemove = GetViewForHex(hexToRemove);
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
            var (w, h) = _hex2Pix.ComputeHexDimensions(Size);

            var view = ItemTemplate switch
            {
                _ when !(ItemTemplate is null) => ItemTemplate.CreateContent(),
                _ when ItemTemplate is null => GetDefaultItemTemplate(payload, w, h).CreateContent()
            } as View;

            // Hide the view as long as layouting and transformation isn't ready
            view.IsVisible = false;

            // Add the view to the layout
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

            // Transform the view
            TransformChild(view);

            // Show the view when ready
            view.IsVisible = true;
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
                imageButton.InputTransparent = true;

                imageButton.SetBinding(ImageButton.CommandProperty, nameof(IItem.Command));

                imageButton.Pressed += async (sender, args) => { await OnChildPressed(payload); };
                imageButton.Released += async (sender, args) => { await OnChildReleased(payload); };

                return imageButton;
            });
        }

        private async Task OnChildPressed(T payload)
        {
            // If the pressed or released animation is still running then wait a bit
            while (this.AnimationIsRunning(ChildPressedAnimationName) || this.AnimationIsRunning(ChildReleasedAnimationName))
            {
                await Task.Delay(10);
            }

            var parentAnimation = new Animation();
            var childrenToAnimate = FindChildrenToAnimate(payload);

            foreach (var childView in childrenToAnimate)
            {
                var startScale = childView.Scale;
                var endScale = 0.9d * startScale;
                var childAnimation = new Animation(d => { childView.Scale = d; }, startScale, endScale, Easing.CubicInOut);

                parentAnimation.Add(0.0, 1.0, childAnimation);
            }

            this.AbortAnimation(ChildReleasedAnimationName);
            parentAnimation.Commit(this, ChildPressedAnimationName);
        }

        private async Task OnChildReleased(T payload)
        {
            // If the pressed or released animation is still running then wait a bit
            while (this.AnimationIsRunning(ChildPressedAnimationName) || this.AnimationIsRunning(ChildReleasedAnimationName))
            {
                await Task.Delay(10);
            }

            var parentAnimation = new Animation();
            var childrenToAnimate = FindChildrenToAnimate(payload);

            foreach (var childView in childrenToAnimate)
            {
                var startScale = childView.Scale;
                var endScale = childView.Scale + childView.Scale / 9d;
                var childAnimation = new Animation(d => { childView.Scale = d; }, startScale, endScale, Easing.CubicInOut);
                parentAnimation.Add(0.0d, 1.0d, childAnimation);
            }

            this.AbortAnimation(ChildPressedAnimationName);
            parentAnimation.Commit(this, ChildReleasedAnimationName);
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
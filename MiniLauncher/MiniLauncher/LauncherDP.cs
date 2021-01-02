using System.Collections.Generic;
using Xamarin.Forms;

namespace MiniLauncher
{
    public partial class Launcher
    {
        public static BindableProperty ItemsProperty = BindableProperty.Create(
            nameof(Items),
            typeof(IEnumerable<int>),
            typeof(Launcher),
            new[] {1}
        );

        public IEnumerable<int> Items
        {
            get => (IEnumerable<int>) GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(Launcher),
            default(DataTemplate),
            BindingMode.OneWay
        );

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate) GetValue(ItemTemplateProperty);
            set => SetValue(ItemTemplateProperty, value);
        }
    }
}
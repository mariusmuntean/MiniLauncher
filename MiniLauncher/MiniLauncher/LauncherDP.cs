using System.Collections.Generic;
using Xamarin.Forms;

namespace MiniLauncher
{
    public partial class Launcher<T>
    {
        public static BindableProperty ItemsProperty = BindableProperty.Create(
            nameof(Items),
            typeof(IEnumerable<T>),
            typeof(Launcher<T>),
            default(IEnumerable<T>)
        );

        public IEnumerable<T> Items
        {
            get => (IEnumerable<T>) GetValue(ItemsProperty);
            set => SetValue(ItemsProperty, value);
        }

        public static readonly BindableProperty ItemTemplateProperty = BindableProperty.Create(
            nameof(ItemTemplate),
            typeof(DataTemplate),
            typeof(Launcher<T>),
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
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MiniLauncher.Sample
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Playground : ContentPage
    {
        private Assembly _thisAssembly;

        public Playground()
        {
            Items = new ObservableCollection<Item>();
            InitializeComponent();
        }

        public ObservableCollection<Item> Items { get; set; }

        private void OnAddClicked(object sender, EventArgs e)
        {
            var availableIcons = typeof(MainPage).GetTypeInfo().Assembly.GetManifestResourceNames().Where(r => r.ToLower().EndsWith(".png"));

            if (!availableIcons.Any())
            {
                Console.WriteLine("No icons found");
                return;
            }

            var iconToAddIdx = Items.Count % availableIcons.Count();
            var icon = availableIcons.ElementAt(iconToAddIdx);
            var imageSource = ImageSource.FromResource(icon, _thisAssembly ??= typeof(MainPage).GetTypeInfo().Assembly);
            Items.Add(new Item
            {
                Icon = imageSource,
                Command = new Command(async () => { ChosenAppImage.Source = imageSource; })
            });
        }

        private void OnRemoveClicked(object sender, EventArgs e)
        {
            if (Items.Count > 0)
            {
                Items.Remove(Items.Last());
            }
        }
    }
}
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using Path = System.IO.Path;

namespace MiniLauncher.Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            Items = new ObservableCollection<Item>();
            InitializeComponent();
        }

        public ObservableCollection<Item> Items { get; set; }

        private void OnAddClicked(object sender, EventArgs e)
        {
            var availableIcons = typeof(MainPage).GetTypeInfo().Assembly.GetManifestResourceNames().Where(r => r.ToLower().EndsWith(".png"));

            var iconToAddIdx = Items.Count % availableIcons.Count();
            var icon = availableIcons.ElementAt(iconToAddIdx);
            Items.Add(new Item()
            {
                Icon = icon,
                Command = new Command(() => Console.WriteLine(Path.GetFileName(icon)))
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
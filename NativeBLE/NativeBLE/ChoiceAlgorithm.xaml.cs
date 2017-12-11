using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using NativeBLE.Core;

namespace NativeBLE.Core.Forms
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ChoiceAlgorithm : ContentPage
    {
        ConnectionAlgorithmViewModel algVM;

        public ChoiceAlgorithm(ConnectionAlgorithmViewModel vm)
        {
            InitializeComponent();

            this.algVM = vm;
            BindingContext = vm;

            // Add all types of exercises
            foreach (var algorithmType in ConnectionAlgorithmType.GetAllValues())
                algorithmTypePicker.Items.Add(algorithmType.ToString());

            algorithmTypePicker.SelectedIndexChanged += (sender, e) =>
                algVM.IsAlgorithmSelected = true;
        }

        private async void OnSelectionSaved(object sender, EventArgs e)
        {
            var algorithmType = ConnectionAlgorithmType.GetAllValues()[algorithmTypePicker.SelectedIndex];
            var connectionAlgorithm = new ConnectionAlgorithm();
            connectionAlgorithm.Algorithm = algorithmType;

            algVM.SaveAlgoritmSelection(connectionAlgorithm);

            await Navigation.PopAsync();
        }
    }
}
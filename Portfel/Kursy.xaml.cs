using Microsoft.Maui.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Portfel;

public partial class Kursy : ContentPage
{
    private readonly KursyWalut _kursyWalut;

    public Kursy()
    {
        InitializeComponent();
        _kursyWalut = new KursyWalut();
        LoadRates();
    }
    private async void LoadRates()
    {
        try
        {
            var ratesData = await _kursyWalut.GetLatestRatesAsync();
            var rates = ratesData["rates"].ToObject<Dictionary<string, double>>();

            var currenciesToDisplay = new List<string>
                {
                    "USD",
                    "EUR",
                    "GBP",
                    "JPY",
                    "CNY",
                    "CHF",
                    "DKK",
                    "NOK",
                    "SEK",
                    "PLN",
                    "AUD",
                    "VND",
                    "KRW"
                };

            var displayRates = rates
                .Where(rate => currenciesToDisplay.Contains(rate.Key))
                .Select(rate => $"{rate.Key}: {rate.Value:F2}")
                .ToList();

            RatesValueLabel.Text = string.Join("\n", displayRates);
        }
        catch (Exception ex)
        {
            RatesLabel.Text = $"Error loading rates: {ex.Message}";
        }
    }
    private async void WrocButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}


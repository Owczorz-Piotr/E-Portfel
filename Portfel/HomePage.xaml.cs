using Microsoft.Data.SqlClient;

namespace Portfel;

public partial class HomePage : ContentPage
{
    private DatabaseService _databaseService;
    int IdUser;
    public HomePage(int IdUzytkownik)
    {
        IdUser = IdUzytkownik;
        InitializeComponent();
        _databaseService = new DatabaseService();
        ZaladujZBazy();

    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ZaladujZBazy();
    }
    private async void ZaladujZBazy()
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_databaseService.connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT Imie FROM Klienci WHERE NrKlienta = @id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", IdUser);
                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                        Witaj.Text = "Witaj " + result?.ToString() ?? "Brak danych";
                    else Navigation.PopAsync();
                }

                string query2 = "SELECT StanKonta FROM Klienci WHERE NrKlienta = @id";

                using (SqlCommand command = new SqlCommand(query2, connection))
                {
                    command.Parameters.AddWithValue("@id", IdUser);
                    object result = await command.ExecuteScalarAsync();
                    if (result != null && decimal.TryParse(result.ToString(), out decimal saldo))
                    {
                        Saldo.Text = saldo.ToString("F2")+" z³";
                    }
                    else
                    {
                        Saldo.Text = "B³êdne dane";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"B³¹d: {ex.Message}", "OK");
        }
    }
    private async void PrzelewButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Przelew(IdUser));
        ZaladujZBazy();
    }
    private async void DodajSrodkiButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new DodajSaldo(IdUser));
        ZaladujZBazy();
    }
    private async void WyplacSrodkiButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Wyp³ata(IdUser));
        ZaladujZBazy();
    }
    private async void HistoriaTransakcjiButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Historia(IdUser));
        ZaladujZBazy();
    }
    private async void KursyButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new Kursy());
        ZaladujZBazy();
    }

    private async void DaneButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushModalAsync(new DaneKonta(IdUser));
        ZaladujZBazy();
    }

    private async void ExitButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
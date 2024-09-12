using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Portfel;

public partial class Wypłata : ContentPage
{
    private DatabaseService _databaseService;
    int IdUser;
    public Wypłata(int IdUzytkownik)
	{
        IdUser = IdUzytkownik;
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    private async void WyplacButtonClicked(object sender, EventArgs e)
    {
        if (decimal.TryParse(Kwota.Text, out decimal kwota))
        {
            if (Regex.IsMatch(Kwota.Text, @"^\d+(\,\d{1,2})?$"))
            {
                if (kwota <= 0)
                    await DisplayAlert("Błąd", "Kwota musi być większa od 0.", "OK");
                else
                    await WyplacPieniadze(kwota);
            }
            else
            {
                await DisplayAlert("Błąd", "Kwota może mieć maksymalnie 2 miejsca po przecinku.", "OK");
            }
        }
        else
        {
            await DisplayAlert("Błąd", "Niepoprawna kwota. Proszę wprowadzić prawidłową liczbę.", "OK");
        }
    }
    private async void WrocButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    public async Task<bool> WyplacPieniadze(decimal kwota)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_databaseService.connectionString))
            {
                await connection.OpenAsync();

                using (SqlTransaction sqlTransaction = connection.BeginTransaction())
                {
                    try
                    {
                        string ZmienSaldo1Query = "UPDATE Klienci SET StanKonta = StanKonta - @kwota WHERE NrKlienta = @idKlient1";
                        using (SqlCommand ZmienSaldo1Command = new SqlCommand(ZmienSaldo1Query, connection, sqlTransaction))
                        {
                            ZmienSaldo1Command.Parameters.AddWithValue("@kwota", kwota);
                            ZmienSaldo1Command.Parameters.AddWithValue("@idKlient1", IdUser);
                            await ZmienSaldo1Command.ExecuteNonQueryAsync();
                        }

                        string DodajTransakcje1Query = "INSERT INTO Transakcjie (NrKlienta, Zmiana, Opis, Data) VALUES (@idKlient1, @zmiana, @opis, @data)";
                        using (SqlCommand DodajTransakcje1Command = new SqlCommand(DodajTransakcje1Query, connection, sqlTransaction))
                        {
                            DodajTransakcje1Command.Parameters.AddWithValue("@idKlient1", IdUser);
                            DodajTransakcje1Command.Parameters.AddWithValue("@zmiana", -kwota);
                            DodajTransakcje1Command.Parameters.AddWithValue("@data", DateTime.Now);
                            DodajTransakcje1Command.Parameters.AddWithValue("@opis", "Wypłata środków");
                            await DodajTransakcje1Command.ExecuteNonQueryAsync();
                        }
                        sqlTransaction.Commit();
                        await DisplayAlert("Gotowe", "Wypłata została wykonana pomyślnie", "OK");
                        await Navigation.PopModalAsync();
                        return true;
                    }
                    catch (Exception)
                    {
                        sqlTransaction.Rollback();
                        throw;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Błąd", $"Błąd: {ex.Message}", "OK");
            return false;
        }
    }
}
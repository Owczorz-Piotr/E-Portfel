using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Portfel;

public partial class DodajSaldo : ContentPage
{
    private DatabaseService _databaseService;
    int IdUser;
    public DodajSaldo(int IdUzytkownik)
    {
        IdUser = IdUzytkownik;
        InitializeComponent();
        _databaseService = new DatabaseService();
    }
    private async void DodajButtonClicked(object sender, EventArgs e)
    {
        if (decimal.TryParse(Kwota.Text, out decimal kwota))
        {
            if (Regex.IsMatch(Kwota.Text, @"^\d+(\,\d{1,2})?$"))
            {
                    await DodajPieniadze(kwota);
            }
            else
            {
                await DisplayAlert("B³¹d", "Kwota mo¿e mieæ maksymalnie 2 miejsca po przecinku.", "OK");
            }
        }
        else
        {
            await DisplayAlert("B³¹d", "Niepoprawna kwota. Proszê wprowadziæ prawid³ow¹ liczbê.", "OK");
        }
    }
    private async void WrocButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    public async Task<bool> DodajPieniadze(decimal kwota)
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
                        string ZmienSaldo1Query = "UPDATE Klienci SET StanKonta = StanKonta + @kwota WHERE NrKlienta = @idKlient1";
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
                            DodajTransakcje1Command.Parameters.AddWithValue("@zmiana", kwota);
                            DodajTransakcje1Command.Parameters.AddWithValue("@data", DateTime.Now);
                            DodajTransakcje1Command.Parameters.AddWithValue("@opis", "Wp³ata œrodków");
                            await DodajTransakcje1Command.ExecuteNonQueryAsync();
                        }
                        sqlTransaction.Commit();
                        await DisplayAlert("Gotowe", "Wp³ata zosta³a wykonana pomyœlnie", "OK");
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
            await DisplayAlert("B³¹d", $"B³¹d: {ex.Message}", "OK");
            return false;
        }
    }
}
                
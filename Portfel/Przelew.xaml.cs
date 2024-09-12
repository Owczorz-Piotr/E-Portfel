using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Server;
using System.Text.RegularExpressions;
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Portfel;

public partial class Przelew : ContentPage
{
    private DatabaseService _databaseService;
    int IdUser;

    public Przelew(int IdUzytkownik)
    {
        IdUser = IdUzytkownik;
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

private async void PrzelejButtonClicked(object sender, EventArgs e)
    {
        if (int.TryParse(NrOdbiorcy.Text, out int NrOdbior))
        {
            if (decimal.TryParse(Kwota.Text, out decimal kwota))
            {
                if (Regex.IsMatch(Kwota.Text, @"^\d+(\,\d{1,2})?$"))
                {
                    if (NrOdbior == IdUser)
                        await DisplayAlert("B³¹d", "Nie mo¿na przesy³aæ pieniêdzy samemu sobie", "OK");
                    else if (kwota <= 0)
                        await DisplayAlert("B³¹d", "Kwota musi byæ wiêksza od 0.", "OK");
                    else
                        await PrzeslijPieniadze(NrOdbior, kwota, Nazwa.Text);
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
        else
        {
            await DisplayAlert("B³¹d", "Niepoprawny numer odbiorcy. Proszê wprowadziæ prawid³owy numer.", "OK");
        }
    }

    private async void WrocButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    public async Task<bool> PrzeslijPieniadze(int IdOdbiorca, decimal kwota, string nazwaTransakcji)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_databaseService.connectionString))
            {
                await connection.OpenAsync();

                string SprawdzienieSaldaQuery = "SELECT StanKonta FROM Klienci WHERE NrKlienta = @idKlient1";
                decimal saldoKlienta = 0;

                using (SqlCommand SprawdzienieSaldaCommand = new SqlCommand(SprawdzienieSaldaQuery, connection))
                {
                    SprawdzienieSaldaCommand.Parameters.AddWithValue("@idKlient1", IdUser);

                    object result = await SprawdzienieSaldaCommand.ExecuteScalarAsync();
                    if (result != null && decimal.TryParse(result.ToString(), out saldoKlienta))
                    {
                        if (saldoKlienta < kwota)
                        {
                            await DisplayAlert("B³¹d", "Nie masz wystarczaj¹cych œrodków na koncie.", "OK");
                            return false;
                        }
                    }
                    else
                    {
                        await DisplayAlert("B³¹d", "B³¹d podczas sprawdzania salda.", "OK");
                        return false;
                    }
                }

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

                        string ZmienSaldo2Query = "UPDATE Klienci SET StanKonta = StanKonta + @kwota WHERE NrKlienta = @idKlient2";
                        using (SqlCommand ZmienSaldo2Command = new SqlCommand(ZmienSaldo2Query, connection, sqlTransaction))
                        {
                            ZmienSaldo2Command.Parameters.AddWithValue("@kwota", kwota);
                            ZmienSaldo2Command.Parameters.AddWithValue("@idKlient2", IdOdbiorca);
                            await ZmienSaldo2Command.ExecuteNonQueryAsync();
                        }

                        string DodajTransakcje1Query = "INSERT INTO Transakcjie (NrKlienta, Zmiana, Opis, Data) VALUES (@idKlient1, @zmiana, @nazwa, @data)";
                        using (SqlCommand DodajTransakcje1Command = new SqlCommand(DodajTransakcje1Query, connection, sqlTransaction))
                        {
                            DodajTransakcje1Command.Parameters.AddWithValue("@idKlient1", IdUser);
                            DodajTransakcje1Command.Parameters.AddWithValue("@zmiana", -kwota);
                            DodajTransakcje1Command.Parameters.AddWithValue("@nazwa", nazwaTransakcji);
                            DodajTransakcje1Command.Parameters.AddWithValue("@data", DateTime.Now);
                            await DodajTransakcje1Command.ExecuteNonQueryAsync();
                        }

                        string DodajTransakcje2Query = "INSERT INTO Transakcjie (NrKlienta, Zmiana, Opis, Data) VALUES (@idKlient2, @zmiana, @nazwa, @data)";
                        using (SqlCommand DodajTransakcje2Command = new SqlCommand(DodajTransakcje2Query, connection, sqlTransaction))
                        {
                            DodajTransakcje2Command.Parameters.AddWithValue("@idKlient2", IdOdbiorca);
                            DodajTransakcje2Command.Parameters.AddWithValue("@zmiana", kwota);
                            DodajTransakcje2Command.Parameters.AddWithValue("@nazwa", nazwaTransakcji);
                            DodajTransakcje2Command.Parameters.AddWithValue("@data", DateTime.Now);
                            await DodajTransakcje2Command.ExecuteNonQueryAsync();
                        }

                        sqlTransaction.Commit();
                        await DisplayAlert("Gotowe", "Przelew zosta³ wykonany pomyœlnie", "OK");
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

using Microsoft.Data.SqlClient;

namespace Portfel;

public partial class StworzKonto : ContentPage
{
    private DatabaseService _databaseService;

    public StworzKonto()
    {
        InitializeComponent();
        _databaseService = new DatabaseService();
    }

    private async void StworzButtonClicked(object sender, EventArgs e)
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_databaseService.connectionString))
            {
                await connection.OpenAsync();

                int telLength = Tel.Text?.Length ?? 0;

                if ((string.IsNullOrEmpty(Tel.Text) || int.TryParse(Tel.Text, out _) && telLength == 9) && Haslo.Text == Haslo2.Text)
                {
                    string query = "INSERT INTO Klienci (Imie, Nazwisko, Haslo, Email, Telefon, StanKonta) " +
                                   "VALUES (@imie, @nazwisko, @haslo, @email, @telefon, 0)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@imie", Imie.Text);
                        command.Parameters.AddWithValue("@nazwisko", Nazwisko.Text);
                        command.Parameters.AddWithValue("@haslo", Haslo.Text);

                        if (string.IsNullOrEmpty(Email.Text))
                        {
                            command.Parameters.AddWithValue("@email", DBNull.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@email", Email.Text);
                        }

                        if (string.IsNullOrEmpty(Tel.Text))
                        {
                            command.Parameters.AddWithValue("@telefon", DBNull.Value);
                        }
                        else
                        {
                            command.Parameters.AddWithValue("@telefon", Tel.Text);
                        }

                        int rowsAffected = await command.ExecuteNonQueryAsync();

                        if (rowsAffected > 0)
                        {
                            string query2 = "SELECT TOP 1 NrKlienta FROM Klienci ORDER BY NrKlienta DESC";

                            using (SqlCommand command2 = new SqlCommand(query2, connection))
                            {
                                object result = await command2.ExecuteScalarAsync();

                                await DisplayAlert("Konto zosta�o stworzone", $"Konto zosta�o stworzone. Tw�j numer klienta to: {result}", "OK");
                                await Navigation.PopModalAsync();
                            }
                        }
                    }
                }
                else if (Haslo.Text != Haslo2.Text)
                {
                    await DisplayAlert("B��d", "Has�a musz� by� takie same.", "OK");
                }
                else
                {
                    await DisplayAlert("B��d", "Numer telefonu musi sk�ada� si� z 9 cyfr, je�li jest wprowadzany.", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B��d", $"B��d: {ex.Message}", "OK");
        }
    }

    private async void WrocButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
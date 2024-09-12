using Microsoft.Data.SqlClient;

namespace Portfel;

public partial class DaneKonta : ContentPage
{
    private DatabaseService _databaseService;
    int IdUser;
    bool error;
    bool isEditing = false;
    public DaneKonta(int IdUzytkownik)
    {
        IdUser = IdUzytkownik;
        InitializeComponent();
        _databaseService = new DatabaseService();
        ZaladujZBazy();
    }

    private async Task ZaladujZBazy()
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
                    {
                        Imie.Text = result.ToString();
                    }
                    else
                    {
                        error = true;
                    }
                }

                string query2 = "SELECT Nazwisko FROM Klienci WHERE NrKlienta = @id";

                using (SqlCommand command = new SqlCommand(query2, connection))
                {
                    command.Parameters.AddWithValue("@id", IdUser);
                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        Nazwisko.Text = result.ToString();
                    }
                    else
                    {
                        error = true;
                    }
                }

                string query3 = "SELECT Email FROM Klienci WHERE NrKlienta = @id";

                using (SqlCommand command = new SqlCommand(query3, connection))
                {
                    command.Parameters.AddWithValue("@id", IdUser);
                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        Email.Text = result.ToString();
                    }
                    else
                    {
                        Email.Text = string.Empty;
                    }
                }

                string query4 = "SELECT Telefon FROM Klienci WHERE NrKlienta = @id";

                using (SqlCommand command = new SqlCommand(query4, connection))
                {
                    command.Parameters.AddWithValue("@id", IdUser);
                    object result = await command.ExecuteScalarAsync();
                    if (result != null)
                    {
                        Tel.Text = result.ToString();
                    }
                    else
                    {
                        Tel.Text = string.Empty;
                    }
                }
                if (error)
                {
                    await DisplayAlert("B³¹d", "Nie uda³o siê za³adowaæ danych", "OK");
                }

            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"B³¹d podczas ³adowania danych: {ex.Message}");
        }
    }


    private async Task UsunZBazy()
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_databaseService.connectionString))
            {
                await connection.OpenAsync();
                string query = "DELETE FROM Transakcjie WHERE NrKlienta = @id";
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", IdUser);
                    int rowsAffected = await command.ExecuteNonQueryAsync();
                }

                string query2 = "Delete FROM Klienci WHERE NrKlienta = @id";

                using (SqlCommand command = new SqlCommand(query2, connection))
                {
                    command.Parameters.AddWithValue("@id", IdUser);
                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected == 1)
                    {
                        await DisplayAlert("Gotowe", "Uda³o siê usun¹æ konto", "OK");
                    }
                    else
                    {
                        await DisplayAlert("B³¹d", "Nie uda³o siê usun¹æ konta", "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"B³¹d podczas usuwania wiersza: {ex.Message}", "OK");
        }
    }

    private async void WrocButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }

    private async void UsunButtonClicked(object sender, EventArgs e)
    {
        await UsunZBazy();
        await Navigation.PopModalAsync();
    }

    private async Task RemoveHomePageAsync()
    {
        var navigationStack = Navigation.NavigationStack;
        if (navigationStack.Count > 1)
        {
            var homePage = navigationStack[navigationStack.Count - 2];

            await Task.Run(() => Navigation.RemovePage(homePage));
        }
    }

    private async void EditButtonClicked(object sender, EventArgs e)
    {
        if (!isEditing)
        {
            Imie.IsEnabled = true;
            Nazwisko.IsEnabled = true;
            Tel.IsEnabled = true;
            Email.IsEnabled = true;

            EditButton.Text = "Zapisz zmiany";
            EditButton.BackgroundColor = Colors.Green;
            isEditing = true;
        }
        else
        {
            await ZapiszZmiany();

            Imie.IsEnabled = false;
            Nazwisko.IsEnabled = false;
            Tel.IsEnabled = false;
            Email.IsEnabled = false;

            EditButton.Text = "Edytuj Dane";
            EditButton.BackgroundColor = Colors.Blue;
            isEditing = false;
        }

    }

    private async Task ZapiszZmiany()
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_databaseService.connectionString))
            {
                await connection.OpenAsync();

                string query = "UPDATE Klienci SET Imie = @imie, Nazwisko = @nazwisko, Telefon = @telefon, Email = @email WHERE NrKlienta = @id";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@imie", Imie.Text);
                    command.Parameters.AddWithValue("@nazwisko", Nazwisko.Text);
                    command.Parameters.AddWithValue("@telefon", Tel.Text);
                    command.Parameters.AddWithValue("@email", Email.Text);
                    command.Parameters.AddWithValue("@id", IdUser);

                    int rowsAffected = await command.ExecuteNonQueryAsync();

                    if (rowsAffected > 0)
                    {
                        await DisplayAlert("Gotowe", "Zmiany zosta³y zapisane.", "OK");
                    }
                    else
                    {
                        await DisplayAlert("B³¹d", "Nie uda³o siê zapisaæ zmian.", "OK");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"B³¹d podczas zapisywania zmian: {ex.Message}");
        }
    }
}
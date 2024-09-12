using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;

namespace Portfel;

public partial class Historia : ContentPage
{
    private DatabaseService _databaseService;
    int IdUser;
    public ObservableCollection<Transakcja> Transakcje { get; set; }

    public Historia(int IdUzytkownik)
    {
        IdUser = IdUzytkownik;
        InitializeComponent();
        _databaseService = new DatabaseService();
        Transakcje = new ObservableCollection<Transakcja>();
        TransakcjeCollectionView.ItemsSource = Transakcje;
        ZaladujHistorieTransakcji();
    }

    private async void ZaladujHistorieTransakcji()
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(_databaseService.connectionString))
            {
                await connection.OpenAsync();

                string query = "SELECT IdTransakcji, Data, Opis, Zmiana FROM Transakcjie WHERE NrKlienta = @id ORDER BY IdTransakcji DESC";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", IdUser);

                    using (SqlDataReader reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Transakcje.Add(new Transakcja
                            {
                                IdTransakcji = reader.GetInt32(0),
                                Data = reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                                Nazwa = reader.GetString(2),
                                Zmiana = reader.GetDecimal(3)
                            });
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("B³¹d", $"B³¹d: {ex.Message}", "OK");
        }
    }
    private async void WrocButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
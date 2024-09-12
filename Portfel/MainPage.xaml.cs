using Microsoft.Data.SqlClient;
using System.Net.Sockets;

namespace Portfel
{
    public partial class MainPage : ContentPage
    {
        private DatabaseService _databaseService;

        public MainPage()
        {
            InitializeComponent();
            _databaseService = new DatabaseService();
        }

        public async void Logowanie(int id, string haslo)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(_databaseService.connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT Haslo FROM Klienci WHERE NrKlienta = @Id";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Id", id);

                        object result = await command.ExecuteScalarAsync();


                        if (result != null && result.ToString() == haslo)
                        {

                            await Navigation.PushAsync(new HomePage(id));
                        }
                        else
                        {
                            await DisplayAlert("Błąd", "Błędne dane logowania", "OK");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Błąd", $"Błąd: {ex.Message}", "OK");
            }
        }

        private async void ZalogujButtonClicked(object sender, EventArgs e)
        {
            if (int.TryParse(Id.Text, out int id))
            {
                Logowanie(id, Haslo.Text);
            }
            else
            {
                await DisplayAlert("Błąd", "Błędne dane logowania.", "OK");
            }
        }

        private async void StworzKontoButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PushModalAsync(new StworzKonto());
        }
        protected override void OnAppearing()
        {
            base.OnAppearing();
            Id.Text = string.Empty;
            Haslo.Text = string.Empty;
        }
    }
}
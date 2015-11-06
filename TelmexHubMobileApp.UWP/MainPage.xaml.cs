using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.Security.Credentials;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Microsoft.WindowsAzure.MobileServices;
using TelmexHubMobileApp.UWP.DataModel;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace TelmexHubMobileApp.UWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    /// 

    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {

        private MobileServiceCollection<Person, Person> persons;

        private readonly IMobileServiceTable<Person> personsTable = App.MobileServiceClient.GetTable<Person>();

        private ObservableCollection<Person> personsCollection;

        private MobileServiceUser user;

        public ObservableCollection<Person> PersonsCollection
        {
            get { return personsCollection ?? (personsCollection = new ObservableCollection<Person>()); }
            set
            {
                personsCollection = value;
                OnPropertyChanged();
            }
        }

        private Person currentPerson;

        public Person CurrentPerson
        {
            get { return currentPerson ?? (currentPerson = new Person()); }
            set
            {
                currentPerson = value;
                OnPropertyChanged();
            }
        }

        public MainPage()
        {
            this.DataContext = this;
            this.InitializeComponent();

        }


        private async Task InsertPerson(Person person)
        {
            await personsTable.InsertAsync(person);
            persons.Add(person);
        }


        private async Task RefreshPersons()
        {
            MobileServiceInvalidOperationException exception = null;
            try
            {

                persons = await personsTable.ToCollectionAsync();

            }
            catch (MobileServiceInvalidOperationException e)
            {
                exception = e;
            }
            if (exception != null)
            {
                await new MessageDialog(exception.Message, "Error loading").ShowAsync();
            }
            else
            {
                PersonsCollection = new ObservableCollection<Person>(persons);
            }
        }

        private async Task UpdatePersons(Person person)
        {
            await personsTable.UpdateAsync(person);
            persons.Remove(person);
            ListItems.Focus(FocusState.Unfocused);
        }


        private async void ButtonRefresh_Click(object sender, RoutedEventArgs e)
        {
            ButtonRefresh.IsEnabled = false;
            await RefreshPersons();
            ButtonRefresh.IsEnabled = true;
        }

        private async void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(CurrentPerson.Id))
            {
                await InsertPerson(currentPerson);
            }
            else
            {
                await UpdatePersons(CurrentPerson);
            }
            CurrentPerson = null;
            await RefreshPersons();
        }

        private async Task AuthenticateAsync()
        {
            while (user == null)
            {
                string message=string.Empty;

                var provider = "AAD";

                PasswordVault vault=new PasswordVault();
                PasswordCredential credential = null;

                try
                {
                    credential = vault.FindAllByResource(provider).FirstOrDefault();
                }
                catch (Exception)
                {
                    //Iganoramos la excepción
                }
                if (credential != null)
                {
                    // Se crea un usuario a partir de las credenciales
                    user = new MobileServiceUser(credential.UserName);
                    credential.RetrievePassword();
                    user.MobileServiceAuthenticationToken = credential.Password;

                    // Se asigna el usuario a las credenciales almacenadas
                    App.MobileServiceClient.CurrentUser = user;

                    try
                    {
                        //intentamos obtener un elemento para determinar si nuestro cache ha experidado
                        await App.MobileServiceClient.GetTable<Person>().Take(1).ToListAsync();
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        if (ex.Response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        {
                            //Se remueven las credenciales con el token que ha expirado
                            vault.Remove(credential);
                            credential = null;
                            continue;
                        }
                    }
                }
                else
                {
                    try
                    {
                        //Hacemos un login con el provedor
                        user = await App.MobileServiceClient
                            .LoginAsync(provider);
                        
                        //Creamos y almacenamos las credenciales de usuario
                        credential = new PasswordCredential(provider,
                            user.UserId, user.MobileServiceAuthenticationToken);
                        vault.Add(credential);
                    }
                    catch (MobileServiceInvalidOperationException ex)
                    {
                        message = "You must log in. Login Required";
                    }
                }
                message = string.Format("You are now logged in - {0}", user.UserId);
                var dialog = new MessageDialog(message);
                dialog.Commands.Add(new UICommand("OK"));
                await dialog.ShowAsync();

            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void ButtonLogin_OnClick(object sender, RoutedEventArgs e)
        {
            await AuthenticateAsync();

            ButtonLogin.Visibility=Visibility.Collapsed;

            await RefreshPersons();

        }
    }
}

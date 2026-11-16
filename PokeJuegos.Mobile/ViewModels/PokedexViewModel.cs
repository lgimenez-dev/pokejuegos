using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PokeJuegos.Mobile.Models;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

/// <summary>
/// Pokédex catalog: search, all/favorites filter and the detail modal.
/// Port of <c>usePokemon</c>, <c>usePokemonFilter</c> and <c>usePokemonSelection</c>.
/// </summary>
public partial class PokedexViewModel(
    IPokeApiService pokeApi,
    IFavoritesService favorites,
    INavigationService navigation,
    ILogger<PokedexViewModel> logger) : ObservableObject
{
    private const string NoResultsMessage = "You look lost on your journey!";
    private const string DataErrorMessage = "Failed to load data.";

    private IReadOnlyList<PokemonItem> allPokemon = [];

    /// <summary>The list after applying the search and the all/favorites filter.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage), nameof(HasError), nameof(IsListVisible))]
    public partial IReadOnlyList<PokemonItem> VisiblePokemon { get; set; } = [];

    [ObservableProperty]
    public partial string SearchText { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsAllSelected))]
    public partial bool ShowFavoritesOnly { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsListVisible))]
    public partial bool IsLoading { get; set; }

    /// <summary>Set when the list or a Pokémon's detail can't be read.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(ErrorMessage), nameof(HasError), nameof(IsListVisible))]
    public partial string DataError { get; set; } = string.Empty;

    /// <summary>Row shown in the detail modal (name, number, sprite, favorite). Kept after closing so the modal doesn't blank out while it fades.</summary>
    [ObservableProperty]
    public partial PokemonItem? SelectedPokemon { get; set; }

    /// <summary>Weight, height and types of <see cref="SelectedPokemon"/>, looked up when it was tapped.</summary>
    [ObservableProperty]
    public partial PokemonDetail? SelectedDetail { get; set; }

    [ObservableProperty]
    public partial bool IsDetailOpen { get; set; }

    /// <summary>The search field is collapsed behind the magnifier icon until the user opens it.</summary>
    [ObservableProperty]
    public partial bool IsSearchOpen { get; set; }

    public bool IsAllSelected => !ShowFavoritesOnly;

    /// <summary>A data failure, or the "no results" message when a search matches nothing.</summary>
    public string ErrorMessage =>
        DataError.Length > 0 ? DataError
        : SearchText.Trim().Length > 0 && VisiblePokemon.Count == 0 ? NoResultsMessage
        : string.Empty;

    public bool HasError => ErrorMessage.Length > 0;

    public bool IsListVisible => !IsLoading && !HasError;

    partial void OnSearchTextChanged(string value) => ApplyFilter();

    partial void OnShowFavoritesOnlyChanged(bool value) => ApplyFilter();

    /// <summary>
    /// Loads the list and the saved favorites. Does nothing once loaded, so it's safe on every appearance.
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (allPokemon.Count > 0)
        {
            return;
        }

        IsLoading = true;
        DataError = string.Empty;
        try
        {
            IReadOnlyList<PokemonSummary> list = await pokeApi.GetListAsync();
            IReadOnlySet<int> favoriteIds = await favorites.GetFavoriteIdsAsync();
            allPokemon = list.Select(summary => PokemonItem.From(summary, favoriteIds.Contains)).ToList();
            ApplyFilter();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load the Pokédex list");
            DataError = DataErrorMessage;
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Switching tabs clears the search, like the original toggleSelector.
    [RelayCommand]
    private void ShowAll()
    {
        SearchText = string.Empty;
        ShowFavoritesOnly = false;
    }

    [RelayCommand]
    private void ShowFavorites()
    {
        SearchText = string.Empty;
        ShowFavoritesOnly = true;
    }

    /// <summary>"Go back home" on the error: clears the search and the error, and reloads the list if it never loaded.</summary>
    [RelayCommand]
    private Task RecoverAsync()
    {
        SearchText = string.Empty;
        DataError = string.Empty;
        return allPokemon.Count == 0 ? LoadAsync() : Task.CompletedTask;
    }

    /// <summary>
    /// Looks up the tapped Pokémon's detail and opens the modal. While it runs the command can't
    /// execute again, so a double tap doesn't open it twice.
    /// </summary>
    [RelayCommand]
    private async Task SelectPokemonAsync(PokemonItem pokemon)
    {
        try
        {
            Pokemon detail = await pokeApi.GetByNameAsync(pokemon.Name);
            SelectedPokemon = pokemon;
            SelectedDetail = PokemonDetail.From(detail);
            IsDetailOpen = true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load the detail of {Pokemon}", pokemon.Name);
            DataError = DataErrorMessage;
        }
    }

    [RelayCommand]
    private void CloseDetail() => IsDetailOpen = false;

    [RelayCommand]
    private void OpenSearch() => IsSearchOpen = true;

    /// <summary>Hides the search field and clears it, so the list isn't left filtered by text nobody can see.</summary>
    [RelayCommand]
    private void CloseSearch()
    {
        SearchText = string.Empty;
        IsSearchOpen = false;
    }

    [RelayCommand]
    private async Task ToggleFavoriteAsync(PokemonItem pokemon)
    {
        pokemon.IsFavorite = await favorites.ToggleAsync(pokemon.Id);
        if (ShowFavoritesOnly && !pokemon.IsFavorite)
        {
            ApplyFilter();
        }
    }

    /// <summary>Opens the system share sheet ("Share to my friends" copied to the clipboard on the web).</summary>
    [RelayCommand]
    private Task ShareAsync()
    {
        if (SelectedPokemon is not { } pokemon || SelectedDetail is not { } detail)
        {
            return Task.CompletedTask;
        }

        return Share.Default.RequestAsync(new ShareTextRequest
        {
            // Same text the web version copied to the clipboard.
            Title = pokemon.Title,
            Text = $"name: {pokemon.Name}, weight: {detail.Weight}, height: {detail.Height}, types: {detail.Types.Replace(",", string.Empty)}",
        });
    }

    [RelayCommand]
    private Task GoBackAsync() => navigation.GoBackAsync();

    private void ApplyFilter()
    {
        IEnumerable<PokemonItem> result = ShowFavoritesOnly ? allPokemon.Where(p => p.IsFavorite) : allPokemon;

        string query = SearchText.Trim();
        if (query.Length > 0)
        {
            result = result.Where(p => p.Matches(query));
        }

        VisiblePokemon = result.ToList();
    }
}

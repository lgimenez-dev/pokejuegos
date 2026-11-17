using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using PokeJuegos.Mobile.Models;
using PokeJuegos.Mobile.Resources.Strings;
using PokeJuegos.Mobile.Services;

namespace PokeJuegos.Mobile.ViewModels;

/// <summary>Round phase, derived like the <c>state</c> computed in <c>RouletteView.vue</c>.</summary>
public enum RouletteState
{
    /// <summary>Waiting to spin, or spinning.</summary>
    Spin,

    /// <summary>The roulette stopped: pick a name before the fuse burns out.</summary>
    Choose,

    /// <summary>The guess (or the fuse) decided the round.</summary>
    Result,
}

public enum GuessResult
{
    None,
    Correct,
    Incorrect,
}

/// <summary>
/// "Who's That Pokémon?" game: spin, guess before the fuse burns out, top scores.
/// Port of <c>useRoulette.ts</c>, <c>useTopScoresStore.ts</c> and the state of <c>RouletteView.vue</c>.
/// Animations (fuse, shake, pulse) live in the page's code-behind.
/// </summary>
public partial class RouletteViewModel(
    IPokeApiService pokeApi,
    ITopScoresService topScores,
    INavigationService navigation,
    ILogger<RouletteViewModel> logger) : ObservableObject
{
    private const int SpinTickMs = 80;
    private const int MinSpinMs = 3000;
    private const int MaxSpinMs = 5000;
    private const int PointsPerCorrectGuess = 20;
    private const int BaseGuessOptionsCount = 5;
    private const int GameOverModalDelayMs = 3000;

    // Difficulty ramps every N correct guesses: the fuse burns faster and the guess list grows.
    private const int CorrectGuessesPerTier = 5;
    private const int OptionsAddedPerTier = 3;
    private const double FuseSpeedupPerTier = 0.3;
    private const double BaseFuseDurationMs = 8000;

    // Properties computed from the round state, raised together whenever any of their inputs change.
    private static readonly string[] DerivedProperties =
    [
        nameof(State),
        nameof(DisplayScore),
        nameof(HasPokemon),
        nameof(IsSilhouette),
        nameof(IsRevealed),
        nameof(IsSpinButtonVisible),
        nameof(IsChooseButtonVisible),
        nameof(IsResetButtonVisible),
        nameof(IsPickerEnabled),
        nameof(CanChoose),
        nameof(IsResultBadgeVisible),
        nameof(IsCorrect),
        nameof(IsFuseVisible),
        nameof(IsUploadEnabled),
    ];

    private IReadOnlyList<RoulettePokemon> pool = [];

    // Cancels the running spin and fuse (a new spin, a guess, reset or leaving the page).
    private CancellationTokenSource? roundCancellation;

    // Cancels the delayed opening of the top scores after a game over.
    private CancellationTokenSource? gameOverCancellation;

    /// <summary>The sprite on screen: changes every tick while spinning, then stays on the result.</summary>
    [ObservableProperty]
    public partial RoulettePokemon? CurrentPokemon { get; set; }

    /// <summary>Where the roulette stopped; the guess is checked against it.</summary>
    [ObservableProperty]
    public partial RoulettePokemon? ResultPokemon { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsSpinEnabled))]
    public partial bool IsSpinning { get; set; }

    /// <summary>The roulette stopped (<c>finished</c> on the web).</summary>
    [ObservableProperty]
    public partial bool IsFinished { get; set; }

    /// <summary>Option picked in the select; <see langword="null"/> is "Choose a Pokémon".</summary>
    [ObservableProperty]
    public partial RoulettePokemon? SelectedGuess { get; set; }

    [ObservableProperty]
    public partial GuessResult GuessResult { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<RoulettePokemon> GuessOptions { get; set; } = [];

    [ObservableProperty]
    public partial int Score { get; set; }

    [ObservableProperty]
    public partial int CorrectGuesses { get; set; }

    [ObservableProperty]
    public partial bool IsGameOver { get; set; }

    /// <summary>Score when the game ended; shown and uploaded while <see cref="IsGameOver"/>.</summary>
    [ObservableProperty]
    public partial int FinalScore { get; set; }

    [ObservableProperty]
    public partial bool IsTopScoresOpen { get; set; }

    [ObservableProperty]
    public partial bool IsLeaveConfirmOpen { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<TopScoreRow> TopScores { get; set; } = [];

    /// <summary>The player's own rank (<c>#12</c>), even outside the visible top; empty if they never uploaded.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasLastScore))]
    public partial string LastScoreRank { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string LastScorePoints { get; set; } = string.Empty;

    public bool HasLastScore => LastScoreRank.Length > 0;

    public RouletteState State =>
        !IsFinished ? RouletteState.Spin
        : GuessResult == GuessResult.None ? RouletteState.Choose
        : RouletteState.Result;

    public int DisplayScore => IsGameOver ? FinalScore : Score;

    public bool HasPokemon => CurrentPokemon is not null;

    /// <summary>The sprite is drawn black until the round is decided.</summary>
    public bool IsSilhouette => HasPokemon && GuessResult == GuessResult.None;

    public bool IsRevealed => HasPokemon && GuessResult != GuessResult.None;

    public bool IsSpinButtonVisible =>
        State == RouletteState.Spin || (State == RouletteState.Result && GuessResult == GuessResult.Correct);

    /// <summary>The Spin button is dimmed while spinning (<c>is-selected="spinning"</c> on the web).</summary>
    public bool IsSpinEnabled => !IsSpinning;

    public bool IsChooseButtonVisible => State == RouletteState.Choose;

    public bool IsResetButtonVisible => !IsSpinButtonVisible && !IsChooseButtonVisible;

    public bool IsPickerEnabled => State == RouletteState.Choose;

    public bool CanChoose => SelectedGuess is not null;

    public bool IsResultBadgeVisible => State == RouletteState.Result;

    public bool IsCorrect => GuessResult == GuessResult.Correct;

    public bool IsFuseVisible => !IsGameOver && State == RouletteState.Choose;

    public bool IsUploadEnabled => DisplayScore > 0;

    /// <summary>How long the fuse burns this round; shorter every difficulty tier.</summary>
    public double FuseDurationMs => BaseFuseDurationMs / Math.Pow(1 + FuseSpeedupPerTier, DifficultyTier);

    private int DifficultyTier => CorrectGuesses / CorrectGuessesPerTier;

    private int GuessOptionsCount => BaseGuessOptionsCount + (DifficultyTier * OptionsAddedPerTier);

    partial void OnCurrentPokemonChanged(RoulettePokemon? value) => RaiseDerivedProperties();

    partial void OnIsFinishedChanged(bool value) => RaiseDerivedProperties();

    partial void OnSelectedGuessChanged(RoulettePokemon? value) => RaiseDerivedProperties();

    partial void OnGuessResultChanged(GuessResult value) => RaiseDerivedProperties();

    partial void OnScoreChanged(int value) => RaiseDerivedProperties();

    partial void OnIsGameOverChanged(bool value) => RaiseDerivedProperties();

    partial void OnFinalScoreChanged(int value) => RaiseDerivedProperties();

    /// <summary>
    /// Loads the roulette pool and the top scores. Does nothing once loaded, so it's safe on every appearance.
    /// </summary>
    [RelayCommand]
    private async Task LoadAsync()
    {
        if (pool.Count > 0)
        {
            return;
        }

        try
        {
            IReadOnlyList<PokemonSummary> list = await pokeApi.GetListAsync();
            pool = list.Select(RoulettePokemon.From).OrderBy(p => p.Name, StringComparer.Ordinal).ToList();
            await LoadTopScoresAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load the roulette data");
        }
    }

    [RelayCommand]
    private void Spin()
    {
        if (IsSpinning || pool.Count == 0)
        {
            return;
        }

        CancellationToken token = RestartRound();
        IsFinished = false;
        ResultPokemon = null;
        SelectedGuess = null;
        GuessResult = GuessResult.None;
        IsSpinning = true;
        _ = RunRoundAsync(token);
    }

    [RelayCommand]
    private void Choose()
    {
        if (ResultPokemon is not { } result || SelectedGuess is not { } guess)
        {
            return;
        }

        // Stops the fuse.
        CancelRound();
        if (guess.Name == result.Name)
        {
            GuessResult = GuessResult.Correct;
            Score += PointsPerCorrectGuess;
            CorrectGuesses += 1;
        }
        else
        {
            GuessResult = GuessResult.Incorrect;
            EndGame();
        }
    }

    [RelayCommand]
    private void Reset()
    {
        Stop();
        CurrentPokemon = null;
        ResultPokemon = null;
        IsSpinning = false;
        IsFinished = false;
        SelectedGuess = null;
        GuessResult = GuessResult.None;
        GuessOptions = [];
        Score = 0;
        CorrectGuesses = 0;
        IsGameOver = false;
    }

    [RelayCommand]
    private async Task UploadAsync()
    {
        try
        {
            await topScores.UploadAsync(DisplayScore);
            await LoadTopScoresAsync();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save the score");
        }
    }

    [RelayCommand]
    private async Task OpenTopScoresAsync()
    {
        IsTopScoresOpen = true;
        await LoadTopScoresAsync();
    }

    [RelayCommand]
    private void CloseTopScores() => IsTopScoresOpen = false;

    [RelayCommand]
    private void RequestLeave() => IsLeaveConfirmOpen = true;

    [RelayCommand]
    private void CancelLeave() => IsLeaveConfirmOpen = false;

    [RelayCommand]
    private Task ConfirmLeaveAsync()
    {
        IsLeaveConfirmOpen = false;
        Stop();
        return navigation.GoBackAsync();
    }

    /// <summary>Cancels the spin, the fuse and the pending top scores popup (the page is going away).</summary>
    public void Stop()
    {
        CancelRound();
        gameOverCancellation?.Cancel();
        gameOverCancellation = null;
    }

    /// <summary>
    /// Spins (a random sprite every tick for 3-5 s), then stops on the result and burns the fuse. The fuse is the
    /// real round timeout, independent of the bar's animation, like <c>startFuseTimer</c> on the web.
    /// </summary>
    private async Task RunRoundAsync(CancellationToken token)
    {
        try
        {
            TimeSpan duration = TimeSpan.FromMilliseconds(MinSpinMs + (Random.Shared.NextDouble() * (MaxSpinMs - MinSpinMs)));
            Stopwatch elapsed = Stopwatch.StartNew();
            while (elapsed.Elapsed < duration)
            {
                CurrentPokemon = pool[Random.Shared.Next(pool.Count)];
                await Task.Delay(SpinTickMs, token);
            }

            IsSpinning = false;
            ResultPokemon = CurrentPokemon;
            GuessOptions = ResultPokemon is { } result ? BuildGuessOptions(result) : [];
            IsFinished = true;

            await Task.Delay(TimeSpan.FromMilliseconds(FuseDurationMs), token);
            if (GuessResult == GuessResult.None)
            {
                GuessResult = GuessResult.Incorrect;
                EndGame();
            }
        }
        catch (OperationCanceledException)
        {
            // A new spin, a guess, reset or leaving the page stopped this round.
        }
    }

    /// <summary>The right answer plus random wrong ones, shuffled.</summary>
    private RoulettePokemon[] BuildGuessOptions(RoulettePokemon correct)
    {
        RoulettePokemon[] wrong = pool.Where(p => p.Id != correct.Id).ToArray();
        Random.Shared.Shuffle(wrong);
        RoulettePokemon[] options = [correct, .. wrong.Take(GuessOptionsCount - 1)];
        Random.Shared.Shuffle(options);
        return options;
    }

    private void EndGame()
    {
        FinalScore = Score;
        CorrectGuesses = 0;
        IsGameOver = true;
        _ = OpenTopScoresAfterDelayAsync();
    }

    private async Task OpenTopScoresAfterDelayAsync()
    {
        gameOverCancellation?.Cancel();
        gameOverCancellation = new CancellationTokenSource();
        try
        {
            await Task.Delay(GameOverModalDelayMs, gameOverCancellation.Token);
            if (IsGameOver)
            {
                await OpenTopScoresAsync();
            }
        }
        catch (OperationCanceledException)
        {
            // Reset or left the page before the delay.
        }
    }

    private async Task LoadTopScoresAsync()
    {
        TopScoresSnapshot snapshot = await topScores.GetAsync();
        TopScores = snapshot.Top
            .Select((entry, index) => new TopScoreRow($"#{index + 1} {entry.Username}", FormatPoints(entry.Points)))
            .ToList();
        LastScoreRank = snapshot.CurrentPlayer is { } player ? $"#{player.Rank}" : string.Empty;
        LastScorePoints = snapshot.CurrentPlayer is { } own ? FormatPoints(own.Points) : string.Empty;
    }

    // toLocaleString() on the web: thousands separator of the device's culture.
    private static string FormatPoints(int points) =>
        string.Format(AppResources.Roulette_Points, points.ToString("N0", CultureInfo.CurrentCulture));

    private CancellationToken RestartRound()
    {
        CancelRound();
        roundCancellation = new CancellationTokenSource();
        return roundCancellation.Token;
    }

    private void CancelRound()
    {
        roundCancellation?.Cancel();
        roundCancellation = null;
    }

    private void RaiseDerivedProperties()
    {
        foreach (string property in DerivedProperties)
        {
            OnPropertyChanged(property);
        }
    }
}

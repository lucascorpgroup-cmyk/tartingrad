using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ZLinq;

[Command("suit")]
sealed class SuitCommand : ICommand
{
    internal static Dictionary<string, Unlockable> SuitUnlockables =>
        Enum.GetValues(typeof(Unlockable))
            .Cast<Unlockable>()
            .Where(u => u.ToString().EndsWith("_SUIT"))
            .ToDictionary(
                suit => suit.ToString().Replace("_SUIT", "").ToLower(),
                suit => suit
            );

    private static bool _spamActive = false;

    public async Task Execute(Arguments args, CancellationToken cancellationToken)
    {
        // Vérifie si args contient au moins 1 argument
        bool hasArg = false;
        try { var _ = args[0]; hasArg = true; } catch { hasArg = false; }

        if (!hasArg)
        {
            // Si spam déjà actif, on l'arrête
            if (_spamActive)
            {
                _spamActive = false;
                Chat.Print("Stopped suit spam.");
                return;
            }

            _spamActive = true;
            Chat.Print("Starting suit spam...");
            await SpamSuits();
            return;
        }

        // Sinon, porter un suit spécifique
        if (args[0] is not string suitName)
        {
            Chat.Print("Usage: /suit <suit>");
            return;
        }

        if (!suitName.FuzzyMatch(SuitUnlockables.Keys, out string key))
        {
            Chat.Print("Suit not found!");
            return;
        }

        var selectedSuit = SuitUnlockables[key];
        EquipSuit(selectedSuit);
    }

    private async Task SpamSuits()
    {
        var suits = SuitUnlockables.Values.ToList();
        int index = 0;

        while (_spamActive)
        {
            var suit = suits[index];
            EquipSuit(suit);

            index = (index + 1) % suits.Count;
            await Task.Delay(50); // 50ms entre chaque changement
        }
    }

    private void EquipSuit(Unlockable suit)
    {
        Helper.BuyUnlockable(suit);

        var suitObj = Helper
            .FindObjects<UnlockableSuit>()
            .FirstOrDefault(s => suit.Is(s.suitID));

        if (suitObj != null && Helper.LocalPlayer != null)
        {
            suitObj.SwitchSuitToThis(Helper.LocalPlayer);
        }

        string suitTitle = string.Join(" ", suit.ToString()
            .Split('_')
            .Select(s => s.ToLower())
            .Select(s => char.ToUpper(s[0]) + s.Substring(1)));

        Chat.Print($"Wearing {suitTitle}!");
    }
}

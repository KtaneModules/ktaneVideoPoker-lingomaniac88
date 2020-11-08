using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VideoPokerScript : MonoBehaviour
{
    private const int JackpotThreshold = 1200;

    private const int SolveStreakTarget = 5;

    static int ModuleIdCounter = 1;
    int ModuleId;

    public KMAudio Audio;
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;

    public KMSelectable[] Raw_ButtonSelectables;
    public Renderer[] Raw_ButtonBoxes;
    public TextMesh[] Raw_ButtonTexts;
    public TextMesh Raw_ButtonSpeedArrows;

    public KMSelectable[] Raw_CardSelectables;
    public GameObject[] Raw_CardBacks;
    public TextMesh[] Raw_CardTexts;
    public GameObject[] Raw_CardFaces;

    public TextMesh[] Raw_UITexts;

    public Material[] Raw_MaterialInfoMaterials;

    public Renderer Raw_PayTableBackground;
    public TextMesh[] Raw_PayTableTexts;

    public Renderer[] Raw_ProgressLightCylinders;
    public Light[] Raw_ProgressLightLights;
    public Material Raw_ProgressLightRed;
    public Material Raw_ProgressLightGreen;
    public Material Raw_ProgressLightOff;

    public KtaneVideoPoker.UI.CardObject[] CardObjects;

    public KtaneVideoPoker.UI.Button GameInfoButton;
    public KtaneVideoPoker.UI.SpeedButton SpeedButton;
    public KtaneVideoPoker.UI.Button BetOneButton;
    public KtaneVideoPoker.UI.Button BetMaxButton;
    public KtaneVideoPoker.UI.Button DealButton;

    public KtaneVideoPoker.UI.OutlinedText BetText;
    public KtaneVideoPoker.UI.OutlinedText CreditsText;
    public KtaneVideoPoker.UI.OutlinedText GameMessageText;
    
    public KtaneVideoPoker.UI.OutlinedText JackpotText;
    public KtaneVideoPoker.UI.OutlinedText VariationText;

    public KtaneVideoPoker.UI.PayTable PayTable;

    public KtaneVideoPoker.UI.MaterialInfo MaterialInfo;

    public KtaneVideoPoker.UI.ProgressLightManager ProgressLightManager;

    // These fields are used for the machine
    private bool IsSolved = false;
    private KtaneVideoPoker.State State = KtaneVideoPoker.State.Idle;
    private KtaneVideoPoker.VariantInfo VariantInfo;

    private bool NextBetOneSetsBetAmountToOne = false;
    private int BetAmount = 1;
    private int Credits = 1000;
    private int Streak = 0;

    private Stack<KtaneVideoPoker.Core.Card> AvailableCards;

    private HashSet<int> AcceptablePlays;

    private Coroutine JackpotCoroutine;
    private int JackpotValue;

    // Unity-specific methods

    void Awake()
    {
        Debug.Log("VideoPokerScript.Awake");
        ModuleId = ModuleIdCounter++;

        // Initialize UI objects
        CardObjects = Enumerable.Range(0, 5).Select(i =>
        {
            var cardObject = new KtaneVideoPoker.UI.CardObject(Raw_CardSelectables[i], Raw_CardBacks[i], Raw_CardTexts[5*i], Raw_CardTexts[5*i + 1], Raw_CardTexts[5*i + 2], Raw_CardTexts[5*i + 3], Raw_CardTexts[5*i + 4], Raw_CardFaces[i]);
            Raw_CardSelectables[i].OnInteract += delegate()
            {
                Raw_CardSelectables[i].AddInteractionPunch(0.5f);
                Audio.PlaySoundAtTransform(State == KtaneVideoPoker.State.ChooseHolds ? "touch" : "badtouch", transform);
                if (State == KtaneVideoPoker.State.ChooseHolds)
                {
                    CardObjects[i].Held = !CardObjects[i].Held;
                }
                return false;
            };
            return cardObject;
        }).ToArray();

        BetText = new KtaneVideoPoker.UI.OutlinedText(Raw_UITexts[0], Raw_UITexts[1]);
        CreditsText = new KtaneVideoPoker.UI.OutlinedText(Raw_UITexts[2], Raw_UITexts[3]);
        GameMessageText = new KtaneVideoPoker.UI.OutlinedText(Raw_UITexts[4], Raw_UITexts[5]);
        JackpotText = new KtaneVideoPoker.UI.OutlinedText(Raw_UITexts[6], Raw_UITexts[7]);
        VariationText = new KtaneVideoPoker.UI.OutlinedText(Raw_UITexts[8], Raw_UITexts[9]);

        MaterialInfo = new KtaneVideoPoker.UI.MaterialInfo(
            Raw_MaterialInfoMaterials[0],
            Raw_MaterialInfoMaterials[1],
            Raw_MaterialInfoMaterials.Skip(2).Take(4).ToArray(),
            Raw_MaterialInfoMaterials.TakeLast(4).ToArray());

        ProgressLightManager = new KtaneVideoPoker.UI.ProgressLightManager(Raw_ProgressLightCylinders, Raw_ProgressLightLights, Raw_ProgressLightRed, Raw_ProgressLightGreen, Raw_ProgressLightOff);
        ProgressLightManager.SetValue(0);

        PayTable = new KtaneVideoPoker.UI.PayTable(Raw_PayTableBackground, Raw_PayTableTexts);

        // TODO: Do we want to weight these?
        VariantInfo = KtaneVideoPoker.VariantInfo.AllVariants.PickRandom();
        VariationText.Text = VariantInfo.ShortName;
        ModuleLog("Selected variant: {0}", VariantInfo.DetailedName);

        JackpotText.Visible = false;

        PayTable.Visible = false;
        PayTable.LoadVariant(VariantInfo);

        GameInfoButton = new KtaneVideoPoker.UI.Button(Raw_ButtonSelectables[0], Raw_ButtonBoxes[0], Raw_ButtonTexts[0]);
        SpeedButton = new KtaneVideoPoker.UI.SpeedButton(Raw_ButtonSelectables[1], Raw_ButtonBoxes[1], Raw_ButtonTexts[1], Raw_ButtonSpeedArrows);
        BetOneButton = new KtaneVideoPoker.UI.Button(Raw_ButtonSelectables[2], Raw_ButtonBoxes[2], Raw_ButtonTexts[2]);
        BetMaxButton = new KtaneVideoPoker.UI.Button(Raw_ButtonSelectables[3], Raw_ButtonBoxes[3], Raw_ButtonTexts[3]);
        DealButton = new KtaneVideoPoker.UI.Button(Raw_ButtonSelectables[4], Raw_ButtonBoxes[4], Raw_ButtonTexts[4]);
    }

    // Use this for initialization
    void Start()
    {
        BombModule.OnActivate += OnActivate;
        ResetMachine();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Our own methods

    public void ModuleLog(string format, params object[] args)
    {
        var prefix = string.Format("[Video Poker #{0}] ", ModuleId);
        Debug.LogFormat(prefix + format, args);
    }

    // This is called when the lights turn on
    private void OnActivate()
    {
        GameInfoButton.OnShortPress = OnPressGameInfo;
        GameInfoButton.OnLongPress = OnLongPress;

        SpeedButton.OnShortPress = OnPressSpeed;
        SpeedButton.OnLongPress = OnLongPress;

        BetOneButton.OnShortPress = OnPressBetOne;
        BetOneButton.OnLongPress = OnLongPress;

        BetMaxButton.OnShortPress = OnPressBetMax;
        BetMaxButton.OnLongPress = OnLongPress;

        DealButton.OnShortPress = OnPressDeal;
        DealButton.OnLongPress = OnLongPress;
    }

    private IEnumerator BeginDeal()
    {
        // TODO: Disable buttons

        Credits -= BetAmount;
        CreditsText.Text = String.Format("CREDIT {0}", Credits);
        BetText.Text = String.Format("BET {0}", BetAmount);
        GameMessageText.Text = "GOOD LUCK";
        State = KtaneVideoPoker.State.FirstDeal;
        
        for (int i = 0; i < 5; i++)
        {
            CardObjects[i].Card = null;
            CardObjects[i].Held = false;
        }

        GameInfoButton.Disable();
        SpeedButton.Disable();
        BetOneButton.Disable();
        BetMaxButton.Disable();
        DealButton.Disable();

        yield return new WaitForSeconds(2 * SpeedButton.GetDelay());

        int deckSize = KtaneVideoPoker.Core.Util.StandardDeckSize + VariantInfo.VariantIfUsingMaxBet(BetAmount == 5).JokerCount;
        AvailableCards = new Stack<KtaneVideoPoker.Core.Card>(Enumerable.Range(0, deckSize).Select(KtaneVideoPoker.Core.Card.CreateWithId).ToList().Shuffle());

        for (int i = 0; i < 5; i++)
        {
            Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TypewriterKey, transform);
            CardObjects[i].Card = AvailableCards.Pop();
            yield return new WaitForSeconds(SpeedButton.GetDelay());
        }

        // Log cards
        if (!IsSolved)
        {
            ModuleLog("Dealt cards: {0}", CardObjects.Select(co => co.RankText.text + co.SuitText.text).Join(" "));
        }

        var hand = new KtaneVideoPoker.Core.Hand(CardObjects.Select(co => co.Card).Where(c => c.HasValue).Select(c => c.Value));
        var strategyResult = VariantInfo.StrategyMaxBet.Evaluate(hand);

        if (BetAmount == 5)
        {
            AcceptablePlays = new HashSet<int>(strategyResult.Strategies);

            if (!IsSolved)
            {
                if (strategyResult.RuleIndex >= 0)
                {
                    ModuleLog("Use rule {0}", strategyResult.RuleIndex + 1);
                    ModuleLog("Applicable special rules: {0}", strategyResult.ExtraRules.Length > 0 ? strategyResult.ExtraRules.Join(", ") : "none");
                }
                else
                {
                    ModuleLog("Applicable exceptions: {0}", strategyResult.ExtraRules.Length > 0 ? strategyResult.ExtraRules.Join(", ") : "none");
                    ModuleLog("No valid rules found");
                }

                ModuleLog("Acceptable plays:");
                foreach (int strategy in strategyResult.Strategies)
                {
                    if (strategy == 0)
                    {
                        ModuleLog("• Discard everything");
                    }
                    else
                    {
                        ModuleLog("• Hold {0}", CardObjects.Where((co, i) => (strategy & (1 << i)) != 0).Select(co => co.RankText.text + co.SuitText.text).Join(" "));
                    }
                }
            }
        }
        

        State = KtaneVideoPoker.State.ChooseHolds;

        var intermediateHandType = VariantInfo.VariantIfUsingMaxBet(BetAmount == 5).Evaluate(hand);
        if (intermediateHandType == KtaneVideoPoker.Core.HandResult.Nothing)
        {
            GameMessageText.Text = "";
        }
        else
        {
            Audio.PlaySoundAtTransform("gliss1", transform);
            GameMessageText.Text = intermediateHandType.ToFriendlyString();
        }

        DealButton.Text.text = "Draw";
        DealButton.Enable();
    }

    private IEnumerator BlinkProgressLightsRed()
    {
        for (int i = 0; i < 8; i++)
        {
            ProgressLightManager.SetAllRed(i % 2 == 0);
            yield return new WaitForSeconds(0.1f);
        }
    }

    private IEnumerator DrawCards()
    {
        var heldCards = CardObjects.Where(co => co.Held).Select(co => co.RankText.text + co.SuitText.text);
        if (!IsSolved)
        {
            ModuleLog("Attempting to {0}...", heldCards.Count() > 0 ? ("hold " + heldCards.Join(" ")) : "discard everything");
        }

        int strategyIndex = Enumerable.Range(0, 5).Where(i => CardObjects[i].Held).Sum(i => 1 << i);
        if (!IsSolved && !AcceptablePlays.Contains(strategyIndex))
        {
            ModuleLog("Strike! This isn't an optimal play. Resetting streak to 0.");
            Streak = 0;
            ProgressLightManager.SetValue(0);
            StartCoroutine(BlinkProgressLightsRed());
            BombModule.HandleStrike();
        }
        else
        {
            if (!IsSolved)
            {
                Streak++;
                ProgressLightManager.SetValue(Streak);
                ModuleLog("Yes, that's an optimal play! Current streak: {0}", Streak);
            }

            GameMessageText.Text = "";

            State = KtaneVideoPoker.State.SecondDeal;

            DealButton.Disable();

            for (int i = 0; i < 5; i++)
            {
                if (!CardObjects[i].Held)
                {
                    CardObjects[i].Card = null;
                }
            }

            yield return new WaitForSeconds(2 * SpeedButton.GetDelay());

            for (int i = 0; i < 5; i++)
            {
                if (CardObjects[i].Card == null)
                {
                    Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TypewriterKey, transform);
                    CardObjects[i].Card = AvailableCards.Pop();
                    yield return new WaitForSeconds(SpeedButton.GetDelay());
                }
            }

            if (!IsSolved)
            {
                ModuleLog("Final hand: {0}", CardObjects.Select(co => co.RankText.text + co.SuitText.text).Join(" "));
            }

            // Evaluate the hand
            var hand = new KtaneVideoPoker.Core.Hand(CardObjects.Select(co => co.Card).Where(c => c.HasValue).Select(c => c.Value));
            var variant = VariantInfo.VariantIfUsingMaxBet(BetAmount == 5);

            var finalHandType = variant.Evaluate(hand);
            int payout = variant.PayoutForResult(finalHandType) * BetAmount;

            if (finalHandType == KtaneVideoPoker.Core.HandResult.Nothing)
            {
                if (!IsSolved)
                {
                    ModuleLog("Oh well, this hand doesn't pay. Better luck next time!");
                }
                GameMessageText.Text = "PLAY 5 CREDITS";

                StartCoroutine(Payout(0));
            }
            else
            {
                if (!IsSolved)
                {
                    ModuleLog("Hand type {0} with bet {1} pays {2}", finalHandType.ToFriendlyString(), BetAmount, payout);
                }
                GameMessageText.Text = finalHandType.ToFriendlyString();

                if (payout >= JackpotThreshold)
                {
                    JackpotValue = payout;
                    JackpotCoroutine = StartCoroutine(Jackpot());
                    yield break;
                }
                else
                {
                    StartCoroutine(Payout(payout));
                }
            }
        }
    }

    private IEnumerator Jackpot()
    {
        if (!IsSolved)
        {
            ModuleLog("JACKPOT!");
        }
        Audio.PlaySoundAtTransform("jackpot_-3db", transform);
        State = KtaneVideoPoker.State.JackpotPending;

        DealButton.Text.text = "Claim";
        DealButton.Enable();

        // Only commaify numbers >= 10,000
        JackpotText.Text = string.Format("JACKPOT!  {0}\nCALL ATTENDANT", string.Format(JackpotValue >= 10000 ? "{0:n0}" : "{0}", JackpotValue));

        while (true)
        {
            JackpotText.Visible = !JackpotText.Visible;
            yield return new WaitForSeconds(1);
        }
    }

    private void OnLongPress(KMSelectable sender)
    {
        ResetMachine();
    }

    private IEnumerator Payout(int payout)
    {
        JackpotText.Visible = false;

        if (payout > 0)
        {
            State = KtaneVideoPoker.State.Paying;

            int amountPaid = 0;

            BetText.Text = "";

            Audio.PlaySoundAtTransform("gliss2", transform);
            yield return new WaitForSeconds(1f);

            while (amountPaid < payout)
            {
                int increment = (payout - amountPaid) > 50 ? (payout - amountPaid) / 25 : 1;
                Credits += increment;
                amountPaid += increment;
                BetText.Text = string.Format("WIN {0}", amountPaid);
                CreditsText.Text = string.Format("CREDIT {0}", Credits);
                Audio.PlaySoundAtTransform("beep_25ms", transform);
                yield return new WaitForSeconds(0.05f);
            }
        }

        if (payout >= JackpotThreshold && !IsSolved)
        {
            Streak = SolveStreakTarget;
            ProgressLightManager.SetValue(Streak);
            ModuleLog("Lucky you! You won a jackpot. Module disarmed!");
            IsSolved = true;
            BombModule.HandlePass();
        }
        else if (Streak >= SolveStreakTarget && !IsSolved)
        {
            ModuleLog("You played {0} hands correctly in a row. Module disarmed!", SolveStreakTarget);
            IsSolved = true;
            BombModule.HandlePass();
        }

        GameInfoButton.Enable();
        SpeedButton.Enable();
        BetOneButton.Enable();
        BetMaxButton.Enable();
        DealButton.Text.text = "Deal";
        DealButton.Enable();

        State = KtaneVideoPoker.State.Idle;

        NextBetOneSetsBetAmountToOne = true;
    }

    // Buttons

    private void OnPressGameInfo(KMSelectable sender)
    {
        if (State == KtaneVideoPoker.State.Idle)
        {
            State = KtaneVideoPoker.State.ShowPayTable;
            PayTable.Visible = true;

            GameInfoButton.Text.text = "Back";
            SpeedButton.Disable();
            BetOneButton.Disable();
            BetMaxButton.Disable();
            DealButton.Disable();

            // Not the prettiest, but the KMSelectables are the top-level card objects
            foreach (var card in Raw_CardSelectables)
            {
                card.gameObject.SetActive(false);
            }
        }
        else if (State == KtaneVideoPoker.State.ShowPayTable)
        {
            State = KtaneVideoPoker.State.Idle;
            PayTable.Visible = false;

            GameInfoButton.Text.text = "Game Info";
            SpeedButton.Enable();
            BetOneButton.Enable();
            BetMaxButton.Enable();
            DealButton.Enable();

            // Not the prettiest, but the KMSelectables are the top-level card objects
            foreach (var card in Raw_CardSelectables)
            {
                card.gameObject.SetActive(true);
            }
        }
    }

    private void OnPressSpeed(KMSelectable sender)
    {
        if (State == KtaneVideoPoker.State.Idle)
        {
            SpeedButton.ChangeSpeed();
        }
    }

    private void OnPressBetOne(KMSelectable sender)
    {
        if (State == KtaneVideoPoker.State.Idle)
        {
            if (NextBetOneSetsBetAmountToOne)
            {
                BetAmount = 1;
                NextBetOneSetsBetAmountToOne = false;
            }
            else
            {
                BetAmount += 1;
                if (BetAmount == 6)
                {
                    BetAmount = 1;
                }
            }
            BetText.Text = String.Format("BET {0}", BetAmount);
            GameMessageText.Text = "PLAY 5 CREDITS";
        }
    }

    private void OnPressBetMax(KMSelectable sender)
    {
        ModuleLog("OnPressBetMax");
        if (State == KtaneVideoPoker.State.Idle)
        {
            BetAmount = 5;
            BetText.Text = "BET 5";
            if (Credits < 5)
            {
                GameMessageText.Text = "INSUFFICIENT FUNDS";
            }
            else
            {
                StartCoroutine(BeginDeal());
            }
        }
    }

    private void OnPressDeal(KMSelectable sender)
    {
        if (State == KtaneVideoPoker.State.Idle)
        {
            if (!IsSolved && BetAmount != 5)
            {
                ModuleLog("Strike! You should always bet max to keep the payback rate as high as possible. Don't be a wimp; you're not even playing with real money!");
                BombModule.HandleStrike();
            }
            else if (Credits < BetAmount)
            {
                GameMessageText.Text = "INSUFFICIENT FUNDS";
            }
            else
            {
                StartCoroutine(BeginDeal());
            }
        }
        else if (State == KtaneVideoPoker.State.ChooseHolds)
        {
            StartCoroutine(DrawCards());
        }
        else if (State == KtaneVideoPoker.State.JackpotPending)
        {
            StopCoroutine(JackpotCoroutine);
            JackpotCoroutine = null;
            DealButton.Disable();
            StartCoroutine(Payout(JackpotValue));
        }
    }

    private void ResetMachine()
    {
        ModuleLog("Resetting machine...");
        for (int i = 0; i < 5; i++)
        {
            CardObjects[i].Card = null;
        }
        Streak = 0;
        ProgressLightManager.SetValue(0);
        BetAmount = 1;
        BetText.Text = "BET 1";
        Credits = 1000;
        CreditsText.Text = "CREDIT 1000";
        GameMessageText.Text = "PLAY 5 CREDITS";
        State = KtaneVideoPoker.State.Idle;
        GameInfoButton.Text.text = "Game Info";
        DealButton.Text.text = "Deal";

        foreach (var co in CardObjects)
        {
            co.Held = false;
        }

        GameInfoButton.Enable();
        SpeedButton.Enable();
        BetOneButton.Enable();
        BetMaxButton.Enable();
        DealButton.Enable();

        while (SpeedButton.GetSpeedIndex() != 2)
        {
            SpeedButton.ChangeSpeed();
        }

        PayTable.Visible = false;
    }

    #pragma warning disable 414
    string TwitchHelpMessage = "This module uses a context-sensitive help system. Use \"!{0} detailedhelp\" for context-sensitive help. Alternatively, use \"!{0} reset\" to reset the module.";
    #pragma warning restore 414

    private string TPGetContextSensitiveHelpMessage()
    {
        switch (State)
        {
            case KtaneVideoPoker.State.Idle:
            case KtaneVideoPoker.State.SecondDeal:
            case KtaneVideoPoker.State.Paying:
                return "Use \"!{1} deal\" or \"!{1} bet #\" to begin a deal. Use \"!{1} gameinfo\" to view game info. \"!{1} speed (0|1|2|3)\" to set the game's speed (if no number is specified, the speed button will be pressed once).";
            case KtaneVideoPoker.State.ShowPayTable:
                return "Use \"!{1} back\" to go back to the main screen.";
            case KtaneVideoPoker.State.FirstDeal:
            case KtaneVideoPoker.State.ChooseHolds:
                return "Use \"!{1} hold ...\" to select which cards to hold. You can use positions or card names separated by spaces or commas, or the word \"none\". Examples include \"hold none\", \"hold 1 3 5\", \"hold 2,3, 4,5\", \"hold 3c,7c,10c,Kc\", \"hold Td Th 2s\", \"hold 5\u2663, 5\u2665, 5\u2660\", etc.";
            case KtaneVideoPoker.State.JackpotPending:
                return "Lucky you! Use \"!{1} claim\" to claim your jackpot. This will also solve the module, so mind the queue if it exists!";
            default:
                return "Uh-oh, the module is in an unexpected state! Please report this as a bug, and maybe use \"!{1} reset\" to try restting the module.";
        }
    }

    IEnumerator ProcessTwitchCommand(string command)
    {
        yield return null;

        var strippedCommand = command.Trim().ToLowerInvariant();
        if (strippedCommand == "detailedhelp")
        {
            yield return string.Format("sendtochat {0}", TPGetContextSensitiveHelpMessage());
            yield break;
        }

        var tokens = strippedCommand.Split(new[] {' '});
        if (tokens.Length == 0)
        {
            yield break;
        }

        if (State == KtaneVideoPoker.State.FirstDeal || State == KtaneVideoPoker.State.SecondDeal || State == KtaneVideoPoker.State.Paying)
        {
            yield return "sendtochat Hang on! The machine is still busy...";
            yield break;
        }

        if (State == KtaneVideoPoker.State.Idle)
        {
            if (strippedCommand.Equals("deal"))
            {
                // Pressing Bet Max will start a deal anyway, so we'll just be kind to the user
                yield return new[] {BetMaxButton.Selectable};
            }
            else if (tokens[0].Equals("bet") && tokens.Length == 2)
            {
                var betAmount = tokens[1];
                if (betAmount.Length == 1 && ((int) betAmount[0]).InRange('1', '5'))
                {
                    int desiredBet = betAmount[0] - '0';
                    while (BetAmount != desiredBet)
                    {
                        yield return new[] {BetOneButton.Selectable};
                        yield return new WaitForSeconds(0.1f);
                    }
                    yield return new[] {DealButton.Selectable};
                }
            }
            else if (strippedCommand.Equals("gameinfo"))
            {
                yield return new[] {GameInfoButton.Selectable};
            }
            else if (tokens[0] == "speed" && tokens.Length <= 2)
            {
                if (tokens.Length == 1)
                {
                    yield return new[] {SpeedButton.Selectable};
                }
                else if (((int) tokens[1][0]).InRange('0', '3'))
                {
                    int desiredSpeed = tokens[1][0] - '0';
                    while (desiredSpeed != SpeedButton.GetSpeedIndex())
                    {
                        yield return new[] {SpeedButton.Selectable};
                        yield return new WaitForSeconds(0.1f);
                    }
                }
            }
        }
        else if (State == KtaneVideoPoker.State.ShowPayTable && strippedCommand.Equals("back"))
        {
            yield return new[] {GameInfoButton.Selectable};
        }
        else if (State == KtaneVideoPoker.State.ChooseHolds && tokens[0].Equals("hold"))
        {
            // This might solve
            yield return "solve";

            var tokensToPositions = Enumerable.Range(0, 5).ToDictionary(i => (i + 1).ToString(), i => i);
            for (int i = 0; i < CardObjects.Length; i++)
            {
                var optionalCard = CardObjects[i].Card;
                // Only non-jokers can be identified by their values.
                if (optionalCard.HasValue && !optionalCard.Value.IsJoker)
                {
                    var card = optionalCard.Value;
                    var validRankIdentifiers = new[] {card.ToString().Substring(0, 1).ToLowerInvariant()}.ToList();
                    if (card.Rank == 10)
                    {
                        // In this case, accept "T" (which comes from ToString()) or "10" (which we must add explicitly)
                        validRankIdentifiers.Add("10");
                    }
                    var validSuitIdentifiers = new[] {card.ToString().Substring(1)}.ToList();
                    switch (card.Suit)
                    {
                        case KtaneVideoPoker.Core.Suit.Clubs:
                            validSuitIdentifiers.Add("c");
                            break;
                        case KtaneVideoPoker.Core.Suit.Diamonds:
                            validSuitIdentifiers.Add("d");
                            break;
                        case KtaneVideoPoker.Core.Suit.Hearts:
                            validSuitIdentifiers.Add("h");
                            break;
                        case KtaneVideoPoker.Core.Suit.Spades:
                            validSuitIdentifiers.Add("s");
                            break;
                    }
                    var validCardIdentifiers = validRankIdentifiers.SelectMany(r => validSuitIdentifiers.Select(s => r + s));
                    foreach (var identifier in validCardIdentifiers)
                    {
                        tokensToPositions[identifier] = i;
                    }
                }
            }

            var holdThese = tokens.Skip(1).SelectMany(token => token.Split(new[] {','})).Where(token => token.Length > 0);
            if (holdThese.Count() == 1 && holdThese.First().Equals("none"))
            {
                // Special case: discard all cards
                yield return CardObjects.Where(co => co.Held).Select(co => co.Selectable).Concat(new[] {DealButton.Selectable}).ToArray();
            }
            else if (holdThese.Count() > 0)
            {
                var unknownCards = holdThese.Where(token => !tokensToPositions.ContainsKey(token));
                if (unknownCards.Any())
                {
                    yield return string.Format("sendtochaterror I can't find these cards: {0}", unknownCards.Select(str => "\"" + str + "\"").Join(", "));
                    yield break;
                }
                var bitmaskToHold = holdThese.Select(token => tokensToPositions[token]).Aggregate(0, (bitmask, i) => bitmask | (1 << i));
                var indexShouldBeHeld = Enumerable.Range(0, 5).Select(i => (bitmaskToHold & (1 << i)) != 0).ToArray();

                var cardsToTouch = Enumerable.Range(0, 5).Where(i => CardObjects[i].Held != indexShouldBeHeld[i]).Select(i => CardObjects[i]);

                if (cardsToTouch.Any())
                {
                    yield return cardsToTouch.Select(cardObject => cardObject.Selectable).ToArray();
                }
                yield return new[] {DealButton.Selectable};
            }
        }
        else if (State == KtaneVideoPoker.State.JackpotPending && strippedCommand.Equals("claim"))
        {
            // This will solve after the pay finishes, so attribute the solve to this player right now
            yield return "solve";
            yield return new[] {DealButton.Selectable};
        }
    }
}

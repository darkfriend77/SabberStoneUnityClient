using SabberStoneBasicAI.Nodes;
using SabberStoneBasicAI.Score;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model;
using SabberStoneCore.Tasks.PlayerTasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
public partial class GameController
{
    private GameConfig DruidVsWarrior => new GameConfig
    {
        StartPlayer = 1,
        FormatType = FormatType.FT_WILD,
        Player1HeroClass = CardClass.DRUID,
        Player1Deck = new List<Card>() {
            Cards.FromName("Innervate"),
            Cards.FromName("Innervate"),
            Cards.FromName("Living Roots"),
            Cards.FromName("Living Roots"),
            Cards.FromName("Raven Idol"),
            Cards.FromName("Raven Idol"),
            Cards.FromName("Wrath"),
            Cards.FromName("Wrath"),
            Cards.FromName("Feral Rage"),
            Cards.FromName("Mulch"),
            Cards.FromName("Wild Growth"),
            Cards.FromName("Wild Growth"),
            Cards.FromName("Fandral Staghelm"),
            Cards.FromName("Mire Keeper"),
            Cards.FromName("Mire Keeper"),
            Cards.FromName("Swipe"),
            Cards.FromName("Swipe"),
            Cards.FromName("Moonglade Portal"),
            Cards.FromName("Moonglade Portal"),
            Cards.FromName("Nourish"),
            Cards.FromName("Nourish"),
            Cards.FromName("Aviana"),
            Cards.FromName("Barnes"),
            Cards.FromName("Emperor Thaurissan"),
            Cards.FromName("Bog Creeper"),
            Cards.FromName("Kel'Thuzad"),
            Cards.FromName("Ragnaros the Firelord"),
            Cards.FromName("The Lich King"),
            Cards.FromName("Ysera"),
            Cards.FromName("Y'Shaarj, Rage Unbound")
            },
        Player2HeroClass = CardClass.WARRIOR,
        Player2Deck = new List<Card>() {
            Cards.FromName("Upgrade!"),
            Cards.FromName("Upgrade!"),
            Cards.FromName("Heroic Strike"),
            Cards.FromName("Heroic Strike"),
            Cards.FromName("Woodcutter's Axe"),
            Cards.FromName("Woodcutter's Axe"),
            Cards.FromName("Frothing Berserker"),
            Cards.FromName("Frothing Berserker"),
            Cards.FromName("Kor'kron Elite"),
            Cards.FromName("Kor'kron Elite"),
            Cards.FromName("Mortal Strike"),
            Cards.FromName("Mortal Strike"),
            Cards.FromName("Arcanite Reaper"),
            Cards.FromName("Arcanite Reaper"),
            Cards.FromName("Sul'thraze"),
            Cards.FromName("Bloodsail Corsair"),
            Cards.FromName("Bloodsail Corsair"),
            Cards.FromName("Southsea Deckhand"),
            Cards.FromName("Southsea Deckhand"),
            Cards.FromName("Hench-Clan Thug"),
            Cards.FromName("Hench-Clan Thug"),
            Cards.FromName("Nightmare Amalgam"),
            Cards.FromName("Nightmare Amalgam"),
            Cards.FromName("Southsea Captain"),
            Cards.FromName("Southsea Captain"),
            Cards.FromName("Dread Corsair"),
            Cards.FromName("Dread Corsair"),
            Cards.FromName("Captain Greenskin"),
            Cards.FromName("Leeroy Jenkins"),
            Cards.FromName("Zilliax")
            },
        SkipMulligan = false,
        Shuffle = true,
        FillDecks = false,
        Logging = true,
        History = true
    };
    private Func<int, Game, bool> DruidVsWarriorMoves = (step, _game) =>
    {
        if (step == 0)
        {
            _game.StartGame();
            return true;
        }

        if (step == 1 && _game.Player1.MulliganState == Mulligan.INPUT)
        {
            List<int> mulligan1 = new RampScore().MulliganRule().Invoke(_game.Player1.Choice.Choices.Select(p => _game.IdEntityDic[p]).ToList());
            _game.Process(ChooseTask.Mulligan(_game.Player1, new List<int>()));
            return true;
        }

        if (step == 2 && _game.Player2.MulliganState == Mulligan.INPUT)
        {
            List<int> mulligan2 = new RampScore().MulliganRule().Invoke(_game.Player2.Choice.Choices.Select(p => _game.IdEntityDic[p]).ToList());
            _game.Process(ChooseTask.Mulligan(_game.Player2, mulligan2));
            return true;
        }

        if (step == 3 && _game.Player1.MulliganState == Mulligan.DONE && _game.Player2.MulliganState == Mulligan.DONE)
        {
            _game.MainReady();
            return true;
        }

        if (_game.Player1 == _game.CurrentPlayer)
        {
            List<OptionNode> solutions = OptionNode.GetSolutions(_game, _game.CurrentPlayer.Id, new RampScore(), 10, 500);
            var solution = new List<PlayerTask>();
            solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
            _game.Process(solution.First());
        }
        else if (_game.Player2 == _game.CurrentPlayer)
        {
            List<OptionNode> solutions = OptionNode.GetSolutions(_game, _game.CurrentPlayer.Id, new AggroScore(), 10, 500);
            var solution = new List<PlayerTask>();
            solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
            _game.Process(solution.First());
        }
        else
        {
            Debug.Log("What the fuck is going on!");
            return false;
        }

        return true;
    };
    private GameConfig RogueVsWarlock => new GameConfig
    {
        StartPlayer = 1,
        FormatType = FormatType.FT_STANDARD,
        Player1HeroClass = CardClass.ROGUE,
        Player1Deck = new List<Card>() {
            Cards.FromName("Sap"),
            Cards.FromName("Acidic Swamp Ooze"),
            Cards.FromName("Assassin's Blade"),
            Cards.FromName("Bloodfen Raptor"),
            Cards.FromName("Backstab"),
            Cards.FromName("Backstab"),
            Cards.FromName("Deadly Poison"),
            Cards.FromName("Deadly Poison"),
            Cards.FromName("Shiv"),
            Cards.FromName("Shiv"),
            Cards.FromName("Fan of Knives"),
            Cards.FromName("Fan of Knives"),
            Cards.FromName("Assassin's Blade"),
            Cards.FromName("Assassinate"),
            Cards.FromName("Assassinate"),
            Cards.FromName("Sprint"),
            Cards.FromName("Acidic Swamp Ooze"),
            Cards.FromName("Bloodfen Raptor"),
            Cards.FromName("Kobold Geomancer"),
            Cards.FromName("Razorfen Hunter"),
            Cards.FromName("Shattered Sun Cleric"),
            Cards.FromName("Shattered Sun Cleric"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Gnomish Inventor"),
            Cards.FromName("Gnomish Inventor"),
            Cards.FromName("Sen'jin Shieldmasta"),
            Cards.FromName("Sen'jin Shieldmasta"),
            Cards.FromName("Boulderfist Ogre"),
            Cards.FromName("Boulderfist Ogre")
            },
        Player2HeroClass = CardClass.WARLOCK,
        Player2Deck = new List<Card>() {
            Cards.FromName("Voidwalker"),
            Cards.FromName("Dread Infernal"),
            Cards.FromName("Corruption"),
            Cards.FromName("Corruption"),
            Cards.FromName("Mortal Coil"),
            Cards.FromName("Mortal Coil"),
            Cards.FromName("Soulfire"),
            Cards.FromName("Soulfire"),
            Cards.FromName("Voidwalker"),
            Cards.FromName("Felstalker"),
            Cards.FromName("Felstalker"),
            Cards.FromName("Drain Life"),
            Cards.FromName("Drain Life"),
            Cards.FromName("Shadow Bolt"),
            Cards.FromName("Shadow Bolt"),
            Cards.FromName("Hellfire"),
            Cards.FromName("Hellfire"),
            Cards.FromName("Dread Infernal"),
            Cards.FromName("Voodoo Doctor"),
            Cards.FromName("Voodoo Doctor"),
            Cards.FromName("Kobold Geomancer"),
            Cards.FromName("Kobold Geomancer"),
            Cards.FromName("Ogre Magi"),
            Cards.FromName("Ogre Magi"),
            Cards.FromName("Sen'jin Shieldmasta"),
            Cards.FromName("Sen'jin Shieldmasta"),
            Cards.FromName("Darkscale Healer"),
            Cards.FromName("Darkscale Healer"),
            Cards.FromName("Gurubashi Berserker"),
            Cards.FromName("Gurubashi Berserker")
            },
        SkipMulligan = true,
        Shuffle = false,
        FillDecks = false,
        Logging = true,
        History = true
    };
    private Func<int, Game, bool> RogueVsWarlockMoves = (step, _game) =>
    {
        if (step == 0)
        {
            _game.StartGame();
            return true;
        }

        if (_game.Player1 == _game.CurrentPlayer)
        {
            List<OptionNode> solutions = OptionNode.GetSolutions(_game, _game.CurrentPlayer.Id, new AggroScore(), 10, 500);
            var solution = new List<PlayerTask>();
            solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
            _game.Process(solution.First());
        }
        else if (_game.Player2 == _game.CurrentPlayer)
        {
            List<OptionNode> solutions = OptionNode.GetSolutions(_game, _game.CurrentPlayer.Id, new ControlScore(), 10, 500);
            var solution = new List<PlayerTask>();
            solutions.OrderByDescending(p => p.Score).First().PlayerTasks(ref solution);
            _game.Process(solution.First());
        }
        else
        {
            Debug.Log("What the fuck is going on!");
            return false;
        }

        return true;
    };

    private GameConfig PaladinVsPriest => new GameConfig
    {
        StartPlayer = 1,
        FormatType = FormatType.FT_STANDARD,
        Player1HeroClass = CardClass.PALADIN,
        Player1Deck = new List<Card>() {
            Cards.FromName("Murloc Tidehunter"),
            Cards.FromName("Frostwolf Warlord"),
            Cards.FromName("Stormwind Champion"),
            Cards.FromName("Blessing of Kings"),
            Cards.FromName("Consecration"),
            Cards.FromName("Hammer of Wrath"),
            Cards.FromName("Truesilver Champion"),
            Cards.FromName("Razorfen Hunter"),
            Cards.FromName("Razorfen Hunter"),
            Cards.FromName("Guardian of Kings"),
            Cards.FromName("Shattered Sun Cleric"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Frostwolf Warlord"),
            Cards.FromName("Boulderfist Ogre"),
            Cards.FromName("Bloodfen Raptor"),
            Cards.FromName("Acidic Swamp Ooze"),
            Cards.FromName("Murloc Tidehunter"),
            Cards.FromName("River Crocolisk"),
            Cards.FromName("Shattered Sun Cleric"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Frostwolf Warlord"),
            Cards.FromName("Boulderfist Ogre"),
            Cards.FromName("Stormwind Champion"),
            Cards.FromName("Blessing of Kings"),
            Cards.FromName("Consecration"),
            Cards.FromName("Hammer of Wrath"),
            Cards.FromName("Truesilver Champion"),
            Cards.FromName("Guardian of Kings"),
            Cards.FromName("Acidic Swamp Ooze"),
            Cards.FromName("Bloodfen Raptor")
            },
        Player2HeroClass = CardClass.PRIEST,
        Player2Deck = new List<Card>() {
            Cards.FromName("Holy Smite"),
            Cards.FromName("Voodoo Doctor"),
            Cards.FromName("Acidic Swamp Ooze"),
            Cards.FromName("Northshire Cleric"),
            Cards.FromName("Power Word: Shield"),
            Cards.FromName("Divine Spirit"),
            Cards.FromName("Shadow Word: Pain"),
            Cards.FromName("River Crocolisk"),
            Cards.FromName("River Crocolisk"),
            Cards.FromName("Ironfur Grizzly"),
            Cards.FromName("Ironfur Grizzly"),
            Cards.FromName("Shattered Sun Cleric"),
            Cards.FromName("Holy Nova"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Gnomish Inventor"),
            Cards.FromName("Darkscale Healer"),
            Cards.FromName("Darkscale Healer"),
            Cards.FromName("Gurubashi Berserker"),
            Cards.FromName("Boulderfist Ogre"),
            Cards.FromName("Stormwind Champion"),
            Cards.FromName("Stormwind Champion"),
            Cards.FromName("Northshire Cleric"),
            Cards.FromName("Holy Smite"),
            Cards.FromName("Power Word: Shield"),
            Cards.FromName("Shadow Word: Pain"),
            Cards.FromName("Shadow Word: Death"),
            Cards.FromName("Holy Nova"),
            Cards.FromName("Mind Control"),
            Cards.FromName("Holy Nova"),
            Cards.FromName("Shattered Sun Cleric")
            },
        SkipMulligan = true,
        Shuffle = false,
        FillDecks = false,
        Logging = true,
        History = true
    };
    private Func<int, Game, bool> PaladinVsPriestMoves = (step, _game) =>
    {
        switch (step)
        {
            case 0: _game.StartGame(); break;

            // --- FULL GAME ---
            // ROUND 1 
            // turn player 1
            case 1: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 2: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 2 
            // turn player 1
            case 3: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Murloc Tidehunter")); break;
            case 4: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 5: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Holy Smite", _game.CurrentOpponent.BoardZone[0])); break;
            case 6: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 3 
            // turn player 1
            case 7: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 8: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer)); break;
            case 9: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 10: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "The Coin")); break;
            case 11: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Northshire Cleric")); break;
            case 12: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Power Word: Shield", _game.CurrentPlayer.BoardZone[0])); break;
            case 13: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 3 
            // turn player 1
            case 14: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[0])); break;
            case 15: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[0])); break;
            case 16: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Truesilver Champion")); break;
            case 17: _game.Process(HeroAttackTask.Any(_game.CurrentPlayer, _game.CurrentOpponent.Hero)); break;
            case 18: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 19: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0])); break;
            case 20: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Acidic Swamp Ooze")); break;
            case 21: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 4 Razorfen Hunter
            // turn player 1
            case 22: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Razorfen Hunter")); break;
            case 23: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer)); break;
            case 24: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 25: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Divine Spirit", _game.CurrentPlayer.BoardZone[0])); break;
            case 26: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Shadow Word: Pain", _game.CurrentOpponent.BoardZone[0])); break;
            case 27: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 28: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 29: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 5 
            // turn player 1
            case 30: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Frostwolf Warlord")); break;
            case 31: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 32: _game.Process(PlayCardTask.MinionTarget(_game.CurrentPlayer, "Shattered Sun Cleric", _game.CurrentPlayer.BoardZone[0])); break;
            case 33: _game.Process(PlayCardTask.Minion(_game.CurrentPlayer, "Ironfur Grizzly")); break;
            case 34: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 35: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 36: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 6 
            // turn player 1
            case 37: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Stormwind Champion")); break;
            case 38: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 39: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[1])); break;
            case 40: _game.Process(PlayCardTask.Spell(_game.CurrentPlayer, "Holy Nova")); break;
            case 41: _game.Process(PlayCardTask.MinionTarget(_game.CurrentPlayer, "Voodoo Doctor", _game.CurrentPlayer.Hero)); break;
            case 42: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 43: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2], _game.CurrentOpponent.Hero)); break;
            case 44: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[3], _game.CurrentOpponent.Hero)); break;
            case 45: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 7
            // turn player 1
            case 46: _game.Process(PlayCardTask.Spell(_game.CurrentPlayer, "Consecration")); break;
            case 47: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Hammer of Wrath", _game.CurrentOpponent.BoardZone[1])); break;
            case 48: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 49: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 50: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 51: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.Hero)); break;
            case 52: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Ironfur Grizzly")); break;
            case 53: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "River Crocolisk")); break;
            case 54: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 8
            // turn player 1
            case 55: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[1])); break;
            case 56: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 57: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Guardian of Kings")); break;
            case 58: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer)); break;
            case 59: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 60: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.Hero)); break;
            case 61: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 62: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 63: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Chillwind Yeti")); break;
            case 64: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "River Crocolisk")); break;
            case 65: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 9
            // turn player 1
            case 66: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Blessing of Kings", _game.CurrentPlayer.BoardZone[0])); break;
            case 67: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 68: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;

            // ENDED

            default:
                Debug.Log("Next step is not implemented, please add it!");
                return false;
        }

        return true;
    };
    private GameConfig MageVsMage => new GameConfig
    {
        StartPlayer = 1,
        FormatType = FormatType.FT_STANDARD,
        Player1HeroClass = CardClass.MAGE,
        Player1Deck = new List<Card>() {
                Cards.FromName("Arcane Missiles"),
                Cards.FromName("Frostwolf Grunt"),
                Cards.FromName("Frostbolt"),
                Cards.FromName("Kobold Geomancer"),
                Cards.FromName("Shattered Sun Cleric"),
                Cards.FromName("Goldshire Footman"),
                Cards.FromName("Sen'jin Shieldmasta"),
                Cards.FromName("Gurubashi Berserker"),
                Cards.FromName("Fireball"),
                Cards.FromName("Arcane Missiles"),
                Cards.FromName("Frostbolt"),
                Cards.FromName("Fireball"),
                Cards.FromName("Polymorph"),
                Cards.FromName("Polymorph"),
                Cards.FromName("Water Elemental"),
                Cards.FromName("Water Elemental"),
                Cards.FromName("Flamestrike"),
                Cards.FromName("Flamestrike"),
                Cards.FromName("Arcane Intellect"),
                Cards.FromName("Goldshire Footman"),
                Cards.FromName("Frostwolf Grunt"),
                Cards.FromName("Kobold Geomancer"),
                Cards.FromName("Dalaran Mage"),
                Cards.FromName("Dalaran Mage"),
                Cards.FromName("Sen'jin Shieldmasta"),
                Cards.FromName("Darkscale Healer"),
                Cards.FromName("Darkscale Healer"),
                Cards.FromName("Gurubashi Berserker"),
                Cards.FromName("Boulderfist Ogre"),
                Cards.FromName("Boulderfist Ogre")
            },
        Player2HeroClass = CardClass.MAGE,
        Player2Deck = new List<Card>() {
                Cards.FromName("Arcane Missiles"),
                Cards.FromName("Frostwolf Grunt"),
                Cards.FromName("Goldshire Footman"),
                Cards.FromName("Frostbolt"),
                Cards.FromName("Arcane Intellect"),
                Cards.FromName("Dalaran Mage"),
                Cards.FromName("Darkscale Healer"),
                Cards.FromName("Kobold Geomancer"),
                Cards.FromName("Gurubashi Berserker"),
                Cards.FromName("Kobold Geomancer"),
                Cards.FromName("Fireball"),
                Cards.FromName("Arcane Missiles"),
                Cards.FromName("Frostbolt"),
                Cards.FromName("Fireball"),
                Cards.FromName("Polymorph"),
                Cards.FromName("Polymorph"),
                Cards.FromName("Water Elemental"),
                Cards.FromName("Water Elemental"),
                Cards.FromName("Flamestrike"),
                Cards.FromName("Flamestrike"),
                Cards.FromName("Arcane Intellect"),
                Cards.FromName("Goldshire Footman"),
                Cards.FromName("Frostwolf Grunt"),
                Cards.FromName("Dalaran Mage"),
                Cards.FromName("Sen'jin Shieldmasta"),
                Cards.FromName("Sen'jin Shieldmasta"),
                Cards.FromName("Darkscale Healer"),
                Cards.FromName("Gurubashi Berserker"),
                Cards.FromName("Boulderfist Ogre"),
                Cards.FromName("Boulderfist Ogre")
            },
        SkipMulligan = true,
        Shuffle = false,
        FillDecks = false,
        Logging = true,
        History = true
    };
    private Func<int, Game, bool> MageVsMageMoves = (step, _game) =>
    {
        switch (step)
        {
            case 0: _game.StartGame(); break;

            // --- FULL GAME ---
            // ROUND 1 
            // turn player 1
            case 1: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Arcane Missiles")); break;
            case 2: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 3: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "The Coin")); break;
            case 4: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Frostwolf Grunt")); break;
            case 5: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 2 
            // turn player 1
            case 6: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Frostbolt", _game.CurrentOpponent.BoardZone[0])); break;
            case 7: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 8: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Goldshire Footman")); break;
            case 9: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 3 
            // turn player 1
            case 10: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Frostwolf Grunt")); break;
            case 11: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Goldshire Footman", zonePosition: 1)); break;
            case 12: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 13: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Arcane Intellect")); break;
            case 14: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 4 
            // turn player 1
            case 15: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.BoardZone[0])); break;
            case 16: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentOpponent.BoardZone[0])); break;
            case 17: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Kobold Geomancer", zonePosition: 1)); break;
            case 18: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 19: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 20: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Kobold Geomancer")); break;
            case 21: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Frostbolt", _game.CurrentOpponent.Hero)); break;
            case 22: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 5 
            // turn player 1
            case 23: _game.Process(PlayCardTask.MinionTarget(_game.CurrentPlayer, "Shattered Sun Cleric", _game.CurrentPlayer.BoardZone[0])); break;
            case 24: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentOpponent.Hero)); break;
            case 25: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 26: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 27: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2], _game.CurrentOpponent.Hero)); break;
            case 28: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 29: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentOpponent.BoardZone[0])); break;
            case 30: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[0])); break;
            case 31: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Dalaran Mage")); break;
            case 32: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 6 
            // turn player 1
            case 33: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Gurubashi Berserker")); break;
            case 34: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 35: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 36: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2], _game.CurrentOpponent.Hero)); break;
            case 37: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 38: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[1])); break;
            case 39: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Darkscale Healer")); break;
            case 40: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 7
            // turn player 1
            case 41: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2])); break;
            case 42: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 43: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            case 44: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2], _game.CurrentOpponent.Hero)); break;
            case 45: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Fireball", _game.CurrentOpponent.Hero)); break;

            // ENDED

            default:
                Debug.Log("Next step is not implemented, please add it!");
                return false;
        }

        return true;
    };
}

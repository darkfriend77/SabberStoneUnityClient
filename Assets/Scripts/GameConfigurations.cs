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
            Cards.FromName("Guardian of Kings"),
            Cards.FromName("Razorfen Hunter"),
            Cards.FromName("Shattered Sun Cleric"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Frostwolf Warlord"),
            Cards.FromName("Boulderfist Ogre"),
            Cards.FromName("Bloodfen Raptor"),
            Cards.FromName("Acidic Swamp Ooze"),
            Cards.FromName("Murloc Tidehunter"),
            Cards.FromName("River Crocolisk"),
            Cards.FromName("Razorfen Hunter"),
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
            Cards.FromName("Shattered Sun Cleric"),
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
            Cards.FromName("Holy Nova")
            },
        SkipMulligan = true,
        Shuffle = false,
        FillDecks = false,
        Logging = true,
        History = true
    };
    private Action<int, Game> PaladinVsPriestMoves = (step, _game) =>
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

            // ROUND 3 
            // turn player 1
            case 5: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Holy Smite", _game.CurrentOpponent.BoardZone[0])); break;
            case 6: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 7: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            case 8: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer)); break;
            case 9: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            // ROUND 3 
            // turn player 1
            case 10: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "The Coin")); break;
            case 11: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Northshire Cleric")); break;
            case 12: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Power Word: Shield", _game.CurrentPlayer.BoardZone[0])); break;
            case 13: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            // turn player 2
            case 14: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[0])); break;
            case 15: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[0])); break;
            case 16: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Truesilver Champion")); break;
            case 17: _game.Process(HeroAttackTask.Any(_game.CurrentPlayer, _game.CurrentOpponent.Hero)); break;
            case 18: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            //// ROUND 4 
            //// turn player 1
            //case 15: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.BoardZone[0])); break;
            //case 16: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentOpponent.BoardZone[0])); break;
            //case 17: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Kobold Geomancer", zonePosition: 1)); break;
            //case 18: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            //case 19: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            //// turn player 2
            //case 20: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Kobold Geomancer")); break;
            //case 21: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Frostbolt", _game.CurrentOpponent.Hero)); break;
            //case 22: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            //// ROUND 5 
            //// turn player 1
            //case 23: _game.Process(PlayCardTask.MinionTarget(_game.CurrentPlayer, "Shattered Sun Cleric", _game.CurrentPlayer.BoardZone[0])); break;
            //case 24: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentOpponent.Hero)); break;
            //case 25: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            //case 26: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            //case 27: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2], _game.CurrentOpponent.Hero)); break;
            //case 28: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            //// turn player 2
            //case 29: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentOpponent.BoardZone[0])); break;
            //case 30: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[0])); break;
            //case 31: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Dalaran Mage")); break;
            //case 32: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            //// ROUND 6 
            //// turn player 1
            //case 33: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Gurubashi Berserker")); break;
            //case 34: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            //case 35: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            //case 36: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2], _game.CurrentOpponent.Hero)); break;
            //case 37: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;
            //// turn player 2
            //case 38: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.BoardZone[1])); break;
            //case 39: _game.Process(PlayCardTask.Any(_game.CurrentPlayer, "Darkscale Healer")); break;
            //case 40: _game.Process(EndTurnTask.Any(_game.CurrentPlayer)); break;

            //// ROUND 7
            //// turn player 1
            //case 41: _game.Process(HeroPowerTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2])); break;
            //case 42: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[0], _game.CurrentOpponent.Hero)); break;
            //case 43: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[1], _game.CurrentOpponent.Hero)); break;
            //case 44: _game.Process(MinionAttackTask.Any(_game.CurrentPlayer, _game.CurrentPlayer.BoardZone[2], _game.CurrentOpponent.Hero)); break;
            //case 45: _game.Process(PlayCardTask.SpellTarget(_game.CurrentPlayer, "Fireball", _game.CurrentOpponent.Hero)); break;

            // ENDED

            default:
                Debug.Log("Next step is not implemented, please add it!");
                return;
        }
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
    private Action<int, Game> MageVsMageMoves = (step, _game) =>
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
                return;
        }
    };
}

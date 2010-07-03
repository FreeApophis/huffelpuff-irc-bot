using System;
using System.Collections.Generic;

namespace Plugin
{
    internal class PokerGame
    {
        internal PokerGame()
        {
            players = new List<PokerPlayer>();
            State = GameState.CleanBoard;
            deck = new RandomDeck();
        }

        public GameState Next()
        {
            switch (State)
            {
                case GameState.CleanBoard:
                    Start();
                    break;
                case GameState.PocketCards:
                    break;
                case GameState.Flop:
                    break;
                case GameState.Turn:
                    break;
                case GameState.River:
                    break;
                case GameState.PayOut:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return State;
        }

        private void Start()
        {
            deck.InitDeck();

            foreach (var player in players)
            {
                deck.NextCard();
                player.PocketCards.Card1 = deck.CurrentCard;

                deck.NextCard();
                player.PocketCards.Card2 = deck.CurrentCard;
            }

            State = GameState.PocketCards;
        }

        private readonly RandomDeck deck;

        public GameState State { get; private set; }

        private readonly List<PokerPlayer> players;

        public List<PokerPlayer> Players
        {
            get { return players; }
        }

        public enum GameState
        {
            CleanBoard,
            PocketCards,
            Flop,
            Turn,
            River,
            PayOut
        }
    }
}

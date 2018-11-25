using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FsCheck;

namespace Sheepish.CSharp
{
    class E_Example_Generators
    {

        void Examples()
        {
            int size = 12;
            Card exampleCard1 = new Card();
            Card exampleCard2 = new Card { Rank = Rank.Two };
            Gen<Card> queenSpadeGen = Gen.Constant(Cards.QueenOfSpades);
            Gen<Card> trickyCardGen = Gen.OneOf(heartGen, queenSpadeGen, cardGen);
            Gen<IList<Card>> handGen = cardGen.ListOf(5);
            Gen<Tuple<Card, Card>> twoGen = cardGen.Two();
            Gen<Tuple<Card, Card, Card>> threeGen = cardGen.Three();
            Gen<Tuple<Card, Card, Card, Card>> fourGen = cardGen.Four();
            Gen<Card> exampleCardGen = Gen.Elements(new Card[] { exampleCard1, exampleCard2 });
            Gen<Card> exampleCardGen2 = Gen.GrowingElements(new Card[] { exampleCard1, exampleCard2 });
            Gen<IList<Card>> cardsGen = cardGen.ListOf(size);
            Gen<IList<Card>> wildGen = cardGen.NonEmptyListOf();
            Gen<Card> heartFilterGen = cardGen.Where(c => c.Suit == Suit.Hearts);
            Gen<Card> twoSuitedCardGen = cardGen.Where(c => c.Suit == Suit.Hearts && c.Suit == Suit.Clubs); // impossible or improbable
            Gen<Card[]> shuffledExampleCards = Gen.Shuffle(new Card[] { exampleCard1, exampleCard2 });
        }

        Gen<Card> heartGen =
            Gen.Elements((Rank[])Enum.GetValues(typeof(Rank)))
            .Select(rank => new Card { Rank = rank, Suit = Suit.Hearts });

        Gen<Card> cardGen =
            Gen.zip(
                Gen.Elements((Rank[])Enum.GetValues(typeof(Rank))),
                Gen.Elements((Suit[])Enum.GetValues(typeof(Suit))))
            .Select(c => new Card { Rank = c.Item1, Suit = c.Item2 });
    }

public struct Card
    {
        public Suit Suit;
        public Rank Rank;
    }
    public static class Cards
    {
        public static readonly Card QueenOfSpades = new Card { Rank = Rank.Queen, Suit = Suit.Spades };
    }
    public enum Suit
    {
        Clubs, Hearts, Spades, Diamonds
    }
    public enum Rank
    {
        King, Queen, Jack, Ten, Nine, Eight, Seven, Six, Five, Four, Three, Two, Ace
    }
}
